//强力击穿模块，失去所有穿透和弹射属性，然后开火，打出较大快速水弹，波数增加子弹伤害倍率的两倍，子弹造成的伤害*2，之后的模块波数+1


using Unity.VisualScripting;

public class QiangLiJiChuan : WeaponModule
{
    public QiangLiJiChuan()
    {
        displayName = "强力击穿";
        description = "失去所有穿透和弹射属性，然后开火，打出较大快速水弹，波数增加子弹伤害倍率的两倍，子弹造成的伤害*2，之后的模块波数+1";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.pierce = 0f;
        ctx.bounce = 0;
        ctx.waveCount += 1;
        preFire = delegate (AttackContext oneshot)
        {
            oneshot.waveCount -= 1;
            int plus = oneshot.damage >= 0 ? (int)(2 * oneshot.damage - (2 * oneshot.damage % 1)) : 1;
            oneshot.waveCount *= plus;
            oneshot.damage *= 2f;
            oneshot.bulletSpeed *= 1.5f;
            oneshot.bulletSize *= 3f;
        };
        isFireModule = true;
    }
}

