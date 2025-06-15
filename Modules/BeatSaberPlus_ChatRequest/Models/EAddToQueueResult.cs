namespace BeatSaberPlus_ChatRequest.Models
{
    /// <summary>
    /// Add to queue result codes
    /// </summary>
    public enum EAddToQueueResult
    {
        OK,
        QueueClosed,
        MapBanned,
        RequesterBanned,
        MapperBanned,
        AlreadyRequestedThisSession,
        AlreadyInQueue,
        RequestLimit,
        NotFound,
        FilterError
    }
}
