using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploreTogether
{
    static class Settings
    {
        public static ConfigEntry<bool> OthersRevealMap { get; private set; }
        public static ConfigEntry<bool> SharePinsWithOtherPlayers { get; private set; }
        public static ConfigEntry<bool> CoordsInMinimap { get; private set; }
        public static ConfigEntry<bool> CoordsInMap { get; private set; }
        public static ConfigEntry<bool> AddCoordsToPings { get; private set; }
        public static ConfigEntry<bool> PersistentPings { get; private set; }
        public static ConfigEntry<bool> PingWhereLooking { get; private set; }
        public static ConfigEntry<string> PingKey { get; private set; }
        public static ConfigEntry<string> ShareMapKey { get; private set; }
        public static ConfigEntry<string> SharePinsKey { get; private set; }
        public static ConfigEntry<bool> ShareIndividualPin { get; private set; }
        public static ConfigEntry<bool> ShareIndividualPinRequireKey { get; private set; }
        public static ConfigEntry<string> ShareIndividualPinKey { get; private set; }
        public static ConfigEntry<bool> ShowPingWhenSharingIndividualPin { get; private set; }
        public static ConfigEntry<bool> ShareDeathMarkers { get; private set; }
        public static ConfigEntry<bool> PersistentDeathMarkers { get; private set; }
        public static ConfigEntry<float> SharedPinOverlapDistance { get; private set; }
        public static ConfigEntry<bool> MoreDetailsOnDeathMarkers { get; private set; }
        public static ConfigEntry<bool> ShowBoats { get; private set; }
        public static ConfigEntry<bool> ShowCarts { get; private set; }

        public static void Init()
        {
            OthersRevealMap = Plugin.Instance.Config.Bind("Minimap",
                "OthersRevealMap",
                true,
                "Other players will reveal areas on the map near their position if they are sharing it");

            SharePinsWithOtherPlayers = Plugin.Instance.Config.Bind("Pins",
                "SharePinsWithOtherPlayers",
                true,
                "Pins shared by other players will be added to your map");

            CoordsInMinimap = Plugin.Instance.Config.Bind("Minimap",
                "CoordsInMinimap",
                true,
                "Display your players current coordinates in the small minimap");

            CoordsInMap = Plugin.Instance.Config.Bind("Minimap",
                "CoordsInMap",
                true,
                "Display the coordinates of your cursor in the map");

            AddCoordsToPings = Plugin.Instance.Config.Bind("Minimap",
                "AddCoordsToPings",
                true,
                "Adds the coordinates of a ping to the message shown in chat in the form (x, y)");

            PingWhereLooking = Plugin.Instance.Config.Bind("General",
                "PingWhereLooking",
                true,
                "Create a ping where you are looking when you press <PingKey>");

            PingKey = Plugin.Instance.Config.Bind("General",
                "PingInputKey",
                "T",
                "The keybind to trigger a ping where you are looking");

            ShareMapKey = Plugin.Instance.Config.Bind("Minimap",
                "ShareMapKey",
                "F10",
                "The keybind to trigger sharing your map exploration with other players");

            SharePinsKey = Plugin.Instance.Config.Bind("Minimap",
                "SharePinsKey",
                "F9",
                "The keybind to trigger sharing pins with other players");

            ShareIndividualPin = Plugin.Instance.Config.Bind("Minimap",
                "ShareIndividualPin",
                true,
                "Enables the ability to share specific pins by middle clicking while holding a key.");

            ShareIndividualPinRequireKey = Plugin.Instance.Config.Bind("Minimap",
                "ShareIndividualPinRequireKey",
                true,
                "Enables or disables requiring holding a key to share the middle-clicked pin.");

            ShareIndividualPinKey = Plugin.Instance.Config.Bind("Minimap",
                "ShareIndividualPinKey",
                "LeftAlt",
                "The key that, when held, will allow middle clicking to share individual pins on the map.");

            ShowPingWhenSharingIndividualPin = Plugin.Instance.Config.Bind("Minimap",
                "ShowPingWhenSharingIndividualPin",
                true,
                "Show the map ping when sharing an individual pin.");

            ShareDeathMarkers = Plugin.Instance.Config.Bind("Pins",
                "ShareDeathMarkers",
                false,
                "Share your death marker with other players"
                );

            PersistentDeathMarkers = Plugin.Instance.Config.Bind("Pins",
                "PersistentDeathMarkers",
                true,
                "Keep your death markers, even if you die again and again"
                );

            SharedPinOverlapDistance = Plugin.Instance.Config.Bind("Pins",
                "SharedPinOverlapDistance",
                1f,
                "The distance around existing pins in which received pins will not be added (higher = less overlapping)"
                );

            MoreDetailsOnDeathMarkers = Plugin.Instance.Config.Bind("Pins",
                "MoreDetailsOnDeathMarkers",
                true,
                "Tag your death marker with your name and time of death"
                );

            ShowBoats =
            MoreDetailsOnDeathMarkers = Plugin.Instance.Config.Bind("Pins",
                "ShowBoatsOnMap",
                true,
                "Show boats as pins on your map"
                );

            ShowCarts =
            MoreDetailsOnDeathMarkers = Plugin.Instance.Config.Bind("Pins",
                "ShowCartsOnMap",
                true,
                "Show carts as pins on your map"
                );
        }
    }
}
