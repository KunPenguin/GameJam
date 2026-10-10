using System.Collections;
using System.Collections.Generic;
using UnityEngine;
                                               //高级远程敌人的代码哈，哈气红祥，欢迎指正依旧修bug，注释我会尽量给
public class HighWaterWisp : MonoBehaviour    
{
    //1.生命值相关                             //基础面板
    [Header("总生命")]
    public float maxHp = 10;
    public float currentHp;
    //2.移动相关
    public float moveSpeed = 2f;
    //3.攻击相关
    public float range = 5f;
    [Header("炮塔攻速")]
    public float fireRate = 1f;
    [Header("子弹速度")]
    public float bulletSpeed = 5f;
    public float spawnOffset = 0.8f;
    //4.子弹的预制体
    public GameObject bulletPrefab;
    //5.检测部分
    public string playerTag = "Player";
    public Transform player;
    public Rigidbody2D rb;
    public float fireTimer;

    // ★高级专属：散射角度
    [Header("三发散射的角度")]
    public float spreadAngle = 60f;              //角度，以中间的那个子弹为中心哈

    void Start()
    {
        currentHp = maxHp;                       //寻找玩家
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;             //防报错机制，省流
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        if (Vector2.Distance(transform.position, player.position) <= range)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Fire();
                fireTimer = 1f / Mathf.Max(fireRate, 0.1f);
            }
        }
    }

    // ★一次发三发，zdjd
    void Fire()
    {
        if (bulletPrefab == null || player == null) return;
        Vector2 dir = (player.position - transform.position).normalized;

        FireOne(dir);                          // 中间
        FireOne(Rotate(dir, -spreadAngle));    // 剩余两颗是以前面设好的函数，通过旋转角度，来实现散射的
        FireOne(Rotate(dir, spreadAngle));    
    }

    void FireOne(Vector2 dir)                  //发射单发子弹
    {
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;   //避免子弹卡怪身体里
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;              //把方向换算成角度，这样就可以使子弹朝该方向飞
        Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, angle));   //实现三颗射的话，就是复制预制体
    }

    Vector2 Rotate(Vector2 v, float deg)                            //ds跑的 主要是把角度转弧度
    {
        float r = deg * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    public void OnHit(float finalDamage)
    {
        currentHp -= finalDamage;
        if (currentHp <= 0f) Destroy(gameObject);
        //应该炮塔是写完了，需要啥功能欢迎添加，与啥不懂的来qq私我就行了
        Debug.Log("炮塔受到伤害：" + finalDamage + "，剩余血量：" + currentHp);
    }
}