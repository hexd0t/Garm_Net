using System;

namespace Garm.Base.Enums
{
    /// <summary>
    /// The different RunModes
    /// </summary>
    [Flags]
    public enum RunMode
    {
        /// <summary>
        /// Start a normal SP/MP game
        /// </summary>
        Play            = 0x01,
        /// <summary>
        /// Starts the game editor for creating/importing ingame content, either locally or on a collabserver
        /// </summary>
        Edit            = 0x02,
        /// <summary>
        /// Starts a dedicated MP gameserver
        /// </summary>
        GameServer      = 0x04,
        /// <summary>
        /// Starts a dedicated collabserver for collaboratively creating ingame content
        /// </summary>
        CollabServer    = 0x08,
        /* === Combinations below === */
        //Invalid       0x03 (PlayEdit)
        /// <summary>
        /// Starts both a MP game and a MP gameserver and connects the game to the local gameserver
        /// </summary>
        ServerPlay      = 0x05,
        //Invalid       0x06 (EditGameServer)
        //Invalid       0x07 (PlayEditGameserver)
        //Invalid       0x09 (PlayCollabServer)
        //Invalid       0x0A (EditCollabServer)
        //Invalid       0x0B (PlayEditCollabServer)
        /// <summary>
        /// Starts both a game- and a collabserver
        /// </summary>
        AIOServer       = 0x0C,
        //Invalid       0x0D (PlayAIOServer)
        //Invalid       0x0E (EditAIOServer)
        //Invalid       0x0F(Play
    }
}
