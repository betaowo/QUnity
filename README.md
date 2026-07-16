# QUnity Engine

Quake 1 movement & gameplay framework for Unity.

> ⚠️ **Not a 1:1 port.** Loose recreation. Bugs are features. If you want pixel-perfect accuracy, go play QuakeSpasm.
> PRs welcome. Bug reports too. Just don't expect id Software level of polish.

## Features

### Movement
- Quake 1 physics — ground friction, air acceleration, bunnyhopping
- Crouching with camera & collider adjustment
- Noclip mode
- Water volumes (Water, Slime, Lava) with buoyancy
- Fall damage
- Step-up for stairs & ledges
- Camera bob & viewmodel sway (V_CalcBob port)
- Camera roll on strafe

### Weapons
- Modular weapon system — Melee, SingleShot, Burst (shotgun)
- Magazine-based ammo with reserve
- Ammo types — Shells, Nails, Rockets, Cells
- Reload types — Magazine, Single shell
- Auto-fire toggle
- Weapon ownership — collect to unlock
- BlendShape-based animations
- Muzzle flash & impact particles

### Enemies
- Melee & Ranged types
- Sight detection with raycast
- Pain & death animations
- Gibs on heavy damage
- Loot drops

### Console
- Open/close with tilde (~) or Escape
- Commands: `noclip`, `god`, `map`, `sv_gravity`, `impulse`, `fov`
- Cvars: `cl_bob`, `cl_rollangle`, `hud_style`
- Command history & autoscroll

### HUD
- 5 styles — center, side split, corners
- HP, Armor, Ammo with reserve
- Crosshair
- Intermission screen with stats

### World
- Pickups — Health, Armor, Ammo, Weapons (respawnable toggle)
- Teleport & Kill triggers
- Secret detection with on-screen message
- End-level trigger with stats screen
- Pause menu

## Requirements
- Unity 2022.3+
- Input System package

## Setup
1. Clone or download this repo
2. Open in Unity
3. Install Input System via Package Manager (Window → Package Manager → Unity Registry → Input System)
4. Open `Scenes/MainMenu` and hit Play

## Credits
- Original movement system: [International9/Quake-Unity-Movement](https://github.com/International9/Quake-Unity-Movement)
- Original Quake by id Software (1996)
- Models & sounds from various Quake mods (Navy Seals, etc.)

## License
MIT — do whatever. Just don't sue me.
