// 加压模块：穿透+2，弹速+50%，子弹大小-20%

public class JiaYaMoKuai : WeaponModule
{
    public JiaYaMoKuai()
    {
        displayName = "“加压模块”";
        description = "穿透+2，弹速+50%，子弹大小-20%";
    }

    public override void Apply(AttackContext ctx)
    {
        ctx.pierce = ctx.pierce + 2;
        ctx.bulletSpeed = ctx.bulletSpeed + 0.5f;
        ctx.bulletSize = ctx.bulletSize - 0.2f;
    }
}