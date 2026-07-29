using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ClashMonkPhaseOneBuilder
{
    private const string UnitDataPath = "Assets/Prefabs/Units/Factions/Base/Mythic/Monk/MonkUnitData.asset";
    private const string HealEffectPrefabPath = "Assets/Prefabs/Effects/HealEffect.prefab";
    private const string HealthBarPath = "Assets/Prefabs/UI/HealthBar.prefab";
    private const string HealEffectControllerPath = "Assets/Tiny Swords/Units/Extra/Heal Effect/Heal.controller";
    private const string HealEffectClipPath = "Assets/Tiny Swords/Units/Extra/Heal Effect/Heal_Animation.anim";

    private sealed class MonkVariant
    {
        public string Colour;
        public string UnitFolder;
        public string AnimationFolder;
        public string ControllerName;
    }

    private static readonly MonkVariant[] Variants =
    {
        Variant("Black", "Black Units", "Monk Black Animations", "Monk_Black"),
        Variant("Blue", "Blue Units", "Monk Blue Animations", "Monk_Blue"),
        Variant("Purple", "Purple Units", "Monk Purple Animations", "Monk_Purple"),
        Variant("Red", "Red Units", "Monk Red Animations", "Monk_Red"),
        Variant("Yellow", "Yellow Units", "Monk Yellow Animations", "Monk_Yellow")
    };

    [MenuItem("Tools/Clash of Pantheons/Build Phase 1 Monk Healers")]
    public static void BuildPhaseOne()
    {
        EnsureFolder("Assets/Prefabs/Effects");
        EnsureFolder("Assets/Prefabs/Units/Factions/Base/Mythic/Monk");

        UnitData unitData = CreateOrUpdateUnitData();
        GameObject healEffectPrefab = CreateHealEffectPrefab();

        foreach (MonkVariant variant in Variants)
        {
            ConfigureControllerAndClips(variant);
            CreateMonkPrefab(variant, unitData, healEffectPrefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Phase 1 monk healers generated for all five colour factions.");
    }

    private static MonkVariant Variant(
        string colour,
        string unitFolder,
        string animationFolder,
        string controllerName)
    {
        return new MonkVariant
        {
            Colour = colour,
            UnitFolder = unitFolder,
            AnimationFolder = animationFolder,
            ControllerName = controllerName
        };
    }

    private static UnitData CreateOrUpdateUnitData()
    {
        UnitData data = AssetDatabase.LoadAssetAtPath<UnitData>(UnitDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<UnitData>();
            AssetDatabase.CreateAsset(data, UnitDataPath);
        }

        SerializedObject serializedData = new SerializedObject(data);
        serializedData.FindProperty("maxHealth").floatValue = 300f;
        serializedData.FindProperty("damage").floatValue = 0f;
        serializedData.FindProperty("attackRange").floatValue = 0f;
        serializedData.FindProperty("attackSpeed").floatValue = 1f;
        serializedData.FindProperty("moveSpeed").floatValue = 1.8f;
        serializedData.FindProperty("cost").intValue = 220;
        serializedData.FindProperty("spawnInterval").floatValue = 12f;
        serializedData.FindProperty("unitDamageMultiplier").floatValue = 0f;
        serializedData.FindProperty("buildingDamageMultiplier").floatValue = 0f;
        serializedData.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(data);
        return data;
    }

    private static GameObject CreateHealEffectPrefab()
    {
        AnimationClip clip = RequireAsset<AnimationClip>(HealEffectClipPath);
        SetLoop(clip, false);

        RuntimeAnimatorController controller = RequireAsset<RuntimeAnimatorController>(HealEffectControllerPath);
        Sprite initialSprite = GetFirstSprite(clip);

        GameObject root = new GameObject("HealEffect");
        try
        {
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = initialSprite;
            renderer.sortingOrder = 3;

            Animator animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            return PrefabUtility.SaveAsPrefabAsset(root, HealEffectPrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void ConfigureControllerAndClips(MonkVariant variant)
    {
        string basePath = GetMonkBasePath(variant);
        AnimationClip idle = RequireAsset<AnimationClip>(
            $"{basePath}/{variant.AnimationFolder}/Monk_Idle_{variant.Colour}.anim");
        AnimationClip run = RequireAsset<AnimationClip>(
            $"{basePath}/{variant.AnimationFolder}/Monk_Run_{variant.Colour}.anim");
        AnimationClip heal = RequireAsset<AnimationClip>(
            $"{basePath}/{variant.AnimationFolder}/Monk_Heal_{variant.Colour}.anim");
        AnimatorController controller = RequireAsset<AnimatorController>(
            $"{basePath}/{variant.AnimationFolder}/{variant.ControllerName}.controller");

        SetLoop(idle, true);
        SetLoop(run, true);
        SetLoop(heal, false);

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
        foreach (ChildAnimatorState childState in machine.states)
        {
            machine.RemoveState(childState.state);
        }

        foreach (AnimatorStateTransition transition in machine.anyStateTransitions)
        {
            machine.RemoveAnyStateTransition(transition);
        }

        AnimatorState idleState = machine.AddState($"Monk_Idle_{variant.Colour}");
        idleState.motion = idle;
        AnimatorState runState = machine.AddState($"Monk_Run_{variant.Colour}");
        runState.motion = run;
        AnimatorState healState = machine.AddState($"Monk_Heal_{variant.Colour}");
        healState.motion = heal;
        machine.defaultState = idleState;

        AnimatorStateTransition toRun = idleState.AddTransition(runState);
        ConfigureImmediateTransition(toRun);
        toRun.AddCondition(AnimatorConditionMode.If, 0f, "isMoving");

        AnimatorStateTransition toIdle = runState.AddTransition(idleState);
        ConfigureImmediateTransition(toIdle);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "isMoving");

        AnimatorStateTransition toHeal = machine.AddAnyStateTransition(healState);
        ConfigureImmediateTransition(toHeal);
        toHeal.canTransitionToSelf = false;
        toHeal.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        AnimatorStateTransition healComplete = healState.AddTransition(idleState);
        healComplete.hasExitTime = true;
        healComplete.exitTime = 0.95f;
        healComplete.duration = 0.02f;

        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureImmediateTransition(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.duration = 0.02f;
        transition.hasFixedDuration = true;
    }

    private static MonkUnit CreateMonkPrefab(
        MonkVariant variant,
        UnitData unitData,
        GameObject healEffectPrefab)
    {
        string basePath = GetMonkBasePath(variant);
        AnimationClip idle = RequireAsset<AnimationClip>(
            $"{basePath}/{variant.AnimationFolder}/Monk_Idle_{variant.Colour}.anim");
        RuntimeAnimatorController controller = RequireAsset<RuntimeAnimatorController>(
            $"{basePath}/{variant.AnimationFolder}/{variant.ControllerName}.controller");
        GameObject healthBarPrefab = RequireAsset<GameObject>(HealthBarPath);
        HealthBar healthBar = healthBarPrefab.GetComponent<HealthBar>();
        if (healthBar == null)
        {
            throw new InvalidOperationException($"HealthBar component missing from {HealthBarPath}.");
        }

        GameObject root = new GameObject($"{variant.Colour}MonkUnit");
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

            GameObject visual = new GameObject("Visual");
            visual.layer = 7;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, -1f);

            GameObject rendererObject = new GameObject("Renderer");
            rendererObject.layer = 7;
            rendererObject.transform.SetParent(visual.transform, false);
            rendererObject.transform.localPosition = new Vector3(0f, 0.3f, 0f);

            SpriteRenderer renderer = rendererObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetFirstSprite(idle);
            renderer.sortingOrder = 2;

            Animator animator = rendererObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            MonkUnit monk = root.AddComponent<MonkUnit>();
            SerializedObject serializedMonk = new SerializedObject(monk);
            serializedMonk.FindProperty("unitData").objectReferenceValue = unitData;
            serializedMonk.FindProperty("healthBarPrefab").objectReferenceValue = healthBar;
            serializedMonk.FindProperty("healthBarOffset").vector3Value = new Vector3(0f, 0.8f, 0f);
            serializedMonk.FindProperty("visualTransform").objectReferenceValue = visual.transform;
            serializedMonk.FindProperty("spriteRenderer").objectReferenceValue = renderer;
            serializedMonk.FindProperty("animator").objectReferenceValue = animator;
            serializedMonk.FindProperty("friendlySeparationStrength").floatValue = 1.5f;
            serializedMonk.FindProperty("healRange").floatValue = 2f;
            serializedMonk.FindProperty("baseHealAmount").floatValue = 5f;
            serializedMonk.FindProperty("healInterval").floatValue = 3f;
            serializedMonk.FindProperty("healEffectPrefab").objectReferenceValue = healEffectPrefab;
            serializedMonk.FindProperty("healEffectLifetime").floatValue = 1.1f;
            serializedMonk.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath =
                $"Assets/Prefabs/Units/Factions/Base/Mythic/Monk/{variant.Colour}MonkUnit.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            return savedPrefab.GetComponent<MonkUnit>();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
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

    private static void SetLoop(AnimationClip clip, bool loop)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty loopProperty = serializedClip.FindProperty("m_AnimationClipSettings.m_LoopTime");
        if (loopProperty == null)
        {
            throw new InvalidOperationException($"Could not set loop state on {clip.name}.");
        }

        loopProperty.boolValue = loop;
        serializedClip.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(clip);
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

    private static string GetMonkBasePath(MonkVariant variant)
    {
        return $"Assets/Tiny Swords/Units/{variant.UnitFolder}/Monk";
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
