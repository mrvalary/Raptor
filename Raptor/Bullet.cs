using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raptor
{
    public class Bullet
    {
        public float X, Y;
        public int Texture;
        public Bullet(float x, float y, int texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }
    }

    
}
