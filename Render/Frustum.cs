using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Garm.View.Human.Render
{
    public class Frustum
    {
        private Plane[] _planes;
        private Matrix _lastFrustum;

        public Frustum()
        {
            _planes = new Plane[6];
        }

        /// <summary>
        /// Updates the frustum-planes
        /// </summary>
        public void Update(Matrix viewMatrix, Matrix projectionMatrix)
        {
            var frustum = viewMatrix * projectionMatrix;
            if (frustum == _lastFrustum)
                return;
            _lastFrustum = frustum;
            //left
            _planes[0] = new Plane(
                frustum.M14 + frustum.M11,
                frustum.M24 + frustum.M21,
                frustum.M34 + frustum.M31,
                frustum.M44 + frustum.M41);
            //right
            _planes[1] = new Plane(
                frustum.M14 - frustum.M11,
                frustum.M24 - frustum.M21,
                frustum.M34 - frustum.M31,
                frustum.M44 - frustum.M41);
            //top
            _planes[2] = new Plane(
                frustum.M14 - frustum.M12,
                frustum.M24 - frustum.M22,
                frustum.M34 - frustum.M32,
                frustum.M44 - frustum.M42);
            //bottom
            _planes[3] = new Plane(
                frustum.M14 + frustum.M12,
                frustum.M24 + frustum.M22,
                frustum.M34 + frustum.M32,
                frustum.M44 + frustum.M42);
            //near
            _planes[4] = new Plane(
                frustum.M13,
                frustum.M23,
                frustum.M33,
                frustum.M43);
            //far
            _planes[5] = new Plane(
                frustum.M14 - frustum.M13,
                frustum.M24 - frustum.M23,
                frustum.M34 - frustum.M33,
                frustum.M44 - frustum.M43);
        }

        /// <summary>
        /// Checks if the Point is within the frustum
        /// </summary>
        /// <param name="point">Point to be tested</param>
        /// <returns></returns>
        public bool Check(Vector3 point)
        {
            return _planes.All(plane => Vector3.Dot(plane.Normal, point) + plane.D >= 0);
        }

        /// <summary>
        /// Checks if the Sphere defined by its center and its radius is within the frustum
        /// </summary>
        /// <param name="point">Sphere's center</param>
        /// <param name="radius">Sphere's radius</param>
        /// <returns></returns>
        public bool Check(Vector3 point, float radius)
        {
            return _planes.All(plane => Vector3.Dot(plane.Normal, point) + plane.D + radius >= 0);
        }

        /// <summary>
        /// Checks if the Volume defined by the given Points is in the Frustum
        /// The function also returns true in some cases where a side could possibly intersect, but does not
        /// </summary>
        /// <param name="points">The edges defining the Volume</param>
        /// <returns></returns>
        public bool Check(Vector3[] points)
        {
            return _planes.All(plane => points.Any(point => Vector3.Dot(plane.Normal, point) + plane.D >= 0f));
        }
    }
}
