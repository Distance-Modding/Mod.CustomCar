# Distance Custom Car

BepInEx mod for Distance that loads custom car models from asset bundles.

## Requirements

- Distance installed via Steam (default path: `C:\Program Files (x86)\Steam\steamapps\common\Distance`)
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.x installed in your Distance game directory
- .NET Framework 3.5 SDK (installed with Visual Studio 2022 Community or Build Tools)

## Build

By default, the project references DLLs from your Steam installation path. If Distance is installed elsewhere, set the `DISTANCE_GAME_DIR` environment variable:

```powershell
$env:DISTANCE_GAME_DIR = "G:\SteamLibrary\steamapps\common\Distance"
```

Or pass it at build time:

```powershell
msbuild Distance.CustomCar.sln /p:Configuration=Debug /p:DistanceGameDir="G:\SteamLibrary\steamapps\common\Distance"
```

The publicized `publicized_assemblies\Assembly-CSharp.dll` is auto-generated on first build (requires `DistanceGameDir` to find the game's `Assembly-CSharp.dll`).

You can also open `Distance.CustomCar.sln` in Visual Studio 2022 and build from there.

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
