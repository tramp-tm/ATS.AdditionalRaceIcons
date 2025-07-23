using System;
using System.Collections.Generic;
using ATS_API.Helpers;
using BepInEx;
using Eremite.Buildings.UI;
using Eremite.Characters.UI;
using Eremite.Characters.Villagers;
using HarmonyLib;
using UnityEngine;

namespace AdditionalRaceIcons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        private Harmony harmony;

        public static void LogInfo(object obj) => Instance.Logger.LogInfo(obj);

        static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loading...");

            Instance = this;
            harmony = Harmony.CreateAndPatchAll(typeof(Plugin));

            LoadIcons();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public void LoadIcons()
        {
            try
            {
                var map = new Dictionary<string, string>
                {
                    { "Beaver", "ats_beaver_f.png" },
                    { "Foxes", "ats_fox.png" },
                    { "Frog", "ats_frog_f.png" },
                    { "Human", "ats_human_m.png" },
                    { "Lizard", "ats_lizard_f.png" },
                    { "Harpy", "ats_harpy_m.png" },
                    { "Bat", "ats_bat_m.png" }
                };

                foreach (var item in map)
                {
                    sprites.Add(item.Key, TextureHelper.GetImageAsSprite(item.Value, TextureHelper.SpriteType.RaceIcon));
                    LogInfo($"sprite {item.Value} is added as '{item.Key}'");
                }
                
                LogInfo($"sprites  ({sprites.Count}) is loaded!");
                foreach (var entry in sprites)
                {
                    LogInfo($"sprite:  {entry.Key} {entry.Value.name}");
                }
            }
            catch (Exception e)
            {
                LogInfo($"!!! LoadIcons  exception");
                Console.WriteLine(e);
                throw;
            }
        }

        private static Sprite GetIcon(Villager villager)
        {
            var raceModelName = villager.raceModel.name;
            var icon = villager.raceModel.roundIcon;

            // LogInfo($"raceModelName is: {raceModelName}");
            var shouldChangeIcon = raceModelName switch
            {
                "Human" or "Harpy" or "Bat" => villager.state.isMale,
                "Beaver" or "Foxes" or "Frog" or "Lizard" => !villager.state.isMale,
                _ => false
            };

            Sprite newIcon;
            if (shouldChangeIcon && sprites.TryGetValue(raceModelName, out newIcon))
            {
                // LogInfo("Changing icon");
                icon = newIcon;
            }
            else
            {
                // LogInfo("Changing icon, but icon  not found");
            }

            return icon;
        }

        [HarmonyPatch(typeof(VillagerPanel), nameof(VillagerPanel.SetUpMainPanel))]
        [HarmonyPostfix]
        private static void VillagerPanel_SetUpMainPanel_Postfix(VillagerPanel __instance)
        {
            // LogInfo("SetUpMainPanel_Postfix");
            __instance.raceIcon.sprite = GetIcon(VillagerPanel.current);
        }

        [HarmonyPatch(typeof(HouseResidentButton), nameof(HouseResidentButton.SetUpIcon))]
        [HarmonyPostfix]
        private static void HouseResidentButton_SetUpIcon_Postfix(HouseResidentButton __instance)
        {
            // LogInfo("HouseResidentButton_Postfix");
            if (__instance.isLocked)
            {
                return;
            }

            if (__instance.villager != (UnityEngine.Object)null)
            {
                __instance.raceIcon.sprite = GetIcon(__instance.villager);
            }
        }

        [HarmonyPatch(typeof(Villager), nameof(Villager.GetRoundIcon))]
        [HarmonyPostfix]
        private static void Villager_GetRoundIcon_Postfix(Villager __instance, ref Sprite __result)
        {
            // LogInfo("SetUpMainPanel_Postfix");
            __result = GetIcon(__instance);
        }
    }
}