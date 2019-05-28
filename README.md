# DuckGame.PersistentSettings

Saves your Duck Game match settings into DuckGame/Options/matchsettings.json

Optionally starts a multiplayer game when you go to the team selection screen right away,
however that'll need a direct edit of the option "StartOnline" in the settings file as I didn't add UI for it.

# How to build

Put this repo in your Duck Game's mods folder like this: `DuckGame/Mods/PersistentSettings/PersistentSettings.sln`

Copy Duck Game's exe into the `ThirdParty` folder ( I couldn't be arsed to use an ENV variable so make do with this )

Then just build it and Duck Game should pickup the dll with the same name as the folder, if the name differs it will NOT load.
