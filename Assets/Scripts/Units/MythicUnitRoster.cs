using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MythicUnitRoster", menuName = "Clash of Pantheons/Mythic Unit Roster")]
public class MythicUnitRoster : ScriptableObject
{
    [SerializeField] private BaseUnit[] units;
    [SerializeField] private Sprite[] avatars;
    [SerializeField] private Sprite defaultIcon;

    public IReadOnlyList<BaseUnit> Units => units;
    public Sprite DefaultIcon => defaultIcon;

    public bool Contains(BaseUnit unit)
    {
        if (unit == null || units == null) return false;

        foreach (BaseUnit candidate in units)
        {
            if (candidate == unit) return true;
        }

        return false;
    }

    public Sprite GetAvatar(BaseUnit unit)
    {
        if (unit == null || units == null || avatars == null) return null;

        int count = Mathf.Min(units.Length, avatars.Length);
        for (int i = 0; i < count; i++)
        {
            if (units[i] == unit)
            {
                return avatars[i];
            }
        }

        string unitName = unit.name.Replace("(Clone)", string.Empty).Trim();
        for (int i = 0; i < count; i++)
        {
            if (units[i] != null && units[i].name == unitName)
            {
                return avatars[i];
            }
        }

        return null;
    }
}
