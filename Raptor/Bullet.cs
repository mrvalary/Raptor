namespace Raptor
{
    public class Bullet
    {
        public float X, Y;
        public float Speed { get; set; }
        public int Texture;
        public Bullet(float x, float y, int texture, float speed)
        {
            X = x;
            Y = y;
            Speed = speed;
            Texture = texture;
        }
    }
}
