# Distance Custom Car

BepInEx mod for Distance that loads custom car models from asset bundles.

## Requirements

- [BepInEx](https://github.com/BepInEx/BepInEx) 5.x installed in your Distance game directory
- .NET Framework 3.5 SDK (installed with Visual Studio 2022 Community or Build Tools)

## Build

Open `Distance.CustomCar.sln` in Visual Studio 2022 and build from there.

Or from the command line:

```powershell
msbuild Distance.CustomCar.sln /p:Configuration=Debug
```

The `lib\Assembly-CSharp.dll` is auto-generated (publicized from the game's assembly) and committed. To regenerate it, delete the file and set `DISTANCE_GAME_DIR` to your Distance install path:

```powershell
$env:DISTANCE_GAME_DIR = "G:\SteamLibrary\steamapps\common\Distance"
msbuild /p:DistanceGameDir="%DISTANCE_GAME_DIR%"
```

## Deploy

Copy `Distance.CustomCar\bin\Debug\Distance.CustomCar.dll` to your BepInEx plugins folder:

```
<GameDir>\BepInEx\plugins\Distance.CustomCar\Distance.CustomCar.dll
```

## How it works

- On startup, asset bundles (car `.prefab` files) are loaded in parallel batches
- A file-signature based cache (`Settings\car_cache.json`) tracks each bundle's state
- Unchanged bundles skip the prefab-name scan on subsequent launches
- Bundles are unloaded after assets are extracted, keeping memory low
