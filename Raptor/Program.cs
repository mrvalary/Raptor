using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Raptor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(600, 700, "Raptor");
            game.Run(60);
            game.VSync = VSyncMode.On;
            
            while (true)
            {
                Thread.Sleep(5000);
                Console.WriteLine(game.enemiesRemovedCount);
                Console.WriteLine($"Enemies passed: {game.enemiesPassed}");
                if (game.enemiesRemovedCount == 20)
                {
                    game.cooldownTime = 0.15f;
                    game.bulletSpeed = 0.06f;
                    break;
                }
            }
        }
    }
}
