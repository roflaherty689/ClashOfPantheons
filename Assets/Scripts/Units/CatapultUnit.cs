using UnityEngine;

public class CatapultUnit : BaseUnit
{
    private Vector3 originalPosition;

    protected void Start()
    {
        originalPosition = visualTransform.localPosition;
    }

    protected override void PlayAttackAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(Recoil());
    }

    private System.Collections.IEnumerator Recoil()
    {
        float direction = Team == Team.Left ? -1f : 1f;

        visualTransform.localPosition =
            originalPosition + new Vector3(-0.15f * direction, 0f, 0f);

        yield return new WaitForSeconds(0.05f);

        visualTransform.localPosition = originalPosition;
    }
}