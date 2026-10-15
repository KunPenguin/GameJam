//这是储存发射时每一枪所有的属性的数据包。模块会修改它，最后扔给 Fire 函数去发射。
//这个数据包还承担了每一枪子弹基础属性的初始化
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackContext
{
    public float damage;//伤害倍率（1 = 100%）。最终伤害 = PlayerShoot.baseDamage × 这个值
    public float pierce;//穿透（个数，用加法）
    public float lifesteal;//吸血比例（0.1 = 10%，用加法）
    public float bulletSize;//子弹大小倍率（1 = 原始大小，用加法。0.5 = 一半大）
    public float heat;//热力值
    public int bounce;//弹射（个数，用加法）
    public int waveCount;//子弹波数（用加法）
    public int bulletsPerWave;//每波子弹数（用加法）
    public float spreadAngle;//散射角（度，用加法）
    public float waveSpacing;//每波子弹时间间距
    public float bulletSpeed;//子弹速度倍率（1 = 100%，用加法）。实际弹速 = PlayerShoot.baseBulletSpeed × 这个值
    public float lifeTime;//子弹存在时间倍率（1 = 100%，用加法）。实际时间 = PlayerShoot.baseLifeTime × 这个值

    // ⚠️ 给所有写模块的人：
    //   倍率型属性（damage / bulletSpeed / lifeTime / bulletSize）：写"加成值"。
    //       伤害+80%  →  ctx.damage = ctx.damage + 0.8f;
    //       弹速+50%  →  ctx.bulletSpeed = ctx.bulletSpeed + 0.5f;
    //       子弹大小-20% → ctx.bulletSize = ctx.bulletSize - 0.2f;
    //       这样两个 +50% 叠加是 1+0.5+0.5 = 2.0 倍，不会指数爆炸。
    //   个数型属性（pierce / bounce / waveCount / bulletsPerWave / spreadAngle / lifesteal）：直接加个数。
    //       "失去所有X" → 直接赋值（失去所有波数 = ctx.waveCount = 0;）
    //       ⚠️ 但"失去所有伤害"是 ctx.damage = 1f; （归 100%，不是归 0，归 0 就没伤害了）

    // 克隆方法：复制一份全新的数据包，以免每次开火对数值的影响不重置
    public AttackContext Clone()
    {
        AttackContext copy = new AttackContext();
        copy.damage = this.damage;
        copy.pierce = this.pierce;
        copy.lifesteal = this.lifesteal;
        copy.bulletSize = this.bulletSize;
        copy.heat = this.heat;
        copy.bounce = this.bounce;
        copy.waveCount = this.waveCount;
        copy.bulletsPerWave = this.bulletsPerWave;
        copy.spreadAngle = this.spreadAngle;
        copy.waveSpacing = this.waveSpacing;
        copy.lifeTime=this.lifeTime;
        copy.bulletSpeed = this.bulletSpeed;
        return copy;
    }
}