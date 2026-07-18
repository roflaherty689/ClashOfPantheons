using System;
using UnityEditor;
using UnityEngine;

public static class ClashInitialSoundBuilder
{
    private const string PrefabPath = "Assets/Resources/Audio/SoundManager.prefab";
    private const string PackRoot = "Assets/400 Sounds Pack/";

    [MenuItem("Tools/Clash of Pantheons/Audio/Build Initial SFX Setup")]
    public static void Build()
    {
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "Audio");

        GameObject root = new GameObject("SoundManager");
        SoundManager manager = root.AddComponent<SoundManager>();
        SerializedObject serialized = new SerializedObject(manager);
        SerializedProperty cues = serialized.FindProperty("cues");

        Cue[] definitions =
        {
            new(SoundCue.UiClick, "UI/pop_2.wav", .35f, .04f, 0f, .04f),
            new(SoundCue.UiReject, "UI/synth_error.wav", .4f, .2f, 0f, .01f),
            new(SoundCue.Purchase, "Items/coins_gather_quick.wav", .55f, .08f, 0f, .04f),
            new(SoundCue.WorkerDeposit, "Items/coin_jingle_small.wav", .3f, .25f, .25f, .06f),
            new(SoundCue.UnitSpawn, "Other/whoosh_1.wav", .25f, .18f, .35f, .08f),
            new(SoundCue.MeleeAttack, "Weapons/sword_clash.wav", .28f, .11f, .45f, .08f),
            new(SoundCue.RangedAttack, "Other/elastic_twang.wav", .25f, .12f, .45f, .08f),
            new(SoundCue.SiegeAttack, "Retro/explosion_medium.wav", .4f, .22f, .5f, .05f),
            new(SoundCue.MythicAttack, "Weapons/harsh_thud.wav", .36f, .14f, .5f, .08f),
            new(SoundCue.UnitDeath, "Combat and Gore/crunch_quick.wav", .26f, .1f, .45f, .1f),
            new(SoundCue.Heal, "Items/heart_collect.wav", .4f, .2f, .4f, .04f),
            new(SoundCue.StrongholdHit, "Materials/metal_clang.wav", .55f, .12f, .5f, .04f),
            new(SoundCue.StrongholdDestroyed, "Retro/explosion_large.wav", .7f, .5f, .5f, .03f),
            new(SoundCue.Victory, "Musical Effects/brass_level_complete.wav", .75f, .5f, 0f, 0f),
            new(SoundCue.Defeat, "Musical Effects/brass_defeated.wav", .7f, .5f, 0f, 0f),
            new(SoundCue.Draw, "Musical Effects/xylophone_mystery.wav", .65f, .5f, 0f, 0f)
        };

        cues.arraySize = definitions.Length;
        for (int i = 0; i < definitions.Length; i++)
        {
            SerializedProperty item = cues.GetArrayElementAtIndex(i);
            Cue definition = definitions[i];
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(PackRoot + definition.Path);
            if (clip == null) throw new InvalidOperationException($"Missing audio clip: {definition.Path}");
            item.FindPropertyRelative("cue").enumValueIndex = (int)definition.Sound;
            item.FindPropertyRelative("clip").objectReferenceValue = clip;
            item.FindPropertyRelative("volume").floatValue = definition.Volume;
            item.FindPropertyRelative("cooldown").floatValue = definition.Cooldown;
            item.FindPropertyRelative("spatialBlend").floatValue = definition.SpatialBlend;
            item.FindPropertyRelative("pitchVariation").floatValue = definition.PitchVariation;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log($"Built initial SFX setup at {PrefabPath} with {definitions.Length} cues.");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private readonly struct Cue
    {
        public Cue(SoundCue cue, string path, float volume, float cooldown, float spatialBlend, float pitchVariation)
        {
            Sound = cue; Path = path; Volume = volume; Cooldown = cooldown;
            SpatialBlend = spatialBlend; PitchVariation = pitchVariation;
        }
        public SoundCue Sound { get; }
        public string Path { get; }
        public float Volume { get; }
        public float Cooldown { get; }
        public float SpatialBlend { get; }
        public float PitchVariation { get; }
    }
}
