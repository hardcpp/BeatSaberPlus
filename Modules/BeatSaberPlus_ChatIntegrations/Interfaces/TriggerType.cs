namespace BeatSaberPlus_ChatIntegrations.Interfaces
{
    /// <summary>
    /// Trigger type
    /// </summary>
    public enum TriggerType : uint
    {
        None,
        Dummy,
        ChatBits,
        ChatMessage,
        ChatFollow,
        ChatPointsReward,
        ChatSubscription,
        LevelStarted,
        LevelPaused,
        LevelResumed,
        LevelEnded,
        VoiceAttackCommand,

        _CUSTOM_START = 1000
    }
}
