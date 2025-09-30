using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace megabonkDpsMeter
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("Megabonk.exe")]
    public class Plugin : BasePlugin
    {
        public const string
            MODNAME = "megabonkDpsMeter",
            AUTHOR = "tahaaydn",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0";

        internal static ConfigEntry<KeyCode> toggleKey;
        internal static ManualLogSource log;
        internal static bool disableMeter;

        public Plugin() => log = Log;

        public override void Load()
        {
            toggleKey = Config.Bind(
                "General", "ToggleKey", KeyCode.F1,
                "Key to toggle the Stats Window"
            );

            log.LogInfo($"Loading {MODNAME} v{VERSION} by {AUTHOR}");

            AddComponent<InputDetector>();

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
            log.LogInfo($"{MODNAME} loaded.");
        }
        
    }

    public class InputDetector : MonoBehaviour
    {
        private GameObject statsParent;
        private GameObject damageWindow;
        private GameObject statsWindow;
        private GameObject questsWindow;

        private void Update()
        {
            if (Input.GetKeyDown(Plugin.toggleKey.Value) && !Plugin.disableMeter)
            {
                ToggleStatsWindow();
            }
        }

        private void ToggleStatsWindow()
        {
            if (statsParent == null)
            { 
                statsParent = GameObject.Find("GameUI/GameUI/DeathScreen/StatsWindows");
                if (statsParent != null)
                {
                    damageWindow = statsParent.transform.Find("W_Damage").gameObject;
                    statsWindow = statsParent.transform.Find("W_Stats").gameObject;
                    questsWindow = statsParent.transform.Find("W_Quests").gameObject;
                }
            }

            if (statsParent != null)
            {
                bool newState = !statsParent.activeSelf;
                statsParent.SetActive(newState);
                statsWindow.SetActive(!newState);
                questsWindow.SetActive(!newState);

                if (newState)
                {
                    var ui = statsParent.GetComponentInChildren<GameOverDamageSourcesUi>();
                    ui?.Start();
                }
            }
            else
            {
                return;
            }
        }
    }

    [HarmonyPatch(typeof(GameOverDamageSourcesUi), "Start")]
    public static class Patch_GameOverDamageSourcesUi_Start
    {
        private static void Prefix(GameOverDamageSourcesUi __instance)
        {
            var contentEntries = GameObject.Find("GameUI/GameUI/DeathScreen/StatsWindows/W_Damage/WindowLayers/Content/ScrollRect/ContentEntries");
            if (contentEntries == null)
            {
                return;
            }

            Transform contentTransform = contentEntries.transform;
            for (int i = contentTransform.childCount - 1; i >= 3; i--)
            {
                GameObject.Destroy(contentTransform.GetChild(i).gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(GameManager), "StartPlaying")]
    public static class Patch_GameManager_StartPlaying
    {
        private static void Postfix(GameManager __instance)
        {
            Plugin.disableMeter = false;
        }
    }

    [HarmonyPatch(typeof(GameManager), "OnDied")]
    public static class Patch_GameManager_OnDied
    {
        private static void Postfix(GameManager __instance)
        {
            Plugin.disableMeter = true;
        }
    }
}
