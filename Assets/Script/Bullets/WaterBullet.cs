//这个脚本挂在水弹预制体上（水弹预制体放在 Assets/Prefabs/Bullets 里）
//作用：1.水弹一直往前飞，飞太久自动销毁
//      2.打中带 Enemy 标签的敌人：打印日志、通知敌人（OnHit）、按吸血回血
//      3.按穿透和弹射决定这颗子弹接下来怎么办：还有穿透就继续往前飞，穿透用完还有弹射就转向最近的另一个敌人
//需要组件：同一个物体上要有 Rigidbody2D（Gravity Scale = 0）
//          和 CircleCollider2D，并且把 Collider 的 Is Trigger 勾上
using UnityEngine;

public class WaterBullet : MonoBehaviour
{
    private AttackContext ctx;

    public float speed = 14f;//飞行速度（单位/秒）
    public float lifeTime = 2.5f;//最长存活时间（秒），到时间自动销毁
    public Rigidbody2D rb;//刚体组件（Inspector 里留空的话，会自动找自己身上的）
    public float aliveTime;//运行时的存活计时，不用手动改

    //========== 下面这些是"这一枪"带过来的数值，由 PlayerShoot 生成子弹时写入 ==========
    public float damage = 1;//伤害（现在只用来算吸血）
    public float pierceLeft = 0f;//还能再命中几个敌人（初始值 = 穿透数 + 1）
    public float lifesteal = 0f;//吸血比例（0.1 = 10%）
    public float bulletSize = 0f;//子弹大小加成（生成时已经放大过了，这里只是记录一下）
    public float heat = 0f;//热力值（现在只是存着，还没有实际效果）
    public int bounceLeft = 0;//还能弹射几次
    public PlayerHealth playerHealth;//玩家的血量脚本（吸血用，由 PlayerShoot 传进来）

    //最近打中的那个敌人：弹射的时候不要弹回它身上
    public Collider2D lastHitEnemy;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        //2D 俯视角不需要重力
        rb.gravityScale = 0f;
    }

    //由 PlayerShoot 在生成子弹时调用：把这一枪的数值交给这颗子弹
    public void Setup(AttackContext ctx, PlayerHealth health)
    {
        // 从数据包里取值
        damage = ctx.damage;//伤害
        pierceLeft = ctx.pierce + 1f; // 穿透2 = 能命中3个敌人
        lifesteal = ctx.lifesteal;//吸血
        bulletSize = ctx.bulletSize;//子弹大小
        heat = ctx.heat;//热力值
        bounceLeft = ctx.bounce;//弹射
        playerHealth = health;//生命值
        speed = ctx.bulletSpeed;//弹速
        lifeTime = ctx.bulletLife;//存活时间

        // 子弹大小
        transform.localScale = transform.localScale * (1f + bulletSize);
    }

    void Update()
    {
        //飞太久就销毁（免得子弹永远留在场上）
        aliveTime = aliveTime + Time.deltaTime;
        if (aliveTime >= lifeTime)
        {
            Debug.Log("水弹飞了 " + lifeTime + " 秒还没打到东西，自动销毁");
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        //沿自己的"右方向"飞；发射它的 PlayerShoot 已经把它转向鼠标方向了
        //注意：必须用 velocity 让物理系统来移动，不能写 transform.Translate
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //用标签判断打中的是不是敌人（敌人的标签必须是 Enemy）
        if (other.CompareTag("Enemy") == false)
        {
            return;
        }

        // 从 Setup 里存下来的伤害值，把 this.damage 传给敌人
        // SendMessage 的第二个参数，会把数据传给目标的 OnHit 方法
        other.SendMessage("OnHit", this.damage, SendMessageOptions.DontRequireReceiver);

        //2.吸血：回复量 = 伤害 × 吸血比例
        if (playerHealth != null && lifesteal > 0f)
        {
            playerHealth.Heal(damage * lifesteal);
        }

        //3.记下这次打中的是谁（弹射时不要弹回它）
        lastHitEnemy = other;

        //4.先看穿透：还有穿透就继续往前飞，不销毁
        pierceLeft = pierceLeft - 1f;
        if (pierceLeft > 0f)
        {
            Debug.Log("水弹穿透，继续飞（还能再命中 " + pierceLeft + " 个敌人）");
            return;
        }

        //5.穿透用完了，再看弹射：转向最近的另一个敌人，继续打
        if (bounceLeft > 0)
        {
            bool found = TurnToNearestEnemy();
            if (found)
            {
                bounceLeft--;
                Debug.Log("水弹弹射，转向下一个敌人（还能弹 " + bounceLeft + " 次）");
                return;
            }
            Debug.Log("水弹想弹射，但附近没有别的敌人了");
        }

        //6.穿透和弹射都用完了，销毁
        Debug.Log("水弹用完了穿透和弹射，销毁");
        Destroy(gameObject);
    }

    //转向"最近的、不是刚打中的那个"敌人；找到了返回 true
    bool TurnToNearestEnemy()
    {
        //用标签把所有敌人找出来（这是统一要求里找对象的写法）
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        GameObject nearest = null;
        float nearestDistance = 99999f;

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null) continue;
            if (lastHitEnemy != null && enemies[i] == lastHitEnemy.gameObject) continue;//跳过刚打中的那个

            float distance = Vector2.Distance(transform.position, enemies[i].transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemies[i];
            }
        }

        if (nearest == null) return false;

        //算出朝这个敌人的角度，把子弹转过去
        Vector2 dir = (Vector2)nearest.transform.position - (Vector2)transform.position;
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        return true;
    }
}
