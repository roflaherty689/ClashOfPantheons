using System;
using UnityEditor;
using UnityEngine;

public static class ClashMythicRosterBuilder
{
    private const string ResourceRoot = "Assets/Resources";
    private const string RosterPath = ResourceRoot + "/MythicUnitRoster.asset";
    private const string MythicRoot = "Assets/Prefabs/Units/Factions/Base/Mythic";
    private const string AvatarRoot = "Assets/Tiny Swords - Enemy Pack/Enemy Avatars";
    private const string HumanAvatarRoot = "Assets/Tiny Swords/UI Elements/Human Avatars";
    private const string DefaultIconPath = "Assets/Tiny Swords/UI Elements/Icons/Icon_05.png";

    private static readonly string[] UnitPaths =
    {
        MythicRoot + "/Monk/BlackMonkUnit.prefab",
        MythicRoot + "/Monk/BlueMonkUnit.prefab",
        MythicRoot + "/Monk/PurpleMonkUnit.prefab",
        MythicRoot + "/Monk/RedMonkUnit.prefab",
        MythicRoot + "/Monk/YellowMonkUnit.prefab",
        MythicRoot + "/MeleeMythicAnimatedUnit.prefab",
        MythicRoot + "/EnemyPack/BearMythicUnit.prefab",
        MythicRoot + "/EnemyPack/GnollMythicUnit.prefab",
        MythicRoot + "/EnemyPack/GnomeMythicUnit.prefab",
        MythicRoot + "/EnemyPack/HarpoonFishMythicUnit.prefab",
        MythicRoot + "/EnemyPack/LancerMythicUnit.prefab",
        MythicRoot + "/EnemyPack/LizardMythicUnit.prefab",
        MythicRoot + "/EnemyPack/PaddleFishMythicUnit.prefab",
        MythicRoot + "/EnemyPack/PandaMythicUnit.prefab",
        MythicRoot + "/EnemyPack/ShamanMythicUnit.prefab",
        MythicRoot + "/EnemyPack/SkullMythicUnit.prefab",
        MythicRoot + "/EnemyPack/SnakeMythicUnit.prefab",
        MythicRoot + "/EnemyPack/SpiderMythicUnit.prefab",
        MythicRoot + "/EnemyPack/ThiefMythicUnit.prefab",
        MythicRoot + "/EnemyPack/TrollMythicUnit.prefab",
        MythicRoot + "/EnemyPack/TurtleMythicUnit.prefab"
    };

    private static readonly int[] AvatarNumbers =
    {
        0, 0, 0, 0, 0,
        9, 14, 10, 15, 3, 4, 13, 2, 12, 5, 1, 7, 11, 6, 16, 8
    };

    private static readonly int[] MonkAvatarNumbers = { 24, 4, 19, 9, 14 };

    [MenuItem("Tools/Clash of Pantheons/Build Phase 3 Mythic Roster")]
    public static void BuildPhaseThreeRoster()
    {
        EnsureFolder(ResourceRoot);

        MythicUnitRoster roster = AssetDatabase.LoadAssetAtPath<MythicUnitRoster>(RosterPath);
        if (roster == null)
        {
            roster = ScriptableObject.CreateInstance<MythicUnitRoster>();
            AssetDatabase.CreateAsset(roster, RosterPath);
        }

        SerializedObject serializedRoster = new SerializedObject(roster);
        SerializedProperty units = serializedRoster.FindProperty("units");
        SerializedProperty avatars = serializedRoster.FindProperty("avatars");
        units.arraySize = UnitPaths.Length;
        avatars.arraySize = UnitPaths.Length;

        for (int i = 0; i < UnitPaths.Length; i++)
        {
            BaseUnit prefab = AssetDatabase.LoadAssetAtPath<BaseUnit>(UnitPaths[i]);
            if (prefab == null)
            {
                throw new InvalidOperationException($"Mythic roster prefab is missing: {UnitPaths[i]}");
            }

            units.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
            avatars.GetArrayElementAtIndex(i).objectReferenceValue = i < MonkAvatarNumbers.Length
                ? LoadFirstSprite($"{HumanAvatarRoot}/Avatars_{MonkAvatarNumbers[i]:00}.png")
                : LoadFirstSprite($"{AvatarRoot}/Enemy Avatars_{AvatarNumbers[i]:00}.png");
        }

        serializedRoster.FindProperty("defaultIcon").objectReferenceValue = LoadFirstSprite(DefaultIconPath);

        serializedRoster.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(roster);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Phase 3 mythic roster generated with {UnitPaths.Length} choices.");
    }

    private static Sprite LoadSprite(string path, string spriteName)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite && sprite.name == spriteName)
            {
                return sprite;
            }
        }

        throw new InvalidOperationException($"Sprite '{spriteName}' is missing from {path}");
    }

    private static Sprite LoadFirstSprite(string path)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        throw new InvalidOperationException($"No sprite is available at {path}");
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
