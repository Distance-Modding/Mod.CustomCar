# Distance Custom Car

BepInEx mod for Distance that loads custom car models from asset bundles.

## Requirements

- [BepInEx](https://github.com/BepInEx/BepInEx) 5.x installed in your Distance game directory
- .NET Framework 3.5 SDK (installed with Visual Studio 2022 Community or Build Tools)

## Setup

### 1. Reference DLLs

Before building, copy the following DLLs from your Distance game installation into the `libs/` folder:

| File | Source |
|---|---|
| `0Harmony.dll` | `<GameDir>\BepInEx\core\0Harmony.dll` |
| `BepInEx.dll` | `<GameDir>\BepInEx\core\BepInEx.dll` |
| `UnityEngine.dll` | `<GameDir>\Distance_Data\Managed\UnityEngine.dll` |

The publicized `Assembly-CSharp.dll` is auto-generated during the build — no manual copy needed.

### 2. Set the game directory

Set the `DISTANCE_GAME_DIR` environment variable to your Distance install path:

```powershell
# PowerShell
$env:DISTANCE_GAME_DIR = "G:\SteamLibrary\steamapps\common\Distance"
```

Or pass it via MSBuild:

```powershell
msbuild /p:DistanceGameDir="G:\SteamLibrary\steamapps\common\Distance"
```

### 3. Build

```powershell
msbuild Distance.CustomCar.sln /p:Configuration=Debug /p:DistanceGameDir="%DISTANCE_GAME_DIR%"
```

Or open `Distance.CustomCar.sln` in Visual Studio 2022 and build from there.

### 4. Deploy

Copy the output DLL from `Distance.CustomCar\bin\Debug\Distance.CustomCar.dll` to your BepInEx plugins folder:

```
<GameDir>\BepInEx\plugins\Distance.CustomCar\Distance.CustomCar.dll
```

## How it works

- On startup, asset bundles (car `.prefab` files) are loaded in parallel batches
- A file-signature based cache (`Settings\car_cache.json`) tracks each bundle's state
- Unchanged bundles skip the prefab-name scan on subsequent launches
- Bundles are unloaded after assets are extracted, keeping memory low
