//这是储存发射时每一枪所有的属性的数据包。模块会修改它，最后扔给 Fire 函数去发射。
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackContext
{
    public int damage = 1;//伤害
    public float pierce = 0f;//穿透
    public float lifesteal = 0f;//吸血
    public float bulletSize = 0f;//子弹大小
    public float heat = 0f;//热力值
    public int bounce = 0;//弹射
    public int waveCount = 1;//子弹波数
    public int bulletsPerWave = 1;//每波子弹数
    public float spreadAngle = 0f;//散射角
    public bool isFireTrigger = false; // 【核心】是否遇到开火节点
}