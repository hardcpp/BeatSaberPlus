namespace BeatSaberPlus_ChatRequest.Models
{
    /// <summary>
    /// Add to queue result
    /// </summary>
    public class AddToQueueResult
    {
        public EAddToQueueResult    Result;
        public string               BSRKeyOrHash;
        public SongEntry            FoundOrCreatedSongEntry;
        public RequesterRateLimit   RateLimit;
        public string               FilterError;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result code</param>
        /// <param name="bsrKey">Transformed BSR key or level hash</param>
        public AddToQueueResult(EAddToQueueResult result, string bsrKey)
        {
            Result          = result;
            BSRKeyOrHash    = bsrKey;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result code</param>
        /// <param name="bsrKeyOrHash">Transformed BSR key or level hash</param>
        /// <param name="foundOrCreatedSongEntry">Found or created song entry</param>
        public AddToQueueResult(EAddToQueueResult result, string bsrKeyOrHash, SongEntry foundOrCreatedSongEntry)
        {
            Result                  = result;
            BSRKeyOrHash            = bsrKeyOrHash;
            FoundOrCreatedSongEntry = foundOrCreatedSongEntry;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result code</param>
        /// <param name="bsrKeyOrHash">Transformed BSR key or level hash</param>
        /// <param name="foundOrCreatedSongEntry">Found or created song entry</param>
        /// <param name="filterError">Error from the filter</param>
        public AddToQueueResult(EAddToQueueResult result, string bsrKeyOrHash, SongEntry foundOrCreatedSongEntry, string filterError)
        {
            Result                  = result;
            BSRKeyOrHash            = bsrKeyOrHash;
            FoundOrCreatedSongEntry = foundOrCreatedSongEntry;
            FilterError             = filterError;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="result">Result code</param>
        /// <param name="bsrKeyOrHash">Transformed BSR key or level hash</param>
        /// <param name="rateLimit">Requester rate limit</param>
        public AddToQueueResult(EAddToQueueResult result, string bsrKeyOrHash, RequesterRateLimit rateLimit)
        {
            Result          = result;
            BSRKeyOrHash    = bsrKeyOrHash;
            RateLimit       = rateLimit;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get readable error result
        /// </summary>
        /// <returns></returns>
        public string GetFormattedError()
        {
            switch (Result)
            {
                case EAddToQueueResult.OK:
                    return string.Empty;
                case EAddToQueueResult.QueueClosed:
                    return "Queue is closed";
                case EAddToQueueResult.MapBanned:
                    return "Map is banned";
                case EAddToQueueResult.RequesterBanned:
                    return "You are not allowed to request maps!";
                case EAddToQueueResult.MapperBanned:
                    return "Mapper is banned";
                case EAddToQueueResult.AlreadyRequestedThisSession:
                    return "Already request this session";
                case EAddToQueueResult.AlreadyInQueue:
                    return "Already in the request queue";
                case EAddToQueueResult.RequestLimit:
                    return "You have reached your request limit!";
                case EAddToQueueResult.NotFound:
                    return "Map not found!";
                case EAddToQueueResult.FilterError:
                    return FilterError;
            }

            return "<unk>";
        }
    }
}
