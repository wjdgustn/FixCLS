using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using GDMiniJSON;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FixCLS.MainPatch {
    #if DEBUG
    public class Text : MonoBehaviour {
        public static string Content = "Wa sans!";
        
        void OnGUI() {
            // if (RDC.debug) Content = SceneManager.GetActiveScene().name;
            // else Content = "";

            GUIStyle style = new GUIStyle();
            style.fontSize = (int) 50.0f;
            style.font = RDString.GetFontDataForLanguage(RDString.language).font;
            style.normal.textColor = Color.white;
    
            GUI.Label(new Rect(10, 250, Screen.width, Screen.height), Content, style);
        }
    }
    #endif

    // [HarmonyPatch(typeof(scnCLS), "MakeFloor")]
    //
    // internal static class GetFloorTIme {
    //     private static Stopwatch Timer = new Stopwatch();
    //
    //     private static void Prefix() {
    //         Timer.Reset();
    //         Timer.Start();
    //     }
    //
    //     private static void Postfix() {
    //         Timer.Stop();
    //         Main.Mod.Logger.Log($"MakeFloor took {Timer.ElapsedMilliseconds}ms");
    //     }
    // }
    
    [HarmonyPatch(typeof(scnCLS), "ScanLevels")]

    internal static class GetScanLevelTime {
        private static Stopwatch Timer = new Stopwatch();

        private static void Prefix() {
            Timer.Reset();
            Timer.Start();
        }

        private static void Postfix() {
            Timer.Stop();
            Main.Mod.Logger.Log($"ScanLevels took {Timer.ElapsedMilliseconds}ms");
        }
    }

    [HarmonyPatch(typeof(scnCLS), "LoadSong")]

    internal static class FixMusicPreview {
        private static void Postfix(scnCLS __instance) {
            var lastSongsLoaded =
                (List<string>) AccessTools.Field(__instance.GetType(), "lastSongsLoaded").GetValue(__instance);

            while (lastSongsLoaded.Count > 0) {
                string key = lastSongsLoaded[0];
                lastSongsLoaded.RemoveAt(0);
                if (ADOBase.audioManager.audioLib.ContainsKey(key))
                {
                    AudioClip audioClip = ADOBase.audioManager.audioLib[key];
                    ADOBase.audioManager.audioLib.Remove(key);
                    audioClip.UnloadAudioData();
                    Object.DestroyImmediate(audioClip, true);
                    Resources.UnloadAsset(audioClip);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Json), "Deserialize")]
    
    internal static class OptimizeCLSLoad {
        private static bool AllowPass;
        
        private static bool Prefix(ref object __result, string json) {
            if ((GCS.sceneToLoad == null || !GCS.sceneToLoad.Contains("scnCLS")) && !SceneManager.GetActiveScene().name.Contains("scnCLS")) return true;

            if (AllowPass) {
                AllowPass = false;
                return true;
            }
    
            AllowPass = true;

            if (json == null) {
                __result = null;
                return false;
            }

            // remove actions
            var result = Regex.Replace(json, @"(?<=\t\[).+(?=\])", "", RegexOptions.Singleline);
            // remove pathData
            result = Regex.Replace(result, "(?<=\"pathData\": \").+(?=\")", "");
            // remove angleData
            result = Regex.Replace(result, "(?<=\"angleData\": \\[).+(?=\\])", "");
            __result = Json.Deserialize(result);

            return false;
        }
    }

    #if DEBUG
    [HarmonyPatch(typeof(scnCLS), "Update")]

    internal static class Debug {
        private static void Postfix(scnCLS __instance) {
            if (Input.GetKey(KeyCode.LeftControl)) {
                if (Input.GetKeyDown(KeyCode.R)) __instance.Refresh();
                if (Input.GetKeyDown(KeyCode.Q)) SceneManager.LoadScene("scnNewIntro");
            }
        }
    }
    #endif
}