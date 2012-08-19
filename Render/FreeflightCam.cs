using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Garm.View.Human.Render
{
    public class FreeflightCam
    {
        public FreeflightCam()
        {
            LookAt = new Vector3(0,0,0);
            Height = 0.75f;
            Distance = 2f;
            Rotation = -0.785398163397448f;
        }
        public Vector3 LookAt;
        public Vector3 Position { get { return new Vector3(LookAt.X - (float)Math.Cos(Rotation) * Distance, LookAt.Y + Height, LookAt.Z + (float)Math.Sin(Rotation)*Distance); } }
        public float Height;
        public float Distance;
        public float Rotation;
    }
}
