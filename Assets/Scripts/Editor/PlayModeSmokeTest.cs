using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoinPusher.Core;
using CoinPusher.Gameplay;

namespace CoinPusher.EditorTools
{
    public static class PlayModeSmokeTest
    {
        const string ScenePath = "Assets/Scenes/Task001_BasicPusher.unity";
        const string CoinPrefabPath = "Assets/Prefabs/Coin.prefab";
        const string RunningKey = "CoinPusher.SmokeTest.Running";
        const string StartTicksKey = "CoinPusher.SmokeTest.StartTicks";
        const string DroppedKey = "CoinPusher.SmokeTest.Dropped";
        const float TestDurationSeconds = 25f;
        const float DropIntervalSeconds = 1f;
        const int CoinsToDrop = 6;
        const float HardTimeoutSeconds = 60f;
        const float ProgressLogIntervalSeconds = 3f;

        static float _playStartRealtime = -1f;
        static float _lastProgressLog = -999f;
        static bool _exiting;

        [MenuItem("Coin Pusher/Run Play Mode Smoke Test")]
        public static void Run()
        {
            SessionState.SetBool(RunningKey, true);
            SessionState.SetString(StartTicksKey, DateTime.UtcNow.Ticks.ToString());
            SessionState.SetInt(DroppedKey, 0);
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        static void Hook()
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (_exiting) return;

            long startTicks = long.Parse(SessionState.GetString(StartTicksKey, "0"));
            double elapsedWall = (DateTime.UtcNow - new DateTime(startTicks)).TotalSeconds;
            if (elapsedWall > HardTimeoutSeconds)
            {
                Finish(1, "Hard timeout exceeded without completing.");
                return;
            }

            if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;

            if (_playStartRealtime < 0f) _playStartRealtime = Time.realtimeSinceStartup;
            float playElapsed = Time.realtimeSinceStartup - _playStartRealtime;

            int dropped = SessionState.GetInt(DroppedKey, 0);
            if (dropped < CoinsToDrop && playElapsed >= dropped * DropIntervalSeconds)
            {
                if (!SpawnCoin())
                {
                    Finish(1, "Failed to spawn coin (missing prefab/DropPoint/config in scene).");
                    return;
                }
                SessionState.SetInt(DroppedKey, dropped + 1);
            }

            if (playElapsed - _lastProgressLog >= ProgressLogIntervalSeconds)
            {
                _lastProgressLog = playElapsed;
                LogProgress(playElapsed);
            }

            if (playElapsed >= TestDurationSeconds)
            {
                EvaluateAndFinish();
            }
        }

        static bool SpawnCoin()
        {
            var dropController = UnityEngine.Object.FindFirstObjectByType<CoinDropController>();
            var config = GameManager.Instance != null ? GameManager.Instance.Config : null;
            var coinPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoinPrefabPath);
            if (dropController == null || config == null || coinPrefab == null) return false;

            var coinObj = UnityEngine.Object.Instantiate(coinPrefab, dropController.transform.position, Quaternion.identity);
            coinObj.GetComponent<Coin>()?.Configure(config);
            return true;
        }

        static void LogProgress(float playElapsed)
        {
            var coins = UnityEngine.Object.FindObjectsByType<Coin>(FindObjectsSortMode.None);
            float minZ = float.MaxValue, maxZ = float.MinValue, minY = float.MaxValue;
            foreach (var c in coins)
            {
                var p = c.transform.position;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
                if (p.y < minY) minY = p.y;
            }
            int score = GameManager.Instance != null ? GameManager.Instance.Score.CurrentScore : -1;
            Debug.Log($"[SmokeTest] t={playElapsed:F1}s score={score} coins={coins.Length} zRange=[{minZ:F2},{maxZ:F2}] minY={minY:F2}");
        }

        static void EvaluateAndFinish()
        {
            if (GameManager.Instance == null)
            {
                Finish(1, "GameManager.Instance was null during play.");
                return;
            }

            int score = GameManager.Instance.Score.CurrentScore;
            int coinsInScene = UnityEngine.Object.FindObjectsByType<Coin>(FindObjectsSortMode.None).Length;
            Debug.Log($"[SmokeTest] RESULT score={score} coinsDropped={CoinsToDrop} coinsStillInScene={coinsInScene}");
            Finish(0, "Completed normally.");
        }

        static void Finish(int exitCode, string message)
        {
            _exiting = true;
            EditorApplication.update -= Tick;
            SessionState.SetBool(RunningKey, false);
            Debug.Log("[SmokeTest] " + message);
            EditorApplication.Exit(exitCode);
        }
    }
}
