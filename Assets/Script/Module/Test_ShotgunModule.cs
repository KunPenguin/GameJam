// 散射霰弹枪：一次打出一排子弹，但每发伤害降低。
// 这个模块同时演示两种改变：
//   永久改变 —— 子弹数 +2、散射角 +18度（会传给后面的模块）
//   一次性改变 —— 单发伤害 ×0.6（只有这一枪生效，不会传给后面的模块）
public class Test_ShotgunModule : WeaponModule
{
    // 构造函数：出生时就把名片填好
    public Test_ShotgunModule()
    {
        displayName = "散射";
        description = "一次多打两发，散布更开，但每发伤害降低";
    }

    public override void Apply(AttackContext ctx)
    {
        // ===== 第一段：永久改变（会传给后面的模块） =====
        // 注意是"加上去"，不是"直接赋值"。
        // 如果写成 ctx.bulletsPerWave = 2，装两张散射卡也只有 2 发。
        ctx.spreadAngle = ctx.spreadAngle + 18f;

        // ===== 第二段：一次性改变（只有这一枪生效） =====
        preFire = delegate (AttackContext oneShot)
        {
            // 注意改的是 oneShot（复印件），不是 ctx（原件）
            // 所以这个伤害折扣不会影响后面的模块
            oneShot.damage = oneShot.damage * 0.6f;
            ctx.bulletsPerWave = ctx.bulletsPerWave + 2;
        };

        // ===== 第三段：通知管道"我要开火了" =====
        isFireModule = true;
    }
}
