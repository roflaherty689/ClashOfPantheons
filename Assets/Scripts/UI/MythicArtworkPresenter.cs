using UnityEngine;
using UnityEngine.UI;

public static class MythicArtworkPresenter
{
    public static void Update(
        Image art,
        BaseUnit selectedMythic,
        MythicUnitRoster roster,
        Sprite parchmentSprite)
    {
        if (art == null || roster == null) return;

        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (selectedMythic != null)
        {
            if (crossedSwords != null)
            {
                crossedSwords.gameObject.SetActive(false);
            }

            art.sprite = roster.GetAvatar(selectedMythic) ?? GetUnitSprite(selectedMythic);
            art.preserveAspect = true;
            return;
        }

        art.sprite = null;
        if (crossedSwords == null)
        {
            crossedSwords = CreateUIObject("Crossed Swords", art.transform);
            RectTransform crossedRect = (RectTransform)crossedSwords;
            crossedRect.anchorMin = Vector2.zero;
            crossedRect.anchorMax = Vector2.one;
            crossedRect.offsetMin = Vector2.zero;
            crossedRect.offsetMax = Vector2.zero;

            CreateParchmentImage(crossedSwords, parchmentSprite);
            CreateSwordImage(crossedSwords, "Sword A", 0f, roster.DefaultIcon);
            CreateSwordImage(crossedSwords, "Sword B", 90f, roster.DefaultIcon);
        }

        crossedSwords.gameObject.SetActive(true);
    }

    public static void SetCrossedSwordsColour(Image art, Color colour)
    {
        if (art == null) return;

        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (crossedSwords == null) return;

        for (int i = 0; i < crossedSwords.childCount; i++)
        {
            Transform child = crossedSwords.GetChild(i);
            if (child.name.StartsWith("Sword") && child.TryGetComponent(out Image sword))
            {
                sword.color = colour;
            }
        }
    }

    public static void HideCrossedSwords(Image art)
    {
        if (art == null) return;

        Transform crossedSwords = art.transform.Find("Crossed Swords");
        if (crossedSwords != null)
        {
            crossedSwords.gameObject.SetActive(false);
        }
    }

    public static Sprite GetUnitSprite(BaseUnit prefab)
    {
        SpriteRenderer renderer = prefab != null
            ? prefab.GetComponentInChildren<SpriteRenderer>(true)
            : null;
        return renderer != null ? renderer.sprite : null;
    }

    public static string GetDisplayName(BaseUnit prefab)
    {
        if (prefab == null) return "Unknown";

        return prefab.name
            .Replace("MeleeMythicAnimatedUnit", "Minotaur")
            .Replace("MeleeMythicUnit", "Minotaur")
            .Replace("MythicUnit", string.Empty)
            .Replace("MonkUnit", " Monk")
            .Replace("Fish", " Fish")
            .Trim();
    }

    private static void CreateParchmentImage(Transform parent, Sprite parchmentSprite)
    {
        RectTransform parchmentRect = CreateUIObject("Parchment", parent);
        parchmentRect.anchorMin = Vector2.zero;
        parchmentRect.anchorMax = Vector2.one;
        parchmentRect.offsetMin = Vector2.zero;
        parchmentRect.offsetMax = Vector2.zero;
        Image parchment = parchmentRect.gameObject.AddComponent<Image>();
        parchment.sprite = parchmentSprite;
        parchment.color = Color.white;
        parchment.preserveAspect = true;
        parchment.raycastTarget = false;
    }

    private static void CreateSwordImage(
        Transform parent,
        string objectName,
        float rotation,
        Sprite swordSprite)
    {
        RectTransform swordRect = CreateUIObject(objectName, parent);
        swordRect.anchorMin = new Vector2(0.5f, 0.5f);
        swordRect.anchorMax = new Vector2(0.5f, 0.5f);
        swordRect.sizeDelta = new Vector2(78f, 78f);
        swordRect.anchoredPosition = Vector2.zero;
        swordRect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        Image sword = swordRect.gameObject.AddComponent<Image>();
        sword.sprite = swordSprite;
        sword.preserveAspect = true;
        sword.raycastTarget = false;
    }

    private static RectTransform CreateUIObject(string objectName, Transform parent)
    {
        GameObject instance = new GameObject(objectName, typeof(RectTransform));
        RectTransform rect = instance.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }
}
