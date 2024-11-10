using System.Collections.Generic;
namespace Raptor
{
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Texture { get; set; }
        public float Health { get; set; }
        public float Speed { get; set; }
        public float ShootCooldown { get; set; }
        public List<EnemyBullet> Bullets { get; set; }  // Список пуль врага
        public Enemy(float x, float y, int texture, float health, float speed)
        {
            X = x;
            Y = y;
            Texture = texture;
            Health = health;
            Speed = speed;
            ShootCooldown = 0;
            Bullets = new List<EnemyBullet>();  // Инициализация списка пуль врага
        }
        public void TakeDamage(float damage)
        {
            Health -= damage;
        }
    }
}
