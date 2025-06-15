using System;
using System.Collections.Generic;

namespace ChatPlexMod_MenuMusic.Data
{
    /// <summary>
    /// Music provider interface
    /// </summary>
    public abstract class IMusicProvider
    {
        public abstract MusicProviderType.E Type                { get; }
        public abstract bool                IsReady             { get; }
        public abstract bool                SupportPlayIt       { get; }
        public abstract bool                SupportAddToQueue   { get; }
        public abstract List<Music>         Musics              { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Per game implementation of the Play It button
        /// </summary>
        /// <param name="music">Target music</param>
        /// <param name="viewController">Source view controller</param>
        public abstract void StartGameSpecificGamePlay(Music music, CP_SDK.UI.IViewController viewController);
        /// <summary>
        /// Per game implementation of the Add to queue button
        /// </summary>
        /// <param name="p_Music">Target music</param>
        /// <param name="viewController">Source view controller</param>
        public abstract void AddToQueue(Music p_Music, CP_SDK.UI.IViewController viewController);
        /// <summary>
        /// Shuffle music collection
        /// </summary>
        public abstract void Shuffle();
    }
}
