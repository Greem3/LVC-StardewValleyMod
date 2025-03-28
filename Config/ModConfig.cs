using StardewModdingAPI;
using System;

namespace LVCMod
{
    class ModConfig
    {
        public UserConfig User { get; set; } = new();

        public BotConfig Bot { get; set; } = new();

        public HostConfig Host { get; set; } = new();
    }
}
