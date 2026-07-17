using System;
using UnityEditor;
using UnityEngine;

public static class ClashArcherProjectileBuilder
{
    private const string ArrowSpritePath = "Assets/Tiny Swords/Units/Extra/Arrow/Arrow.png";
    private const string OutputPath = "Assets/Prefabs/Units/Factions/Base/Archer/TinySwordsArrowProjectile.prefab";

    private static readonly string[] ArcherPrefabPaths =
    {
        "Assets/Prefabs/Units/Factions/Base/Archer/ArcherUnit.prefab",
        "Assets/Prefabs/Units/Factions/Base/Archer/BlackArcherAnimatedUnit.prefab",
        "Assets/Prefabs/Units/Factions/Base/Archer/BlueArcherAnimatedUnit.prefab",
        "Assets/Prefabs/Units/Factions/Base/Archer/PurpleArcherAnimatedUnit.prefab",
        "Assets/Prefabs/Units/Factions/Base/Archer/RedArcherAnimatedUnit.prefab",
        "Assets/Prefabs/Units/Factions/Base/Archer/YellowArcherAnimatedUnit.prefab"
    };

    [MenuItem("Tools/Clash of Pantheons/Build Tiny Swords Archer Projectile")]
    public static void BuildArcherProjectile()
    {
        GameObject projectileRoot = new GameObject("TinySwordsArrowProjectile");
        try
        {
            projectileRoot.layer = 7;
            projectileRoot.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            SpriteRenderer renderer = projectileRoot.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadFirstSprite(ArrowSpritePath);
            renderer.sortingOrder = 5;
            Projectile projectile = projectileRoot.AddComponent<Projectile>();
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(projectileRoot, OutputPath);
            projectile = savedPrefab.GetComponent<Projectile>();

            foreach (string path in ArcherPrefabPaths)
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    BaseUnit archer = prefabRoot.GetComponent<BaseUnit>();
                    if (archer == null)
                    {
                        throw new InvalidOperationException($"Archer component is missing from {path}");
                    }

                    SerializedObject serializedArcher = new SerializedObject(archer);
                    serializedArcher.FindProperty("projectilePrefab").objectReferenceValue = projectile;
                    serializedArcher.FindProperty("usesProjectile").boolValue = true;
                    serializedArcher.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(projectileRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Tiny Swords arrow projectile assigned to {ArcherPrefabPaths.Length} archer prefabs.");
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
}
