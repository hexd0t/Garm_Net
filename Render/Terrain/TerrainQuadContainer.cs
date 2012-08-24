using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace Garm.View.Human.Render.Terrain
{
    public class QuadContainer
    {
        protected int ContainedQuad;
        protected List<QuadContainer> SubContainer;
        protected Quad.QuadBounds Bounds; //float[6] {minX, maxX, minY, maxY, minZ, maxZ}
        protected Vector3[] BoundingBox;

        public QuadContainer(int startX, int endX, int startZ, int endZ, ref Quad[] quads, int quadCountZ)
        {
            int quadsX = endX - startX;
            int quadsZ = endZ - startZ;
            bool singleX = quadsX == 0;
            bool singleZ = quadsZ == 0;
            int quadsXhalf = quadsX / 2;
            int quadsZhalf = quadsZ / 2;
            if(singleX && singleZ)
            {
                ContainedQuad = startX * quadCountZ + startZ;
                Bounds = quads[ContainedQuad].Bounds;
            }
            else if (singleZ || quadsX / 2 >= quadsZ)
            {
                // -------------
                // |     |     |
                // -------------
                SubContainer = new List<QuadContainer>(2)
                                   {
                                       new QuadContainer(startX, startX + quadsXhalf, startZ, endZ, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsXhalf + 1, endX, startZ, endZ, ref quads, quadCountZ)
                                   };
                Bounds = GetContainingBounds(SubContainer.Select(quad => quad.Bounds));
                
            }
            else if (singleX || quadsZ / 2 >= quadsX)
            {
                // ----
                // |  |
                // ----
                // |  |
                // ----

                SubContainer = new List<QuadContainer>(2)
                                   {
                                       new QuadContainer(startX, endX, startZ, startZ + quadsZhalf, ref quads, quadCountZ),
                                       new QuadContainer(startX, endX, startZ + quadsZhalf + 1, endZ, ref quads, quadCountZ)
                                   };
                Bounds = GetContainingBounds(SubContainer.Select(quad => quad.Bounds));
            }
            else
            {
                // ---------
                // | 1 | 2 |
                // ---------
                // | 3 | 4 |
                // ---------
                SubContainer = new List<QuadContainer>(4)
                                   {
                                       new QuadContainer(startX, startX + quadsXhalf, startZ, startZ + quadsZhalf, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsXhalf + 1, endX, startZ, startZ + quadsZhalf, ref quads, quadCountZ),
                                       new QuadContainer(startX, startX + quadsXhalf, startZ + quadsZhalf + 1, endZ, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsXhalf + 1, endX, startZ + quadsZhalf + 1, endZ, ref quads, quadCountZ)
                                   };
                Bounds = GetContainingBounds(SubContainer.Select(quad => quad.Bounds));
            }
            BoundingBox = CreateBoundingBox(Bounds);
        }

        protected static Quad.QuadBounds GetContainingBounds(IEnumerable<Quad.QuadBounds> contained)
        {
            Quad.QuadBounds bounds = contained.FirstOrDefault();
            foreach (var containedBounds in contained)
            {
                if (containedBounds.MaxX > bounds.MaxX)
                    bounds.MaxX = containedBounds.MaxX;
                if (containedBounds.MaxY > bounds.MaxY)
                    bounds.MaxY = containedBounds.MaxY;
                if (containedBounds.MaxZ > bounds.MaxZ)
                    bounds.MaxZ = containedBounds.MaxZ;
                if (containedBounds.MinX < bounds.MinX)
                    bounds.MinX = containedBounds.MinX;
                if (containedBounds.MinY < bounds.MinY)
                    bounds.MinY = containedBounds.MinY;
                if (containedBounds.MinZ < bounds.MinZ)
                    bounds.MinZ = containedBounds.MinZ;
            }
            return bounds;
        }

        protected Vector3[] CreateBoundingBox(Quad.QuadBounds bounds)
        {
            return new[]
                       {
                           new Vector3(bounds.MinX, bounds.MinY, bounds.MinZ),
                           new Vector3(bounds.MinX, bounds.MinY, bounds.MaxZ),
                           new Vector3(bounds.MinX, bounds.MaxY, bounds.MinZ),
                           new Vector3(bounds.MinX, bounds.MaxY, bounds.MaxZ),
                           new Vector3(bounds.MaxX, bounds.MinY, bounds.MinZ),
                           new Vector3(bounds.MaxX, bounds.MinY, bounds.MaxZ),
                           new Vector3(bounds.MaxX, bounds.MaxY, bounds.MinZ),
                           new Vector3(bounds.MaxX, bounds.MaxY, bounds.MaxZ)
                       };
        }
        
        public IEnumerable<int> GetQuadsInFrustum(Frustum frustum, bool treestart = false)
        {
            if (treestart || frustum.Check(BoundingBox))
            {
                if (SubContainer == null)
                    return new List<int> { ContainedQuad };
                return SubContainer.SelectMany(container => container.GetQuadsInFrustum(frustum)).Where(quadId => quadId != -1);
            }
            return new List<int> { -1 };
        }
    }
}
