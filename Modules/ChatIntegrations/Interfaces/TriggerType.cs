namespace BeatSaberPlus.Modules.ChatIntegrations.Interfaces
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
        LevelEnded,
        LevelStarted,
        VoiceAttackCommand,

        _CUSTOM_START = 1000
    }
}
