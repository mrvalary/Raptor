using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raptor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game(600, 700, "Raptor");
            game.Run(60);
            game.VSync = VSyncMode.On;
        }
    }
}
