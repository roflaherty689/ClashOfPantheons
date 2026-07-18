#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ClashBattleSceneHierarchyMigration
{
    private const string BattleScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Clash of Pantheons/Normalize Battle Scene UI Hierarchy")]
    public static void NormalizeFromMenu()
    {
        NormalizeBattleScene();
    }

    public static void NormalizeBattleScene()
    {
        Scene scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
        GameObject battleUi = FindRoot(scene, "Battle UI");
        if (battleUi == null || battleUi.GetComponent<Canvas>() == null)
            throw new MissingReferenceException("The battle scene has no root 'Battle UI' Canvas.");

        RectTransform battleRect = battleUi.GetComponent<RectTransform>();
        battleRect.localScale = Vector3.one;

        GameObject legacyRoot = FindLegacyCanvasRoot(scene);
        if (legacyRoot != null)
            Object.DestroyImmediate(legacyRoot);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new System.InvalidOperationException("Unity could not save the normalized battle scene.");

        Debug.Log(legacyRoot == null
            ? "Battle UI scale normalized; no legacy Canvas hierarchy remained."
            : "Removed the legacy Canvas/Canvas/VictoryText hierarchy and normalized the active Battle UI scale.");
    }

    private static GameObject FindLegacyCanvasRoot(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != "Canvas" || root.transform.childCount != 1 || root.GetComponent<Canvas>() != null)
                continue;

            Transform nested = root.transform.GetChild(0);
            if (nested.name != "Canvas" || nested.GetComponent<Canvas>() == null || nested.childCount != 1)
                continue;

            Transform legacyText = nested.GetChild(0);
            if (legacyText.name == "VictoryText" && legacyText.GetComponent<TMPro.TMP_Text>() != null)
                return root;
        }

        return null;
    }

    private static GameObject FindRoot(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == objectName) return root;
        return null;
    }
}
#endif
