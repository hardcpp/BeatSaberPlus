# BeatSaberPlus (BS+)

**Discord** https://discord.gg/63ebPMC (**Download, Support, Testing, Early releases** and cool new mods coming soon)
**Patreon** [https://www.patreon.com/BeatSaberPlus](https://www.patreon.com/BeatSaberPlus) **if you want to support this project development!**

Current version : 4.6.1

IMPORTANT Most of the modules are disabled by default, you can enable then in BeatSaberPlus -> Settings  
IMPORTANT When you enable Chat module, it will open a page in your browser for configuring it  
IMPORTANT When you enable Multiplayer+ module, you can setup them in BeatSaberPlus -> Multiplayer+. To use the Multiplayer+ mod, don't use the **Online** button but use the **Multiplayer+** button on the left panel !   

## **Main features**

 - **Chat:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat))
	 - Display in game your Twitch & Youtube (Patreons only) chat (7TV & FFZ & BTTV support).
	 - **Emotes cache system to save bandwidth on game start!** 
	 - See subscriptions, follow, raid, bits events, channel redeems (channel points).
	 - Twitch: Polls, Predictions(bets), HypeTrains
	 - Viewer count.
	 - Hide messages started with "!"
	 - Lot of customization options (Color, Size, Filter TTS messages, hide specific events...).
 - **ChatEmoteRain:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-emote-rain))
	 - See emotes used in chat raining in game!
	 - Support **GIF / APNG / PNG / WEBP** files.
	 - Custom emote rain when someone subscribe to your Twitch channel.
	 - Advanced configuration options.
- **ChatIntegrations:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations))
 	 - Make some different actions with events
	 - Events can use -> **Chat events / Level status / VoiceAttackCommands(plugin install requied)**
	 - Conditions -> **Chat Request / Event enabled ? / Playing map ? / Cooldown / OBS Status**
	 - Actions -> **Camera2 / Chat / EmoteRain / Event / GamePlay / Misc(waiting events) / NoteTweaker(profile changing) / OBS interact / SongChartVisualizer / Twitch**
 - **ChatRequest:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-request))
	 - Chat request system that allow your viewers to make requests with [https://beatsaver.com/](https://beatsaver.com/) website.
	 - Display information about all difficulties for a song including NPS/Offset.
	 - Display song description, votes, upload date when you select a song.
	 - Display scores on song when you over one.
	 - User, Mapper, Song ban system
	 - Safe mode that hide any sensitive informations (Song name, artist, uploader..)
	 - History & blacklist tab that let you see your request history and manage your blacklist.
	 - **An intelligent "!link" command that show current played song or last one and provide a link to beatsaver.com if the map is public, the command also works outside of request and in multiplayer!  
	 There are lots of other useful commands you can find in [the wiki](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-Chat-Request#4---chat-commands)**
 - **GameTweaker:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#game-tweaker))  
     - **Can remove note debris even with the Liv capture**
	 - **Can remove note debris, cut particles, obstacles particles, floor burn particles, floor burn effects, saber clash particles, world particles.**
	 - **Precise reaction time (AKA offset) selection**
	 - **Add an override light intensity option that let you boost/dim lights from 0% to 2000% (also work in static lights).**
	 - Can remove BTS/LinkinPark assets in a play environment, FullCombo loss animation, Editor button on the main menu, Promotional content from the menu.
	 - Can re-order player options menu for better accessibility.
	 - Can automatically clean all old logs entry from your game folder to keep it clean.
	 - Can add keyboard bindings to pause/continue/restart/exit a song in FPFC mode.
 - **MenuMusic:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#menu-music))
	 - Replace menu ambient sounds with music!
	 - Play any songs from your custom levels or your own selection of music!
	 - Player interface shows in the menu on left with Prev/Random/Play/Pause/Next buttons and with a volume selector.
	 - **A play button to play the current song level**.
 - **Multiplayer+:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#multiplayer-plus))  
	 - Basic features included like Quick Play, Server creation, Joining code and room finding with basic search
	 - In the rooms, you can change room code, hide and show the code, play up to 20 people (40 with one patreon present)
	 - With your multiplayer+ setup, you can manage the permission for Mods, Vips or users to be able to get the code in chat with ‘!room’ or no
	 - Custom Songs with BeatSaver support
	 - ScoreSaber and BeatLeader support
	 - Noodle/Chroma & 360/90 maps support
	 - In game avatars & movement sync
	 - An option inside the room to see/not see other player while playing included (arena)
 - **NoteTeaker:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#note-tweaker))   
	 - Profiles can be created and [ChatIntegrations](https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations) can switch it with your events
	 - Changing any parts of notes like dots(with sliders or not), arrow, arcs, note or bomb scale
	 - A preview of your changes is on the right panel
	 - Hide/show dot on directional notes support
 - **SongChartVisualizer:** ([Documentation](https://github.com/hardcpp/BeatSaberPlus/wiki#song-chart-visualizer))
	 - Preview map difficulty with a nice and beautiful graph in game that illustrate NPS (Notes per second).
	 - Support for 90 & 360 levels.
	 - Lot of customization options (Colors, Legend, Dash lines...).


## **Dependencies**

- SongCore [https://github.com/Kylemc1413/SongCore](https://github.com/Kylemc1413/SongCore)
- BeatSaberMarkupLanguage [https://github.com/monkeymanboy/BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage)

## **Other questions? See the FAQ [Here](https://github.com/hardcpp/BeatSaberPlus/wiki/%5BEN%5D-FAQ)**

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
