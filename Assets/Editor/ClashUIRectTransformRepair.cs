#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class ClashUIRectTransformRepair
{
    [MenuItem("Tools/Clash of Pantheons/Repair NaN UI Values")]
    private static void RepairSelectedOrBattleUI()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null)
            root = GameObject.Find("Battle UI");

        if (root == null)
        {
            EditorUtility.DisplayDialog(
                "Battle UI not found",
                "Select the affected Canvas or a parent UI object and run the command again.",
                "OK");
            return;
        }

        RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
        int repaired = 0;

        Undo.RegisterCompleteObjectUndo(rects, "Repair NaN UI Values");

        foreach (RectTransform rect in rects)
        {
            bool changed = false;

            Vector2 anchorMin = RepairVector(rect.anchorMin, Vector2.zero, ref changed);
            Vector2 anchorMax = RepairVector(rect.anchorMax, Vector2.one, ref changed);
            Vector2 pivot = RepairVector(rect.pivot, new Vector2(0.5f, 0.5f), ref changed);
            Vector2 anchoredPosition = RepairVector(rect.anchoredPosition, Vector2.zero, ref changed);
            Vector2 sizeDelta = RepairVector(rect.sizeDelta, Vector2.zero, ref changed);

            Vector3 localPosition = RepairVector3(rect.localPosition, Vector3.zero, ref changed);
            Vector3 localScale = RepairVector3(rect.localScale, Vector3.one, ref changed);
            Quaternion localRotation = RepairQuaternion(rect.localRotation, Quaternion.identity, ref changed);

            if (!changed)
                continue;

            // Apply anchors before position/size because Unity recalculates offsets from them.
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localPosition = localPosition;
            rect.localScale = localScale;
            rect.localRotation = localRotation;

            EditorUtility.SetDirty(rect);
            repaired++;
        }

        if (root.name == "Safe Area" || root.transform.Find("Safe Area") != null)
        {
            Transform safeTransform = root.name == "Safe Area" ? root.transform : root.transform.Find("Safe Area");
            if (safeTransform is RectTransform safe)
            {
                Undo.RecordObject(safe, "Reset Safe Area");
                safe.anchorMin = Vector2.zero;
                safe.anchorMax = Vector2.one;
                safe.pivot = new Vector2(0.5f, 0.5f);
                safe.anchoredPosition = Vector2.zero;
                safe.sizeDelta = Vector2.zero;
                safe.offsetMin = Vector2.zero;
                safe.offsetMax = Vector2.zero;
                EditorUtility.SetDirty(safe);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
        EditorUtility.DisplayDialog(
            "UI repair complete",
            $"Repaired {repaired} RectTransform(s).\n\nAny coordinate that had already become NaN was reset to 0 (or a safe default), so some manually positioned elements may need moving again.",
            "OK");
    }

    private static Vector2 RepairVector(Vector2 value, Vector2 fallback, ref bool changed)
    {
        float x = RepairFloat(value.x, fallback.x, ref changed);
        float y = RepairFloat(value.y, fallback.y, ref changed);
        return new Vector2(x, y);
    }

    private static Vector3 RepairVector3(Vector3 value, Vector3 fallback, ref bool changed)
    {
        float x = RepairFloat(value.x, fallback.x, ref changed);
        float y = RepairFloat(value.y, fallback.y, ref changed);
        float z = RepairFloat(value.z, fallback.z, ref changed);
        return new Vector3(x, y, z);
    }

    private static Quaternion RepairQuaternion(Quaternion value, Quaternion fallback, ref bool changed)
    {
        float x = RepairFloat(value.x, fallback.x, ref changed);
        float y = RepairFloat(value.y, fallback.y, ref changed);
        float z = RepairFloat(value.z, fallback.z, ref changed);
        float w = RepairFloat(value.w, fallback.w, ref changed);
        Quaternion repaired = new Quaternion(x, y, z, w);
        if (repaired == default)
        {
            changed = true;
            return fallback;
        }
        return repaired;
    }

    private static float RepairFloat(float value, float fallback, ref bool changed)
    {
        if (float.IsFinite(value))
            return value;

        changed = true;
        return fallback;
    }
}
#endif
