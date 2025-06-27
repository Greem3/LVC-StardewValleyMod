using System;

namespace LVCMod
{
    class BotConfig
    {
        public string Token { get; set; } = "";

        public string MainVoiceChatName { get; set; } = "Talk";

        public string VoiceChatsCategoryName { get; set; } = "Stardew Valley LVC";

        public bool DeleteVoiceChats { get; set; } = true;
    }
}
