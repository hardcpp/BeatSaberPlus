namespace BeatSaberPlus.SDK.UI.Data
{
    public interface SongListController
    {
        void OnSongListItemCoverFetched(SongListItem p_Item);
        bool PlayPreviewAudio();
        float PreviewAudioVolume();
    }
}
