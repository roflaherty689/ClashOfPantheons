#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ClashProductionCardBindingMigration
{
    private const string BattleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ProductionPath = "Safe Area/Bottom HUD/Independent Production";

    [MenuItem("Tools/Clash of Pantheons/Bind Production Card Views")]
    public static void BindFromMenu()
    {
        BindProductionCards();
    }

    public static void BindProductionCards()
    {
        Scene scene = SceneManager.GetSceneByPath(BattleScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("Production-card binding migration cancelled; no scenes were changed.");
                return;
            }

            scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
        }

        GameObject battleUi = FindRoot(scene, "Battle UI");
        Transform production = battleUi != null ? battleUi.transform.Find(ProductionPath) : null;
        if (production == null) throw new MissingReferenceException("The battle production-card hierarchy was not found.");

        EnsureMonkCard(production);
        BindCard(production, ProductionSlotId.Standard0, "MELEE");
        BindCard(production, ProductionSlotId.Standard1, "ARCHER");
        BindCard(production, ProductionSlotId.Standard2, "CAVALRY");
        BindCard(production, ProductionSlotId.Standard3, "SIEGE");
        BindCard(production, ProductionSlotId.Standard4, "MONK");
        BindCard(production, ProductionSlotId.Mythic, "MYTHIC");
        LayoutCards(production);

        ProductionCardView[] views = production.GetComponentsInChildren<ProductionCardView>(true);
        if (views.Length != 6) throw new InvalidOperationException($"Expected six bound production cards, found {views.Length}.");

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new InvalidOperationException("Unity could not save the production-card binding migration.");
        Debug.Log("Bound all six production cards through serialized ProductionCardView references.");
    }

    private static void EnsureMonkCard(Transform production)
    {
        if (production.Find("MONK Production") != null) return;

        Transform source = production.Find("SIEGE Production");
        if (source == null) throw new MissingReferenceException("Cannot create the monk card without the SIEGE production card template.");

        GameObject clone = UnityEngine.Object.Instantiate(source.gameObject, production);
        clone.name = "MONK Production";
        Transform card = clone.transform;

        Transform art = card.Find("SIEGE Art");
        if (art != null) art.name = "MONK Art";

        Transform button = card.Find("Unlock SIEGE");
        if (button != null) button.name = "Unlock MONK";

        TextMeshProUGUI title = FindDirectText(card, "SIEGE");
        if (title != null) title.text = "MONK";
    }

    private static void LayoutCards(Transform production)
    {
        string[] roles = { "MELEE", "ARCHER", "CAVALRY", "SIEGE", "MONK", "MYTHIC" };
        for (int index = 0; index < roles.Length; index++)
        {
            RectTransform card = production.Find(roles[index] + " Production") as RectTransform;
            if (card == null) continue;

            card.anchoredPosition = new Vector2(-595f + index * 238f, -5f);
            card.sizeDelta = new Vector2(218f, 274f);

            RectTransform button = card.Find("Unlock " + roles[index]) as RectTransform;
            if (button != null) button.sizeDelta = new Vector2(196f, 52f);
        }
    }

    private static void BindCard(
        Transform production,
        ProductionSlotId slotId,
        string roleName)
    {
        Transform card = production.Find(roleName + " Production");
        if (card == null) throw new MissingReferenceException($"Missing {roleName} production card.");

        Image interactionGraphic = card.GetComponent<Image>();
        Image art = Require<Image>(card, roleName + " Art");
        Image portraitPaper = Require<Image>(card, "Portrait Paper");
        Button button = Require<Button>(card, "Unlock " + roleName);
        TextMeshProUGUI actionText = button.GetComponentInChildren<TextMeshProUGUI>(true);
        TextMeshProUGUI titleText = FindDirectText(card, roleName);
        TextMeshProUGUI statusText = FindDirectText(card, "LOCKED");
        TextMeshProUGUI tierText = FindDirectText(card, "STARS");
        if (interactionGraphic == null || actionText == null || titleText == null ||
            statusText == null || tierText == null)
            throw new MissingReferenceException($"{roleName} production card has incomplete presentation children.");

        ProductionCardView view = card.GetComponent<ProductionCardView>();
        if (view == null) view = card.gameObject.AddComponent<ProductionCardView>();
        view.Configure(
            slotId,
            interactionGraphic,
            button,
            art,
            portraitPaper,
            titleText,
            statusText,
            tierText,
            actionText);
        EditorUtility.SetDirty(view);
    }

    private static T Require<T>(Transform card, string childName) where T : Component
    {
        Transform child = card.Find(childName);
        T component = child != null ? child.GetComponent<T>() : null;
        if (component == null) throw new MissingReferenceException($"{card.name} has no '{childName}' {typeof(T).Name}.");
        return component;
    }

    private static TextMeshProUGUI FindDirectText(Transform card, string content)
    {
        for (int i = 0; i < card.childCount; i++)
            if (card.GetChild(i).TryGetComponent(out TextMeshProUGUI text) && text.text.Contains(content)) return text;
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
