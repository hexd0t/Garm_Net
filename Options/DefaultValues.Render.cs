using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Options
{
    internal static partial class DefaultValues
    {
        private static void LoadDefaultsRender(ref Dictionary<string, IEntry> dict)
        {
            dict.Add("rndr_nearPlane", new Entry<float>(0.5f));
            dict.Add("rndr_farPlane", new Entry<float>(150f));
            dict.Add("rndr_rawGBufferView", new Entry<bool>(false, true));
            dict.Add("rndr_wireframe", new Entry<bool>(false, true));
            dict.Add("rndr_cull", new Entry<bool>(true, true));
            dict.Add("rndr_terrain_quadVerticesPerEdge", new Entry<int>(50, false, false, true));
            dict.Add("rndr_terrain_frustumCheck", new Entry<bool>(true));
            dict.Add("rndr_terrain_renderEdges", new Entry<bool>(true));
            dict.Add("rndr_incorporeal", new Entry<bool>(false, true));
            dict.Add("rndr_terrain_lods", new Entry<int>(3));//Min:1 (=>complete)
            dict.Add("rndr_terrain_lodDistances", new Entry<List<float>>(new List<float> { 20f, 35f }));
            dict.Add("rndr_terrain_lodFactors", new Entry<List<int>>(new List<int> { 2, 8 }));
        }
    }
}
