using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using NAudio.Wave;
using NAudio.SoundFont;



namespace Raptor
{
    public class Game : GameWindow
    {
        private WaveOutEvent backgroundSoundPlayer = new WaveOutEvent();// Плеер для фоновой музыки
        private AudioFileReader backgroundSound =  new AudioFileReader("Sounds/Bckg-1.mp3");// Файл с музыкой
        private WaveOutEvent shootSoundPlayer = new WaveOutEvent();// Плеер для звука выстрела
        private AudioFileReader shootSound = new AudioFileReader("Sounds/punch.mp3");// Файл со звуком выстрела
        
        public int enemiesPassed = 0;  // Счётчик пропущеных врагов
        private float shootCooldown = 0f; // Перезарядка после выстрела. Не меняем
        public float cooldownTime = 0.15f; // Время между выстрелами, в секундах которое можно задать
        int playerTexture, bulletTexture, enemyTexture;
        float playerX = 0.0f;//стартовая позиция 
        float playerY = -0.8f;
        private List<Bullet> bullets = new List<Bullet>();
        private List<Enemy> enemies = new List<Enemy>();
        Random random = new Random();
        float spawnTimer = 0;
        public float bulletSpeed = 0.05f;
        public int enemiesRemovedCount = 0; //кол-во убитых врагов
        public Game(int w, int h, string title) :
            base(w, h, GraphicsMode.Default, title, GameWindowFlags.FixedWindow)
        {
            this.X = 600;
            this.Y = 50;
        }
        protected override void OnLoad(EventArgs e)
        {
            shootSoundPlayer.Init(shootSound);
            backgroundSoundPlayer.Init(backgroundSound);
            backgroundSoundPlayer.Play();
            GL.Enable(EnableCap.Blend);
            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            playerTexture = LoadTexture("Textures/player.png");
            bulletTexture = LoadTexture("Textures/bullet.png");
            enemyTexture = LoadTexture("Textures/enemy.png");
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
        }
        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTexture(playerTexture);
            GL.DeleteTexture(bulletTexture);
            GL.DeleteTexture(enemyTexture);
            backgroundSoundPlayer.Stop();
            backgroundSoundPlayer.Dispose();
            backgroundSound.Dispose();
            shootSoundPlayer.Dispose();
            shootSound.Dispose();
            base.OnUnload(e);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            var keyboard = Keyboard.GetState();
            shootCooldown -= (float)e.Time;
            if (shootCooldown < 0) shootCooldown = 0;
            if (keyboard.IsKeyDown(Key.Left) && playerX > -0.95f) playerX -= 0.03f;
            if (keyboard.IsKeyDown(Key.Right) && playerX < 0.95f) playerX += 0.03f;
            if (keyboard.IsKeyDown(Key.Up) &&  playerY < 0.95f) playerY += 0.03f;
            if (keyboard.IsKeyDown(Key.Down) && playerY > -0.95f) playerY -= 0.04f;
            if (keyboard.IsKeyDown(Key.Escape))Exit();
            if (keyboard.IsKeyDown(Key.B) && enemiesRemovedCount >= 2) {
                cooldownTime = 0.11f;
                bulletSpeed = 0.09f; } ;
            if (keyboard.IsKeyDown(Key.Space) && shootCooldown <= 0)
            {
                bullets.Add(new Bullet(playerX + 0.03f, playerY + 0.03f, bulletTexture, bulletSpeed));
                bullets.Add(new Bullet(playerX - 0.03f, playerY + 0.03f, bulletTexture, bulletSpeed));
                shootCooldown = cooldownTime; // Устанавливаем время перезарядки после выстрела
                PlayShootSound(); // Воспроизводим звук выстрела
            }
            foreach (var bullet in bullets)// Обновление позиций пуль
                bullet.Y += bullet.Speed;
            foreach (var enemy in enemies) // Обновление врагов
            {
                enemy.Y -= enemy.Speed; // Движение врага
                enemy.ShootCooldown -= (float)e.Time; // Обновление времени перезарядки у врагов
                if (enemy.ShootCooldown <= 0)// Стрельба врагов
                {
                    enemy.Bullets.Add(new EnemyBullet(enemy.X, enemy.Y - 0.1f, bulletTexture));
                    enemy.ShootCooldown = 2.0f; // Устанавливаем время перезарядки для следующего выстрела
                }
                foreach (var bullet in enemy.Bullets)// Обновление пуль врагов
                    bullet.Y -= 0.02f;  // Пули врагов летят медленно

                // Удаление пуль, которые вышли за экран
                enemy.Bullets.RemoveAll(b => b.Y < -1.0f);
            }
            bullets.RemoveAll(b => b.Y > 1.0f);// Удаление пуль вне экрана                                               
            for (int i = enemies.Count - 1; i >= 0; i--)//если враг вылетел за нижнюю часть карты
            {
                if (enemies[i].Y < -1.0f) // Если враг вышел за нижний край
                {
                    enemiesPassed++; // Увеличиваем счётчик
                    enemies.RemoveAt(i); // Удаляем врага
                }
            }
            // Спавн врагов
            spawnTimer += (float)e.Time;
            if (spawnTimer >= 1.3f)
            {
                float x = (float)(random.NextDouble() * 1.8 - 0.9); // Генерация позиции врага по X
                int health = random.Next(4, 9);
                float speed;
                if (health >= 4 && health < 6)
                {
                    speed = 0.009f;
                }
                else 
                {
                    if (health >= 6 && health < 8)
                    {
                        speed = 0.0065f;
                    }
                    else
                    {
                        speed = 0.0012f;
                    }
                }
                enemies.Add(new Enemy(x, 1.0f, enemyTexture, health, speed));
                spawnTimer = 0;
            }
            // Проверка столкновений пуль с врагами и с пулями врагов
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bool bulletRemoved = false; // Флаг для отслеживания удаления пули игрока
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    var enemy = enemies[j];
                    // 1. Проверка столкновения пули игрока с пулями врага
                    for (int k = enemy.Bullets.Count - 1; k >= 0; k--)
                    {
                        if (CheckCollision(bullets[i].X, bullets[i].Y, 0.05f, 0.05f,
                                           enemy.Bullets[k].X, enemy.Bullets[k].Y, 0.05f, 0.05f))
                        {
                            // Если столкновение произошло, удаляем пулю игрока и пулю врага
                            bullets.RemoveAt(i);
                            enemy.Bullets.RemoveAt(k);
                            bulletRemoved = true; // Отмечаем, что пуля игрока удалена
                            break;
                        }
                    }
                    // Если пуля игрока была удалена при столкновении с пулей врага, выходим из цикла врагов
                    if (bulletRemoved) break;
                    // 2. Проверка столкновения пули игрока с врагом
                    if (CheckCollision(bullets[i].X, bullets[i].Y, 0.05f, 0.05f, enemy.X, enemy.Y, 0.1f, 0.1f))
                    {
                        enemy.TakeDamage(1); // Наносим урон врагу
                        bullets.RemoveAt(i); // Удаляем пулю игрока
                        bulletRemoved = true; // Отмечаем, что пуля игрока удалена
                        // Если здоровье врага 0 или меньше, удаляем врага
                        if (enemy.Health <= 0)
                        {
                            enemies.RemoveAt(j);
                            enemiesRemovedCount++;
                        }
                        break;
                    }
                }
            }
        }
        private bool CheckCollision(float x1, float y1, float width1, float height1, float x2, float y2, float width2, float height2)
        {
            return x1 + width1 / 2 > x2 - width2 / 2 &&
                   x1 - width1 / 2 < x2 + width2 / 2 &&
                   y1 + height1 / 2 > y2 - height2 / 2 &&
                   y1 - height1 / 2 < y2 + height2 / 2;
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawObject(playerTexture, playerX, playerY, 0.1f, 0.1f); // Рисуем игрока
            
            foreach (var bullet in bullets)// Рисуем пули и врагов
                DrawObject(bullet.Texture, bullet.X, bullet.Y, 0.05f, 0.05f);
            foreach (var enemy in enemies)
                DrawObject(enemy.Texture, enemy.X, enemy.Y, 0.1f, 0.1f);
            foreach (var enemy in enemies) // Рисуем пули врагов
                foreach (var bullet in enemy.Bullets)
                    DrawObject(bullet.Texture, bullet.X, bullet.Y, 0.05f, 0.05f);
            
            if (enemiesPassed >= 110)
            {
                Console.WriteLine("Проиграли");
                Exit();
            }
            SwapBuffers();
        }
        int LoadTexture(string filePath)//переделать надо
        {
            if(!System.IO.File.Exists(filePath))
                throw new Exception($"Файл не найден: {filePath}");

            Bitmap bitmap = new Bitmap(filePath);
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                          PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            return textureId;
        }
        void DrawObject(int texture, float x, float y, float width, float height)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Begin(PrimitiveType.Quads);//рисуем квадратики
            //GL.Color4(1.0f, 1.0f, 1.0f, 1.0f); // Установка цвета с альфа-каналом (где 1.0f — непрозрачный, 0.0f — полностью прозрачный)

            GL.TexCoord2(0, 0); GL.Vertex2(x - width / 2, y - height / 2);//GL.TexCoord2 устанавливает координату текстурки
            GL.TexCoord2(1, 0); GL.Vertex2(x + width / 2, y - height / 2);
            GL.TexCoord2(1, 1); GL.Vertex2(x + width / 2, y + height / 2);
            GL.TexCoord2(0, 1); GL.Vertex2(x - width / 2, y + height / 2);
            // Сбрасываем цвет, чтобы он не применялся к другим объектам
            //GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);

            GL.End();
        }
        private void PlayShootSound()
        {
            shootSound.Position = 0; // Сбрасываем позицию звука на начало
            shootSoundPlayer.Play();
        }
    }
}
