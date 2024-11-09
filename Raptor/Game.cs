using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using NAudio.Wave;



namespace Raptor
{
    public class Game : GameWindow
    {
        private const int enemySize = 35;
        private int enemiesPassed = 0;  // Счётчик вылетевших врагов
        private float shootCooldown = 0f; // Перезарядка после выстрела
        private const float cooldownTime = 0.15f; // Время между выстрелами, в секундах
        int playerTexture, bulletTexture, enemyTexture;
        float playerX = 0, playerY = -0.8f;
        private List<Bullet> bullets = new List<Bullet>();
        private List<Enemy> enemies = new List<Enemy>();
        Random random = new Random();
        float spawnTimer = 0;
        

        public Game(int w, int h, string title) :
            base(w, h, GraphicsMode.Default, title, GameWindowFlags.FixedWindow)
        { }
        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.Blend);

            base.OnLoad(e);
            GL.ClearColor(Color.Black);
            // Загрузка текстур
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
            GL.DeleteTexture(enemyTexture);;
            base.OnUnload(e);
        }
        
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            var keyboard = Keyboard.GetState();
            shootCooldown -= (float)e.Time;
            if (shootCooldown < 0) shootCooldown = 0;

            // Управление игроком
            if (keyboard.IsKeyDown(Key.Left) && playerX > -0.95f) playerX -= 0.03f;
            if (keyboard.IsKeyDown(Key.Right) && playerX < 0.95f) playerX += 0.03f;
            if (keyboard.IsKeyDown(Key.Up) &&  playerY < 0.95f) playerY += 0.03f;
            if (keyboard.IsKeyDown(Key.Down) && playerY > -0.95f) playerY -= 0.04f;
            if (keyboard.IsKeyDown(Key.Escape))Exit();
            // Стрельба
            if (keyboard.IsKeyDown(Key.Space) && shootCooldown <= 0)
            {
                bullets.Add(new Bullet(playerX, playerY + 0.1f, bulletTexture));
                shootCooldown = cooldownTime; // Устанавливаем время перезарядки после выстрела
            }
            foreach (var bullet in bullets)// Обновление позиций пуль
                bullet.Y += 0.05f;
            foreach (var enemy in enemies) // Обновление врагов
            {
                enemy.Y -= enemy.Speed; // Движение врага
                enemy.ShootCooldown -= (float)e.Time; // Обновление времени перезарядки у врагов

                // Стрельба врагов
                if (enemy.ShootCooldown <= 0)
                {
                    enemy.Bullets.Add(new EnemyBullet(enemy.X, enemy.Y - 0.1f, bulletTexture));
                    enemy.ShootCooldown = 2.0f; // Устанавливаем время перезарядки для следующего выстрела
                }

                // Обновление пуль врагов
                foreach (var bullet in enemy.Bullets)
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
                int health = random.Next(2, 5);
                float speed;
                if (health == 2)
                {
                    speed = 0.009f;
                }
                else 
                {
                    if (health == 3)
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
            // Проверка столкновений пуль с врагами
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (CheckCollision(bullets[i].X, bullets[i].Y, 0.05f, 0.05f, enemies[j].X, enemies[j].Y, 0.1f, 0.1f))
                    {
                        enemies[j].TakeDamage(1); // Наносим урон врагу
                        bullets.RemoveAt(i); // Удаляем пулю
                        if (enemies[j].Health <= 0) // Если здоровье врага 0 или меньше, удаляем его
                        {
                            enemies.RemoveAt(j);
                        }
                        break; // Прекращаем проверку текущей пули
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
            Console.WriteLine($"Enemies passed: {enemiesPassed}");
            if (enemiesPassed >= 20)
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
    }
}
