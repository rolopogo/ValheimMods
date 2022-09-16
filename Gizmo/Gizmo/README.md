# ComfyGizmo v1.4.0

*Comfy-specific version of Gizmo.*

  * Configurable modifier hot-keys for:
    * X-axis rotation (default: hold `LeftShift`)
    * Z-axis rotation (default: hold `LeftAlt`)
    * Reset selected axis rotation (default: `V`)
    * Reset **ALL** axis rotations (disabled by default)
  * Can disable the Gizmo placement visual.
  * Can set the snap angles per 180 degrees from 2 - 128.
  * Original Euler-style rotation.
 
## Installation

### Manual installation:

  1. Unzip `Gizmo.dll` to your `/valheim/BepInEx/plugins/` folder

### Thunderstore

  1. **Disable or uninstall** the existing `Gizmo v1.0.0` mod by Rolo.
  2. Go to Settings > Import local mod > select `ComfyGizmo_v1.4.0.zip`.
  3. Click "OK/Import local mod" on the pop-up for information.
  
## Changelog

### 1.4.0 (WIP)

  * Create a new GameObject `ComfyGizmo` to maintain the current Quaternion rotation state.
  * Re-ordered the mapping/assignment of original Gizmo's prefab XYZ roots/transforms per request by Jere.
  * Increased the snap-angles maximum from 128 to 256.
  * Moved plugin configuration logic into its own class `PluginConfig`.

### 1.3.1

  * Try to add compatability with other mods that also transpile `UpdatePlacementGhost`.

### 1.3.0

  * Added configuration option for a 'reset all rotations' hot-key (default to un-set).
  * Cleaned-up the UpdatePlacementGhost transpiler.

### 1.2.0

  * Modified GizmoRoot instantiate to trigger on `Game.Start()`.

### 1.1.0

  * Turn SnapDivisions into a slider.
  * Moved `Resources.GetResources()` into the main plugin file.

### 1.0.0

  * Rewrite of Gizmo for Comfy-specific use.
  * Supprot re-binding of modifier keys and simplify code to one file.