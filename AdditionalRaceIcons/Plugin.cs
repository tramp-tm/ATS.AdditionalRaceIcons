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
                sprites.Add("Beaver",
                    TextureHelper.GetImageAsSprite("ats_beaver_f.png", TextureHelper.SpriteType.RaceIcon));
                sprites.Add("Foxes", TextureHelper.GetImageAsSprite("ats_fox.png", TextureHelper.SpriteType.RaceIcon));
                sprites.Add("Frog",
                    TextureHelper.GetImageAsSprite("ats_frog_f.png", TextureHelper.SpriteType.RaceIcon));
                sprites.Add("Human",
                    TextureHelper.GetImageAsSprite("ats_human_m.png", TextureHelper.SpriteType.RaceIcon));
                sprites.Add("Lizard",
                    TextureHelper.GetImageAsSprite("ats_lizard_f.png", TextureHelper.SpriteType.RaceIcon));
                sprites.Add("Harpy",
                    TextureHelper.GetImageAsSprite("sta_harpy_m.png", TextureHelper.SpriteType.RaceIcon));
                LogInfo($"sprites  ({sprites.Count}) is loaded!");
                foreach (var entry in sprites)
                {
                    LogInfo($"sprite:  {entry.Key} {entry.Value.name}");
                }

                {
                }
            }
            catch (Exception e)
            {
                LogInfo($"!!!!!!!!!!!!!");
                Console.WriteLine(e);
                throw;
            }
        }

        private static Sprite GetIcon(Villager villager)
        {
            var raceModelName = villager.raceModel.name;
            var icon = villager.raceModel.roundIcon;

            bool shouldChangeIcon = raceModelName switch
            {
                "Human" or "Harpy" => villager.state.isMale,
                "Beaver" or "Foxes" or "Frog" or "Lizard" => !villager.state.isMale,
                _ => false
            };

            if (shouldChangeIcon)
            {
                LogInfo("Changing icon");
                icon = sprites[raceModelName];
            }

            return icon;
        }

        [HarmonyPatch(typeof(VillagerPanel), nameof(VillagerPanel.SetUpMainPanel))]
        [HarmonyPostfix]
        private static void VillagerPanel_SetUpMainPanel_Postfix(VillagerPanel __instance)
        {
            LogInfo("SetUpMainPanel_Postfix");
            __instance.raceIcon.sprite = GetIcon(VillagerPanel.current);
        }

        [HarmonyPatch(typeof(HouseResidentButton), nameof(HouseResidentButton.SetUpIcon))]
        [HarmonyPostfix]
        private static void HouseResidentButton_SetUpIcon_Postfix(HouseResidentButton __instance)
        {
            LogInfo("HouseResidentButton_Postfix");
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
            LogInfo("SetUpMainPanel_Postfix");
            __result = GetIcon(__instance);
        }
    }
}