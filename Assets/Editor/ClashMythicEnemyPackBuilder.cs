using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ClashMythicEnemyPackBuilder
{
    private const string SourceRoot = "Assets/Tiny Swords - Enemy Pack/Enemies";
    private const string OutputRoot = "Assets/Prefabs/Units/Factions/Base/Mythic/EnemyPack";
    private const string HealthBarPath = "Assets/Prefabs/UI/HealthBar.prefab";

    private sealed class Definition
    {
        public string Name;
        public string Folder;
        public string AnimationFolder;
        public string Prefix;
        public string MoveClip;
        public string AttackClip;
        public string Controller;
        public string Projectile;

        public bool IsRanged => !string.IsNullOrEmpty(Projectile);
    }

    private static readonly Definition[] Definitions =
    {
        Unit("Bear", "Bear", "Bear Animations", "Bear", "Run", "Attack", "Bear_Controller"),
        Unit("Gnoll", "Gnoll", "Gnoll Animations", "Gnoll", "Run", "Throw", "Gnoll_Controller", "Bone"),
        Unit("Gnome", "Gnome", "Gnome Animations", "Gnome", "Run", "Attack", "Gnome_Controller"),
        Unit("HarpoonFish", "Harpoon Fish", "Harpoon Fish Animations", "HarpoonFish", "Run", "Throw", "HarpoonFish_Controller", "Harpoon"),
        Unit("Lancer", "Lancer", "Lancer_Animations", "Lancer", "Run", "Attack", "Lancer_Controller"),
        Unit("Lizard", "Lizard", "Lizard Animations", "Lizard", "Run", "Attack", "Lizard_Controller"),
        Unit("PaddleFish", "Paddle Fish", "Paddle Fish Animations", "PaddleFish", "Run", "Attack", "PaddleFish_Controller"),
        Unit("Panda", "Panda", "Panda Animations", "Panda", "Run", "Attack", "Panda_Controller"),
        Unit("Shaman", "Shaman", "Shaman Animations", "Shaman", "Run", "Attack", "Shaman_Controller", "Shaman"),
        Unit("Skull", "Skull", "Skull Animations", "Skull", "Run", "Attack", "Skull_Controller"),
        Unit("Snake", "Snake", "Snake Animations", "Snake", "Run", "Attack", "Snake_Controller"),
        Unit("Spider", "Spider", "Spider Animations", "Spider", "Run", "Attack", "Spider_Controller"),
        Unit("Thief", "Thief", "Thief Animations", "Thief", "Run", "Attack", "Thief_Controller"),
        Unit("Troll", "Troll", "Troll Animations", "Troll", "Walk", "Attack", "Troll_Controller"),
        Unit("Turtle", "Turtle", "Turtle Animation", "Turtle", "Run", "Attack", "Turtle_Controller")
    };

    [MenuItem("Tools/Clash of Pantheons/Build Phase 2 Enemy Pack Mythics")]
    public static void BuildPhaseTwo()
    {
        EnsureFolder(OutputRoot);
        HealthBar healthBar = RequireAsset<GameObject>(HealthBarPath).GetComponent<HealthBar>();
        if (healthBar == null)
        {
            throw new InvalidOperationException($"HealthBar component missing from {HealthBarPath}.");
        }

        Projectile bone = CreateProjectile("Bone");
        Projectile harpoon = CreateProjectile("Harpoon");
        Projectile shaman = CreateProjectile("Shaman");

        foreach (Definition definition in Definitions)
        {
            AnimationClip idle = GetClip(definition, "Idle");
            AnimationClip move = GetClip(definition, definition.MoveClip);
            AnimationClip attack = GetClip(definition, definition.AttackClip);
            AnimatorController controller = ConfigureController(definition, idle, move, attack);
            UnitData data = CreateOrUpdateUnitData(definition);
            Projectile projectile = definition.Projectile switch
            {
                "Bone" => bone,
                "Harpoon" => harpoon,
                "Shaman" => shaman,
                _ => null
            };

            CreateUnitPrefab(definition, idle, controller, data, healthBar, projectile);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase 2 Enemy Pack mythic prefabs generated successfully.");
    }

    private static Definition Unit(
        string name,
        string folder,
        string animationFolder,
        string prefix,
        string moveClip,
        string attackClip,
        string controller,
        string projectile = null)
    {
        return new Definition
        {
            Name = name,
            Folder = folder,
            AnimationFolder = animationFolder,
            Prefix = prefix,
            MoveClip = moveClip,
            AttackClip = attackClip,
            Controller = controller,
            Projectile = projectile
        };
    }

    private static AnimatorController ConfigureController(
        Definition definition,
        AnimationClip idle,
        AnimationClip move,
        AnimationClip attack)
    {
        SetLoop(idle, true);
        SetLoop(move, true);
        SetLoop(attack, false);

        string controllerPath =
            $"{SourceRoot}/{definition.Folder}/{definition.AnimationFolder}/{definition.Controller}.controller";
        AnimatorController controller = RequireAsset<AnimatorController>(controllerPath);
        controller.parameters = new[]
        {
            new AnimatorControllerParameter
            {
                name = "isMoving",
                type = AnimatorControllerParameterType.Bool
            },
            new AnimatorControllerParameter
            {
                name = "Attack",
                type = AnimatorControllerParameterType.Trigger
            }
        };

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in machine.states)
        {
            machine.RemoveState(child.state);
        }

        foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
        {
            machine.RemoveAnyStateTransition(transition);
        }

        AnimatorState idleState = machine.AddState(idle.name);
        idleState.motion = idle;
        AnimatorState moveState = machine.AddState(move.name);
        moveState.motion = move;
        AnimatorState attackState = machine.AddState(attack.name);
        attackState.motion = attack;
        machine.defaultState = idleState;

        AnimatorStateTransition toMove = idleState.AddTransition(moveState);
        ConfigureImmediate(toMove);
        toMove.AddCondition(AnimatorConditionMode.If, 0f, "isMoving");

        AnimatorStateTransition toIdle = moveState.AddTransition(idleState);
        ConfigureImmediate(toIdle);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "isMoving");

        AnimatorStateTransition toAttack = machine.AddAnyStateTransition(attackState);
        ConfigureImmediate(toAttack);
        toAttack.canTransitionToSelf = false;
        toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        AnimatorStateTransition attackComplete = attackState.AddTransition(idleState);
        attackComplete.hasExitTime = true;
        attackComplete.exitTime = 0.95f;
        attackComplete.duration = 0.02f;

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static UnitData CreateOrUpdateUnitData(Definition definition)
    {
        string path = $"{OutputRoot}/{definition.Name}UnitData.asset";
        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<UnitData>();
            AssetDatabase.CreateAsset(data, path);
        }

        SerializedObject serialized = new SerializedObject(data);
        serialized.FindProperty("maxHealth").floatValue = 300f;
        serialized.FindProperty("damage").floatValue = 25f;
        serialized.FindProperty("attackRange").floatValue = definition.IsRanged ? 4f : 1f;
        serialized.FindProperty("attackSpeed").floatValue = 1f;
        serialized.FindProperty("moveSpeed").floatValue = 1.8f;
        serialized.FindProperty("cost").intValue = 220;
        serialized.FindProperty("spawnInterval").floatValue = 12f;
        serialized.FindProperty("unitDamageMultiplier").floatValue = 1f;
        serialized.FindProperty("buildingDamageMultiplier").floatValue = 1.5f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }

    private static void CreateUnitPrefab(
        Definition definition,
        AnimationClip idle,
        RuntimeAnimatorController controller,
        UnitData data,
        HealthBar healthBar,
        Projectile projectile)
    {
        GameObject root = new GameObject($"{definition.Name}MythicUnit");
        root.layer = 7;
        root.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        try
        {
            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.offset = new Vector2(0f, 0.21f);
            collider.size = Vector2.one;

            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            GameObject visual = CreateChild("Visual", root.transform, new Vector3(0f, 0f, -1f));
            GameObject rendererObject = CreateChild("Renderer", visual.transform, new Vector3(0f, 0.3f, 0f));

            SpriteRenderer renderer = rendererObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFirstSprite(idle);
            renderer.sortingOrder = 2;

            Animator animator = rendererObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            GameObject spawnPointObject = CreateChild(
                "ProjectileSpawnPoint",
                root.transform,
                new Vector3(0.55f, 0.45f, -1f));

            BaseUnit unit = definition.IsRanged
                ? root.AddComponent<ArcherUnit>()
                : root.AddComponent<MeleeUnit>();

            SerializedObject serialized = new SerializedObject(unit);
            serialized.FindProperty("unitData").objectReferenceValue = data;
            serialized.FindProperty("healthBarPrefab").objectReferenceValue = healthBar;
            serialized.FindProperty("healthBarOffset").vector3Value = new Vector3(0f, 0.8f, 0f);
            serialized.FindProperty("visualTransform").objectReferenceValue = visual.transform;
            serialized.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            serialized.FindProperty("animator").objectReferenceValue = animator;
            serialized.FindProperty("friendlySeparationStrength").floatValue = 1.5f;
            serialized.FindProperty("usesProjectile").boolValue = definition.IsRanged;
            serialized.FindProperty("projectilePrefab").objectReferenceValue = projectile;
            serialized.FindProperty("projectileSpawnPoint").objectReferenceValue = spawnPointObject.transform;
            serialized.FindProperty("projectileTravelTime").floatValue = 0.4f;
            serialized.FindProperty("projectileArcHeight").floatValue = definition.Projectile == "Shaman" ? 0.15f : 0.5f;
            serialized.FindProperty("projectileSpawnDelay").floatValue =
                definition.Projectile == "Shaman" ? 0.5f : 0.4f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, $"{OutputRoot}/{definition.Name}MythicUnit.prefab");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static Projectile CreateProjectile(string type)
    {
        GameObject root = new GameObject($"{type}Projectile");
        try
        {
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 3;

            Animator animator = null;
            switch (type)
            {
                case "Bone":
                {
                    string basePath = $"{SourceRoot}/Gnoll/Bone Animation";
                    AnimationClip clip = RequireAsset<AnimationClip>($"{basePath}/Bone_Idle.anim");
                    SetLoop(clip, true);
                    renderer.sprite = GetFirstSprite(clip);
                    animator = root.AddComponent<Animator>();
                    animator.runtimeAnimatorController =
                        RequireAsset<RuntimeAnimatorController>($"{basePath}/Bone_Controller.controller");
                    break;
                }
                case "Harpoon":
                    renderer.sprite = GetFirstSpriteAtPath($"{SourceRoot}/Harpoon Fish/Harpoon.png");
                    break;
                case "Shaman":
                {
                    string basePath = $"{SourceRoot}/Shaman/Proyectile Animations";
                    AnimationClip clip = RequireAsset<AnimationClip>($"{basePath}/Proyectile_Idle.anim");
                    SetLoop(clip, true);
                    renderer.sprite = GetFirstSprite(clip);
                    animator = root.AddComponent<Animator>();
                    animator.runtimeAnimatorController =
                        RequireAsset<RuntimeAnimatorController>($"{basePath}/Proyectile_Controller.controller");
                    break;
                }
            }

            root.transform.localScale = type == "Harpoon"
                ? new Vector3(0.75f, 0.75f, 1f)
                : new Vector3(0.5f, 0.5f, 1f);
            Projectile projectile = root.AddComponent<Projectile>();
            SerializedObject serializedProjectile = new SerializedObject(projectile);
            serializedProjectile.FindProperty("rotateToTrajectory").boolValue = type != "Shaman";
            serializedProjectile.ApplyModifiedPropertiesWithoutUndo();
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, $"{OutputRoot}/{type}Projectile.prefab");
            return saved.GetComponent<Projectile>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static AnimationClip GetClip(Definition definition, string suffix)
    {
        return RequireAsset<AnimationClip>(
            $"{SourceRoot}/{definition.Folder}/{definition.AnimationFolder}/{definition.Prefix}_{suffix}.anim");
    }

    private static GameObject CreateChild(string name, Transform parent, Vector3 localPosition)
    {
        GameObject child = new GameObject(name);
        child.layer = 7;
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        return child;
    }

    private static Sprite GetFirstSprite(AnimationClip clip)
    {
        foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
        {
            ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            if (frames.Length > 0 && frames[0].value is Sprite sprite)
            {
                return sprite;
            }
        }

        throw new InvalidOperationException($"{clip.name} has no sprite frames.");
    }

    private static Sprite GetFirstSpriteAtPath(string path)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        throw new InvalidOperationException($"No sprite found at {path}.");
    }

    private static void SetLoop(AnimationClip clip, bool loop)
    {
        SerializedObject serialized = new SerializedObject(clip);
        SerializedProperty property = serialized.FindProperty("m_AnimationClipSettings.m_LoopTime");
        if (property == null)
        {
            throw new InvalidOperationException($"Could not set loop state on {clip.name}.");
        }

        property.boolValue = loop;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(clip);
    }

    private static void ConfigureImmediate(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.hasFixedDuration = true;
        transition.duration = 0.02f;
    }

    private static T RequireAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new InvalidOperationException($"Required asset not found: {path}");
        }

        return asset;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int index = 1; index < parts.Length; index++)
        {
            string next = $"{current}/{parts[index]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }

            current = next;
        }
    }
}
