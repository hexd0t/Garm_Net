using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Base.Enums
{
    /// <summary>
    /// The different RunModes
    /// </summary>
    [Flags]
    public enum RunMode
    {
        Play            = 0x01,
        Edit            = 0x02,
        GameServer      = 0x04,
        CollabServer    = 0x08,
        /* === Combinations below === */
        //Invalid       0x03 (PlayEdit)
        ServerPlay      = 0x05,
        //Invalid       0x06 (EditGameServer)
        //Invalid       0x07 (PlayEditGameserver)
        //Invalid       0x09 (PlayCollabServer)
        //Invalid       0x0A (EditCollabServer)
        //Invalid       0x0B (PlayEditCollabServer)
        AIOServer       = 0x0C,
        //Invalid       0x0D (PlayAIOServer)
        //Invalid       0x0E (EditAIOServer)
        //Invalid       0x0F(Play
    }
}
