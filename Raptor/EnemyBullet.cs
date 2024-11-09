using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raptor
{
    public class EnemyBullet
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Texture { get; set; }

        public EnemyBullet(float x, float y, int texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }
    }
}
