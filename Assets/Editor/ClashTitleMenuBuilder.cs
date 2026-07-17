#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;

public static class ClashTitleMenuBuilder
{
    private const string ScenePath = "Assets/Scenes/TitleMenu.unity";
    private const string CatalogPath = "Assets/ScriptableObjects/Factions/FactionCatalog.asset";
    private const string FactionRoot = "Assets/ScriptableObjects/Factions";
    private const string TinySwordsUi = "Assets/Tiny Swords/UI Elements";
    private static readonly Color Background = new Color32(24, 31, 43, 255);
    private static readonly Color Panel = new Color32(48, 52, 58, 245);
    private static readonly Color Trim = new Color32(203, 157, 70, 255);

    [MenuItem("Tools/Clash of Pantheons/Create Title Menu Scene")]
    public static void CreateTitleMenuScene()
    {
        FactionCatalog catalog = CreateOrUpdateFactionCatalog();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        camera.orthographic = true;

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystem.GetComponent<EventSystem>().sendNavigationEvents = true;

        GameObject canvasObject = new GameObject("Title Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(TitleMenuController));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform backdrop = CreateRect("Backdrop", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image backdropImage = AddTinySwordsSprite(backdrop.gameObject, TinySwordsUi + "/Wood Table/WoodTable.png", "WoodTable_4", Background, false);
        backdropImage.color = new Color32(72, 58, 48, 255);

        RectTransform titleView = CreateRect("Title View", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform card = CreateRect("Menu Card", titleView, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660, 640));
        AddTinySwordsSprite(card.gameObject, TinySwordsUi + "/Papers/RegularPaper.png", "RegularPaper_4", Panel, false);
        RectTransform titleBanner = CreateRect("Title Banner", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(610, 230));
        AddTinySwordsSprite(titleBanner.gameObject, TinySwordsUi + "/Banners/Banner.png", "Banner_4", Trim, false);
        AddText(card, "CLASH OF\nPANTHEONS", 46, new Vector2(0, 180), new Vector2(430, 125));

        Button play = CreateButton(card, "Play Button", "PLAY", new Vector2(0, -40));
        Button exit = CreateButton(card, "Exit Button", "EXIT", new Vector2(0, -160));

        RectTransform selectionView = CreateRect("Faction Selection View", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform selectionCard = CreateRect("Selection Card", selectionView, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1120, 760));
        AddTinySwordsSprite(selectionCard.gameObject, TinySwordsUi + "/Papers/RegularPaper.png", "RegularPaper_4", Panel, false);
        RectTransform selectionBanner = CreateRect("Selection Banner", selectionCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 290), new Vector2(910, 120));
        AddTinySwordsSprite(selectionBanner.gameObject, TinySwordsUi + "/Banners/Banner.png", "Banner_4", Trim, false);
        AddText(selectionCard, "CHOOSE YOUR FACTION", 52, new Vector2(0, 290), new Vector2(900, 90));

        RectTransform content = CreateRect("Faction Options", selectionCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(960, 440));
        Image factionPanel = AddTinySwordsSprite(content.gameObject, TinySwordsUi + "/Wood Table/WoodTable_Slots.png", "WoodTable_Slots_0", new Color32(31, 35, 40, 220), false);
        factionPanel.color = new Color32(108, 82, 61, 255);
        GridLayoutGroup layout = content.gameObject.AddComponent<GridLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = new Vector2(16, 16);
        layout.cellSize = new Vector2(448, 82);
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 2;

        Button factionTemplate = CreateFactionButtonTemplate(content);
        Button back = CreateButton(selectionCard, "Back Button", "BACK", new Vector2(0, -305));

        TitleMenuController controller = canvasObject.GetComponent<TitleMenuController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("titleView").objectReferenceValue = titleView.gameObject;
        serializedController.FindProperty("factionSelectionView").objectReferenceValue = selectionView.gameObject;
        serializedController.FindProperty("factionCatalog").objectReferenceValue = catalog;
        serializedController.FindProperty("factionButtonContainer").objectReferenceValue = content;
        serializedController.FindProperty("factionButtonTemplate").objectReferenceValue = factionTemplate;
        serializedController.FindProperty("battleSceneBuildIndex").intValue = 1;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(play.onClick, controller.Play);
        UnityEventTools.AddPersistentListener(exit.onClick, controller.Exit);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowTitle);
        selectionView.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"Created title menu scene at {ScenePath} and placed it first in Build Settings.");
    }

    private static FactionCatalog CreateOrUpdateFactionCatalog()
    {
        FactionCatalog catalog = AssetDatabase.LoadAssetAtPath<FactionCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<FactionCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        FactionData[] factions = AssetDatabase.FindAssets("t:FactionData", new[] { FactionRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => string.Equals(
                Path.GetDirectoryName(path)?.Replace('\\', '/'),
                FactionRoot,
                StringComparison.OrdinalIgnoreCase))
            .Select(AssetDatabase.LoadAssetAtPath<FactionData>)
            .Where(faction => faction != null)
            .OrderBy(faction => faction.FactionName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        SerializedObject serializedCatalog = new SerializedObject(catalog);
        SerializedProperty entries = serializedCatalog.FindProperty("factions");
        entries.arraySize = factions.Length;
        for (int i = 0; i < factions.Length; i++)
        {
            entries.GetArrayElementAtIndex(i).objectReferenceValue = factions[i];
        }
        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static Button CreateFactionButtonTemplate(RectTransform parent)
    {
        RectTransform rect = CreateRect("Faction Button Template", parent, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(448, 82));
        Image image = AddTinySwordsSprite(rect.gameObject, TinySwordsUi + "/Buttons/BigBlueButton_Regular.png", "BigBlueButton_Regular_4", new Color32(42, 111, 138, 255), false);
        image.raycastTarget = true;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform icon = CreateRect("Faction Icon", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-105, 0), new Vector2(72, 72));
        Image iconImage = AddImage(icon.gameObject, Color.white);
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        RectTransform labelRect = CreateRect("Faction Label", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(45, 0), new Vector2(200, 62));
        TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = "FACTION";
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18;
        label.fontSizeMax = 32;
        label.raycastTarget = false;
        button.gameObject.SetActive(false);
        return button;
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
        };
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Image AddTinySwordsSprite(GameObject target, string path, string spriteName, Color fallback, bool preserveAspect)
    {
        Image image = target.AddComponent<Image>();
        image.sprite = LoadSprite(path, spriteName);
        image.color = image.sprite == null ? fallback : Color.white;
        image.type = image.sprite != null && image.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        if (image.sprite == null)
            Debug.LogWarning($"Title menu could not load Tiny Swords sprite '{spriteName}' from '{path}'.");
        return image;
    }

    private static Sprite LoadSprite(string path, string spriteName)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            if (asset is Sprite sprite && sprite.name == spriteName)
                return sprite;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void AddOutline(GameObject target)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = Trim;
        outline.effectDistance = new Vector2(4, -4);
    }

    private static TMP_Text AddText(RectTransform parent, string value, float size, Vector2 position, Vector2 dimensions)
    {
        RectTransform rect = CreateRect(value.Replace('\n', ' '), parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, dimensions);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18;
        text.fontSizeMax = size;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(RectTransform parent, string name, string label, Vector2 position)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(430, 82));
        Image image = AddTinySwordsSprite(rect.gameObject, TinySwordsUi + "/Buttons/BigBlueButton_Regular.png", "BigBlueButton_Regular_4", new Color32(42, 111, 138, 255), false);
        image.raycastTarget = true;
        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = Trim;
        outline.effectDistance = new Vector2(3, -3);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color32(62, 145, 174, 255);
        colors.pressedColor = new Color32(29, 77, 100, 255);
        button.colors = colors;
        AddText(rect, label, 34, Vector2.zero, new Vector2(380, 58));
        return button;
    }
}
#endif
