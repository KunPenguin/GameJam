using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterWisp : MonoBehaviour      //依旧填写一些数值，这里是远程炮塔型敌人的代码，千万小心别改错了，注释啥的我基本都会给、
                                            //另外我这个炮台我还没测试过，但应该只要有玩家存在就可以触发，所以等你们玩家做好了，要是不行的话，就私我，我马上改
                                            //还有一点，我这个敌人随机刷新还没搞，等你们其他敌人的刷新弄好了，就直接复制粘贴给炮塔吧，谢谢
{
    //1.生命值相关,这些数值我就先随便填一些，你们后面再改
    [Header("总生命")]
    public int maxHp = 10;
    public int currentHp;                   //当前生命值
    //2.移动相关，虽然我这个好像根本不用动吧。。。无所谓先填一个
    public float moveSpeed = 2f;
    //3.攻击相关
    public float range = 5f;
    [Header("炮塔攻速")]
    public float fireRate = 1f;             //炮塔攻速
    [Header("子弹速度")]
    public float bulletSpeed = 5f;          //子弹速度      
    public float spawnOffset = 0.8f;        //子弹生成的偏移
    //4.子弹的预制体
    public GameObject bulletPrefab;
    //5.检测部分，不太懂，拿ds跑的，欢迎各位修正
    public string playerTag = "Player";     //用来识别玩家

    public Transform player;                //用来瞄准敌人
    public Rigidbody2D rb;                  //控制移动
    public float fireTimer;                 //开火计时器

    // Start is called before the first frame update
    void Start()
    {
        currentHp = maxHp;                  //给当前生命值赋值满血开局
        rb = GetComponent<Rigidbody2D>();   //拿组件，方便后面rb
        GameObject p = GameObject.FindGameObjectWithTag("Player");  //全场怪会来find玩家
        if (p != null) player = p.transform;  //判断是否相等 就是说是否找到了玩家 因为null是空值，那定然！=为true 后面就可以记下玩家位置
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;         //检测玩家死没死，及时结束程序
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;      //好东西，直接ctrl c+v 

        //攻击检测，这段拿ds跑的，我觉得应该没啥，数据应该可以调，说白了就是个大if，玩家在就直接fire
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

    void Fire()
    {
        if (bulletPrefab == null) return;   //依旧检测预制体，防止报错
        Vector2 dir = (player.position - transform.position).normalized;      //算出子弹的方向

        //后面三个我拿ds跑的，虽然看不懂但我觉得不影响，主要是起个完善效果
        Vector2 spawnPos = (Vector2)transform.position + dir * spawnOffset;   //算出子弹的生成位置
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;              //算出子弹的旋转角度

        Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        //instantiarte是克隆一个预制体（子弹）S+Q两个就是使子弹朝向玩家运动
    }

    // 被玩家水弹打中时调用
    public void OnHit()
    {
        currentHp--;
        if (currentHp <= 0) Destroy(gameObject);   //输出
        //应该炮塔是写完了，需要啥功能欢迎添加，与啥不懂的来qq私我就行了
    }
}
