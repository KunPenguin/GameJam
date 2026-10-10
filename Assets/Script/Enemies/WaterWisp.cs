using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWisp : MonoBehaviour              //依旧填写一些数值，这里是远程炮塔型敌人的代码，千万小心别改错了，注释啥的我基本都会给、
                                                    //另外我这个炮台我还没测试过，但应该只要有玩家存在就可以触发，所以等你们玩家做好了，要是不行的话，就私我，我马上改
                                                    //还有一点，我这个敌人随机刷新还没搞，等你们其他敌人的刷新弄好了，就直接复制粘贴给炮塔吧，谢谢
{
    //1.生命值相关
    [Header("总生命")]
    public float maxHp = 10f;                       //各种基础数值，已将int替换为float，为了方便后续数值设定
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

    void Start()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;                                            //新修正的，寻敌加攻击范围检测
        Vector2 dir = (player.position - transform.position).normalized;       
        rb.velocity = dir * moveSpeed;

        if (Vector2.Distance(transform.position, player.position) <= range)
        {
            fireTimer -= Time.deltaTime;    //冷却倒计时
            if (fireTimer <= 0f)
            {
                Fire();
                fireTimer = 1f / Mathf.Max(fireRate, 0.1f);  //重置冷却
            }
        }
    }

    void Fire()
    {
        if (bulletPrefab == null) return;                                      //后面三个我拿ds跑的，虽然看不懂但我觉得不影响，主要是起个完善效果
        Vector2 dir = (player.position - transform.position).normalized;

        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, angle));   //instantiarte是克隆一个预制体（子弹）S+Q两个就是使子弹朝向玩家运动
    }

    public void OnHit(float finalDamage)
    {
        currentHp -= finalDamage;
        if (currentHp <= 0f) Destroy(gameObject);
        //应该炮塔是写完了，需要啥功能欢迎添加，与啥不懂的来qq私我就行了
        Debug.Log("炮塔受到伤害：" + finalDamage + "，剩余血量：" + currentHp);
    }
}