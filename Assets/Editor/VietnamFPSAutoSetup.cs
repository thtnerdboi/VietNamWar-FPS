#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.AI.Navigation; // NavMeshSurface / CollectObjects

public class VietnamFPSAutoSetup
{
    [MenuItem("Tools/Vietnam FPS/Create Level01 (Auto-Setup)")]
    public static void CreateLevel()
    {
        // New scene, remove default Main Camera
        UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
        var defaultCam = GameObject.Find("Main Camera");
        if (defaultCam) Object.DestroyImmediate(defaultCam);

        var envGO     = new GameObject("Environment");
        var enemiesGO = new GameObject("Enemies");

        // ---------- Ground ----------
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(envGO.transform);
        ground.transform.localScale = new Vector3(8, 1, 8); // 80x80 m

        var mr = ground.GetComponent<MeshRenderer>();
        var shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        var jungle = new Color(0.13f, 0.25f, 0.12f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", jungle);
        if (mat.HasProperty("_Color"))     mat.SetColor("_Color", jungle);
        mr.sharedMaterial = mat;

        // Some cover
        for (int i = 0; i < 10; i++)
        {
            var cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.transform.SetParent(envGO.transform);
            cover.transform.localScale = new Vector3(2, 1, 0.5f);
            cover.transform.position   = new Vector3(Random.Range(-30, 30), 0.5f, Random.Range(-30, 30));
            cover.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // ---------- NavMesh ----------
        var surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        // ---------- Player (start in air; first-person always) ----------
        var gm = new GameObject("GameManager").AddComponent<GameManager>();

        Vector3 dropGroundPos = new Vector3(0, 1f, -12f);
        float   dropHeight    = 18f;
        Vector3 dropStartPos  = dropGroundPos + Vector3.up * dropHeight;

        var player = new GameObject("Player") { tag = "Player" };
        player.transform.position = dropStartPos;
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.center = new Vector3(0, 0.9f, 0);
        player.AddComponent<Health>();
        player.AddComponent<PlayerMovement>();
        var look = player.AddComponent<PlayerLook>();

        var camPivot = new GameObject("CameraPivot").transform;
        camPivot.SetParent(player.transform);
        camPivot.localPosition = new Vector3(0, 1.6f, 0);
        look.cameraPivot = camPivot;

        var camGO = new GameObject("MainCamera") { tag = "MainCamera" };
        var cam   = camGO.AddComponent<Camera>();
        camGO.transform.SetParent(camPivot);
        camGO.transform.localPosition = Vector3.zero;
        camGO.transform.localRotation = Quaternion.identity;

        var gunGO = new GameObject("Rifle");
        gunGO.transform.SetParent(camGO.transform);
        gunGO.transform.localPosition = new Vector3(0.2f, -0.15f, 0.4f);
        var gun = gunGO.AddComponent<Gun>();
        gun.cam = cam;
        gun.hitMask = ~0;
        gun.audioSrc = camGO.AddComponent<AudioSource>();

        // ---------- Enemies ----------
        for (int i = 0; i < 5; i++)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = $"Enemy_{i}";
            enemy.transform.SetParent(enemiesGO.transform);
            enemy.transform.position = new Vector3(Random.Range(-20, 20), 1, Random.Range(0, 30));

            Object.DestroyImmediate(enemy.GetComponent<Collider>());
            var col = enemy.AddComponent<CapsuleCollider>(); col.height = 2f; col.center = new Vector3(0, 1f, 0);

            enemy.AddComponent<Health>().destroyOnDeath = true;

            var agent = enemy.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f; agent.angularSpeed = 300; agent.acceleration = 12;

            var p0 = new GameObject(enemy.name + "_P0"); p0.transform.position = enemy.transform.position + new Vector3(Random.Range(-8, 8), 0, Random.Range(5, 12));
            var p1 = new GameObject(enemy.name + "_P1"); p1.transform.position = enemy.transform.position + new Vector3(Random.Range(-8, 8), 0, Random.Range(5, 12));
            p0.transform.SetParent(enemy.transform); p1.transform.SetParent(enemy.transform);

            var ai = enemy.AddComponent<EnemyAI>();
            ai.patrolPoints = new Transform[] { p0.transform, p1.transform };
        }

        // ---------- Helicopter + rope (non-blocking) ----------
        var helo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        helo.name = "Helicopter";
        helo.transform.localScale = new Vector3(3f, 1.2f, 1.2f);
        helo.transform.position   = dropStartPos + new Vector3(-12f, 3f, -8f);
        var heloCol = helo.GetComponent<Collider>(); if (heloCol) heloCol.isTrigger = true;
        helo.AddComponent<AudioSource>(); // optional: assign rotor SFX later

        var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rope.name = "Rope";
        rope.transform.localScale = new Vector3(0.05f, dropHeight * 0.5f, 0.05f);
        rope.transform.position   = dropStartPos - new Vector3(0, dropHeight * 0.5f, 0);
        rope.GetComponent<MeshRenderer>().sharedMaterial = mat;
        var ropeCol = rope.GetComponent<Collider>(); if (ropeCol) ropeCol.isTrigger = true;

        // ---------- Timeline: player drop + helo fly ----------
        System.IO.Directory.CreateDirectory("Assets/Timeline");
        var directorGO = new GameObject("FP_DropCutscene");
        var director   = directorGO.AddComponent<PlayableDirector>();
        var timeline   = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(timeline, "Assets/Timeline/FP_Drop.playable");
        AssetDatabase.SaveAssets();
        director.playableAsset = timeline;

        // Player drop anim
        float dur = 5.0f;
        var dropClip = new AnimationClip { name = "Player_Drop" };
        var px = AnimationCurve.Linear(0, dropStartPos.x, dur, dropGroundPos.x);
        var py = AnimationCurve.Linear(0, dropStartPos.y, dur, dropGroundPos.y);
        var pz = AnimationCurve.Linear(0, dropStartPos.z, dur, dropGroundPos.z);
        dropClip.SetCurve("", typeof(Transform), "localPosition.x", px);
        dropClip.SetCurve("", typeof(Transform), "localPosition.y", py);
        dropClip.SetCurve("", typeof(Transform), "localPosition.z", pz);
        AssetDatabase.CreateAsset(dropClip, "Assets/Timeline/Player_Drop.anim");
        AssetDatabase.SaveAssets();
        var playerTrack = timeline.CreateTrack<AnimationTrack>(null, "Player Drop");
        var playerTClip = playerTrack.CreateClip(dropClip); playerTClip.start = 0; playerTClip.duration = dur;
        director.SetGenericBinding(playerTrack, player.transform);

        // Helo fly anim
        var heloClip = new AnimationClip { name = "Helo_Fly" };
        Vector3 heloStart = helo.transform.position;
        Vector3 heloEnd   = heloStart + new Vector3(30f, 5f, 25f);
        var hx = AnimationCurve.Linear(0, heloStart.x, dur + 2f, heloEnd.x);
        var hy = AnimationCurve.Linear(0, heloStart.y, dur + 2f, heloEnd.y);
        var hz = AnimationCurve.Linear(0, heloStart.z, dur + 2f, heloEnd.z);
        heloClip.SetCurve("", typeof(Transform), "localPosition.x", hx);
        heloClip.SetCurve("", typeof(Transform), "localPosition.y", hy);
        heloClip.SetCurve("", typeof(Transform), "localPosition.z", hz);
        AssetDatabase.CreateAsset(heloClip, "Assets/Timeline/Helo_Fly.anim");
        AssetDatabase.SaveAssets();
        var heloTrack = timeline.CreateTrack<AnimationTrack>(null, "Helicopter Fly");
        var heloTClip = heloTrack.CreateClip(heloClip); heloTClip.start = 0; heloTClip.duration = dur + 2f;
        director.SetGenericBinding(heloTrack, helo.transform);

        // Starter that actually moves the player & toggles controls
        var starter = directorGO.AddComponent<OpeningCutsceneStarter>();
        starter.director    = director;
        starter.player      = player.transform;
        starter.dropEnd     = dropGroundPos;
        starter.dropSeconds = dur;

        // ---------- Save scene ----------
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(),
            "Assets/Scenes/Level01.unity");

        AssetDatabase.SaveAssets();
        Debug.Log("Level01 created with FP helo drop. Press Play. Controls: WASD, Mouse, Space, Shift, LMB, R.");
    }
}
#endif
