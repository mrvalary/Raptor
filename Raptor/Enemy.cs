using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raptor
{
    // Класс для врагов
    public class Enemy
    {
        public float X, Y;
        public int Texture;
        public Enemy(float x, float y, int texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }
    }
}
