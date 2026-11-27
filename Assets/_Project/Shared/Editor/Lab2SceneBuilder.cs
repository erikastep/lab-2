using Unity.AI.Navigation;
using Unity.Behavior;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace Lab2.EditorTools
{
    /// <summary>
    /// Builds the Lab 2 test scene (Gabriele's part) from primitives, adds the free models
    /// as visual children and bakes the NavMesh. Menu: Lab2 > Build Gabriele Scene.
    /// Running it again rebuilds the scene from scratch.
    /// </summary>
    public static class Lab2SceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Shared/Scenes/Lab2_Gabriele_Test.unity";
        const string MatFolder = "Assets/_Project/Shared/Materials";
        const string ModelFolder = "Assets/_Project/Shared/Models";

        // Layout (metres). Ground is 30 x 30 centred on the origin.
        static readonly Vector3 RoomMin = new Vector3(6f, 0f, 5f);
        static readonly Vector3 RoomMax = new Vector3(13f, 0f, 13f);
        const float WallH = 2f, WallT = 0.3f, DoorW = 1.8f;

        [MenuItem("Lab2/Build Gabriele Scene")]
        public static void Build()
        {
            System.IO.Directory.CreateDirectory("Assets/_Project/Shared/Scenes");
            System.IO.Directory.CreateDirectory(MatFolder);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            cam.transform.position = new Vector3(0f, 32f, -20f);
            cam.transform.rotation = Quaternion.Euler(60f, 0f, 0f);

            // ---------- Environment ----------
            var env = new GameObject("Environment").transform;

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(env);
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            Paint(ground, "Ground", new Color(0.35f, 0.38f, 0.33f));
            var surface = ground.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

            BuildRoom(env, out GameObject doorBlocker, out Vector3 doorCenter);

            // ---------- Points ----------
            var points = new GameObject("Points").transform;
            var doorPoint = Point(points, "DoorPoint", doorCenter + new Vector3(-1.2f, 0f, 0f));
            var guardPoint = Point(points, "PoliceGuardPoint", doorCenter + new Vector3(-2.5f, 0f, -2f));
            var away1 = Point(points, "PoliceAwayPoint1", new Vector3(-12f, 0f, -12f));
            var away2 = Point(points, "PoliceAwayPoint2", new Vector3(-12f, 0f, 12f));

            // ---------- Items ----------
            var items = new GameObject("Items").transform;
            var treasure = Item(items, "Treasure", PrimitiveType.Cube, new Vector3(11f, 0.35f, 9f), 0.6f,
                new Color(1f, 0.85f, 0.1f), "Quaternius/Chest_Gold.fbx", 0.8f);
            var key = Item(items, "Key", PrimitiveType.Sphere, new Vector3(-7f, 0.3f, 6f), 0.5f,
                new Color(1f, 0.5f, 0.05f), null, 1f);
            var tool = Item(items, "Tool", PrimitiveType.Cylinder, new Vector3(2f, 0.4f, -9f), 0.4f,
                new Color(0.55f, 0.55f, 0.55f), "KayKit/hammer_common.fbx", 1.2f);
            tool.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);

            // ---------- Refuges ----------
            var refuges = new GameObject("Refuges").transform;
            var r1 = Refuge(refuges, "Refuge1", new Vector3(-13f, 0.05f, -13f), new Color(0.85f, 0.2f, 0.2f));
            var r2 = Refuge(refuges, "Refuge2", new Vector3(-13f, 0.05f, 13f), new Color(0.2f, 0.75f, 0.3f));
            var r3 = Refuge(refuges, "Refuge3", new Vector3(13f, 0.05f, -13f), new Color(0.6f, 0.3f, 0.85f));

            // ---------- Characters ----------
            var police = Character("Police", guardPoint.position, new Color(0.15f, 0.3f, 0.95f),
                "KayKit/character_knight.fbx", 2.5f);
            var pc = police.AddComponent<PoliceController>();
            pc.guardPoint = guardPoint;
            pc.awayPoints = new[] { away1, away2 };

            var thief = Character("Thief", new Vector3(-3f, 0f, -3f), new Color(0.08f, 0.08f, 0.08f),
                "KayKit/character_rogue.fbx", 1.5f);
            var thiefAgent = thief.AddComponent<BehaviorGraphAgent>();
            // The graph (Erika's FSM + Bianca's Robbery) is assigned after merging.

            // ---------- Test setup ----------
            var setup = new GameObject("TestSetup").AddComponent<TestSetup>();
            setup.key = key;
            setup.tool = tool;
            setup.doorBlocker = doorBlocker;
            setup.treasure = treasure;
            setup.thiefAgent = thiefAgent;

            // ---------- Bake NavMesh (door excluded so it can open at runtime) ----------
            surface.BuildNavMesh();
            var navData = surface.navMeshData;
            string navPath = "Assets/_Project/Shared/Scenes/Lab2_NavMesh.asset";
            AssetDatabase.DeleteAsset(navPath);
            AssetDatabase.CreateAsset(navData, navPath);
            surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[Lab2SceneBuilder] Scene built: " + ScenePath);
        }

        // Batch-mode entry point: Unity -batchmode -executeMethod Lab2.EditorTools.Lab2SceneBuilder.BuildBatch
        public static void BuildBatch()
        {
            Build();
            EditorApplication.Exit(0);
        }

        static void BuildRoom(Transform env, out GameObject doorBlocker, out Vector3 doorCenter)
        {
            var room = new GameObject("TreasureRoom").transform;
            room.SetParent(env);
            var wallCol = new Color(0.55f, 0.5f, 0.45f);

            float midZ = (RoomMin.z + RoomMax.z) * 0.5f;
            float lenX = RoomMax.x - RoomMin.x;
            float lenZ = RoomMax.z - RoomMin.z;

            // North, south, east walls.
            Wall(room, "Wall_North", new Vector3((RoomMin.x + RoomMax.x) * 0.5f, WallH / 2, RoomMax.z), new Vector3(lenX + WallT, WallH, WallT), wallCol);
            Wall(room, "Wall_South", new Vector3((RoomMin.x + RoomMax.x) * 0.5f, WallH / 2, RoomMin.z), new Vector3(lenX + WallT, WallH, WallT), wallCol);
            Wall(room, "Wall_East", new Vector3(RoomMax.x, WallH / 2, midZ), new Vector3(WallT, WallH, lenZ), wallCol);

            // West wall in two parts with a gap for the door.
            float gapLo = midZ - DoorW / 2, gapHi = midZ + DoorW / 2;
            float lowLen = gapLo - RoomMin.z, highLen = RoomMax.z - gapHi;
            Wall(room, "Wall_West_A", new Vector3(RoomMin.x, WallH / 2, RoomMin.z + lowLen / 2), new Vector3(WallT, WallH, lowLen), wallCol);
            Wall(room, "Wall_West_B", new Vector3(RoomMin.x, WallH / 2, gapHi + highLen / 2), new Vector3(WallT, WallH, highLen), wallCol);

            doorCenter = new Vector3(RoomMin.x, 0f, midZ);

            // Door = DoorBlocker. NavMeshObstacle with carving blocks the path while active;
            // deactivating the object opens the path. Excluded from the baked NavMesh.
            doorBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorBlocker.name = "DoorBlocker";
            doorBlocker.transform.SetParent(room);
            doorBlocker.transform.position = doorCenter + new Vector3(0f, WallH / 2, 0f);
            doorBlocker.transform.localScale = new Vector3(WallT * 1.5f, WallH, DoorW);
            Paint(doorBlocker, "Door", new Color(0.45f, 0.25f, 0.1f));
            var obstacle = doorBlocker.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
            var mod = doorBlocker.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
        }

        static GameObject Wall(Transform parent, string name, Vector3 pos, Vector3 size, Color c)
        {
            var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
            w.name = name;
            w.transform.SetParent(parent);
            w.transform.position = pos;
            w.transform.localScale = size;
            Paint(w, "Wall", c);
            var mod = w.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.area = 1; // Not Walkable
            return w;
        }

        static Transform Point(Transform parent, string name, Vector3 pos)
        {
            var p = new GameObject(name).transform;
            p.SetParent(parent);
            p.position = pos;
            return p;
        }

        static GameObject Item(Transform parent, string name, PrimitiveType type, Vector3 pos, float size,
            Color c, string model, float modelScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;
            Paint(go, name, c);
            var col = go.GetComponent<Collider>();
            col.isTrigger = true; // items never block navigation
            var mod = go.AddComponent<NavMeshModifier>();
            mod.ignoreFromBuild = true;
            AttachModel(go, model, modelScale);
            return go;
        }

        static GameObject Refuge(Transform parent, string name, Vector3 pos, Color c)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(2.2f, 0.05f, 2.2f);
            Paint(go, name, c);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Character(string name, Vector3 pos, Color c, string model, float speed)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = pos + Vector3.up;
            Paint(go, name, c);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var agent = go.AddComponent<NavMeshAgent>();
            agent.speed = speed;
            agent.radius = 0.4f;
            agent.height = 2f;
            agent.baseOffset = 1f;
            agent.stoppingDistance = 0.3f;
            agent.angularSpeed = 540f;
            agent.acceleration = 16f;
            AttachModel(go, model, 1f, new Vector3(0f, -1f, 0f));
            return go;
        }

        /// <summary>Adds a model as a child and hides the primitive's renderer (team plan step 3).</summary>
        static void AttachModel(GameObject parent, string model, float scale, Vector3? offset = null)
        {
            if (string.IsNullOrEmpty(model)) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{model}");
            if (prefab == null)
            {
                Debug.LogWarning("[Lab2SceneBuilder] Model not found, keeping primitive: " + model);
                return;
            }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            inst.name = "Model";
            inst.transform.localPosition = offset ?? Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;

            // Keep the model at world size `scale` regardless of the primitive's scale.
            Vector3 ps = parent.transform.lossyScale;
            inst.transform.localScale = new Vector3(scale / ps.x, scale / ps.y, scale / ps.z);

            foreach (var r in inst.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.On;
                FixMaterials(r);
            }
            var own = parent.GetComponent<MeshRenderer>();
            if (own != null) own.enabled = false;
        }

        /// <summary>Imported FBX materials can come in with a non-URP shader (pink). Swap to URP Lit, keep colour.</summary>
        static void FixMaterials(Renderer r)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) return;
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null || m.shader == lit) continue;
                if (m.shader != null && m.shader.name.StartsWith("Universal Render Pipeline")) continue;
                Color col = m.HasProperty("_Color") ? m.color : Color.white;
                Texture tex = m.HasProperty("_MainTex") ? m.mainTexture : null;
                string path = $"{MatFolder}/Model_{Sanitize(m.name)}.mat";
                var nm = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (nm == null)
                {
                    nm = new Material(lit);
                    nm.SetColor("_BaseColor", col);
                    if (tex != null) nm.SetTexture("_BaseMap", tex);
                    AssetDatabase.CreateAsset(nm, path);
                }
                mats[i] = nm;
            }
            r.sharedMaterials = mats;
        }

        static void Paint(GameObject go, string matName, Color c)
        {
            string path = $"{MatFolder}/{Sanitize(matName)}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                mat = new Material(sh);
                AssetDatabase.CreateAsset(mat, path);
            }
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            mat.color = c;
            EditorUtility.SetDirty(mat);
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static string Sanitize(string s)
        {
            foreach (char ch in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(ch, '_');
            return s.Replace(' ', '_');
        }
    }
}
