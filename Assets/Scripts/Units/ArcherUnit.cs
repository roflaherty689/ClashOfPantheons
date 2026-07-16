public class ArcherUnit : BaseUnit
{
    protected override void PlayAttackAnimation()
    {
        if (animator != null)
        {
            base.PlayAttackAnimation();
            return;
        }

        PlayRecoilAnimation();
    }
}
