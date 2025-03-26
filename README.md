# LVCMod

## [About This Mod]

This mod adds a voice chat based on each player's zone.

## [How it works]

The mod uses a Discord bot you create and automatically creates voice channels with the name of each new area of the game a player enters.

You don't need to create each channel individually, as the Discord bot does this automatically and moves you to another channel each time you move to a new area.

## [Installation Guide]

- Download and Install SMAPI
- Unzip the mod version you want from the "DownloadRelease" folder in your mods folder

## [How Setup Voice Chat]

Clarification: This mod uses Discord to divide users by location.

1 - Discord Configuration

- Copy your discord user id
- Go to "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\mods\LVCMod\config.json" (If you don't have it purchased on Steam, you have to look for it in the other folder you have)
- Paste your user id into "DiscordUserId"
- Create a Discord Server (or use and existing one)
- Copy your guild ID (To be able to do this, you have to activate developer mode)
- Paste your guild id into "HostDiscordServerId"

2 - Bot Configuration

- Go to "https://discord.com/developers/applications" (Discord Developer Portal)
- Click on "New Application" and give your bot a name
- Go to "Bot" section, and click the button that says "Reset Token"
- Copy your bot token
- Go to "C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\mods\LVCMod\config.json"
- Paste your bot token into "HostDiscordBotToken"
- Return to the Discord Developer Portal and set the bot's permissions. You can find them below this section.
- And you're done! :D

Reminder: Do not publish your bot's token, this can allow malicious people to use your bot for malicious activities.

3 - Bot Permissions

The bot needs four minimum permissions:

- Manage Channels
- Move Members
- Mute Members
- Deafen Member

Or you can give it administrator and that's it.

## [Config Voice Chat]

- "DiscordUserId": It is actually used to save your user's Discord ID, anyone who has the mod will be able to access this ID (it is already public).
- "HostDiscordServerId": Here you save the guild id that will be used as voice chat (Only the host of the game will be used).
- "HostDiscordBotToken": This is your Discord bot token. Please be careful not to share it with anyone. (Only the match host's token is used.)
- "ITalkChannelName": It is used to define which is the default chat where no one can speak and the bot will know if the player wants to use the voice chat or not. (If the voice chat doesn't exist, the bot will create a new one with the same name)
- "StardewVoiceChatsCategory": It is used to tell the bot where to create all the voice chats for the locations to prevent it from creating them anywhere. (If the category doesn't exist, the bot will create a new one with the same name)
- "DiscordUserMicrophoneActivated": It is used so that the bot knows whether to mute you or not the next time you enter a game.
- "DiscordUserDeaferActivated": It is used so that the bot knows whether to deaf you or not the next time you enter a game.
- "DeleteVoiceChannels": This is used to delete all voice channels (except ITalk) created by the bot when you return to the game (it doesn't work if you quit the game directly). If true, it deletes them; otherwise, it does nothing. This improves performance the next time you play on the server, but at the cost of wasting channel space on Discord.
- "ChangeStateMicrophone": Defines the key to change the state of the user's microphone (mute or unmute)
- "ChangeStateAudio": Defines the key to change the user's audio state (Deaf or Undeaf)
- "HostSavesData": It's better if you don't touch this, but it's used to save the Discord IDs of all the users you have in your settings.

## [Possible doubts]

- Is the mod compatible with mods that add new localizations?

I haven't tried it, but it's supposed to.

- Does the mod have a location limit?

No but yes, the only limit is the number of voice chats that discord can support on a single server

- Do all players need to have the mod downloaded?

I don't know, but I imagine that if someone doesn't have it downloaded, the voice chat alone won't work for them.

- Can I use the mod without a bot?

No, the bot is necessary to create the voice chats and to be able to use the voice chat.

- Can I use the mod without a Discord server?

No, the mod is based on the use of Discord Guilds to create voice chats.

- Why does the game crash on me?

There are several possible reasons, one is that some of the necessary data in config.json is not filled in, another is that the bot is not on your Discord server, and another is that your bot's token is incorrect.

## [Default Controls]

- H = Mute or unmute the microphone
- J = Deaf or Undeaf audio