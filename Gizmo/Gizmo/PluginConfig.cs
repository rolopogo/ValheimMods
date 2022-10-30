using BepInEx.Configuration;

using UnityEngine;

namespace Gizmo {
  public static class PluginConfig {
    public static ConfigEntry<int> SnapDivisions { get; private set; }

    public static ConfigEntry<KeyboardShortcut> XRotationKey;
    public static ConfigEntry<KeyboardShortcut> ZRotationKey;
    public static ConfigEntry<KeyboardShortcut> ResetRotationKey;
    public static ConfigEntry<KeyboardShortcut> ResetAllRotationKey;
    public static ConfigEntry<KeyboardShortcut> ChangeRotationModeKey;
    public static ConfigEntry<KeyboardShortcut> CopyPieceRotationKey;
    public static ConfigEntry<KeyboardShortcut> SnapDivisionIncrementKey;
    public static ConfigEntry<KeyboardShortcut> SnapDivisionDecrementKey;

    public static ConfigEntry<bool> ShowGizmoPrefab;
    public static ConfigEntry<bool> ResetRotation;

    public static int MaxSnapDivisions = 256;
    public static int MinSnapDivisions = 2;

    public static void BindConfig(ConfigFile config) {
      SnapDivisions =
          config.Bind(
              "Gizmo",
              "snapDivisions",
              16,
              new ConfigDescription(
                  "Number of snap angles per 180 degrees. Vanilla uses 8.",
                 new AcceptableValueRange<int>(MinSnapDivisions, MaxSnapDivisions)));

      XRotationKey =
          config.Bind(
              "Keys",
              "xRotationKey",
              new KeyboardShortcut(KeyCode.LeftShift),
              "Hold this key to rotate on the x-axis/plane (red circle).");

      ZRotationKey =
          config.Bind(
              "Keys",
              "zRotationKey",
              new KeyboardShortcut(KeyCode.LeftAlt),
              "Hold this key to rotate on the z-axis/plane (blue circle).");

      ResetRotationKey =
          config.Bind(
              "Keys",
              "resetRotationKey",
              new KeyboardShortcut(KeyCode.V),
              "Press this key to reset the selected axis to zero rotation.");

      ResetAllRotationKey =
          config.Bind(
              "Keys",
              "resetAllRotationKey",
              KeyboardShortcut.Empty,
              "Press this key to reset _all axis_ rotations to zero rotation.");


      ChangeRotationModeKey =
          config.Bind(
              "Keys",
              "changeRotationMode",
              new KeyboardShortcut(KeyCode.BackQuote),
              "Press this key to toggle rotation modes.");

      CopyPieceRotationKey =
          config.Bind(
              "Keys",
              "copyPieceRotation",
              KeyboardShortcut.Empty,
              "Press this key to copy targeted piece's rotation.");

      SnapDivisionIncrementKey =
          config.Bind(
              "Keys",
              "snapDivisionIncrement",
              new KeyboardShortcut(KeyCode.PageUp),
              "Doubles snap divisions from current.");

      SnapDivisionDecrementKey =
          config.Bind(
              "Keys",
              "snapDivisionDecrement",
              new KeyboardShortcut(KeyCode.PageDown),
              "Doubles snap divisions from current.");

      ShowGizmoPrefab = config.Bind("UI", "showGizmoPrefab", true, "Show the Gizmo prefab in placement mode.");
      ResetRotation = config.Bind("RotationFrame", "resetOnModeChange", true, "Resets the piece's rotation on mode switch.");
    }
  }
}
