public class GaoXiaoDaoLiu : WeaponModule
{
    public GaoXiaoDaoLiu()
    {
        displayName = "高效导流";
        description = "波数+1，弹速+50%，穿透+1";
    }

    public override void Apply(AttackContext ctx)
    {
        // 波数+1（int类型，不加f）
        ctx.waveCount = ctx.waveCount + 1;

        // 弹速+50%（倍率型，加0.5f，不用乘法）
        ctx.bulletSpeed = ctx.bulletSpeed + 0.5f;

        // 穿透+1（float类型，加1f）
        ctx.pierce = ctx.pierce + 1f;
    }
}
