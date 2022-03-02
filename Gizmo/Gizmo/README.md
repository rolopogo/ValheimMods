# ComfyGizmo v1.2.0

Comfy-specific version of Gizmo.

  * Configurable modifier hot-keys.
  * Can disable the Gizmo placement visual.
  * Can set the snap angles per 180 degrees from 2 - 128.
  * Original Euler-style rotation.
 
## Installation

### Manual installation:

  1. Unzip `Gizmo.dll` to your `/valheim/BepInEx/plugins/` folder

### Thunderstore

  1. **Disable or uninstall** the existing `Gizmo v1.0.0` mod by Rolo
  2. Go to Settings > Import local mod > select `ComfyGizmo_v1.1.0.zip`
  3. Click OK on the pop-up for information
  
## Changelog

### 1.2.0

  * Modified GizmoRoot instantiate to trigger on `Game.Start()`.

### 1.1.0

  * Turn SnapDivisions into a slider.
  * Moved `Resources.GetResources()` into the main plugin file.

### 1.0.0

  * Rewrite of Gizmo for Comfy-specific use.
  * Supprot re-binding of modifier keys and simplify code to one file.