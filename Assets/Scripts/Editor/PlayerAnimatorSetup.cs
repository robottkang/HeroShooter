// Tools > Setup Player Animator
//   AnimatorController 생성 + Stage1 씬 Player 구성
//   (임포트 설정은 이미 Unity 에디터에서 완료된 것으로 간주)
//
// Tools > Player/Configure Animation Rigs  ← 자동 Humanoid 변환이 필요할 때만 별도 실행

using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;

public static class PlayerAnimatorSetup
{
    const string AnimPack      = "Assets/Animations/Rifle 8-Way Locomotion Pack";
    const string ControllerOut = "Assets/Animations/PlayerAnimatorController.controller";
    const string CharModel     = "Assets/Arts/Ch36_nonPBR.fbx";

    // ─────────────────────────────────────────────────────────────────────────
    // 메인 툴: 임포트 설정을 건드리지 않고 컨트롤러 + 씬만 구성
    [MenuItem("Tools/Setup Player Animator")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[PlayerAnimatorSetup] Play Mode 중에는 실행할 수 없습니다.");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Player Animator Setup", "AnimatorController 생성 중...", 0.3f);
            var ctrl = BuildController();

            EditorUtility.DisplayProgressBar("Player Animator Setup", "씬 Player 구성 중...", 0.7f);
            SetupScene(ctrl);

            AssetDatabase.SaveAssets();
            Debug.Log("[PlayerAnimatorSetup] 완료.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // 별도 툴: FBX 임포트를 자동으로 Humanoid + Loop 설정이 필요할 때만 사용
    // 수동으로 임포트 설정을 완료한 경우 실행하지 않아도 됨
    [MenuItem("Tools/Player/Configure Animation Rigs")]
    public static void RunConfigureRigs()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("[PlayerAnimatorSetup] Play Mode 중에는 실행할 수 없습니다.");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Configure Rigs", "Humanoid + Loop 설정 중...", 0f);
            ConfigureRigs();
            AssetDatabase.Refresh();
            Debug.Log("[PlayerAnimatorSetup] 릭 설정 완료.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    // ─── 1. Rig Configuration ─────────────────────────────────────────────────

    static void ConfigureRigs()
    {
        // 캐릭터 모델: Humanoid + 이 모델에서 Avatar 생성
        SetHumanoid(CharModel, createAvatar: true);

        // 애니메이션 FBX: Humanoid (리타게팅 가능하게)
        string sysDir = Path.Combine(Application.dataPath,
            "Animations", "Rifle 8-Way Locomotion Pack");

        string[] files = Directory.GetFiles(sysDir, "*.fbx");

        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                string rel = "Assets" + files[i].Substring(Application.dataPath.Length)
                                                 .Replace('\\', '/');
                ConfigureAnimFbx(rel);
                EditorUtility.DisplayProgressBar("Player Animator Setup",
                    $"릭 + 루프 설정 중... ({i + 1}/{files.Length})",
                    0.6f * (i + 1) / files.Length);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.Refresh();
    }

    // 애니메이션 FBX: Humanoid 릭 + Loop Time을 한 번에 설정
    static void ConfigureAnimFbx(string path)
    {
        if (!(AssetImporter.GetAtPath(path) is ModelImporter imp)) return;

        string baseName  = Path.GetFileNameWithoutExtension(path).ToLower();
        // 루프해야 할 클립: idle, walk, run, sprint, crouch 이동, jump loop, turn
        // 루프 불필요: jump up, jump down, death
        bool shouldLoop = !baseName.Contains("jump up")
                       && !baseName.Contains("jump down")
                       && !baseName.StartsWith("death");

        bool changed = false;

        if (imp.animationType != ModelImporterAnimationType.Human)
        {
            imp.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        // clipAnimations가 비어 있으면 defaultClipAnimations(FBX 내 Take)를 가져와 명시 설정
        var clips = imp.clipAnimations.Length > 0
            ? imp.clipAnimations
            : imp.defaultClipAnimations;

        bool loopDirty = false;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].loopTime != shouldLoop || clips[i].loopPose != shouldLoop)
            {
                loopDirty = true;
                break;
            }
        }

        if (loopDirty)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = shouldLoop;
                clips[i].loopPose = shouldLoop;
            }
            imp.clipAnimations = clips;
            changed = true;
        }

        if (changed) imp.SaveAndReimport();
    }

    static void SetHumanoid(string path, bool createAvatar)
    {
        if (!(AssetImporter.GetAtPath(path) is ModelImporter imp)) return;

        bool alreadyHuman  = imp.animationType == ModelImporterAnimationType.Human;
        bool avatarCorrect = !createAvatar ||
                             imp.avatarSetup == ModelImporterAvatarSetup.CreateFromThisModel;
        if (alreadyHuman && avatarCorrect) return;

        imp.animationType = ModelImporterAnimationType.Human;
        if (createAvatar)
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

        imp.SaveAndReimport();
    }

    // ─── 2. AnimatorController ────────────────────────────────────────────────

    static AnimatorController BuildController()
    {
        AssetDatabase.DeleteAsset(ControllerOut);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ControllerOut);

        ctrl.AddParameter("VelocityX",   AnimatorControllerParameterType.Float);
        ctrl.AddParameter("VelocityY",   AnimatorControllerParameterType.Float);
        ctrl.AddParameter("IsCrouching", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsGrounded",  AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("JumpTrigger", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("IsDead",      AnimatorControllerParameterType.Bool);

        var sm = ctrl.layers[0].stateMachine;
        sm.entryPosition    = new Vector3(-450, 0);
        sm.anyStatePosition = new Vector3(-450, 80);
        sm.exitPosition     = new Vector3(-450, -80);

        // ── States ──────────────────────────────────────────────────────────
        var standState  = sm.AddState("StandLocomotion",  new Vector3(0,   0));
        var crouchState = sm.AddState("CrouchLocomotion", new Vector3(0, 130));
        var jumpUpState = sm.AddState("JumpUp",           new Vector3(300,  0));
        var loopState   = sm.AddState("JumpLoop",         new Vector3(600,  0));
        var deadState   = sm.AddState("Dead",             new Vector3(0, -200));

        sm.defaultState = standState;

        // ── Motions ─────────────────────────────────────────────────────────
        standState.motion  = BuildStandTree(ctrl);
        crouchState.motion = BuildCrouchTree(ctrl);
        jumpUpState.motion = GetClip("jump up.fbx");
        loopState.motion   = GetClip("jump loop.fbx");
        deadState.motion   = GetClip("death from the front.fbx");

        // ── Transitions ──────────────────────────────────────────────────────

        // Stand <-> Crouch
        MakeTrans(standState, crouchState, 0.15f)
            .AddCondition(AnimatorConditionMode.If,    0, "IsCrouching");
        MakeTrans(crouchState, standState, 0.15f)
            .AddCondition(AnimatorConditionMode.IfNot, 0, "IsCrouching");

        // Stand/Crouch → JumpUp (JumpTrigger)
        MakeTrans(standState,  jumpUpState, 0.05f, hasExit: false)
            .AddCondition(AnimatorConditionMode.If, 0, "JumpTrigger");
        MakeTrans(crouchState, jumpUpState, 0.05f, hasExit: false)
            .AddCondition(AnimatorConditionMode.If, 0, "JumpTrigger");

        // JumpUp → StandLocomotion (착지 우선: JumpLoop보다 먼저 평가)
        MakeTrans(jumpUpState, standState, 0.15f, hasExit: false)
            .AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        // JumpUp → JumpLoop (공중 유지 시만: 85% 재생 후, 미착지 조건)
        var tUp = MakeTrans(jumpUpState, loopState, 0.1f);
        tUp.hasExitTime = true;
        tUp.exitTime    = 0.85f;
        tUp.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");

        // JumpLoop → StandLocomotion (착지)
        MakeTrans(loopState, standState, 0.15f)
            .AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");

        // Any → Dead
        var tDead = sm.AddAnyStateTransition(deadState);
        tDead.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
        tDead.duration            = 0.2f;
        tDead.canTransitionToSelf = false;

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // walkSpeed=5, runSpeed=9 기준 속도를 블렌드 트리 좌표로 그대로 사용
    // VelocityX/Y = CharacterController.velocity를 로컬 공간으로 변환한 값
    static BlendTree BuildStandTree(AnimatorController ctrl)
    {
        var t = NewTree(ctrl, "StandLocomotion");
        AddMotions(t,
            // idle
            (   0,      0,    "idle.fbx"),
            // walk (5 m/s)
            (   0,      5f,   "walk forward.fbx"),
            (   0,     -5f,   "walk backward.fbx"),
            (  -5f,     0,    "walk left.fbx"),
            (   5f,     0,    "walk right.fbx"),
            (  -3.54f,  3.54f,"walk forward left.fbx"),
            (   3.54f,  3.54f,"walk forward right.fbx"),
            (  -3.54f, -3.54f,"walk backward left.fbx"),
            (   3.54f, -3.54f,"walk backward right.fbx"),
            // run (9 m/s)
            (   0,      9f,   "run forward.fbx"),
            (   0,     -9f,   "run backward.fbx"),
            (  -9f,     0,    "run left.fbx"),
            (   9f,     0,    "run right.fbx"),
            (  -6.36f,  6.36f,"run forward left.fbx"),
            (   6.36f,  6.36f,"run forward right.fbx"),
            (  -6.36f, -6.36f,"run backward left.fbx"),
            (   6.36f, -6.36f,"run backward right.fbx")
        );
        return t;
    }

    // crouchSpeed=2.5 기준
    static BlendTree BuildCrouchTree(AnimatorController ctrl)
    {
        var t = NewTree(ctrl, "CrouchLocomotion");
        AddMotions(t,
            (   0,     0,     "idle crouching.fbx"),
            (   0,     2.5f,  "walk crouching forward.fbx"),
            (   0,    -2.5f,  "walk crouching backward.fbx"),
            (  -2.5f,  0,     "walk crouching left.fbx"),
            (   2.5f,  0,     "walk crouching right.fbx"),
            (  -1.77f, 1.77f, "walk crouching forward left.fbx"),
            (   1.77f, 1.77f, "walk crouching forward right.fbx"),
            (  -1.77f,-1.77f, "walk crouching backward left.fbx"),
            (   1.77f,-1.77f, "walk crouching backward right.fbx")
        );
        return t;
    }

    static BlendTree NewTree(AnimatorController ctrl, string name)
    {
        var tree = new BlendTree { name = name };
        AssetDatabase.AddObjectToAsset(tree, ctrl);
        tree.blendType              = BlendTreeType.FreeformCartesian2D;
        tree.blendParameter         = "VelocityX";
        tree.blendParameterY        = "VelocityY";
        tree.useAutomaticThresholds = false;
        return tree;
    }

    static void AddMotions(BlendTree tree, params (float x, float y, string file)[] entries)
    {
        foreach (var (x, y, file) in entries)
        {
            var clip = GetClip(file);
            if (clip != null) tree.AddChild(clip, new Vector2(x, y));
        }
    }

    static AnimationClip GetClip(string fileName)
    {
        string path = $"{AnimPack}/{fileName}";

        AnimationClip TryLoad(string p)
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(p))
            {
                if (obj is AnimationClip c && !c.name.StartsWith("__preview__"))
                    return c;
            }
            return null;
        }

        var clip = TryLoad(path);
        if (clip != null) return clip;

        // 에셋이 아직 갱신되지 않았을 때 강제 재임포트 후 재시도
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        clip = TryLoad(path);
        if (clip != null) return clip;

        Debug.LogWarning($"[PlayerAnimatorSetup] 클립을 찾을 수 없음: {path}");
        return null;
    }

    static AnimatorStateTransition MakeTrans(AnimatorState from, AnimatorState to,
        float duration, bool hasExit = false)
    {
        var t = from.AddTransition(to);
        t.duration    = duration;
        t.hasExitTime = hasExit;
        return t;
    }

    // ─── 3. Scene Setup ───────────────────────────────────────────────────────

    static void SetupScene(AnimatorController ctrl)
    {
        var playerGO = GameObject.Find("Player") ?? GameObject.Find("Player_Temp");
        if (playerGO == null)
        {
            Debug.LogError("[PlayerAnimatorSetup] 씬에서 Player 또는 Player_Temp를 찾을 수 없습니다.");
            return;
        }

        playerGO.name = "Player";

        // 루트에 있던 Animator, PlayerAnimatorController 제거
        var rootAnim = playerGO.GetComponent<Animator>();
        if (rootAnim != null) Object.DestroyImmediate(rootAnim);
        var rootPac = playerGO.GetComponent<PlayerAnimatorController>();
        if (rootPac != null) Object.DestroyImmediate(rootPac);

        // 기존 PlayerBody 제거 후 재생성
        var existing = playerGO.transform.Find("PlayerBody");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharModel);
        if (charPrefab == null)
        {
            Debug.LogError("[PlayerAnimatorSetup] 캐릭터 모델을 찾을 수 없음: " + CharModel);
            return;
        }

        // Ch36_nonPBR를 PlayerBody로 인스턴스화
        var bodyGO = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab, playerGO.transform);
        bodyGO.name = "PlayerBody";
        bodyGO.transform.localPosition = Vector3.zero;
        bodyGO.transform.localRotation = Quaternion.identity;
        bodyGO.transform.localScale    = Vector3.one;

        // PlayerBody에 Animator 설정
        var anim = bodyGO.GetComponent<Animator>();
        if (anim == null) anim = bodyGO.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        anim.applyRootMotion            = false;

        // PlayerBody에 PlayerAnimatorController 추가 및 fpc 연결
        var pac = bodyGO.GetComponent<PlayerAnimatorController>();
        if (pac == null) pac = bodyGO.AddComponent<PlayerAnimatorController>();
        var so = new SerializedObject(pac);
        so.FindProperty("fpc").objectReferenceValue =
            playerGO.GetComponent<FirstPersonController>();
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(playerGO.scene);
        Debug.Log($"[PlayerAnimatorSetup] '{playerGO.name}' 구성 완료 — PlayerBody 추가됨.");
    }
}
