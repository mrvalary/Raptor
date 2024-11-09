using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace Raptor
{
    public class Game : GameWindow
    {
        int playerTexture, bulletTexture, enemyTexture;
        float playerX = 0, playerY = -0.8f;
        List<Bullet> bullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();
        Random random = new Random();
        float spawnTimer = 0;

        public Game(int w, int h, string title) :
            base(w, h, GraphicsMode.Default, title, GameWindowFlags.FixedWindow)
        { }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.Black);

            // Загрузка текстур
            playerTexture = LoadTexture("player.png");
            bulletTexture = LoadTexture("bullet.png");
            enemyTexture = LoadTexture("enemy.png");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var keyboard = Keyboard.GetState();

            // Управление игроком
            if (keyboard.IsKeyDown(Key.Left) && playerX > -0.95f) playerX -= 0.05f;
            if (keyboard.IsKeyDown(Key.Right) && playerX < 0.95f) playerX += 0.05f;
            if (keyboard.IsKeyDown(Key.Up) &&  playerY < 0.95f) playerY += 0.05f;
            if (keyboard.IsKeyDown(Key.Down) && playerY > -0.95f) playerY -= 0.05f;
            // Стрельба
            if (keyboard.IsKeyDown(Key.Space))
            {
                bullets.Add(new Bullet(playerX, playerY + 0.1f, bulletTexture));
            }

            // Обновление позиций пуль
            foreach (var bullet in bullets)
                bullet.Y += 0.05f;

            // Обновление позиций врагов
            foreach (var enemy in enemies)
                enemy.Y -= 0.02f;

            // Удаление внеэкранных объектов
            bullets.RemoveAll(b => b.Y > 1.0f);
            enemies.RemoveAll(c => c.Y < -1.0f);

            // Спавн врагов
            spawnTimer += (float)e.Time;
            if (spawnTimer >= 1.0f)
            {
                float x = (float)(random.NextDouble() * 1.8 - 0.9); // Генерация позиции врага по X
                enemies.Add(new Enemy(x, 1.0f, enemyTexture));
                spawnTimer = 0;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawObject(playerTexture, playerX, playerY, 0.1f, 0.1f); // Рисуем игрока

            // Рисуем пули и врагов
            foreach (var bullet in bullets)
                DrawObject(bullet.Texture, bullet.X, bullet.Y, 0.05f, 0.05f);

            foreach (var enemy in enemies)
                DrawObject(enemy.Texture, enemy.X, enemy.Y, 0.1f, 0.1f);

            SwapBuffers();
        }

        int LoadTexture(string filePath)
        {
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

            return textureId;
        }

        void DrawObject(int texture, float x, float y, float width, float height)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0); GL.Vertex2(x - width / 2, y - height / 2);
            GL.TexCoord2(1, 0); GL.Vertex2(x + width / 2, y - height / 2);
            GL.TexCoord2(1, 1); GL.Vertex2(x + width / 2, y + height / 2);
            GL.TexCoord2(0, 1); GL.Vertex2(x - width / 2, y + height / 2);

            GL.End();
        }

    }
}
