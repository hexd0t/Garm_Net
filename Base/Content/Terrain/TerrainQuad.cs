using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Helper;

namespace Garm.Base.Content.Terrain
{
    public class TerrainQuad
    {
        public Point P1;
        public Point P2;
        public Point P3;
        public Point P4;

        public TerrainQuad(Point p1, Point p2, Point p3, Point p4)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
        }
    }
}
