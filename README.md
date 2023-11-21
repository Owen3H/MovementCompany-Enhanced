# MovementCompany-Enhanced
A maintained version of 2018's [Movement-Company](https://github.com/u-2018/Movement-Company).<br>
PRs welcome :)

## Changes
- Hardcoded values were replaced with a config file found in `BepInEx/config`. (Generated when the game launches)
- Current coords + velocity now displayed. To turn it off, set `bDisplayDebugInfo` to `false`.

## Goals
- [**In Progress**] Optimize code and improve maintainability.
- Option for bhopping to drain stamina.
- Air crouching
- Sliding?

# Installation
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) v5 into your game.
2. Install [LC_API](https://thunderstore.io/c/lethal-company/p/2018/LC_API/) into your game.
3. Download `MovementCompanyEnhanced.dll` and put it into `Lethal Company\BepInEx\plugins`.
