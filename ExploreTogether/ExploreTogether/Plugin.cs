using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using ExploreAsOne.Utilities;
using ExploreTogether.Patches;
using HarmonyLib;
using UnityEngine;
using WeylandMod.Utilities;

namespace ExploreTogether
{
    //[BepInProcess("valheim.exe")]
    [BepInPlugin("com.rolopogo.plugins.exploretogether","ExploreTogether","1.1.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool busy { get; private set; }

        public static Plugin Instance { get; private set; }
        public static BepInEx.Logging.ManualLogSource logger => Instance.Logger;
        public static BepInEx.Configuration.ConfigFile config => Instance.Config;

        void Awake()
        {
            Instance = this;
            Settings.Init();
            Harmony.CreateAndPatchAll(typeof(Minimap_Patch), "com.rolopogo.plugins.exploretogether");
            Harmony.CreateAndPatchAll(typeof(Player_Patch), "com.rolopogo.plugins.exploretogether");
            Harmony.CreateAndPatchAll(typeof(Chat_Patch), "com.rolopogo.plugins.exploretogether");
            Harmony.CreateAndPatchAll(typeof(ZNet_Patch), "com.rolopogo.plugins.exploretogether");
        }

        public static void ShareMap()
        {
            if (busy)
            {
                AddString("Can't share map just yet!");
                return;
            }
            busy = true;
            var m_explored = Minimap.instance.GetPrivateField<bool[]>("m_explored");
            var compressed = MapCompression.Compress(m_explored);
            ZPackage z = new ZPackage(compressed);
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ShareExploration", z);
            
            AddString("Finished sharing map!");
            busy = false;
        }

        public static void SendPin(Minimap.PinData pin, string text)
        {
            if (pin == null) return;
            if (pin.m_type == Minimap.PinType.Player) return;
            if (pin.m_type == Minimap.PinType.Ping) return;
            if (pin.m_type == Minimap.PinType.Bed) return;
            if (pin.m_type == Minimap.PinType.Death)
            {
                if (!Settings.ShareDeathMarkers.Value) return;
                if (pin.m_name == string.Empty) text = Player.m_localPlayer.GetPlayerName() + "'s Gravestone";
            }
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "PlayerAddedPin", pin.m_pos, pin.m_type.ToString(), text, Player.m_localPlayer.GetPlayerName());
        }

        public static bool SimilarPinExists(Vector3 pos, Minimap.PinType type, List<Minimap.PinData> pins, out Minimap.PinData match)
        {
            foreach (Minimap.PinData pinData in pins)
            {
                if (Utils.DistanceXZ(pos, pinData.m_pos) < Settings.SharedPinOverlapDistance.Value && type == pinData.m_type)
                {
                    match = pinData;
                    return true;
                }
            }
            match = null;
            return false;
        }

        public static void AddString(string text)
        {
            var buffer = Chat.instance.GetPrivateField<List<string>>("m_chatBuffer");
            buffer.Add(text);
            while (buffer.Count > 15)
            {
                buffer.RemoveAt(0);
            }
            Chat.instance.InvokeMethod("UpdateChat");
        }

        public static void SharePins()
        {
            var pins = Minimap.instance.GetPrivateField<List<Minimap.PinData>>("m_pins").ToArray();
            foreach (var pin in pins)
            {
                var name = pin.m_name;

                SendPin(pin, name);
            }
        }
    }
}
