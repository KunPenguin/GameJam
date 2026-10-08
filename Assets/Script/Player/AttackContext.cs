//这是储存发射时每一枪所有的属性的数据包。模块会修改它，最后扔给 Fire 函数去发射。
//这个数据包还承担了每一枪子弹基础属性的初始化
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackContext
{
    public float damage;//伤害
    public float pierce;//穿透
    public float lifesteal;//吸血
    public float bulletSize;//子弹大小
    public float heat;//热力值
    public int bounce;//弹射
    public int waveCount;//子弹波数
    public int bulletsPerWave;//每波子弹数
    public float spreadAngle;//散射角
    public float waveSpacing;//每波子弹时间间距
    public bool isFireTrigger = false; // 【核心】是否遇到开火节点

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
        copy.isFireTrigger = false; // 复印件永远从“未开火”开始
        return copy;
    }
}