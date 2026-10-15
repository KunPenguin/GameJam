//这个脚本挂在水弹预制体上（水弹预制体放在 Assets/Prefabs/Bullets 里）
//作用：1.水弹一直往前飞，飞太久自动销毁
//      2.打中带 Enemy 标签的敌人：打印日志、通知敌人（OnHit）、按吸血回血
//      3.按穿透和弹射决定这颗子弹接下来怎么办：还有穿透就继续往前飞，穿透用完还有弹射就转向最近的另一个敌人
//需要组件：同一个物体上要有 Rigidbody2D（Gravity Scale = 0）
//          和 CircleCollider2D，并且把 Collider 的 Is Trigger 勾上
using System.Collections.Generic;
using UnityEngine;

public class WaterBullet : MonoBehaviour
{

    public float speed = 14f;//飞行速度（单位/秒），此处修改不奏效，只是保底数值
    public float lifeTime = 2.5f;//最长存活时间（秒），到时间自动销毁，此处修改不奏效，请前往Player的inspector更改
    public Rigidbody2D rb;//刚体组件（Inspector 里留空的话，会自动找自己身上的）
    public float aliveTime;//运行时的存活计时，不用手动改

    //========== 下面这些是"这一枪"带过来的数值，由 PlayerShoot 生成子弹时写入 ==========
    public float damageMultiplier = 1;//伤害倍率（1 = 100%）
    public float baseDamage = 10f;        // 这把枪的基础伤害（由 PlayerShoot 传进来）
    public float pierceLeft = 0f;//还能再命中几个敌人（初始值 = 穿透数 + 1）
    public float lifesteal = 0f;//吸血比例（0.1 = 10%）
    public float bulletSizeMultiplier = 1f;//子弹大小倍率（1 = 原始大小，暂时只是记录一下）
    public float heat = 0f;//热力值（现在只是存着，还没有实际效果）
    public int bounceLeft = 0;//还能弹射几次
    public PlayerHealth playerHealth;//玩家的血量脚本（吸血用，由 PlayerShoot 传进来）

    //最近打中的那个敌人：弹射的时候不要弹回它身上
    public Collider2D lastHitEnemy;
    // 已经打中过的敌人名单：同一颗子弹不会对同一个敌人重复造成伤害。
    // 为什么需要它：一颗子弹能穿透多个敌人时，
    // 如果敌人身上有多个 Collider2D，或者子弹在敌人边缘反复进出，
    // 同一个敌人就会被算好几次，伤害会翻倍。
    private List<Collider2D> hitEnemies = new List<Collider2D>();

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
    //后四个是"基础值"：真正的数值 = 基础值 × 数据包里的倍率
    public void Setup(AttackContext ctx, PlayerHealth health,
                      float baseDamageFromPlayer,
                      float baseSpeedFromPlayer,
                      float baseLifeTimeFromPlayer,
                      float baseBulletSizeFromPlayer)
    {
        // 从数据包里取值
        damageMultiplier = ctx.damage;//伤害倍率
        baseDamage = baseDamageFromPlayer; // 基础伤害
        pierceLeft = ctx.pierce + 1f; // 穿透2 = 能命中3个敌人
        lifesteal = ctx.lifesteal;//吸血
        heat = ctx.heat;//热力值
        bounceLeft = ctx.bounce;//弹射
        playerHealth = health;//生命值

        // ===== 基础值 × 倍率 = 这一枪真正用的数值 =====
        // 弹速：数据包里存的是倍率（1 = 100%），所以直接乘
        speed = baseSpeedFromPlayer * ctx.bulletSpeed;

        // 存在时间：同上
        lifeTime = baseLifeTimeFromPlayer * ctx.lifeTime;

        // 子弹大小：同上。⚠️ 0 会让子弹彻底消失，所以给个很低的下限
        bulletSizeMultiplier = Mathf.Max(0.1f, ctx.bulletSize);
        transform.localScale = transform.localScale * bulletSizeMultiplier;
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

        // 查重：这个敌人这一枪已经打过 → 直接跳过
        if (hitEnemies.Contains(other))
        {
            return;
        }

        // ③ 记进名单（放在这里，就"登记了"）
        hitEnemies.Add(other);


        // 算出这一发的最终伤害
        // Mathf.Max(0f, ...) 是保底：多个"伤害-50%"叠加时，
        // 倍率可能掉到 0 甚至负数。不钳制的话会出现"负数伤害给敌人回血"。
        float finalDamage = Mathf.Max(0f, baseDamage * damageMultiplier);

        // 触发当前被打中敌人的受击闪白效果，新增的！
        EnemyHitEffect effect = other.GetComponent<EnemyHitEffect>();
        if (effect != null)
        {
            effect.FlashWhite(0.1f); // 闪白 0.1 秒
        }

        // 从 Setup 里存下来的伤害值，把 this.damage 传给敌人
        // SendMessage 的第二个参数，会把数据传给目标的 OnHit 方法
        other.SendMessage("OnHit", finalDamage, SendMessageOptions.DontRequireReceiver);

        //2.吸血：回复量 = 伤害 × 吸血比例
        if (playerHealth != null && lifesteal > 0f)
        {
            playerHealth.Heal(finalDamage * lifesteal);
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
