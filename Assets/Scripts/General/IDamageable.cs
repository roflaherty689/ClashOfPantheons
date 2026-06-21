using UnityEngine;

public interface IDamageable
{
    Transform Transform { get; }
    TargetType TargetType { get; }

    void TakeDamage(float damage);
}