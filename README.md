# BeatSaberPlus (BS+)

**Discord** https://discord.gg/63ebPMC (**Download, Support, Testing, Early releases** and cool new mods coming soon)  
**Patreon** https://www.patreon.com/BeatSaberPlus **if you want to support this project development!**  

**[READ THE PATCH NOTES ! 🥖](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-Patchnotes)**

Current version : 6.0.8
BeatSaber compatibility : 1.25.0 to 1.29.1 AND 1.31.0

**Important:** Most modules are disabled by default. Enable them in BeatSaberPlus -> Settings.  
*   If you enable the Chat module, it will open a page in your browser for configuration.
*   To use the Multiplayer+ module, go to BeatSaberPlus -> Multiplayer+ and set it up. Use the **Multiplayer+** button on the left panel instead of the **Online** button.  

## Main features

- **Chat:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat))
	- Display in game your Twitch & YouTube (Patreons only) chat (7TV & FFZ & BTTV support).
	- **Emotes cache system to save bandwidth on game start!** 
	- See subscriptions, follows, raids, bits events and channel redeems (channel points).
	- Twitch-specific features include Polls, Predictions (bets) and HypeTrains.
	- Viewer count.
	- Hide messages started with "!"
	- Lots of customization options (colors, sizes, filter TTS messages, hide specific events and more).
- **ChatEmoteRain:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-emote-rain))
    - See emotes used in chat raining in-game.
    - Support for **GIF / APNG / PNG / WEBP** files.
    - Custom emote rain when someone subscribes to your Twitch channel.
    - Advanced configuration options.
- **ChatIntegrations:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations))
    - Perform various actions with events.
    - Events can use -> **Chat events / Level status / VoiceAttackCommands (plugin installation required)**
    - Conditions -> **Chat Request / Event enabled? / Playing map? / Cooldown / OBS Status**
    - Actions -> **Camera2 / Chat / EmoteRain / Event / GamePlay / Misc (waiting events) / NoteTweaker (profile changing) / OBS interact /   SongChartVisualizer / Twitch**
- **ChatRequest:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-request))
    - Chat request system that allows viewers to make requests using the [https://beatsaver.com/](https://beatsaver.com/) website.
    - Displays information about all difficulties for a song including NPS/Offset.
    - Displays song description, votes, upload date when you select a song.
    - Displays score on songs when you hover over one.
    - User, Mapper, Song ban system
    - Safe mode that hides any sensitive information (song name, artist, uploader, etc.)
    - History & blacklist tab that lets you see your request history and manage your blacklist.
    - **An intelligent "!link" command that shows the current played song or last one and provides a link to beatsaver.com if the map is public. The command also works outside of request and in multiplayer!**
    - **You can find other useful commands in [the wiki](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-Chat-Request#4---chat-commands).**
- **GameTweaker:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#game-tweaker))  
    - Can remove note debris, cut particles, obstacle particles, floor burn particles, floor burn effects, saber clash particles and world particles even with Liv capture.
    - Offers precise reaction time (AKA offset) selection.
    - Provides an option to override light intensity and boost/dim lights from 0% to 2000% (also works in static lights).
    - Can remove BTS/LinkinPark assets in a play environment, FullCombo loss animation, Editor button on the main menu, and promotional content from the menu.
    - Allows re-ordering player options menu for better accessibility.
    - Automatically cleans all old logs entry from your game folder to keep it clean.
    - Adds keyboard bindings to pause/continue/restart/exit a song in FPFC mode.
- **MenuMusic:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#menu-music))
    - Replaces menu ambient sounds with music!
    - Plays any songs from your custom levels or your own selection of music!
    - Shows player interface in the menu on the left with Prev/Random/Play/Pause/Next buttons and with a volume selector.
    - Provides a play button to play the current song level.
- **Multiplayer+:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#multiplayer-plus))  
	- Basic features included like Quick Play, Server creation, Joining code and room finding with basic search
	- In the rooms, you can change the room code, hide and show the code, play up to 20 people (40 with one Patreon present)
	- With your Multiplayer+ setup, you can manage the permission for Mods, Vips or users to be able to get the code in chat with ‘!room’ or not
	- Custom Songs with BeatSaver support
	- ScoreSaber and BeatLeader support
	- Can disable your score submission
	- Noodle/Chroma & 360/90 maps support
	- In-game avatars & movement sync
	- An option inside the room to see/not see other players while playing included
- **NoteTweaker:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#note-tweaker))
    - Create profiles and switch them using [ChatIntegrations](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations)
    - Modify different parts of notes, such as dots (with or without sliders), arrows, arcs, notes and bombs scale
    - Preview changes in a right panel
    - Supports hiding/showing dots on directional notes
- **SongChartVisualizer:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#song-chart-visualizer))
    - Preview map difficulty with a graph that shows NPS (Notes Per Second) in-game
    - Supports 90 & 360 levels
    - Offers various customization options (Colors, Legend, Dash lines...)
 - **SongOverlay:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#song-overlay))
	 - Send game information to some overlays that are compatible with BS+
	 	- Game version
		- Game status (menu, playing)
		- Map info before playing a map
		- Pause or resume events
		- Score events (JSON format)


## **Dependencies**

- SongCore [https://github.com/Kylemc1413/SongCore](https://github.com/Kylemc1413/SongCore)
- BeatSaberMarkupLanguage [https://github.com/monkeymanboy/BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage)


### **[How to install](https://github.com/hardcpp/BeatSaberPlus/wiki#2---How-to-Install)**

### **[See the FAQ](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-FAQ)**

### **[READ THE PATCH NOTES ! 🥖](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-Patchnotes)**

## **Special Thanks**:
- **Vred#0001** For art & documentation
- **Brase#6969** For documentation
- **Crafang#8040** For documentation & translation
- **Lucy#9197** For documentation
- **redegg89#9290** For Documentation syntax/grammar

## **Discord & Download/Update**
https://discord.gg/63ebPMC 

## **Credits / Copyright**
* [EnhancedStreamChat-v3](https://github.com/brian91292/EnhancedStreamChat-v3)
* [TournamentAssistant](https://github.com/MatrikMoon/TournamentAssistant)
* [Beat-Saber-Utils](https://github.com/Kylemc1413/Beat-Saber-Utils)
* [BeatSaverDownloader](https://github.com/Kylemc1413/BeatSaverDownloader)
* [BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage)
* [websocket-sharp](https://github.com/sta/websocket-sharp)

## **Screenshots**
![](https://puu.sh/GO6tf/81ff167aab.png)
![](https://puu.sh/GKKJJ/7a481941c5.png)
![](https://puu.sh/GKPcD/ecee2e5d86.png)
![](https://puu.sh/GH9Rn/d9d4966a04.png)
![](https://puu.sh/GH9RA/f6dc522cd1.png)
![](https://puu.sh/GL7BX/0e5f12cfce.jpg)
