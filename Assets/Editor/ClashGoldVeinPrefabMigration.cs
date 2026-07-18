#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ClashGoldVeinPrefabMigration
{
    private const string BattleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string PrefabFolder = "Assets/Prefabs/Resources";
    private const string PrefabPath = PrefabFolder + "/GoldVein.prefab";

    [MenuItem("Tools/Clash of Pantheons/Convert Gold Veins to Shared Prefab")]
    public static void ConvertFromMenu()
    {
        ConvertGoldVeins();
    }

    public static void ConvertGoldVeins()
    {
        Scene scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
        GameObject left = FindRoot(scene, "LeftGoldNode");
        GameObject right = FindRoot(scene, "RightGoldNode");
        ValidateNode(left, "LeftGoldNode");
        ValidateNode(right, "RightGoldNode");

        GoldVein leftVein = left.GetComponent<GoldVein>();
        GoldVein rightVein = right.GetComponent<GoldVein>();
        Dictionary<WorkerManager, bool> managerSides = CaptureManagerSides(leftVein, rightVein);

        EnsurePrefabFolder();
        string originalRightName = right.name;
        right.name = "GoldVein";
        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(right, PrefabPath, InteractionMode.UserAction);
        if (prefab == null) throw new System.InvalidOperationException("Unity could not create the shared GoldVein prefab.");
        right.name = originalRightName;

        Transform leftTransform = left.transform;
        Vector3 leftPosition = leftTransform.position;
        Quaternion leftRotation = leftTransform.rotation;
        Vector3 leftScale = leftTransform.localScale;
        Object.DestroyImmediate(left);

        GameObject replacement = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        replacement.name = "LeftGoldNode";
        replacement.transform.SetPositionAndRotation(leftPosition, leftRotation);
        replacement.transform.localScale = leftScale;

        GoldVein replacementLeftVein = replacement.GetComponent<GoldVein>();
        GoldVein connectedRightVein = right.GetComponent<GoldVein>();
        foreach (KeyValuePair<WorkerManager, bool> entry in managerSides)
            AssignGoldVein(entry.Key, entry.Value ? replacementLeftVein : connectedRightVein);

        ValidateNode(replacement, "LeftGoldNode");
        ValidateNode(right, "RightGoldNode");
        ValidateWorkerReferences(managerSides.Keys);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new System.InvalidOperationException("Unity could not save the gold-vein prefab migration.");
        AssetDatabase.SaveAssets();
        Debug.Log("Converted both gold nodes to instances of Assets/Prefabs/Resources/GoldVein.prefab and preserved WorkerManager references.");
    }

    private static Dictionary<WorkerManager, bool> CaptureManagerSides(GoldVein left, GoldVein right)
    {
        Dictionary<WorkerManager, bool> result = new Dictionary<WorkerManager, bool>();
        foreach (WorkerManager manager in Object.FindObjectsByType<WorkerManager>())
        {
            SerializedProperty property = new SerializedObject(manager).FindProperty("goldVein");
            if (property.objectReferenceValue == left) result.Add(manager, true);
            else if (property.objectReferenceValue == right) result.Add(manager, false);
        }
        if (result.Count != 2)
            throw new System.InvalidOperationException($"Expected exactly two WorkerManagers bound to the gold nodes, found {result.Count}.");
        return result;
    }

    private static void AssignGoldVein(WorkerManager manager, GoldVein vein)
    {
        SerializedObject serialized = new SerializedObject(manager);
        serialized.FindProperty("goldVein").objectReferenceValue = vein;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(manager);
    }

    private static void ValidateWorkerReferences(IEnumerable<WorkerManager> managers)
    {
        foreach (WorkerManager manager in managers)
        {
            SerializedProperty property = new SerializedObject(manager).FindProperty("goldVein");
            GoldVein vein = property.objectReferenceValue as GoldVein;
            if (vein == null || !PrefabUtility.IsPartOfPrefabInstance(vein))
                throw new MissingReferenceException($"{manager.name} is not bound to a prefab-backed GoldVein.");
        }
    }

    private static void ValidateNode(GameObject node, string expectedName)
    {
        if (node == null) throw new MissingReferenceException($"The battle scene has no root '{expectedName}'.");
        GoldVein vein = node.GetComponent<GoldVein>();
        Transform minePoint = node.transform.Find("MinePoint");
        if (vein == null || minePoint == null)
            throw new MissingReferenceException($"{expectedName} must contain a GoldVein and direct MinePoint child.");
        SerializedProperty property = new SerializedObject(vein).FindProperty("minePoint");
        if (property.objectReferenceValue != minePoint)
            throw new MissingReferenceException($"{expectedName} GoldVein is not bound to its internal MinePoint.");
    }

    private static void EnsurePrefabFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/Prefabs", "Resources");
    }

    private static GameObject FindRoot(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == objectName) return root;
        return null;
    }
}
#endif
