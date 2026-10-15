// 内爆
// 失去所有波数 → 开火（本次：巨大体积、伤害减半、无限穿透、弹速与存在时间极小，
//                      并每点穿透/弹射额外加成体积与伤害）→ 伤害+30%给后面的模块
public class NeiBao : WeaponModule
{
    // 这个加成值要修改的话，preFire 里那行"减回去"也要一起改（两处必须一致）
    private const float DamageBonus = 0.3f;

    public NeiBao()
    {
        displayName = "内爆";
        description = "失去所有波数，打出一发巨大的水弹，原地炸开、无限穿透，但伤害减半，弹射和穿透属性会以一定比例加成体积和伤害,并使之后的模块获得额外伤害加成";
    }

    public override void Apply(AttackContext ctx)
    {
        // ① 首先失去所有波数
        // （Fire() 里有"至少打 1 波"的保底，所以实际还是 1 波）
        ctx.waveCount = 0;

        // ② 仅本次开火生效的改动
        preFire = delegate (AttackContext oneShot)
        {
            // ---- 伤害：先把加过的 30% 撤回，再打五折 ----
            oneShot.damage = oneShot.damage - DamageBonus;   // 撤回，回到原伤害
            oneShot.damage = oneShot.damage * 0.5f;          // 五折

            // ---- 巨量体积（【你试了再调】这里先给 3 = 4 倍大）----
            oneShot.bulletSize = oneShot.bulletSize + 30f;
            // ---- 每点穿透/弹射，加成体积与伤害（每点 +20%）----
            //  必须在把 pierce 设为"无限"之前算，
            //  否则 100 点穿透会被当成 100 点加成，数值直接爆炸
            float bonus = (oneShot.pierce + oneShot.bounce) * 0.2f;
            oneShot.bulletSize = oneShot.bulletSize*(1+bonus);
            oneShot.damage = oneShot.damage*(1+bonus);

            // ---- 然后才把穿透设成"无限" ----
            oneShot.pierce = 100f;

            // ---- 几乎不飞、存活极短（原地炸开；想调手感改这两个）----
            oneShot.bulletSpeed = 0f;
            oneShot.lifeTime = 0.01f;


        };

        // 伤害 +30%（永久，会传给后面的模块）
        ctx.damage = ctx.damage + DamageBonus;

        // 开火
        isFireModule = true;
    }
}