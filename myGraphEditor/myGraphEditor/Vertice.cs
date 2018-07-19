using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace myGraphEditor
{
    public class Vertice
    {
        //public static int counter = 0;
        //public int number;
        public int X;
        public int Y;
        public Color color=Color.Black;
        public const int radius = 25;

        public Vertice(int _x, int _y, Color _color):this(_x,_y)
        {
            color = _color;
        }

        public Vertice(int _x,int _y)
        {
            X = _x;
            Y = _y;
            //number = ++counter;
        }
    }
}
