using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Garm.Base.Helper;
using Garm.Base.Interfaces;

namespace Garm.Base.Content.Terrain
{
    public class Terrain : Base.Abstract.Base
    {
        public float[] Height { get; private set; }
        public int PointsX;
        public int PointsZ;
        public float PointsPerMeter;

        /// <summary>
        /// Creates a flat terrain using the given width and depth
        /// </summary>
        /// <param name="width">Width of the created terrain in meters</param>
        /// <param name="depth">Depth of the created terrain in meters</param>
        /// <param name="pointsPerMeter">Density of terrain controlpoints</param>
        /// <param name="manager">The RunManager for this instance</param>
        public Terrain(float width, float depth, float pointsPerMeter, IRunManager manager) : base (manager)
        {
            PointsPerMeter = pointsPerMeter;
            PointsX = (int)Math.Ceiling(width*PointsPerMeter);
            PointsZ = (int)Math.Ceiling(depth*PointsPerMeter);
            Height = new float[PointsX*PointsZ];
        }
        /// <summary>
        /// Creates a flat terrain using the given width and depth and the default density
        /// </summary>
        /// <param name="width">Width of the created terrain in meters</param>
        /// <param name="depth">Depth of the created terrain in meters</param>
        /// <param name="manager">The RunManager for this instance</param>
        public Terrain(float width, float depth, IRunManager manager) : this(width, depth, manager.Opts.Get<float>("terrain_defaultPointsPerMeter"), manager)
        { }

        public unsafe Terrain(Bitmap heightmap, float minHeight, float maxHeight, float pointsPerMeter, IRunManager manager) : base (manager)
        {
            PointsPerMeter = pointsPerMeter;
            PointsX = heightmap.Size.Width;
            PointsZ = heightmap.Size.Height;
            var bitmapData = heightmap.LockBits(new Rectangle(0, 0, PointsX, PointsZ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Height = new float[PointsX * PointsZ];
            for (int i = 0; i < PointsZ; i++)
            {
                uint* row = (uint*)(bitmapData.Scan0+(i*bitmapData.Stride));
                for (int j = 0; j < PointsX; j++)
                {
                    Height[j * PointsZ + i] = minHeight + ((float)((double)(maxHeight - minHeight) * (double)row[j] / (double)uint.MaxValue));
                }
            }
            heightmap.UnlockBits(bitmapData);
        }

        public Terrain(Heightmap heightmap, IRunManager manager) : this(heightmap.image, heightmap.minHeight, heightmap.maxHeight, heightmap.pointsPerMeter, manager)
        {}

        public unsafe Terrain(Stream terrainDefFile, IRunManager manager) : base(manager)
        {
            var heightmap = new Heightmap();
            using (var reader = XmlReader.Create(terrainDefFile))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                        continue;
                    switch (reader.Name)
                    {
                        case "heightmap":
                            var file = reader["file"];
                            if (String.IsNullOrWhiteSpace(file))
                            {
#if DEBUG
                                Console.WriteLine("[Warning] No heightmapfile specified!");
#endif
                                continue;
                            }
                            var filestream = Manager.Files.Get(file, false);
                            heightmap.image = new Bitmap(Image.FromStream(filestream));
                            filestream.Dispose();
                            break;
                        case "mapinfo":
                            while (!(reader.NodeType == XmlNodeType.EndElement && reader.Name == "mapinfo"))
                            {
                                reader.Read();
                                if(!reader.IsStartElement())
                                    continue;
                                switch (reader.Name)
                                {
                                    case "minheight":
                                        reader.Read();
                                        heightmap.minHeight = reader.ReadContentAsFloat();
                                        break;
                                    case "maxheight":
                                        reader.Read();
                                        heightmap.maxHeight = reader.ReadContentAsFloat();
                                        break;
                                    case "pointspermeter":
                                        reader.Read();
                                        heightmap.pointsPerMeter = reader.ReadContentAsFloat();
                                        break;
                                }
                            }
                            break;
                        case "renderinfo":
                            
                            break;
                    }
                }
            }
            PointsX = heightmap.image.Size.Width;
            PointsZ = heightmap.image.Size.Height;
            var bitmapData = heightmap.image.LockBits(new Rectangle(0, 0, PointsX, PointsZ), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Height = new float[PointsX * PointsZ];
            for (int i = 0; i < PointsZ; i++)
            {
                uint* row = (uint*)(bitmapData.Scan0 + (i * bitmapData.Stride));
                for (int j = 0; j < PointsX; j++)
                {
                    Height[j * PointsZ + i] = heightmap.minHeight + ((float)((double)(heightmap.maxHeight - heightmap.minHeight) * (double)row[j] / (double)uint.MaxValue));
                }
            }
            heightmap.image.UnlockBits(bitmapData);
        }

        public unsafe Bitmap ToHeightmap(out float minHeight, out float maxHeight)
        {
            int width = Height.GetLength(0);
            int depth = Height.GetLength(1);
            var heightmap = new Bitmap(width, depth, PixelFormat.Format32bppArgb);
            var bitmapData = heightmap.LockBits(new Rectangle(0, 0, width, depth), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            minHeight = maxHeight = Height[0];
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (Height[j*PointsZ + i] < minHeight)
                        minHeight = Height[j * PointsZ + i];
                    if (Height[j * PointsZ + i] > maxHeight)
                        maxHeight = Height[j * PointsZ + i];
                }
            }

            for (int i = 0; i < depth; i++)
            {
                uint* row = (uint*)(bitmapData.Scan0 + (i * bitmapData.Stride));
                for (int j = 0; j < width; j++)
                {
                    //Height[j, i] = minHeight + (maxHeight - minHeight) * row[j] / uint.MaxValue;
                    var value = Math.Round((double)(Height[j * PointsZ + i] - minHeight) * (double)uint.MaxValue / (double)(maxHeight - minHeight));
                    if (value > UInt32.MaxValue)
                        value = UInt32.MaxValue;
                    if (value < 0)
                        value = 0;
                    row[j] = Convert.ToUInt32(value);
                }
            }
            heightmap.UnlockBits(bitmapData);
            return heightmap;
        }

        public static Terrain GetRandomTerrain(IRunManager manager)
        {
            var ter = new Terrain(500, 500, manager);
            var r = new Random();
            for (int i = 0; i < ter.Height.Length; i++)
            {
                ter.Height[i] = (float)r.NextDouble();
            }
            return ter;
        }

        public Heightmap ToHeightmap()
        {
            float min, max;
            var img = ToHeightmap(out min, out max);
            return new Heightmap(){image = img, minHeight = min, maxHeight = max, pointsPerMeter = PointsPerMeter};
        }

        /// <summary>
        /// Gets the height at the specified coordinates, interpolates if neccesary
        /// </summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="z">Z-coordinate</param>
        /// <returns>The height above NN</returns>
        public float GetHeightAt(float x, float z)
        {
            float x1z1 = Height[(int)Math.Floor(x) * PointsZ + (int)Math.Floor(z)];
            float x1z2 = Height[(int)Math.Floor(x) * PointsZ + (int)Math.Ceiling(z)];
            float x2z1 = Height[(int)Math.Ceiling(x) * PointsZ + (int)Math.Floor(z)];
            float x2z2 = Height[(int)Math.Ceiling(x) * PointsZ + (int)Math.Ceiling(z)];
            return (x1z1 + x1z2 + x2z1 + x2z2) / 4;
        }

        public struct Heightmap
        {
            public Bitmap image;
            public float minHeight, maxHeight, pointsPerMeter;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
