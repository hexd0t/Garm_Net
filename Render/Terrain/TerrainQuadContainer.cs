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
            int quadsX = endX - startX; //+1, but since every following calculation would do -1 left away
            int quadsZ = endZ - startZ;
            bool singleX = quadsX == 1;
            bool singleZ = quadsZ == 1;
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
                                       new QuadContainer(startX, startX + quadsX/2, startZ, endZ, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsX/2, endX, startZ, endZ, ref quads, quadCountZ)
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
                                       new QuadContainer(startX, endX, startZ, startZ + quadsZ/2, ref quads, quadCountZ),
                                       new QuadContainer(startX, endX, startZ + quadsZ/2, endZ, ref quads, quadCountZ)
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
                                       new QuadContainer(startX, startX + quadsX/2, startZ, startZ + quadsZ/2, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsX/2, endX, startZ, startZ + quadsZ/2, ref quads, quadCountZ),
                                       new QuadContainer(startX, startX + quadsX/2, startZ + quadsZ/2, endZ, ref quads, quadCountZ),
                                       new QuadContainer(startX + quadsX/2, endX, startZ + quadsZ/2, endZ, ref quads, quadCountZ)
                                   };
                Bounds = GetContainingBounds(SubContainer.Select(quad => quad.Bounds));
            }
            BoundingBox = CreateBoundingBox(Bounds);
        }

        protected Quad.QuadBounds GetContainingBounds(IEnumerable<Quad.QuadBounds> contained)
        {
            Quad.QuadBounds bounds = contained.FirstOrDefault();
            foreach (var containedBounds in contained.Skip(1))
            {
                if (containedBounds.MaxX > Bounds.MaxX)
                    Bounds.MaxX = containedBounds.MaxX;
                if (containedBounds.MaxY > Bounds.MaxY)
                    Bounds.MaxY = containedBounds.MaxY;
                if (containedBounds.MaxZ > Bounds.MaxZ)
                    Bounds.MaxZ = containedBounds.MaxZ;
                if (containedBounds.MinX < Bounds.MinX)
                    Bounds.MinX = containedBounds.MinX;
                if (containedBounds.MinY < Bounds.MinY)
                    Bounds.MinY = containedBounds.MinY;
                if (containedBounds.MinZ < Bounds.MinZ)
                    Bounds.MinZ = containedBounds.MinZ;
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
