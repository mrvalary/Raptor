using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Threading.Tasks;

namespace Raptor
{
    public class Texture : IDisposable
    {
        public int GlHandle { get; protected set; }//хранит идентификатор текстуры в OpenGL, сгенерированный функцией GL.GenTexture().
        public int Width { get; protected set; }//ширина 
        public int Height { get; protected set; }//и высота текстуры, взятые из изображения
        public Texture(Bitmap Bitmap)
        {
            GlHandle = GL.GenTexture();
            Bind();
            Width = Bitmap.Width;
            Height = Bitmap.Height;
            var BitmapData = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, BitmapData.Width, BitmapData.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, BitmapData.Scan0);
            Bitmap.UnlockBits(BitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, GlHandle);
        }
        private bool Disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool Disposing)
        {
            if (!Disposed)
            {
                if (Disposing)
                {
                    GL.DeleteTexture(GlHandle);
                }
                Disposed = true;
            }
        }
        Texture()
        {
            Dispose(false);
        }
    }
}
