using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TitanMusic
    {
        private static AudioSource _audioSource;

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "Start")]
        private static void AdventureController_Start_postfix(AdventureController __instance)
        {
            _audioSource = __instance.gameObject.AddComponent<AudioSource>();
            _audioSource.loop = true;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                if (_audioSource != null && _audioSource.isPlaying)
                    _audioSource.Stop();
            };
        }

        [HarmonyPostfix, HarmonyPatch(typeof(AdventureController), "spawnEnemy")]
        private static void AdventureController_spawnEnemy_postfix(int zone, Enemy __result, AdventureController __instance)
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();

            var t = GameData.Zones.TitanZoneIds.IndexOf(zone);
            if (t == -1)
                return;

            var et = __result.enemyType;
            var isGuardian = et == enemyType.guardian
                || et == enemyType.boss7Guardian
                || et == enemyType.boss8Guardian
                || et == enemyType.boss9Guardian;

            __instance.StartCoroutine(playTitanAudio(t + 1, isGuardian));
        }

        [HarmonyPostfix,
            HarmonyPatch(typeof(AdventureController), "enemyDeath"),
            HarmonyPatch(typeof(AdventureController), "playerDeath"),
            HarmonyPatch(typeof(ZoneSelector), "changeZone")]
        private static void AdventureController_enemyDeath_postfix()
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }

        private static IEnumerator playTitanAudio(int titan, bool isGuardian)
        {
            var dir = new DirectoryInfo(Paths.ConfigPath);
            var files = dir.GetFiles();
            var file = files.FirstOrDefault(f => Regex.IsMatch(f.Name, $"^\\([Tt]{titan}\\)"));

            if (isGuardian)
            {
                var guardian = files.FirstOrDefault(f => Regex.IsMatch(f.Name, $"^\\([Tt]{titan}[Gg]\\)"));
                if (guardian != null)
                    file = guardian;
            }

            if (file == null)
                yield break;

            Plugin.ShowNotification($"playing <color=blue><b>{file.Name}</b></color>");
            using (var www = UnityWebRequestMultimedia.GetAudioClip($"file://{file.FullName}", AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                    yield break;

                var audioHandler = www.downloadHandler as DownloadHandlerAudioClip;
                if (audioHandler == null)
                    yield break;

                _audioSource.clip = audioHandler.audioClip;
            }

            _audioSource.Play();
        }
    }
}
