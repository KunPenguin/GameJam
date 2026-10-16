//稳固导水，失去所有个数，然后开火，本次开火弹速*70%，波数*5，最后弹射+1，穿透+1



public class WenGuDaoShui : WeaponModule
{
    public WenGuDaoShui()
    {
        displayName = "稳固导水";
        description = "每波子弹个数变为1，然后开火，发射稍慢速子弹，本次开火发射子弹的波数*5，并使之后的模块弹射+1，穿透+1";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = 1;
        ctx.bounce += 1;
        ctx.pierce += 1;
        preFire = delegate (AttackContext oneshot)
        {
            oneshot.bounce -= 1;
            oneshot.pierce -= 1;
            oneshot.bulletSpeed *= 0.7f;
            oneshot.waveCount *= 5;

        };
        isFireModule = true;
    }
}

