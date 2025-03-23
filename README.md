# LVCMod

## [Installation Guide]

Download and Install SMAPI
Unzip this Mod into the game's Mods folder.

## [How Setup Voice Chat]

Clarification: This mod uses Discord to divide users by location.

1 - Discord Configuration

- Copy your discord user id
- Go to "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\mods\LVCMod\config.json" (If you don't have it purchased on Steam, you have to look for it in the other folder you have)
- Paste your user id into "HostDiscordUserId"
- Create a Discord Server (or use and existing one)
- Copy your guild ID (To be able to do this, you have to activate developer mode)
- Paste your guild id into "DiscordServerId"

2 - Bot Configuration

- Go to "https://discord.com/developers/applications"
- Click on "New Application" and give your bot a name
- Go to "Bot" section, and click the button that says "Reset Token"
- Copy your bot token
- Go to "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\mods\LVCMod\config.json"
- Paste your bot token into "DiscordBotToken"
- And you're done! :D

## [Config Voice Chat]

- "HostDiscordUserId": Although it says Host, it is actually used to save your user's Discord ID, anyone who has the mod will be able to access this ID (it is already public).
- "DiscordServerId": Here you save the guild id that will be used as voice chat (Only the host of the game will be used).
- "DiscordBotToken": This is your Discord bot token. Please be careful not to share it with anyone. (Only the match host's token is used.)
- "ITalkChannelName": It is used to define which is the default chat where no one can speak and the bot will know if the player wants to use the voice chat or not. (If the voice chat doesn't exist, the bot will create a new one with the same name)
- "StardewVoiceChatsCategory": It is used to tell the bot where to create all the voice chats for the locations to prevent it from creating them anywhere. (If the category doesn't exist, the bot will create a new one with the same name)
- "HostSavesData": It's better if you don't touch this, but it's used to save the Discord IDs of all the users you have in your settings.

## [Possible doubts]

- Is the mod compatible with mods that add new localizations?

I haven't tried it, but it's supposed to.

- Does the mod have a location limit?

No but yes, the only limit is the number of voice chats that discord can support on a single server

-Do all players need to have the mod downloaded?

I don't know, but I imagine that if someone doesn't have it downloaded, the voice chat alone won't work for them.