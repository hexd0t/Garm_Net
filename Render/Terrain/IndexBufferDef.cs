using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garm.View.Human.Render.Terrain
{
    public struct IndexBufferDef
    {
        private uint _id;
        public uint Id { get { return _id; } }
        public int Highword
        {
            get { return (int)((_id & 0xFFF00000) >> 20); }
            set { _id = (_id & 0x000FFFFF) | (((uint)value) << 20); }
        }
        public int Lod
        {
            get { return (int)((_id & 0xFFF00) >> 8); }
            set { _id = (_id & 0xFFF000FF) | (((uint)value) << 8); }
        }
        public int Border
        {
            get { return (int)(_id & 0xFF); }
            set { _id = (_id & 0xFFFFFF00) | ((uint)value); }
        }
    }
}
