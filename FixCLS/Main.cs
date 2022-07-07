using System.Reflection;
using FixCLS.MainPatch;
using GDMiniJSON;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;

namespace FixCLS {
    #if DEBUG
    [EnableReloading]
    #endif

    internal static class Main {
        #if DEBUG
        public static Text text;
        #endif
        internal static UnityModManager.ModEntry Mod;
        private static Harmony _harmony;

        internal static object Parser = typeof(Json).GetNestedType("Parser");
        
        internal static bool IsEnabled { get; private set; }
        // internal static MainSettings Settings { get; private set; }

        private static void Load(UnityModManager.ModEntry modEntry) {
            Mod = modEntry;
            Mod.OnToggle = OnToggle;
            // Settings = UnityModManager.ModSettings.Load<MainSettings>(modEntry);
            // Mod.OnGUI = Settings.OnGUI;
            // Mod.OnSaveGUI = Settings.OnSaveGUI;
            
            #if DEBUG
            Mod.OnUnload = Stop;
            #endif
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value) {
            IsEnabled = value;

            if (value) Start();
            else Stop(modEntry);

            return true;
        }

        private static void Start() {
            _harmony = new Harmony(Mod.Info.Id);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
            // GCS.speedTrialMode = true;
            // SceneManager.LoadScene("XT-X");

            #if DEBUG
            text = new GameObject().AddComponent<Text>();
            Object.DontDestroyOnLoad(text);
            #endif
        }

        private static bool Stop(UnityModManager.ModEntry modEntry) {
            _harmony.UnpatchAll(Mod.Info.Id);
            #if RELEASE
            _harmony = null;
            #endif
            
            #if DEBUG
            Object.DestroyImmediate(text);
            text = null;
            #endif

            return true;
        }
    }
}