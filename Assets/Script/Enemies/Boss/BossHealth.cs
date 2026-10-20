//这个脚本挂在 Boss 物体上（和 BossMoving、BossAttack 挂在一起）
//作用：1.Boss的血量：maxHp 在 Inspector 里随便改
//      2.能被打：玩家的水弹只会打 Tag = Enemy 的物体，并且会调用 OnHit(伤害)，
//        所以这里自动给Boss打上 Enemy 标签 + 加一个触发器碰撞体（没有碰撞体玩家就打不到它）
//      3.Boss头顶的【红色矩形】血条：运行时自动生成，不用做UI预制体，血条跟着Boss走
//      4.血量归零：隐藏血条、停止攻击、过一小会儿销毁Boss
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    [Header("=== 血量（可以随便改） ===")]
    [Tooltip("Boss的最大血量（默认 2000：玩家基础伤害 10，打完大约 30~60 秒；觉得太长就调小）")]
    public float maxHp = 2000f;

    [Tooltip("当前血量（运行时用，一般不用手动改）")]
    public float currentHp = 2000f;

    [Tooltip("无敌开关：勾上就打不掉血（调试用）")]
    public bool invincible = false;

    [Header("=== 死亡 ===")]
    [Tooltip("血量归零后要不要销毁Boss")]
    public bool destroyOnDeath = true;

    [Tooltip("血量归零后过多久销毁Boss（秒）")]
    public float destroyDelay = 1.5f;

    [Header("=== 胜利界面（Boss死了要弹的面板） ===")]
    [Tooltip("拖 GameManager。留空会自动在场景里找")]
    public GameOverPanel gameOverPanel;

    [Tooltip("Boss死了就弹胜利界面")]
    public bool showWinOnDeath = true;

    [Header("=== 血条（Boss头顶的红色矩形） ===")]
    [Tooltip("要不要显示血条")]
    public bool showHpBar = true;

    [Tooltip("血条宽度（世界单位，会跟着Boss放大3倍显得小，可以用 barOffsetY/barWidth 微调）")]
    public float barWidth = 1.8f;

    [Tooltip("血条高度（世界单位）")]
    public float barHeight = 0.18f;

    [Tooltip("血条在Boss头顶再往上偏移多少")]
    public float barOffsetY = 0.15f;

    [Tooltip("血条颜色（红色矩形）")]
    public Color barColor = Color.red;

    [Tooltip("血条底槽颜色（剩多少血的对比）")]
    public Color barBackgroundColor = new Color(0f, 0f, 0f, 0.65f);

    [Tooltip("血条渲染层级，数字越大越靠前")]
    public int barSortingOrder = 200;

    private GameObject barBgGo;
    private GameObject barFillGo;
    private SpriteRenderer barFill;
    private SpriteRenderer bossRenderer;

    /// <summary>Boss死了没有（别的脚本靠它停手、停移动）</summary>
    public bool IsDead { get; private set; }

    /// <summary>血量比例：1 = 满血，0 = 倒地（BossAttack 用它判断 30% 大招）</summary>
    public float HpRatio
    {
        get
        {
            if (maxHp <= 0f) return 0f;
            return Mathf.Clamp01(currentHp / maxHp);
        }
    }

    void Awake()
    {
        currentHp = maxHp;

        SetupDamageable();

        if (showHpBar) CreateBar();
    }

    /// <summary>
    /// 让Boss能被玩家的水弹打到：
    /// 玩家水弹的判定是"撞到 Tag = Enemy 的触发器，然后 SendMessage 调 OnHit"，所以这两样必须有
    /// </summary>
    void SetupDamageable()
    {
        if (gameObject.tag != "Enemy") gameObject.tag = "Enemy";

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            //半径按贴图算：Boss放大3倍时碰撞体会跟着Transform一起放大，所以这里填贴图自身的半径就行
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                col.radius = Mathf.Max(sr.sprite.bounds.extents.x, sr.sprite.bounds.extents.y);
            }
            else
            {
                col.radius = 0.5f;
            }
        }
    }

    /// <summary>扣血（玩家水弹通过 SendMessage("OnHit", 伤害) 调用这里）</summary>
    public void OnHit(float damage)
    {
        TakeDamage(damage);
    }

    /// <summary>扣血：别的脚本也可以直接调</summary>
    public void TakeDamage(float damage)
    {
        if (IsDead || invincible) return;
        if (damage <= 0f) return;

        currentHp -= damage;
        if (currentHp < 0f) currentHp = 0f;

        Debug.Log("Boss受到 " + damage + " 点伤害，剩余血量 " + currentHp + " / " + maxHp
                  + "（" + Mathf.RoundToInt(HpRatio * 100f) + "%）");

        if (currentHp <= 0f) Die();
    }

    void Die()
    {
        IsDead = true;
        Debug.Log("Boss血量归零，倒下");

        SetBarVisible(false);

        // ★ 弹胜利界面。
        //   为什么必须在这里弹：原来是 BOSS.cs 负责的，换成这套新脚本以后没人喊"玩家赢了"，
        //   Boss 死了只会原地消失，玩家会以为游戏卡住了。
        //   为什么用 FindObjectOfType 兜底：Boss 是预制体，预制体不能引用场景物体，
        //   Inspector 里根本拖不上（和 PlayerHealth → GameOverPanel 是同一个坑）。
        if (showWinOnDeath)
        {
            if (gameOverPanel == null) gameOverPanel = FindObjectOfType<GameOverPanel>();

            if (gameOverPanel != null) gameOverPanel.ShowWin();
            else Debug.LogWarning("[Boss] 找不到 GameOverPanel，胜利界面弹不出来！");
        }

        if (destroyOnDeath) Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }

    //==================== 头顶的红色矩形血条 ====================

    void CreateBar()
    {
        Sprite white = ShapeSprites.Square();

        //底槽（黑半透明，用来显示"总共有多少血"）
        barBgGo = new GameObject("Boss血条底(运行时生成)");
        SpriteRenderer bg = barBgGo.AddComponent<SpriteRenderer>();
        bg.sprite = white;
        bg.color = barBackgroundColor;
        bg.sortingOrder = barSortingOrder;

        //红条（剩下的血）
        barFillGo = new GameObject("Boss血条(运行时生成)");
        barFill = barFillGo.AddComponent<SpriteRenderer>();
        barFill.sprite = white;
        barFill.color = barColor;
        barFill.sortingOrder = barSortingOrder + 1;

        RefreshBar();
    }

    void LateUpdate()
    {
        if (showHpBar && !IsDead) RefreshBar();
    }

    /// <summary>每帧把血条摆到Boss头顶，并按血量比例缩短红条（从左往右缩）</summary>
    void RefreshBar()
    {
        if (barBgGo == null || barFillGo == null) return;

        if (bossRenderer == null) bossRenderer = GetComponent<SpriteRenderer>();

        //Boss头顶高度：按贴图在世界里的实际高度算，所以Boss放大3倍以后血条会自己往上移
        float halfHeight = 0.5f;
        if (bossRenderer != null && bossRenderer.sprite != null)
        {
            halfHeight = bossRenderer.bounds.extents.y;
        }

        float width = Mathf.Max(0.01f, barWidth);
        float height = Mathf.Max(0.005f, barHeight);
        float filled = width * HpRatio;

        Vector3 center = transform.position + Vector3.up * (halfHeight + barOffsetY + height * 0.5f);

        //底槽
        barBgGo.transform.position = center;
        barBgGo.transform.localScale = new Vector3(width, height, 1f);

        //红条：左端固定，右边按血量缩短
        barFillGo.transform.position = new Vector3(center.x - width * 0.5f + filled * 0.5f, center.y, center.z);
        barFillGo.transform.localScale = new Vector3(Mathf.Max(0.0001f, filled), height * 0.75f, 1f);
        barFillGo.SetActive(HpRatio > 0f);
    }

    void SetBarVisible(bool visible)
    {
        if (barBgGo != null) barBgGo.SetActive(visible);
        if (barFillGo != null) barFillGo.SetActive(visible);
    }

    void OnDestroy()
    {
        //血条是运行时生成的独立物体，Boss没了要一起清掉，不然会留在场景里
        if (barBgGo != null) Destroy(barBgGo);
        if (barFillGo != null) Destroy(barFillGo);
    }
}
