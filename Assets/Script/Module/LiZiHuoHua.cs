// 粒子活化：弹射+1，子弹波数+1，存在时间+30%


public class LiZiHuoHua : WeaponModule
{
    public LiZiHuoHua()
    {
        displayName = "“粒子活化”";
        description = "弹射+1，子弹波数+1，存在时间+30%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.bulletsPerWave = ctx.bulletsPerWave + 1;
        ctx.bounce = ctx.bounce + 1;
        ctx.lifeTime= ctx.lifeTime + 0.3f;
    }
}