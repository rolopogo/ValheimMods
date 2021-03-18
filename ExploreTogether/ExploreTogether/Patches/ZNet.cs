using RoloPogo.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeylandMod.Utilities;

namespace ExploreTogether.Patches
{
    class ZNet_Patch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ZNet), "GetOtherPublicPlayers", new Type[] { typeof(List<ZNet.PlayerInfo>) })]
        public static void GetOtherPublicPlayers(object instance, List<ZNet.PlayerInfo> playerList) => throw new NotImplementedException();

        [HarmonyPatch(typeof(ZNet), "Awake")]
        [HarmonyPostfix]
        private static void ZNet_Awake(ref ZNet __instance)
        {
            try
            {
                ZRoutedRpc.instance.Register<ZPackage>("ShareExploration", RPC_ShareExploration);
            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e);
            }

            try
            {
                ZRoutedRpc.instance.Register<Vector3, string, string, string>("PlayerAddedPin", RPC_PlayerAddedPin);
            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e);
            }
        }

        private static void RPC_PlayerAddedPin(long sender, Vector3 pos, string type, string name, string player)
        {
            Plugin.logger.LogDebug($"{nameof(RPC_PlayerAddedPin)}: {player} shared {name} at ({pos.x},{pos.z})");
            if (Settings.SharePinsWithOtherPlayers.Value)
            {
                Minimap.PinType pinType = (Minimap.PinType)Enum.Parse(typeof(Minimap.PinType), type);

                if (Player.m_localPlayer.GetPlayerID() == sender) return;
                var pins = Minimap.instance.GetPrivateField<List<Minimap.PinData>>("m_pins");
                if (Plugin.SimilarPinExists(pos, pinType, pins, out Minimap.PinData match))
                {
                    if (match.m_name != name)
                    {
                        Plugin.AddString($"{player} changed the name pin: \"{match.m_name}\" at ({Mathf.RoundToInt(pos.x)}, {Mathf.RoundToInt(pos.y)}) to {name}.");
                        match.m_name = name;
                    }
                }
                else
                {
                    Plugin.AddString($"{player} added pin: \"{name}\" at ({Mathf.RoundToInt(pos.x)}, {Mathf.RoundToInt(pos.y)}).");
                    Minimap.instance.AddPin(pos, pinType, name, true, false);
                }
            }
        }

        private static void RPC_ShareExploration(long sender, ZPackage z)
        {
            Plugin.logger.LogDebug($"{nameof(RPC_ShareExploration)}: Received map data from {sender}");
            if (Settings.OthersRevealMap.Value)
            {
                var explored = z.GetArray();
                var m_explored = Minimap.instance.GetPrivateField<bool[]>("m_explored");
                var bits = MapCompression.Decompress(explored);
                
                if (bits.Length != m_explored.Length)
                {
                    Plugin.logger.LogError("mismatched lengths");
                    return;
                }

                var changed = false;
                for (int i = 0; i < bits.Length && i < Minimap.instance.m_textureSize * Minimap.instance.m_textureSize; i++)
                {
                    if (bits[i] && !m_explored[i])
                    {
                        Minimap_Patch.Explore(Minimap.instance, i % Minimap.instance.m_textureSize, i / Minimap.instance.m_textureSize);
                        changed = true;
                    }
                }
                if (changed)
                    Minimap.instance.GetPrivateField<Texture2D>("m_fogTexture").Apply();
            }
        }
    }
}
