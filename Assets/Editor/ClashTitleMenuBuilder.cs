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

        RectTransform decorativeBackground = CreateRect("Decorative Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateDecorativeBackground(decorativeBackground);

        RectTransform backdrop = CreateRect("Backdrop", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backdrop.sizeDelta = new Vector2(-260, -160);
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
        RectTransform selectionCard = CreateRect("Selection Card", selectionView, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1120, 800));
        AddTinySwordsSprite(selectionCard.gameObject, TinySwordsUi + "/Papers/RegularPaper.png", "RegularPaper_4", Panel, false);
        RectTransform selectionBanner = CreateRect("Selection Banner", selectionCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 310), new Vector2(910, 120));
        AddTinySwordsSprite(selectionBanner.gameObject, TinySwordsUi + "/Banners/Banner.png", "Banner_4", Trim, false);
        AddText(selectionCard, "CHOOSE YOUR FACTION", 52, new Vector2(0, 310), new Vector2(900, 90));

        RectTransform content = CreateRect("Faction Options", selectionCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 5), new Vector2(960, 500));
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
        Button back = CreateButton(selectionCard, "Back Button", "BACK", new Vector2(0, -330));

        RectTransform difficultyView = CreateRect("Difficulty Selection View", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform difficultyCard = CreateRect("Difficulty Card", difficultyView, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 800));
        AddTinySwordsSprite(difficultyCard.gameObject, TinySwordsUi + "/Papers/RegularPaper.png", "RegularPaper_4", Panel, false);
        RectTransform difficultyBanner = CreateRect("Difficulty Banner", difficultyCard, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 245), new Vector2(650, 120));
        AddTinySwordsSprite(difficultyBanner.gameObject, TinySwordsUi + "/Banners/Banner.png", "Banner_4", Trim, false);
        AddText(difficultyBanner, "CHOOSE DIFFICULTY", 48, Vector2.zero, new Vector2(620, 90));
        Button easy = CreateButton(difficultyCard, "Easy Difficulty Button", "EASY", new Vector2(0, 95));
        Button medium = CreateButton(difficultyCard, "Medium Difficulty Button", "MEDIUM", new Vector2(0, -10));
        Button hard = CreateButton(difficultyCard, "Hard Difficulty Button", "HARD", new Vector2(0, -115));
        Button difficultyBack = CreateButton(difficultyCard, "Difficulty Back Button", "BACK", new Vector2(0, -245));

        TitleMenuController controller = canvasObject.GetComponent<TitleMenuController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("titleView").objectReferenceValue = titleView.gameObject;
        serializedController.FindProperty("factionSelectionView").objectReferenceValue = selectionView.gameObject;
        serializedController.FindProperty("difficultySelectionView").objectReferenceValue = difficultyView.gameObject;
        serializedController.FindProperty("factionCatalog").objectReferenceValue = catalog;
        serializedController.FindProperty("factionButtonContainer").objectReferenceValue = content;
        serializedController.FindProperty("factionButtonTemplate").objectReferenceValue = factionTemplate;
        serializedController.FindProperty("battleSceneBuildIndex").intValue = 1;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(play.onClick, controller.Play);
        UnityEventTools.AddPersistentListener(exit.onClick, controller.Exit);
        UnityEventTools.AddPersistentListener(back.onClick, controller.ShowTitle);
        UnityEventTools.AddPersistentListener(easy.onClick, controller.BeginEasyBattle);
        UnityEventTools.AddPersistentListener(medium.onClick, controller.BeginMediumBattle);
        UnityEventTools.AddPersistentListener(hard.onClick, controller.BeginHardBattle);
        UnityEventTools.AddPersistentListener(difficultyBack.onClick, controller.ShowFactionSelection);
        selectionView.gameObject.SetActive(false);
        difficultyView.gameObject.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"Created title menu scene at {ScenePath} and placed it first in Build Settings.");
    }

    private static void CreateDecorativeBackground(RectTransform parent)
    {
        RectTransform terrain = CreateRect("Green Terrain", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image terrainImage = AddTinySwordsSprite(
            terrain.gameObject,
            "Assets/Tiny Swords/Terrain/Tileset/Tilemap_color1.png",
            "Tilemap_color1_9",
            new Color32(103, 142, 76, 255),
            false);
        terrainImage.type = Image.Type.Tiled;
        terrainImage.color = new Color32(205, 222, 154, 255);

        CreateDecoration(parent, "Blue Castle", "Assets/Tiny Swords/Buildings/Blue Buildings/Castle.png", "Castle_0", new Vector2(-820, -330), new Vector2(210, 210), false);
        CreateDecoration(parent, "Red Castle", "Assets/Tiny Swords/Buildings/Red Buildings/Castle.png", "Castle_0", new Vector2(820, -330), new Vector2(210, 210), false);
        CreateDecoration(parent, "Purple House", "Assets/Tiny Swords/Buildings/Purple Buildings/House3.png", "House3_0", new Vector2(-825, 350), new Vector2(150, 150), false);
        CreateDecoration(parent, "Yellow House", "Assets/Tiny Swords/Buildings/Yellow Buildings/House3.png", "House3_0", new Vector2(825, 350), new Vector2(150, 150), false);
        CreateDecoration(parent, "Black Tower", "Assets/Tiny Swords/Buildings/Black Buildings/Tower.png", "Tower_0", new Vector2(0, 470), new Vector2(145, 145), false);

        string[] colours = { "Black", "Blue", "Purple", "Red", "Yellow" };
        string minotaurPath = "Assets/Tiny Swords - Enemy Pack/Enemies/Minotaur/Minotaur_Walk.png";

        for (int i = 0; i < colours.Length; i++)
        {
            bool moveRight = i % 2 == 0;
            float startX = -720f + i * 360f;
            string unitRoot = $"Assets/Tiny Swords/Units/{colours[i]} Units";
            float lowerLane = -510f + (i % 3) * 40f;
            float upperLane = 510f - (i % 3) * 40f;

            CreateRunner(parent, $"{colours[i]} Melee Runner", unitRoot + "/Warrior/Warrior_Run.png", new Vector2(startX, lowerLane), 78f + i * 7f, moveRight, new Vector2(100, 100));
            CreateRunner(parent, $"{colours[i]} Archer Runner", unitRoot + "/Archer/Archer_Run.png", new Vector2(-startX, upperLane), 72f + i * 6f, !moveRight, new Vector2(100, 100));
        }

        CreateRunner(parent, "Lower Left Minotaur", minotaurPath, new Vector2(-560, -500), 55f, true, new Vector2(135, 135));
        CreateRunner(parent, "Lower Right Minotaur", minotaurPath, new Vector2(560, -450), 62f, false, new Vector2(135, 135));
        CreateRunner(parent, "Upper Left Minotaur", minotaurPath, new Vector2(-420, 455), 58f, false, new Vector2(135, 135));
        CreateRunner(parent, "Upper Right Minotaur", minotaurPath, new Vector2(420, 505), 66f, true, new Vector2(135, 135));

        CreateSidePatrols(parent, "Left Side Patrols", -850f, true);
        CreateSidePatrols(parent, "Right Side Patrols", 850f, false);
    }

    private static void CreateSidePatrols(RectTransform parent, string name, float xPosition, bool startsRight)
    {
        RectTransform lane = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xPosition, 0), new Vector2(220, 650));
        string[] colours = startsRight
            ? new[] { "Black", "Blue", "Purple", "Red", "Yellow", "Black" }
            : new[] { "Yellow", "Red", "Purple", "Blue", "Black", "Yellow" };
        float[] laneHeights = { -285f, -170f, -55f, 55f, 170f, 285f };

        for (int i = 0; i < colours.Length; i++)
        {
            bool moveRight = (i % 2 == 0) == startsRight;
            bool archer = i % 2 == 1;
            string role = archer ? "Archer" : "Warrior";
            string sheet = archer ? "Archer_Run.png" : "Warrior_Run.png";
            string path = $"Assets/Tiny Swords/Units/{colours[i]} Units/{role}/{sheet}";
            CreateRunner(lane, $"{colours[i]} Side Runner {i + 1}", path, new Vector2(moveRight ? -75 : 75, laneHeights[i]), 42f + i * 6f, moveRight, new Vector2(88, 88));
        }
    }

    private static void CreateDecoration(RectTransform parent, string name, string path, string spriteName, Vector2 position, Vector2 size, bool flipX)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        Image image = AddTinySwordsSprite(rect.gameObject, path, spriteName, Color.clear, true);
        rect.localScale = new Vector3(flipX ? -1f : 1f, 1f, 1f);
        image.raycastTarget = false;
    }

    private static void CreateRunner(RectTransform parent, string name, string path, Vector2 position, float speed, bool moveRight, Vector2 size)
    {
        Sprite[] frames = LoadSprites(path);
        RectTransform rect = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = frames.Length > 0 ? frames[0] : null;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
        rect.localScale = new Vector3(moveRight ? 1f : -1f, 1f, 1f);

        TitleMenuBackgroundActor actor = rect.gameObject.AddComponent<TitleMenuBackgroundActor>();
        actor.Configure(frames, speed, 10f, moveRight, 90f);
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

    private static Sprite[] LoadSprites(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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
