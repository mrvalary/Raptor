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
        public float Speed { get; set; }
        public float Health { get; set; }

        public Enemy(float x, float y, int texture, float health, float speed)
        {
            X = x;
            Y = y;
            Texture = texture;
            Health = health;
            Speed = speed;
        }
        public void TakeDamage(float damage)
        {
            Health -= damage;
        }
    }
}
