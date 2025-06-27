using System.ComponentModel;

namespace LVCMod
{
    enum MessageTypes
    {
        [Description("PlayerJoined")]
        PlayerJoined,

        [Description("PlayerWarped")]
        PlayerWarped,

        [Description("ChangePlayerMicrophoneState")]
        ChangePlayerMicrophoneState,

        [Description("ChangePlayerDeaferState")]
        ChangePlayerDeaferState
    }
}