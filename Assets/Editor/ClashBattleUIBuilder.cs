#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class ClashBattleUIBuilder
{
    private const string RootName = "Battle UI";
    private const string TinySwords = "Assets/Tiny Swords";
    private const string UiRoot = TinySwords + "/UI Elements";

    private static readonly Color Ink = new Color32(30, 31, 31, 248);
    private static readonly Color Panel = new Color32(50, 48, 43, 248);
    private static readonly Color Trim = new Color32(203, 157, 70, 255);
    private static readonly Color Gold = new Color32(255, 218, 82, 255);
    private static readonly Color Muted = new Color32(190, 188, 178, 255);

    [MenuItem("Tools/Clash of Pantheons/Create Battle UI")]
    public static void CreateBattleUI()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null && !EditorUtility.DisplayDialog(
                "Replace Battle UI?",
                "A Battle UI object already exists. Replace it with the Tiny Swords battle HUD?",
                "Replace",
                "Cancel"))
            return;

        if (old != null)
            Object.DestroyImmediate(old);

        EnsureEventSystem();

        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Create Battle UI");
        root.GetComponent<RectTransform>().localScale = Vector3.one;

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2560, 1440);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.AddComponent<BattleEconomyUI>();

        RectTransform safe = CreateRect("Safe Area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        safe.gameObject.AddComponent<BattleSafeArea>();

        CreateTopHUD(safe, out Image leftCastleIcon, out Image rightCastleIcon);
        CreateBottomHUD(safe);
        CreateOverlay(safe);
        AssignFactionCastleIcons(leftCastleIcon, rightCastleIcon);

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("Created the Tiny Swords battle HUD with runtime binding targets. Verify generated references and layout in Play Mode.");
    }

    private static void CreateTopHUD(
        RectTransform parent,
        out Image leftCastleIcon,
        out Image rightCastleIcon)
    {
        RectTransform top = CreateRect("Top HUD", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -63), new Vector2(-32, 110));

        leftCastleIcon = CreateTeamHeader(top, "Player Stronghold", false, "PLAYER", "50 / 50");
        rightCastleIcon = CreateTeamHeader(top, "Enemy Stronghold", true, "ENEMY", "50 / 50");

        RectTransform timer = CreateRect("Match Timer", top, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(330, 104));
        AddSpriteOrPanel(timer.gameObject, UiRoot + "/Banners/Banner.png", "Banner_4", new Color32(225, 207, 159, 255), false);
        AddText(timer, "05:00", 46, TextAlignmentOptions.Center, new Vector2(0, 5), new Vector2(260, 58), new Color32(52, 39, 32, 255)).gameObject.name = "Time Remaining";
        AddText(timer, "MATCH TIME", 17, TextAlignmentOptions.Center, new Vector2(0, -30), new Vector2(180, 24), new Color32(78, 58, 40, 255));
    }

    private static Image CreateTeamHeader(RectTransform parent, string name, bool enemy, string label, string health)
    {
        Vector2 edgeAnchor = new Vector2(enemy ? 0.95f : 0.05f, 0.5f);
        RectTransform header = CreateRect(
            name,
            parent,
            edgeAnchor,
            edgeAnchor,
            new Vector2(enemy ? -425 : 425, 0),
            new Vector2(850, 76));
        AddPanel(header.gameObject, Ink);
        AddBorder(header, Trim, 3);

        string colour = enemy ? "Red" : "Black";
        string castlePath = TinySwords + "/Buildings/" + colour + " Buildings/Castle.png";
        RectTransform castle = CreateRect((enemy ? "Right" : "Left") + " Castle Icon", header, new Vector2(enemy ? 1 : 0, 0.5f), new Vector2(enemy ? 1 : 0, 0.5f), new Vector2(enemy ? -35 : 35, -1), new Vector2(70, 70));
        Image castleIcon = AddSpriteOrPanel(castle.gameObject, castlePath, "Castle_0", Color.white, true);

        Vector2 labelAnchor = new Vector2(enemy ? 1 : 0, 0.5f);
        AddText(header, label, 29, enemy ? TextAlignmentOptions.Right : TextAlignmentOptions.Left,
            new Vector2(enemy ? -145 : 145, 0), new Vector2(150, 48), Color.white, labelAnchor);

        RectTransform healthBar = CreateRect("Health Bar", header, labelAnchor, labelAnchor,
            new Vector2(enemy ? -500 : 500, 0), new Vector2(540, 54));
        AddPanel(healthBar.gameObject, new Color32(18, 20, 20, 255));
        CreateHealthBarFrame(healthBar);
        RectTransform fill = CreateRect("Health Fill", healthBar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(470, 32));
        Image fillImage = AddSpriteOrPanel(fill.gameObject, UiRoot + "/Bars/BigBar_Fill.png", "BigBar_Fill_0",
            enemy ? new Color32(178, 49, 50, 255) : new Color32(35, 121, 170, 255), false);
        fillImage.color = enemy ? new Color32(195, 61, 58, 255) : new Color32(52, 148, 190, 255);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = enemy ? 1 : 0;
        fillImage.fillAmount = 1f;
        AddText(healthBar, health, 24, TextAlignmentOptions.Center, Vector2.zero, new Vector2(450, 36)).gameObject.name = "Stronghold Health Total";
        return castleIcon;
    }

    private static void AssignFactionCastleIcons(Image leftCastleIcon, Image rightCastleIcon)
    {
        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("Battle UI could not bind faction castle icons because no GameManager exists in the scene.");
            return;
        }

        SerializedObject serializedGameManager = new SerializedObject(gameManager);
        serializedGameManager.FindProperty("leftCastleIcon").objectReferenceValue = leftCastleIcon;
        serializedGameManager.FindProperty("rightCastleIcon").objectReferenceValue = rightCastleIcon;
        serializedGameManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameManager);
    }

    private static void CreateHealthBarFrame(RectTransform parent)
    {
        const float capWidth = 26f;
        const float frameHeight = 54f;
        string framePath = UiRoot + "/Bars/BigBar_Base.png";

        RectTransform leftCap = CreateRect("Frame Left", parent, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(capWidth * 0.5f, 0), new Vector2(capWidth, frameHeight));
        AddSpriteOrPanel(leftCap.gameObject, framePath, "BigBar_Base_0", Color.white, false);

        // RectTransform middle = CreateRect("Frame Middle", parent, new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(-capWidth * 2f, frameHeight));
        // AddSpriteOrPanel(middle.gameObject, framePath, "BigBar_Base_1", Color.white, false);

        RectTransform rightCap = CreateRect("Frame Right", parent, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-capWidth * 0.5f, 0), new Vector2(capWidth, frameHeight));
        AddSpriteOrPanel(rightCap.gameObject, framePath, "BigBar_Base_2", Color.white, false);
    }

    private static void CreateBottomHUD(RectTransform parent)
    {
        RectTransform bottom = CreateRect("Bottom HUD", parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 196), new Vector2(0, 392));
        AddPanel(bottom.gameObject, Ink);
        RectTransform wood = CreateRect("Wood Table Backdrop", bottom, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8, -8));
        Image woodImage = AddSpriteOrPanel(wood.gameObject, UiRoot + "/Wood Table/WoodTable.png", "WoodTable_4", Panel, false);
        woodImage.color = new Color32(116, 83, 62, 255);
        AddBorder(bottom, Trim, 4);

        CreateEconomyPanel(bottom);
        CreateProductionPanel(bottom);
        CreateRoleDetailPanel(bottom);
        CreateBattleSummary(bottom);
    }

    private static void CreateEconomyPanel(RectTransform parent)
    {
        RectTransform economy = CreateRect("Economy", parent, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(176, 16), new Vector2(320, 330));
        AddPanel(economy.gameObject, Panel);
        AddBorder(economy, Trim, 3);
        AddText(economy, "RESOURCES", 24, TextAlignmentOptions.Center, new Vector2(0, 132), new Vector2(270, 38), Gold);

        RectTransform coin = CreateRect("Gold Icon", economy, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, 66), new Vector2(54, 54));
        AddSpriteOrPanel(coin.gameObject, UiRoot + "/Icons/Icon_03.png", "Icon_03_0", Gold, true);
        AddText(economy, "540", 36, TextAlignmentOptions.Left, new Vector2(12, 72), new Vector2(170, 48)).gameObject.name = "Gold Total";
        AddText(economy, "+12 PER TRIP", 17, TextAlignmentOptions.Left, new Vector2(16, 43), new Vector2(180, 28), Gold).gameObject.name = "Gold Per Trip";

        RectTransform divider = CreateRect("Divider", economy, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 16), new Vector2(270, 2));
        AddPanel(divider.gameObject, new Color32(133, 112, 71, 255));

        RectTransform worker = CreateRect("Worker Icon", economy, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-100, -39), new Vector2(68, 68));
        AddSpriteOrPanel(worker.gameObject, TinySwords + "/Pawn and Resources/Pawn/Blue Pawn/Pawn_Idle.png", "Pawn_Idle_0", Color.white, true);
        AddText(economy, "WORKERS", 18, TextAlignmentOptions.Left, new Vector2(19, -22), new Vector2(170, 28), Muted);
        AddText(economy, "2 / 5", 32, TextAlignmentOptions.Left, new Vector2(19, -54), new Vector2(170, 42)).gameObject.name = "Worker Total";
        CreateWorkerButton(economy);
    }

    private static void CreateWorkerButton(RectTransform parent)
    {
        RectTransform button = CreateRect("Buy Worker", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -119), new Vector2(270, 58));
        Image buttonImage = AddSpriteOrPanel(
            button.gameObject,
            UiRoot + "/Buttons/BigBlueButton_Regular.png",
            "BigBlueButton_Regular_4",
            new Color32(42, 111, 138, 255),
            false);
        Button selectable = button.gameObject.AddComponent<Button>();
        selectable.targetGraphic = buttonImage;
        buttonImage.raycastTarget = true;

        AddText(button, "BUY WORKER  100", 14, TextAlignmentOptions.Center, new Vector2(-18, 0), new Vector2(155, 30));
        RectTransform coin = CreateRect("Gold Icon", button, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(72, 0), new Vector2(24, 24));
        AddSpriteOrPanel(coin.gameObject, UiRoot + "/Icons/Icon_03.png", "Icon_03_0", Gold, true);
    }

    private static void CreateProductionPanel(RectTransform parent)
    {
        RectTransform production = CreateRect("Independent Production", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-35, 17), new Vector2(1460, 332));
        AddPanel(production.gameObject, new Color32(38, 38, 36, 248));

        string[] roles = { "MELEE", "ARCHER", "CAVALRY", "SIEGE", "MYTHIC" };
        string[] costs = { "50", "60", "100", "130", "220" };
        string[] artPaths =
        {
            UiRoot + "/Human Avatars/Avatars_01.png",
            UiRoot + "/Human Avatars/Avatars_03.png",
            UiRoot + "/Human Avatars/Avatars_02.png",
            "Assets/Tiny Swords - Enemy Pack/Enemy Avatars/Enemy Avatars_16.png",
            UiRoot + "/Icons/Icon_05.png"
        };
        string[] artSprites = { "Avatars_01_0", "Avatars_03_0", "Avatars_02_0", "Enemy Avatars_16_0", "Icon_05_0" };

        for (int i = 0; i < roles.Length; i++)
        {
            float x = -568 + i * 284;
            CreateRoleCard(production, roles[i], costs[i], artPaths[i], artSprites[i], new Vector2(x, -5));
        }
    }

    private static void CreateRoleCard(RectTransform parent, string role, string cost, string artPath, string artSprite, Vector2 pos)
    {
        RectTransform card = CreateRect(role + " Production", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(258, 274));
        AddPanel(card.gameObject, Panel);
        AddBorder(card, new Color32(126, 109, 75, 255), 3);

        AddText(card, role, 23, TextAlignmentOptions.Center, new Vector2(0, 111), new Vector2(220, 34), Color.white);
        RectTransform portraitPaper = CreateRect("Portrait Paper", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 32), new Vector2(154, 142));
        AddSpriteOrPanel(portraitPaper.gameObject, UiRoot + "/Papers/RegularPaper.png", "RegularPaper_4", new Color32(212, 196, 151, 255), false);
        RectTransform art = CreateRect(role + " Art", card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 32), new Vector2(132, 132));
        AddSpriteOrPanel(art.gameObject, artPath, artSprite, Color.white, true);

        AddText(card, "LOCKED", 21, TextAlignmentOptions.Center, new Vector2(0, -47), new Vector2(210, 30), Gold);
        AddText(card, "0 / 3 STARS", 17, TextAlignmentOptions.Center, new Vector2(0, -74), new Vector2(210, 26), Muted);

        RectTransform button = CreateRect("Unlock " + role, card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -111), new Vector2(218, 52));
        Image buttonImage = AddSpriteOrPanel(button.gameObject, UiRoot + "/Buttons/BigBlueButton_Regular.png", "BigBlueButton_Regular_4", new Color32(43, 102, 127, 255), false);
        Button selectable = button.gameObject.AddComponent<Button>();
        selectable.targetGraphic = buttonImage;
        buttonImage.raycastTarget = true;
        RectTransform coin = CreateRect("Gold Icon", button, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-48, 0), new Vector2(30, 30));
        AddSpriteOrPanel(coin.gameObject, UiRoot + "/Icons/Icon_03.png", "Icon_03_0", Gold, true);
        AddText(button, cost, 22, TextAlignmentOptions.Left, new Vector2(31, 0), new Vector2(90, 32), Color.white);
    }

    private static void CreateRoleDetailPanel(RectTransform parent)
    {
        RectTransform detail = CreateRect("Selected Role", parent, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-252, 17), new Vector2(448, 332));
        AddPanel(detail.gameObject, Panel);
        AddBorder(detail, Trim, 3);
        AddText(detail, "SELECTED ROLE", 19, TextAlignmentOptions.Left, new Vector2(-92, 132), new Vector2(210, 28), Gold);
        AddText(detail, "MELEE", 30, TextAlignmentOptions.Left, new Vector2(-93, 99), new Vector2(210, 40));

        RectTransform icon = CreateRect("Role Icon", detail, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150, 112), new Vector2(108, 108));
        AddSpriteOrPanel(icon.gameObject, UiRoot + "/Human Avatars/Avatars_01.png", "Avatars_01_0", Color.white, true);

        AddText(detail, "STATUS", 17, TextAlignmentOptions.Left, new Vector2(-150, 48), new Vector2(110, 26), Muted);
        AddText(detail, "LOCKED", 20, TextAlignmentOptions.Right, new Vector2(99, 48), new Vector2(220, 30), Gold);
        AddText(detail, "TIER", 17, TextAlignmentOptions.Left, new Vector2(-150, 15), new Vector2(110, 26), Muted);
        AddText(detail, "0 / 3 STARS", 20, TextAlignmentOptions.Right, new Vector2(99, 15), new Vector2(220, 30));
        AddText(detail, "Unlock to begin recurring melee production. Upgrades affect future spawns only.", 18,
            TextAlignmentOptions.TopLeft, new Vector2(0, -74), new Vector2(380, 64), new Color32(225, 224, 216, 255), null, true);
        CreateButton(detail, "Role Action", "UNLOCK   50 GOLD", new Vector2(0, -126), new Vector2(380, 54), false, false);
    }

    private static void CreateBattleSummary(RectTransform parent)
    {
        RectTransform summary = CreateRect("Battle Summary", parent, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 15), new Vector2(1230, 34));
        AddText(summary, "WORKERS  2 / 5     |     FRIENDLY UNITS  8     |     ENEMY UNITS  8", 19,
            TextAlignmentOptions.Center, Vector2.zero, new Vector2(1100, 30), Muted).gameObject.name = "Worker Battle Summary";

        RectTransform crossedSwords = CreateRect("Battle Icon", summary, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 25), new Vector2(54, 54));
        AddSpriteOrPanel(crossedSwords.gameObject, UiRoot + "/Swords/Swords 1.png", "Swords_2", Color.white, true);
    }

    private static void CreateOverlay(RectTransform parent)
    {
        RectTransform overlay = CreateRect("Game Over Overlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup group = overlay.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
        AddPanel(overlay.gameObject, new Color32(0, 0, 0, 190));

        RectTransform result = CreateRect("Result Panel", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 330));
        AddPanel(result.gameObject, Ink);
        AddBorder(result, Trim, 5);
        AddText(result, "VICTORY", 72, TextAlignmentOptions.Center, new Vector2(0, 70), new Vector2(780, 100), Gold);
        AddText(result, "ENEMY STRONGHOLD DESTROYED", 25, TextAlignmentOptions.Center, new Vector2(0, 9), new Vector2(700, 42));
        CreateButton(result, "Restart Match", "PLAY AGAIN", new Vector2(0, -85), new Vector2(330, 70), false, false);
    }

    private static void CreateButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, bool round, bool red)
    {
        RectTransform button = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        string shape = round ? "Small" + (red ? "Red" : "Blue") + "RoundButton_Regular" : "Big" + (red ? "Red" : "Blue") + "Button_Regular";
        string path = UiRoot + "/Buttons/" + shape + ".png";
        string sprite = shape + (round ? "_0" : "_4");
        Image image = AddSpriteOrPanel(button.gameObject, path, sprite, red ? new Color32(135, 52, 48, 255) : new Color32(42, 111, 138, 255), false);
        Button selectable = button.gameObject.AddComponent<Button>();
        selectable.targetGraphic = image;
        image.raycastTarget = true;
        AddText(button, label, round ? 22 : 19, TextAlignmentOptions.Center, Vector2.zero, size);
    }

    private static void AddBorder(RectTransform parent, Color colour, float thickness)
    {
        CreateBorderEdge(parent, "Top Border", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -thickness * 0.5f), new Vector2(0, thickness), colour);
        CreateBorderEdge(parent, "Bottom Border", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, thickness * 0.5f), new Vector2(0, thickness), colour);
        CreateBorderEdge(parent, "Left Border", new Vector2(0, 0), new Vector2(0, 1), new Vector2(thickness * 0.5f, 0), new Vector2(thickness, 0), colour);
        CreateBorderEdge(parent, "Right Border", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-thickness * 0.5f, 0), new Vector2(thickness, 0), colour);
    }

    private static void CreateBorderEdge(RectTransform parent, string name, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Color colour)
    {
        RectTransform edge = CreateRect(name, parent, min, max, pos, size);
        AddPanel(edge.gameObject, colour);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        return rect;
    }

    private static Image AddPanel(GameObject go, Color colour)
    {
        Image image = go.AddComponent<Image>();
        image.color = colour;
        image.raycastTarget = false;
        return image;
    }

    private static Image AddSpriteOrPanel(GameObject go, string path, string spriteName, Color fallback, bool preserveAspect)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = LoadSprite(path, spriteName);
        image.color = image.sprite == null ? fallback : Color.white;
        image.type = image.sprite != null && image.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = false;
        if (image.sprite == null)
            Debug.LogWarning("Battle UI could not load sprite '" + spriteName + "' from '" + path + "'. Using a fallback colour.");
        return image;
    }

    private static TextMeshProUGUI AddText(
        RectTransform parent,
        string text,
        float size,
        TextAlignmentOptions alignment,
        Vector2 pos,
        Vector2 boxSize,
        Color? colour = null,
        Vector2? anchor = null,
        bool wrap = false)
    {
        Vector2 resolvedAnchor = anchor ?? new Vector2(0.5f, 0.5f);
        RectTransform rect = CreateRect(text + " Text", parent, resolvedAnchor, resolvedAnchor, pos, boxSize);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = alignment;
        label.color = colour ?? Color.white;
        label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        label.fontStyle = FontStyles.Bold;
        return label;
    }

    private static Sprite LoadSprite(string path, string spriteName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
            if (asset is Sprite sprite && sprite.name == spriteName)
                return sprite;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
            return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
    }
}
#endif
