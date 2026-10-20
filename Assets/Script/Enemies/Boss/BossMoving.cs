//这个脚本挂在 Boss 物体上（Boss 预制体：Assets/Prefabs/Enemies/Boss（bushi））
//作用：1.索敌：玩家进入【索敌范围】就开始追，追起来之后只有玩家跑出【脱离范围】才会放弃（不会在边界上反复横跳）
//      2.一直靠近玩家：没有固定的停止距离了，Boss会一直往玩家身上挤，直到身体和玩家贴上才停（贴上之后的伤害交给 BossAttack）
//      3.体型：开局自动把 Boss 贴图调成玩家的 sizeMultiplier 倍（默认 3 倍）
//伤害不在这里！Boss 是近战角色，伤害在 BossAttack 脚本里（玩家贴上来才扣血，Scene 里那个小红圈就是它的范围）
//需要组件：Boss 身上要有 SpriteRenderer（量贴图大小、翻面要用）；玩家要有 PlayerHealth，并且 Tag = "Player"
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Boss 索敌 + 追踪 + 体型。文件名对应的就是这个类，Add Component 里直接搜 BossMoving 就能挂上。
/// </summary>
public class BossMoving : MonoBehaviour
{
    [Header("=== 索敌（追踪）参数 ===")]
    [Tooltip("Boss的移动速度，数值越小越笨重")]
    public float moveSpeed = 1.5f;

    [Tooltip("索敌范围：玩家进入这个距离，Boss就开始追")]
    public float chaseRange = 12f;

    [Tooltip("脱离范围：已经在追的时候，玩家跑出这个距离Boss才会放弃（建议明显大于索敌范围，否则Player刚出圈又进圈会来回抽搐）")]
    public float loseRange = 20f;

    [Header("=== 贴身（贴到玩家身上） ===")]
    [Tooltip("贴身系数：1 = 身体刚好贴上就停；0.9 = 稍微压进去一点（保证 BossAttack 的接触伤害一定打得到）；0 = 一直挤到玩家中心")]
    public float contactFactor = 0.9f;

    [Header("=== 体型（贴图大小） ===")]
    [Tooltip("Boss贴图要比玩家大几倍")]
    public float sizeMultiplier = 3f;

    [Tooltip("勾上：开局自动把贴图缩放到玩家的 sizeMultiplier 倍；取消勾选就按预制体里原本的缩放来")]
    public bool autoScaleToPlayer = true;

    [Header("=== 目标引用 ===")]
    [Tooltip("玩家的Transform，可以在Inspector里直接拖拽；留空就自动找 Tag = Player")]
    public Transform player;

    [Header("=== 表现（可选，不影响数值） ===")]
    [Tooltip("勾上之后Boss会朝着玩家翻转（只翻图片，不改缩放，所以不影响判定和子物体）")]
    public bool faceTarget = false;

    private SpriteRenderer spriteRenderer;      //Boss自己的贴图
    private SpriteRenderer playerRenderer;      //玩家的贴图（量体型、算贴身距离要用）
    private BossAttack bossAttack;              //放技能时Boss不能动，要问它
    private BossHealth bossHealth;              //Boss倒地以后就不能再动了
    private float lookForPlayerTimer;           //找不到玩家时的重试计时
    private bool isChasing = false;             //调试用：记录Boss现在有没有在索敌
    private bool warnedNoPlayer = false;        //提示只打印一次，免得刷屏
    private bool scaledToPlayer = false;        //体型只调一次

    /// <summary>Boss现在是不是正在追玩家（只读，方便别的脚本或调试看）</summary>
    public bool IsChasing { get { return isChasing; } }

    // Start is called before the first frame update
    void Awake()
    {
        //同一个物体上如果重复挂了 BossMoving / BossController，只保留一个，
        //否则会双重移动 + 双重伤害（BossController 继承自 BossMoving，所以这里会一起查出来）
        BossMoving[] all = GetComponents<BossMoving>();
        if (all.Length > 1)
        {
            foreach (BossMoving other in all)
            {
                if (other != this)
                {
                    other.enabled = false;
                    Destroy(other);
                }
            }
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        bossAttack = GetComponent<BossAttack>();
        bossHealth = GetComponent<BossHealth>();

        //脱离范围比索敌范围还小的话，追人逻辑会变得很奇怪，这里自动兜一下
        if (loseRange <= chaseRange) loseRange = chaseRange * 1.5f;

        //贴身系数不能大于0.95：大于1就会停在 BossAttack 的小红圈外面，导致永远碰不到玩家、打不出伤害
        contactFactor = Mathf.Clamp(contactFactor, 0f, 0.95f);
    }

    void Start()
    {
        FindPlayer();

        //提醒一下：攻击全在 BossAttack 上，没挂的话Boss永远打不到人
        if (bossAttack == null)
        {
            Debug.LogWarning("Boss上没有 BossAttack 脚本！需要挂 BossAttack（近战接触 + 追踪水弹 + 30%血量大招），Boss才打得出伤害。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //玩家还没找到（或者玩家对象被销毁了）：每隔1秒再找一次，找到之前Boss原地待机
        if (player == null)
        {
            isChasing = false;
            lookForPlayerTimer -= Time.deltaTime;
            if (lookForPlayerTimer <= 0f)
            {
                lookForPlayerTimer = 1f;
                FindPlayer();
            }
            return;
        }

        //算一下体型：确认玩家贴图拿到手之后，把Boss贴图调成玩家的 sizeMultiplier 倍
        ApplyScaleToPlayer();

        //计算Boss和玩家之间的距离
        float distance = Vector2.Distance(transform.position, player.position);

        //Boss已经倒地：不再移动
        if (bossHealth != null && bossHealth.IsDead) return;

        //Boss正在放技能（追踪水弹的前摇 / 大招前摇）：原地站住不能动
        //这个开关在 BossAttack 里（IsCasting），两个脚本用同一个状态，不会打架
        if (bossAttack != null && bossAttack.IsCasting) return;

        //===== 1.索敌：进圈就开始追，出圈才放弃（两个圈不一样大，所以不会在边界反复横跳） =====
        if (isChasing)
        {
            //已经在追：只有玩家跑出脱离范围才放弃
            if (distance > loseRange) isChasing = false;
        }
        else
        {
            //还没追：玩家进索敌范围就开始追
            if (distance <= chaseRange) isChasing = true;
        }

        //===== 2.移动：一直朝玩家靠近，直到身体贴上（贴上就停，不然Boss会整个压在玩家头顶上） =====
        if (isChasing && distance > ContactDistance())
        {
            //笨重移动：朝着玩家的方向缓慢滑行
            //Vector2.MoveTowards 会平滑地移动，不会瞬间闪过去
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        //===== 3.朝向（可选）：只有勾了 faceTarget 才会翻 =====
        if (faceTarget && spriteRenderer != null)
        {
            float xOffset = player.position.x - transform.position.x;
            if (Mathf.Abs(xOffset) > 0.01f)
            {
                spriteRenderer.flipX = xOffset < 0f;
            }
        }
    }

    /// <summary>
    /// 贴身距离：Boss身体半径 + 玩家身体半径，再乘 contactFactor。
    /// 距离大于它就继续往玩家身上挤，小于等于它就说明已经贴上了（这时该由 BossAttack 打接触伤害）。
    /// </summary>
    float ContactDistance()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        float myRadius = SpriteRadius(spriteRenderer);
        float playerRadius = SpriteRadius(playerRenderer);
        if (playerRadius <= 0f) playerRadius = 0.25f;   //玩家的贴图没找到时的兜底值

        return Mathf.Max(0.05f, (myRadius + playerRadius) * contactFactor);
    }

    /// <summary>量一个贴图在世界里的“半径”（取长宽里大的一半），没贴图就返回0</summary>
    static float SpriteRadius(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return 0f;

        Bounds b = sr.bounds;
        return Mathf.Max(b.extents.x, b.extents.y);
    }

    /// <summary>
    /// 把Boss贴图缩放到玩家的 sizeMultiplier 倍（默认3倍）。
    /// 不管是几倍，都是按“当前显示大小”算的，所以手动改过预制体缩放也不会被翻倍叠加。
    /// </summary>
    void ApplyScaleToPlayer()
    {
        if (!autoScaleToPlayer || scaledToPlayer) return;
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        if (playerRenderer == null || playerRenderer.sprite == null) return;

        float playerHeight = playerRenderer.bounds.size.y;
        float myHeight = spriteRenderer.bounds.size.y;
        if (playerHeight <= 0.0001f || myHeight <= 0.0001f) return;

        float scaleFactor = (playerHeight * sizeMultiplier) / myHeight;

        //只放大这张贴图（Boss本体），不去动整个物体的缩放，免得以后加的血条、特效也跟着变大
        spriteRenderer.transform.localScale = spriteRenderer.transform.localScale * scaleFactor;
        scaledToPlayer = true;

        Debug.Log("Boss体型已调整为玩家的 " + sizeMultiplier + " 倍（贴图 localScale = " + spriteRenderer.transform.localScale + "）");
    }

    /// <summary>
    /// 找玩家：先用 Inspector 里拖的，没拖就找 Tag = Player 的物体；顺便拿到玩家身上的 SpriteRenderer
    /// </summary>
    void FindPlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log("Boss成功找到玩家： " + p.name);
            }
            else if (!warnedNoPlayer)
            {
                warnedNoPlayer = true;
                Debug.LogError("Boss找不到玩家！请确保玩家对象的Tag是'Player'，或者在Inspector里手动拖拽赋值。");
            }
        }

        if (player != null && playerRenderer == null)
        {
            //玩家的贴图可能挂在玩家本体或者子物体上，都找一遍
            playerRenderer = player.GetComponent<SpriteRenderer>();
            if (playerRenderer == null) playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
        }
    }

    //在 Scene 窗口画出各种范围，方便调试（不需要运行游戏就能看到）
    //注意：小红圈（接触伤害范围）在 BossAttack 脚本里画，这里不画红色
    void OnDrawGizmosSelected()
    {
        //黄色的圈：索敌范围（玩家进这个圈就开始追）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        //青色的圈：脱离范围（已经在追的时候，玩家出这个圈Boss才放弃）
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        //灰色的圈：贴身距离（靠近到这个距离就停下，剩下的交给 BossAttack）
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, ContactDistance());
    }
}

/// <summary>
/// 兼容旧名字：如果你在场景里挂的是 BossController，它也能正常索敌 + 追踪 + 调体型（逻辑和 BossMoving 完全一样）。
/// 注意：同一个物体上不要同时挂 BossMoving 和 BossController，就算挂了也只会生效一个。
/// </summary>
public class BossController : BossMoving
{
}
