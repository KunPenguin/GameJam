//散射模块，首先失去所有伤害加成，然后开火，本次开火散射+60，存在时间*30%，子弹大小*0.5，子弹个数*5，最后波数+1。


public class SanShe : WeaponModule
{
    public SanShe()
    {
        displayName = "散射";
        description = "失去所有子弹伤害加成，然后开火，以更大的散射角度发射小型存在时间短的子弹，并且本次射击实际发射子弹个数等于子弹个数属性*3。最后使之后的模块波数+1";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.damage = 1;
        ctx.waveCount += 1;
        preFire = delegate (AttackContext oneshot)
        {
            oneshot.waveCount -= 1;
            oneshot.spreadAngle += 60;
            oneshot.lifeTime *= 0.3f;
            oneshot.bulletSize *= 0.5f;
            oneshot.bulletsPerWave *= 3;
        };
        isFireModule = true;
    }
}

