//这个脚本挂在 Boss 物体上（和 BossMoving、BossHealth 挂在一起）
//Boss 的全部攻击都在这一个脚本里：
//  1.近战接触伤害：玩家身体碰到Boss就固定扣血（Scene里的【小红圈】就是Boss的身体范围）
//  2.追踪水弹技能：隔一段时间，从Boss身体周围释放几颗【水弹】，水弹会一直追着玩家飞；
//    释放（前摇 + 打出去）期间Boss不能移动 —— 这个状态通过 IsCasting 同步给 BossMoving
//  3.最终大招：血量掉到 30% 以下时发动一次，原地大范围伤害；
//    前摇 2 秒，前摇期间不能移动，并且用一个【黄色圆圈】把大招范围显示出来（运行时画在游戏里，看得见）
//机制说明：Boss 身上的 Tag 和触发器碰撞体由 BossHealth 自动加上，所以玩家的水弹能打到Boss本体；
//          水弹自己也会带上 Enemy 标签和触发器，所以玩家的攻击同样能把它打掉（水弹只有1点血）
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>水弹的释放频率：低 / 中 / 高</summary>
public enum BossFireRate
{
    Low,
    Medium,
    High
}

/// <summary>
/// Boss 的攻击：近战接触 + 追踪水弹 + 30% 血量大招。
/// </summary>
public class BossAttack : MonoBehaviour
{
    [Header("=== 1.近战接触伤害（固定点数） ===")]
    [Tooltip("固定伤害：玩家身体碰到Boss，每次固定扣这么多血（不会随机浮动）")]
    public float attackDamage = 5f;

    [Tooltip("攻击间隔（秒）：两次接触伤害之间至少隔这么久")]
    public float attackInterval = 1f;

    [Header("=== 1.近战范围（Scene里的那个小红圈） ===")]
    [Tooltip("勾上：红圈半径自动按Boss贴图算（大致贴合贴图）；取消勾选：用下面手动填的 attackRadius")]
    public bool autoRadius = true;

    [Tooltip("手动指定红圈半径（只有取消勾选 autoRadius 之后才生效）")]
    public float attackRadius = 0.5f;

    [Tooltip("红圈额外放大系数：1 = 刚好贴合贴图，想打得宽松一点就调大")]
    public float radiusScale = 1f;

    [Header("=== 2.追踪水弹（频率：中） ===")]
    [Tooltip("释放频率：低 = 6秒一次，中 = 4秒一次，高 = 2.5秒一次")]
    public BossFireRate fireRate = BossFireRate.Medium;

    [Tooltip("一次释放几颗水弹（会均匀分布在Boss身体周围）")]
    public int bulletsPerVolley = 3;

    [Tooltip("水弹从Boss身边多远处冒出来（世界单位），0 = 直接从Boss中心冒出来")]
    public float spawnRadius = 1.2f;

    [Tooltip("水弹前摇（秒）：这段时间Boss不能移动，前摇结束才把水弹打出去")]
    public float castTime = 0.6f;

    [Tooltip("玩家离得比这个距离还远就不放水弹了（免得隔着屏幕对空气放技能）")]
    public float castRange = 14f;

    [Tooltip("每颗水弹打到玩家的固定伤害")]
    public float waterBulletDamage = 2f;

    [Tooltip("水弹飞行速度")]
    public float waterBulletSpeed = 3.5f;

    [Tooltip("水弹存在时间（秒）")]
    public float waterBulletLifeTime = 6f;

    [Tooltip("水弹血量：玩家打够这么多伤害就把它打掉（默认1 = 一下一个）")]
    public float waterBulletHp = 1f;

    [Tooltip("水弹圆圈半径（世界单位）")]
    public float waterBulletRadius = 0.22f;

    [Tooltip("水弹颜色（临时用神蓝色实心圆当模型）")]
    public Color waterBulletColor = new Color(0.15f, 0.45f, 1f, 1f);

    [Header("=== 3.最终大招（血量 30% 以下触发一次） ===")]
    [Tooltip("血量降到这个比例以下就放大招（0.3 = 30%）")]
    public float ultimateHpThreshold = 0.3f;

    [Tooltip("大招前摇（秒）：前摇期间不能移动，地上会显示黄色范围圈")]
    public float ultimateWindup = 2f;

    [Tooltip("大招范围半径（世界单位）")]
    public float ultimateRadius = 6f;

    [Tooltip("大招伤害（固定点数，玩家在圈里才吃得到）")]
    public float ultimateDamage = 8f;

    [Header("=== 目标引用 ===")]
    [Tooltip("玩家的Transform，可以在Inspector里直接拖拽；留空就自动找 Tag = Player")]
    public Transform player;

    private BossHealth bossHealth;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerRenderer;

    private float attackTimer;      //近战冷却
    private float castTimer;        //>0 = 水弹前摇中
    private float castCooldown;     //距离下次放水弹还有多久
    private float ultimateTimer;    //>0 = 大招前摇中
    private bool ultimateUsed;      //大招只放一次
    private LineRenderer ultimateCircle;   //地上那个黄色圆圈
    private float lookForPlayerTimer;
    private bool warnedNoPlayer = false;
    private bool warnedNoHealth = false;

    /// <summary>Boss现在是不是在放技能（水弹前摇 / 大招前摇）——BossMoving 靠这个禁止移动</summary>
    public bool IsCasting
    {
        get { return castTimer > 0f || ultimateTimer > 0f; }
    }

    /// <summary>当前生效的小红圈半径 = Boss的身体半径（Scene 里那个红圈画的就是这个值）</summary>
    public float CurrentRadius
    {
        get
        {
            float r = attackRadius;

            if (autoRadius)
            {
                if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    Bounds b = spriteRenderer.bounds;
                    r = Mathf.Max(b.extents.x, b.extents.y);
                }
            }

            return Mathf.Max(0.01f, r * radiusScale);
        }
    }

    /// <summary>实际能打到玩家的近战距离 = 小红圈 + 玩家身体半径（两个身体贴上）</summary>
    public float CurrentHitDistance
    {
        get { return CurrentRadius + PlayerRadius(); }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossHealth = GetComponent<BossHealth>();

        //开局先缓半拍再放第一发水弹
        castCooldown = IntervalFor(fireRate) * 0.5f;
    }

    void Start()
    {
        FindPlayer();

        if (bossHealth == null)
        {
            Debug.LogWarning("Boss上没有 BossHealth 脚本：Boss不会掉血，30%血量大招也不会触发。");
        }
    }

    void Update()
    {
        //Boss已经倒地：停手
        if (bossHealth != null && bossHealth.IsDead)
        {
            StopUltimateCircle();
            return;
        }

        //玩家还没找到：每秒再找一次
        if (player == null)
        {
            lookForPlayerTimer -= Time.deltaTime;
            if (lookForPlayerTimer <= 0f)
            {
                lookForPlayerTimer = 1f;
                FindPlayer();
            }
            return;
        }

        //===== 大招优先：血量掉到阈值以下就发动（只发动一次） =====
        if (!ultimateUsed && bossHealth != null && bossHealth.HpRatio <= ultimateHpThreshold)
        {
            StartUltimate();
        }

        //===== 大招前摇：原地蓄力，地上画黄圈，不能移动 =====
        if (ultimateTimer > 0f)
        {
            ultimateTimer -= Time.deltaTime;
            UpdateUltimateCircle();

            if (ultimateTimer <= 0f) FireUltimate();
            return;
        }

        //===== 水弹前摇：这段时间也不能动，前摇结束才把水弹打出去 =====
        if (castTimer > 0f)
        {
            castTimer -= Time.deltaTime;

            if (castTimer <= 0f) FireWaterBullets();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        //===== 水弹冷却好了就放（玩家在范围内才放） =====
        castCooldown -= Time.deltaTime;
        if (castCooldown <= 0f && distance <= castRange)
        {
            StartCast();
            return;
        }

        //===== 近战接触伤害：玩家贴上就扣血 =====
        attackTimer -= Time.deltaTime;
        if (distance <= CurrentHitDistance)
        {
            TryAttack();
        }
    }

    //==================== 1.近战接触伤害 ====================

    /// <summary>
    /// 试着打一下：固定扣玩家 attackDamage 点血。
    /// 冷却没好、或者玩家已经死了就什么都不做（每帧调用也没关系）。
    /// </summary>
    public void TryAttack()
    {
        if (attackTimer > 0f) return;
        attackTimer = attackInterval;

        if (playerHealth == null) FindPlayer();
        if (playerHealth == null) return;
        if (playerHealth.dead) return;

        playerHealth.TakeDamage(attackDamage);
        Debug.Log("Boss近战接触命中，固定伤害 " + attackDamage + " 点");
    }

    //==================== 2.追踪水弹 ====================

    void StartCast()
    {
        castTimer = Mathf.Max(0.01f, castTime);
        castCooldown = IntervalFor(fireRate);
        Debug.Log("Boss开始蓄力水弹（前摇 " + castTime + " 秒），这期间不会移动");
    }

    void FireWaterBullets()
    {
        castTimer = 0f;

        int count = Mathf.Max(1, bulletsPerVolley);
        float radius = Mathf.Max(0f, spawnRadius);
        Vector3 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            //均匀分布在Boss身体周围
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 pos = origin + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            SpawnWaterBullet(pos);
        }

        Debug.Log("Boss释放追踪水弹 x" + count + "（频率：" + fireRate + "）");
    }

    void SpawnWaterBullet(Vector3 pos)
    {
        GameObject go = new GameObject("水弹(运行时生成)");
        go.transform.position = pos;

        BossWaterBullet bullet = go.AddComponent<BossWaterBullet>();

        //AddComponent 会先跑 Awake，所以数值要在生成之后用 Setup 塞进去
        bullet.Setup(
            waterBulletDamage,
            waterBulletSpeed,
            waterBulletLifeTime,
            waterBulletHp,
            waterBulletRadius,
            waterBulletColor,
            player
        );
    }

    float IntervalFor(BossFireRate rate)
    {
        if (rate == BossFireRate.Low) return 6f;    //低
        if (rate == BossFireRate.High) return 2.5f; //高
        return 4f;                                  //中
    }

    //==================== 3.最终大招 ====================

    void StartUltimate()
    {
        ultimateUsed = true;
        ultimateTimer = Mathf.Max(0.1f, ultimateWindup);
        castTimer = 0f;   //大招优先，把水弹前摇掐掉

        CreateUltimateCircle();

        Debug.Log("Boss开始放大招！前摇 " + ultimateWindup + " 秒，范围 " + ultimateRadius + "，站在圈里会吃 " + ultimateDamage + " 点");
    }

    void FireUltimate()
    {
        ultimateTimer = 0f;

        float distance = 99999f;
        if (player != null) distance = Vector2.Distance(transform.position, player.position);

        if (player != null && distance <= ultimateRadius)
        {
            if (playerHealth == null) FindPlayer();

            if (playerHealth != null && !playerHealth.dead)
            {
                playerHealth.TakeDamage(ultimateDamage);
            }

            Debug.Log("Boss大招命中玩家，固定伤害 " + ultimateDamage + " 点");
        }
        else
        {
            Debug.Log("Boss大招打空了，玩家已经跑出范围");
        }

        StopUltimateCircle();
    }

    //---- 地上那个黄色圆圈（运行时用 LineRenderer 画，游戏里看得见） ----

    void CreateUltimateCircle()
    {
        StopUltimateCircle();

        GameObject go = new GameObject("Boss大招范围(运行时生成)");
        go.transform.SetParent(transform, false);

        LineRenderer line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;     //用世界坐标画，Boss放大3倍也不会把圈画歪
        line.loop = true;
        line.positionCount = 64;
        line.startWidth = 0.09f;
        line.endWidth = 0.09f;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) line.material = new Material(shader);

        Color yellow = new Color(1f, 0.85f, 0f, 0.95f);
        line.startColor = yellow;
        line.endColor = yellow;
        line.sortingOrder = 60;

        ultimateCircle = line;
        UpdateUltimateCircle();
    }

    void UpdateUltimateCircle()
    {
        if (ultimateCircle == null) return;

        int count = ultimateCircle.positionCount;
        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector3 p = new Vector3(
                center.x + Mathf.Cos(angle) * ultimateRadius,
                center.y + Mathf.Sin(angle) * ultimateRadius,
                center.z
            );
            ultimateCircle.SetPosition(i, p);
        }
    }

    void StopUltimateCircle()
    {
        if (ultimateCircle == null) return;

        Destroy(ultimateCircle.gameObject);
        ultimateCircle = null;
    }

    //==================== 找玩家 / 量身体 ====================

    /// <summary>玩家身体的半径（按玩家贴图算）</summary>
    float PlayerRadius()
    {
        if (playerRenderer == null || playerRenderer.sprite == null) return 0.25f;   //兜底值

        Bounds b = playerRenderer.bounds;
        return Mathf.Max(b.extents.x, b.extents.y);
    }

    /// <summary>
    /// 找玩家：先用 Inspector 里拖的，没拖就找 Tag = Player 的物体；顺便拿到 PlayerHealth 和贴图
    /// </summary>
    void FindPlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
            {
                player = p.transform;
                Debug.Log("BossAttack成功找到玩家： " + p.name);
            }
            else if (!warnedNoPlayer)
            {
                warnedNoPlayer = true;
                Debug.LogError("BossAttack找不到玩家！请确保玩家对象的Tag是'Player'，或者在Inspector里手动拖拽赋值。");
            }
        }

        if (player != null && playerRenderer == null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer == null) playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        }

        if (player != null && playerHealth == null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null) playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null) playerHealth = player.GetComponentInChildren<PlayerHealth>();

            if (playerHealth == null && !warnedNoHealth)
            {
                warnedNoHealth = true;
                Debug.LogError("BossAttack找到玩家了，但玩家身上没有 PlayerHealth 脚本，打不出伤害！");
            }
        }
    }

    //===== 额外保险：以后如果给Boss加了别的碰撞体，真正撞上玩家也会按同一个冷却扣血 =====
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) TryAttack();
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player")) TryAttack();
    }

    //在 Scene 窗口画出各种范围，方便调试（不需要运行游戏就能看到）
    void OnDrawGizmosSelected()
    {
        float body = CurrentRadius;

        //实心淡红 + 红色圈线：这就是那个【小红圈】（Boss的身体 / 近战接触范围）
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, body);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, body);

        //更淡的一圈：实际能打到玩家的边界（小红圈 + 玩家身体）
        Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, CurrentHitDistance);

        //黄色大圈：最终大招的范围（游戏里放大招前摇时也会画出同样的黄圈）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
    }
}
