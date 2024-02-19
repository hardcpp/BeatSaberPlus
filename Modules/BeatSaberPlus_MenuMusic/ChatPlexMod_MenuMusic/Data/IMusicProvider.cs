using System.Collections.Generic;

namespace ChatPlexMod_MenuMusic.Data
{
    /// <summary>
    /// Music provider interface
    /// </summary>
    public abstract class IMusicProvider
    {
        public abstract MusicProviderType.E Type            { get; }
        public abstract bool                IsReady         { get; }
        public abstract bool                SupportPlayIt   { get; }
        public abstract List<Music>         Musics          { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Per game implementation of the Play It button
        /// </summary>
        /// <param name="p_Music">Target music</param>
        public abstract bool StartGameSpecificGamePlay(Music p_Music);
        /// <summary>
        /// Shuffle music collection
        /// </summary>
        public abstract void Shuffle();
    }
}
