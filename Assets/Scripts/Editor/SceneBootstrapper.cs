using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using CoinPusher.Core;
using CoinPusher.Gameplay;
using CoinPusher.UI;

namespace CoinPusher.EditorTools
{
    public static class SceneBootstrapper
    {
        const string RootName = "CoinPusher_Root";
        const string ConfigPath = "Assets/Configs/GameConfig.asset";
        const string PrefabFolder = "Assets/Prefabs";
        const string PrefabPath = PrefabFolder + "/Coin.prefab";
        const string SceneFolder = "Assets/Scenes";
        const string ScenePath = SceneFolder + "/Task001_BasicPusher.unity";

        [MenuItem("Coin Pusher/Setup Task 001 Scene")]
        public static void SetupScene()
        {
            var existingRoot = GameObject.Find(RootName);
            if (existingRoot != null) Object.DestroyImmediate(existingRoot);

            var config = LoadOrCreateConfig();
            var coinPrefab = LoadOrCreateCoinPrefab(config);

            var root = new GameObject(RootName);

            BuildCabinet(root.transform, out float halfWidth, out float length);
            BuildPusher(root.transform, config, halfWidth);
            BuildDropPoint(root.transform, config, coinPrefab, length);
            BuildFallDetector(root.transform, halfWidth, length);
            BuildGameManager(root.transform, config);
            BuildCanvas(root.transform);

            EnsureFolder(SceneFolder);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Coin Pusher Task 001 scene setup complete: " + ScenePath);
        }

        static GameConfig LoadOrCreateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config != null) return config;

            EnsureFolder("Assets/Configs");
            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        static GameObject LoadOrCreateCoinPrefab(GameConfig config)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null) return existing;

            EnsureFolder(PrefabFolder);

            var coinObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coinObj.name = "Coin";
            coinObj.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);

            Object.DestroyImmediate(coinObj.GetComponent<CapsuleCollider>());
            var meshCollider = coinObj.AddComponent<MeshCollider>();
            meshCollider.convex = true;

            var rb = coinObj.AddComponent<Rigidbody>();
            rb.mass = config.coinMass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            coinObj.AddComponent<Coin>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(coinObj, PrefabPath);
            Object.DestroyImmediate(coinObj);
            return prefab;
        }

        static void BuildCabinet(Transform parent, out float halfWidth, out float length)
        {
            halfWidth = 2.5f;
            length = 3.5f;
            float wallHeight = 1.2f;
            float wallThickness = 0.2f;

            var cabinet = new GameObject("Cabinet").transform;
            cabinet.SetParent(parent);

            CreateBlock("Floor", cabinet, new Vector3(0, -0.1f, length * 0.5f),
                new Vector3(halfWidth * 2f, 0.2f, length));

            CreateBlock("LeftWall", cabinet, new Vector3(-halfWidth - wallThickness * 0.5f, wallHeight * 0.5f, length * 0.5f),
                new Vector3(wallThickness, wallHeight, length));

            CreateBlock("RightWall", cabinet, new Vector3(halfWidth + wallThickness * 0.5f, wallHeight * 0.5f, length * 0.5f),
                new Vector3(wallThickness, wallHeight, length));

            CreateBlock("BackWall", cabinet, new Vector3(0, wallHeight * 0.5f, -wallThickness * 0.5f),
                new Vector3(halfWidth * 2f + wallThickness * 2f, wallHeight, wallThickness));
        }

        static void CreateBlock(string name, Transform parent, Vector3 localPos, Vector3 scale)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetParent(parent);
            block.transform.localPosition = localPos;
            block.transform.localScale = scale;
        }

        static void BuildPusher(Transform parent, GameConfig config, float halfWidth)
        {
            var pusherObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pusherObj.name = "Pusher";
            pusherObj.transform.SetParent(parent);
            pusherObj.transform.localPosition = new Vector3(0f, 0.25f, 0.6f);
            pusherObj.transform.localScale = new Vector3(halfWidth * 2f - 0.2f, 0.5f, 0.3f);

            var rb = pusherObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var controller = pusherObj.AddComponent<PusherController>();
            var so = new SerializedObject(controller);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("pushDirection").vector3Value = Vector3.forward;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildDropPoint(Transform parent, GameConfig config, GameObject coinPrefab, float length)
        {
            var dropObj = new GameObject("DropPoint");
            dropObj.transform.SetParent(parent);
            dropObj.transform.localPosition = new Vector3(0f, config.dropHeight, length * 0.4f);

            var controller = dropObj.AddComponent<CoinDropController>();
            var so = new SerializedObject(controller);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildFallDetector(Transform parent, float halfWidth, float length)
        {
            var detectorObj = new GameObject("FallDetector");
            detectorObj.transform.SetParent(parent);
            detectorObj.transform.localPosition = new Vector3(0f, -1.5f, length + 0.75f);

            var col = detectorObj.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(halfWidth * 2f + 1f, 3f, 1.5f);

            detectorObj.AddComponent<FallDetector>();
        }

        static void BuildGameManager(Transform parent, GameConfig config)
        {
            var gmObj = new GameObject("GameManager");
            gmObj.transform.SetParent(parent);

            var gm = gmObj.AddComponent<GameManager>();
            var so = new SerializedObject(gm);
            so.FindProperty("config").objectReferenceValue = config;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildCanvas(Transform parent)
        {
            var canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(parent);

            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var scoreText = CreateText(canvasObj.transform, "ScoreText", font, 36, TextAnchor.UpperLeft,
                Color.white, "000000", new Vector2(20f, -20f), new Vector2(300f, 60f));

            var popupText = CreateText(canvasObj.transform, "PopupText", font, 28, TextAnchor.UpperLeft,
                Color.yellow, "+10", new Vector2(20f, -70f), new Vector2(300f, 40f));
            popupText.enabled = false;

            var scoreUI = canvasObj.AddComponent<ScoreUI>();
            var so = new SerializedObject(scoreUI);
            so.FindProperty("scoreText").objectReferenceValue = scoreText;
            so.FindProperty("popupText").objectReferenceValue = popupText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Text CreateText(Transform parent, string name, Font font, int size, TextAnchor anchor,
            Color color, string initial, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);

            var text = obj.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.text = initial;

            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            return text;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
