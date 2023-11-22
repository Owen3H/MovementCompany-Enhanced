# MovementCompany-Enhanced
A maintained version of 2018's [Movement-Company](https://github.com/u-2018/Movement-Company).<br>
PRs welcome :)

## Changes
- Hardcoded values were replaced with a config file found in `BepInEx/config`. (Generated when the game launches)
- Current coords + velocity now displayed. To turn it off, set `bDisplayDebugInfo` to `false`.
- Improved maintainability.
    - Re-organized project and made use of abstraction with aptly named methods.
    - Plugin metadata now has its own class - no longer hidden in `/bin`.
    - Made it easier to PR (post-build event, gitignore)
- Code optimizations.
    - MovementAdder removed. Movement script is now given on player spawn instead of each frame.
    - Harmony now initialized in Awake
- Misc
    - Fixed player spawning mid-air which caused them to fly around the ship.
    - Base player speed slightly increased. `4f` -> `4.2f`
    
## Goals
- [**In Progress**] Remove jump delay.
- Option for bhopping to drain stamina.
- Air crouching
- Sliding?

# Installation
1. Install [BepInEx](https://github.com/BepInEx/BepInEx/releases) v5 into your game.
2. Install [LC_API](https://thunderstore.io/c/lethal-company/p/2018/LC_API/) into your game.
3. Download `MovementCompanyEnhanced.dll` and put it into `Lethal Company\BepInEx\plugins`.
