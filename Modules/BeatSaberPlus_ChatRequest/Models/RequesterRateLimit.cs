using CP_SDK.Chat.Interfaces;

namespace BeatSaberPlus_ChatRequest.Models
{
    /// <summary>
    /// Requester rate limit infomartions
    /// </summary>
    public class RequesterRateLimit
    {
        public int      CurrentRequestCount;
        public int      RequestMaxCount;
        public string   RequestLimitType;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestMaxCount">Max count</param>
        /// <param name="requestLimitType">Type</param>
        public RequesterRateLimit(int requestMaxCount, string requestLimitType)
        {
            RequestMaxCount     = requestMaxCount;
            RequestLimitType    = requestLimitType;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Replace the current rate limit
        /// </summary>
        /// <param name="requestMaxCount">New max count</param>
        /// <param name="requestLimitType">New type</param>
        public void SetNewRateLimit(int requestMaxCount, string requestLimitType)
        {
            RequestMaxCount     = requestMaxCount;
            RequestLimitType    = requestLimitType;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update this rate limit from a chat user
        /// </summary>
        /// <param name="requester">Chat user</param>
        public void UpdateRateLimitFromRequester(IChatUser requester)
        {
            if (requester == null)
                return;

            if (requester.IsVip && !requester.IsSubscriber)
            {
                SetNewRateLimit(
                    RequestMaxCount + CRConfig.Instance.VIPBonusRequest,
                    "VIPs"
                );
            }

            if (requester.IsSubscriber && !requester.IsVip)
            {
                SetNewRateLimit(
                    RequestMaxCount + CRConfig.Instance.SubscriberBonusRequest,
                    "Subscribers"
                );
            }

            if (requester.IsSubscriber && requester.IsVip)
            {
                SetNewRateLimit(
                    RequestMaxCount + CRConfig.Instance.VIPBonusRequest + CRConfig.Instance.SubscriberBonusRequest,
                    "VIP Subscribers"
                );
            }

            if (requester.IsModerator || requester.IsBroadcaster)
            {
                SetNewRateLimit(
                    1000,
                    "Moderators"
                );
            }
        }
    }
}
