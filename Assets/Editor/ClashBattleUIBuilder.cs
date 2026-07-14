#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ClashBattleUIBuilder
{
    private const string RootName = "Battle UI";
    private const string UiRoot = "Assets/Tiny Swords/UI Elements";

    [MenuItem("Tools/Clash of Pantheons/Create Rough Battle UI")]
    public static void CreateBattleUI()
    {
        GameObject old = GameObject.Find(RootName);
        if (old != null && !EditorUtility.DisplayDialog(
                "Replace Battle UI?",
                "A Battle UI object already exists. Replace it?",
                "Replace",
                "Cancel"))
            return;

        if (old != null)
            Object.DestroyImmediate(old);

        EnsureEventSystem();

        GameObject root = new GameObject(RootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Create Battle UI");

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2560, 1440);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform safe = CreateRect("Safe Area", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        safe.gameObject.AddComponent<BattleSafeArea>();

        CreateTopHUD(safe);
        CreateLeftHUD(safe);
        CreateRightHUD(safe);
        CreateBottomHUD(safe);
        CreateOverlay(safe);

        Selection.activeGameObject = root;
        EditorUtility.SetDirty(root);
        Debug.Log("Created rough Clash of Pantheons battle UI. Adjust it under the 'Battle UI' object.");
    }

    private static void CreateTopHUD(RectTransform parent)
    {
        RectTransform top = CreateRect("Top HUD", parent, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -118), new Vector2(0, 118));

        RectTransform left = CreateRect("Left Base", top, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(380, 0), new Vector2(700, 84));
        AddPanel(left.gameObject, new Color32(24, 38, 44, 235));
        AddText(left, "PLAYER 1", 34, TextAlignmentOptions.Left, new Vector2(24, 0), new Vector2(190, 60));
        CreateHealthBar(left, "Left Health", new Vector2(185, 0), new Vector2(480, 42), new Color32(47, 174, 214, 255), "50 / 50");

        RectTransform timer = CreateRect("Timer", top, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250, 92));
        AddSpriteOrPanel(timer.gameObject, $"{UiRoot}/Banners/Banner.png", "Banner_0", new Color32(226, 205, 158, 255));
        AddText(timer, "03:00", 46, TextAlignmentOptions.Center, Vector2.zero, new Vector2(220, 70), new Color32(55, 43, 37, 255));

        RectTransform right = CreateRect("Right Base", top, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-380, 0), new Vector2(700, 84));
        AddPanel(right.gameObject, new Color32(44, 31, 35, 235));
        AddText(right, "PLAYER 2", 34, TextAlignmentOptions.Right, new Vector2(-24, 0), new Vector2(190, 60), Color.white, new Vector2(1, 0.5f));
        CreateHealthBar(right, "Right Health", new Vector2(-185, 0), new Vector2(480, 42), new Color32(219, 63, 72, 255), "50 / 50", true);
    }

    private static void CreateLeftHUD(RectTransform parent)
    {
        RectTransform left = CreateRect("Left HUD", parent, new Vector2(0, 0), new Vector2(0, 1), new Vector2(18, 170), new Vector2(330, -340));

        RectTransform resources = CreateRect("Resources", left, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -185), new Vector2(310, 330));
        AddFramedPanel(resources.gameObject, "RESOURCES");
        AddStatLine(resources, "Gold", "500", "+12 /s", 78);
        AddStatLine(resources, "Favour", "250", "+6 /s", 142);
        AddStatLine(resources, "Essence", "100", "+3 /s", 206);

        RectTransform workers = CreateRect("Workers", left, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -525), new Vector2(310, 260));
        AddFramedPanel(workers.gameObject, "WORKERS");
        AddText(workers, "2 / 5", 38, TextAlignmentOptions.Center, new Vector2(0, 20), new Vector2(240, 70));
        CreateButton(workers, "Buy Worker", "BUY WORKER  100", new Vector2(0, -70), new Vector2(240, 64), false);
    }

    private static void CreateRightHUD(RectTransform parent)
    {
        RectTransform right = CreateRect("Right HUD", parent, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-18, 170), new Vector2(330, -340));

        RectTransform info = CreateRect("Battle Info", right, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -195), new Vector2(310, 350));
        AddFramedPanel(info.gameObject, "BATTLE INFO");
        AddText(info, "UNITS ON FIELD", 22, TextAlignmentOptions.Left, new Vector2(-10, 77), new Vector2(250, 40));
        AddText(info, "Blue  8        Red  8", 26, TextAlignmentOptions.Center, new Vector2(0, 25), new Vector2(260, 44));
        AddText(info, "NEXT SPAWN", 22, TextAlignmentOptions.Left, new Vector2(-10, -40), new Vector2(250, 40));
        AddText(info, "4s", 30, TextAlignmentOptions.Right, new Vector2(-15, -40), new Vector2(250, 40));
        AddText(info, "INCOME", 22, TextAlignmentOptions.Left, new Vector2(-10, -100), new Vector2(250, 40));
        AddText(info, "+12     +6     +3", 26, TextAlignmentOptions.Center, new Vector2(0, -145), new Vector2(260, 40));

        RectTransform speed = CreateRect("Game Speed", right, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -520), new Vector2(310, 220));
        AddFramedPanel(speed.gameObject, "SPEED");
        CreateButton(speed, "Pause", "II", new Vector2(-88, -22), new Vector2(58, 58), true);
        CreateButton(speed, "Normal", ">", new Vector2(0, -22), new Vector2(58, 58), true);
        CreateButton(speed, "Fast", ">>", new Vector2(88, -22), new Vector2(58, 58), true);
        AddText(speed, "x1", 28, TextAlignmentOptions.Center, new Vector2(0, -88), new Vector2(120, 38));
    }

    private static void CreateBottomHUD(RectTransform parent)
    {
        RectTransform bottom = CreateRect("Bottom HUD", parent, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 330));
        AddPanel(bottom.gameObject, new Color32(77, 51, 47, 245));

        RectTransform tabs = CreateRect("Category Tabs", bottom, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(155, 0), new Vector2(280, 290));
        CreateButton(tabs, "Units", "UNITS", new Vector2(0, 100), new Vector2(260, 58), false);
        CreateButton(tabs, "Buildings", "BUILDINGS", new Vector2(0, 33), new Vector2(260, 58), false);
        CreateButton(tabs, "Powers", "POWERS", new Vector2(0, -34), new Vector2(260, 58), false);
        CreateButton(tabs, "Upgrades", "UPGRADES", new Vector2(0, -101), new Vector2(260, 58), false);

        RectTransform units = CreateRect("Unit Selection", bottom, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(910, 0), new Vector2(-760, 285));
        AddPanel(units.gameObject, new Color32(37, 33, 32, 240));
        string[] roles = { "MELEE", "RANGED", "CAVALRY", "SIEGE", "MYTHIC" };
        string[] costs = { "50", "60", "100", "130", "220" };
        for (int i = 0; i < roles.Length; i++)
        {
            float x = -390 + i * 195;
            RectTransform card = CreateRect(roles[i], units, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, -10), new Vector2(170, 230));
            AddPanel(card.gameObject, new Color32(65, 55, 51, 255));
            AddText(card, roles[i], 21, TextAlignmentOptions.Center, new Vector2(0, 82), new Vector2(150, 40));
            AddText(card, "UNIT", 32, TextAlignmentOptions.Center, new Vector2(0, 15), new Vector2(135, 80), new Color32(220, 220, 220, 255));
            AddText(card, "●  " + costs[i], 25, TextAlignmentOptions.Center, new Vector2(0, -82), new Vector2(145, 40), new Color32(255, 225, 66, 255));
            card.gameObject.AddComponent<Button>();
        }

        RectTransform queue = CreateRect("Queue", bottom, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-285, 0), new Vector2(520, 285));
        AddFramedPanel(queue.gameObject, "QUEUE");
        for (int i = 0; i < 4; i++)
        {
            RectTransform slot = CreateRect("Queue Slot " + (i + 1), queue, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-165 + i * 110, 5), new Vector2(92, 92));
            AddPanel(slot.gameObject, new Color32(30, 28, 27, 255));
        }
        AddText(queue, "TOTAL COST     ● 0", 24, TextAlignmentOptions.Center, new Vector2(0, -92), new Vector2(430, 42), new Color32(255, 225, 66, 255));
    }

    private static void CreateOverlay(RectTransform parent)
    {
        RectTransform overlay = CreateRect("Game Over Overlay", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CanvasGroup cg = overlay.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        AddPanel(overlay.gameObject, new Color32(0, 0, 0, 180));
        AddText(overlay, "BLUE TEAM WINS!", 76, TextAlignmentOptions.Center, Vector2.zero, new Vector2(900, 140));
    }

    private static void CreateHealthBar(RectTransform parent, string name, Vector2 pos, Vector2 size, Color fillColour, string value, bool reverse = false)
    {
        RectTransform bar = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        AddPanel(bar.gameObject, new Color32(20, 24, 26, 255));
        RectTransform fill = CreateRect("Fill", bar, Vector2.zero, Vector2.one, new Vector2(reverse ? -8 : 8, 0), new Vector2(-16, -12));
        Image image = fill.gameObject.AddComponent<Image>();
        image.color = fillColour;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = reverse ? 1 : 0;
        image.fillAmount = 1;
        AddText(bar, value, 26, TextAlignmentOptions.Center, Vector2.zero, size);
    }

    private static void AddStatLine(RectTransform parent, string label, string value, string rate, float yFromTop)
    {
        float y = parent.rect.height / 2f - yFromTop;
        AddText(parent, label, 22, TextAlignmentOptions.Left, new Vector2(-75, y), new Vector2(120, 38));
        AddText(parent, value, 25, TextAlignmentOptions.Center, new Vector2(15, y), new Vector2(80, 38));
        AddText(parent, rate, 22, TextAlignmentOptions.Right, new Vector2(85, y), new Vector2(110, 38), new Color32(255, 221, 42, 255));
    }

    private static void AddFramedPanel(GameObject go, string title)
    {
        AddPanel(go, new Color32(49, 41, 38, 245));
        RectTransform rt = go.GetComponent<RectTransform>();
        AddText(rt, title, 27, TextAlignmentOptions.Center, new Vector2(0, rt.rect.height / 2f - 31), new Vector2(rt.rect.width - 30, 44));
    }

    private static void CreateButton(RectTransform parent, string name, string label, Vector2 pos, Vector2 size, bool round)
    {
        RectTransform rt = CreateRect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        string path = round ? $"{UiRoot}/Buttons/SmallBlueRoundButton_Regular.png" : $"{UiRoot}/Buttons/BigBlueButton_Regular.png";
        string sprite = round ? "SmallBlueRoundButton_Regular_0" : "BigBlueButton_Regular_4";
        Image image = AddSpriteOrPanel(rt.gameObject, path, sprite, new Color32(42, 111, 138, 255));
        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        AddText(rt, label, round ? 24 : 22, TextAlignmentOptions.Center, Vector2.zero, size);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
        return rt;
    }

    private static Image AddPanel(GameObject go, Color colour)
    {
        Image image = go.AddComponent<Image>();
        image.color = colour;
        image.raycastTarget = false;
        return image;
    }

    private static Image AddSpriteOrPanel(GameObject go, string path, string spriteName, Color fallback)
    {
        Image image = go.AddComponent<Image>();
        image.sprite = LoadSprite(path, spriteName);
        image.color = image.sprite == null ? fallback : Color.white;
        image.type = image.sprite != null && image.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI AddText(RectTransform parent, string text, float size, TextAlignmentOptions alignment, Vector2 pos, Vector2 boxSize, Color? colour = null, Vector2? anchor = null)
    {
        Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
        RectTransform rt = CreateRect(text + " Text", parent, a, a, pos, boxSize);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = colour ?? Color.white;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        tmp.fontStyle = FontStyles.Bold;
        return tmp;
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
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
#endif
