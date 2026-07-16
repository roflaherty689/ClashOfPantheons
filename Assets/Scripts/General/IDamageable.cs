using UnityEngine;

public interface IDamageable
{
    Transform Transform { get; }
    TargetType TargetType { get; }

    void TakeDamage(float damage);
}

public static class DamageableUtility
{
    public static bool IsValid(IDamageable damageable)
    {
        return damageable is Object unityObject && unityObject != null;
    }
}
