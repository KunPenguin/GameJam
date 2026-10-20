//这个脚本挂在水弹物体上（不用手动挂：BossAttack 放技能时会自己生成一颗）
//作用：1.从Boss身边冒出来之后，一直追着玩家飞（追踪水弹）
//      2.碰到玩家就固定扣玩家血，然后自己消失
//      3.自己只有 1 点血，玩家打一下就炸（玩家的水弹只打 Tag = Enemy 的物体，所以这里自动打上 Enemy 标签）
//外形：临时用【神蓝色实心圆圈】当模型 —— 圆圈贴图是运行时生成的，不用往项目里塞图片
using UnityEngine;

public class BossWaterBullet : MonoBehaviour
{
    [Header("=== 追踪 ===")]
    [Tooltip("飞行速度")]
    public float speed = 3.5f;

    [Tooltip("活太久就自动消失（秒），免得一直在场上追玩家")]
    public float lifeTime = 6f;

    [Header("=== 伤害（固定点数） ===")]
    [Tooltip("碰到玩家扣玩家多少血")]
    public float damage = 2f;

    [Header("=== 血量（能被玩家打掉） ===")]
    [Tooltip("水弹的血量：玩家打够这么多伤害就炸（默认1 = 一下一个）")]
    public float maxHp = 1f;

    [Tooltip("运行时剩余血量，不用手动改")]
    public float currentHp = 1f;

    [Header("=== 外形（神蓝色实心圆） ===")]
    [Tooltip("圆圈半径（世界单位）")]
    public float radius = 0.22f;

    [Tooltip("圆圈颜色：默认神蓝色")]
    public Color color = new Color(0.15f, 0.45f, 1f, 1f);

    [Tooltip("渲染层级，数字越大越靠前")]
    public int sortingOrder = 30;

    [Header("=== 目标引用 ===")]
    [Tooltip("玩家，留空会自动找 Tag = Player")]
    public Transform player;

    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private float aliveTime = 0f;
    private float playerRadius = 0.25f;
    private bool dead = false;

    void Awake()
    {
        //玩家的水弹只会打 Tag = Enemy 的物体，所以自己先打上这个标签
        gameObject.tag = "Enemy";

        //实心圆贴图（运行时生成，1x1世界单位）
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = ShapeSprites.Circle();
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;

        //触发器碰撞体：玩家的水弹撞上来才打得到它
        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle == null) circle = gameObject.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.5f;   //0.5 * 缩放(2*radius) = 世界半径 radius

        //运动学刚体：让2D物理知道它是个会移动的物体，触发器事件才稳
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        ApplyLook();
    }

    void Start()
    {
        currentHp = maxHp;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            if (playerHealth == null) playerHealth = player.GetComponentInParent<PlayerHealth>();

            SpriteRenderer playerSr = player.GetComponentInChildren<SpriteRenderer>();
            if (playerSr != null && playerSr.sprite != null)
            {
                playerRadius = Mathf.Max(playerSr.bounds.extents.x, playerSr.bounds.extents.y);
            }
        }
    }

    /// <summary>
    /// 由 BossAttack 生成水弹时调用：把这一发水弹的数值传进来（伤害、速度、血量、大小、颜色、目标）
    /// </summary>
    public void Setup(float dmg, float spd, float life, float hp, float size, Color col, Transform target)
    {
        damage = dmg;
        speed = spd;
        lifeTime = life;
        maxHp = hp;
        radius = size;
        color = col;
        if (target != null) player = target;

        ApplyLook();
    }

    /// <summary>把颜色和大小应用到圆圈上</summary>
    void ApplyLook()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        transform.localScale = Vector3.one * Mathf.Max(0.02f, radius * 2f);
    }

    void Update()
    {
        if (dead) return;

        //活太久自动消失
        aliveTime += Time.deltaTime;
        if (aliveTime >= lifeTime)
        {
            Debug.Log("水弹飞了 " + lifeTime + " 秒还没碰到玩家，自动消失");
            Destroy(gameObject);
            return;
        }

        //碰到玩家：扣血 + 自己消失（不需要玩家身上有碰撞体）
        if (player != null)
        {
            float hitDistance = radius + playerRadius;
            if (Vector2.Distance(transform.position, player.position) <= hitDistance)
            {
                HitPlayer();
            }
        }
    }

    void FixedUpdate()
    {
        if (dead || rb == null || player == null) return;

        //每一帧都朝玩家"现在"的位置挪，所以会一直追着玩家（追踪水弹）
        Vector2 next = Vector2.MoveTowards(rb.position, (Vector2)player.position, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);
    }

    /// <summary>水弹打到玩家</summary>
    void HitPlayer()
    {
        if (dead) return;
        dead = true;

        if (playerHealth != null && !playerHealth.dead) playerHealth.TakeDamage(damage);
        Debug.Log("水弹命中玩家，固定伤害 " + damage + " 点");

        Destroy(gameObject);
    }

    /// <summary>
    /// 被玩家的水弹打中时，由 SendMessage("OnHit", 伤害) 调到这儿。
    /// 打够 maxHp 点伤害就被打掉（默认只有1点血，一下就没）。
    /// </summary>
    public void OnHit(float dmg)
    {
        if (dead) return;

        currentHp -= dmg;
        Debug.Log("水弹受到 " + dmg + " 点伤害，剩余血量 " + currentHp + " / " + maxHp);

        if (currentHp <= 0f)
        {
            dead = true;
            Debug.Log("水弹被打掉");
            Destroy(gameObject);
        }
    }

    //兜底：万一玩家的水弹把它当触发器撞上来（两边都有触发器时会触发）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) HitPlayer();
    }
}

/// <summary>
/// 运行时生成贴图的小工具：白圆、白方块，都不用往项目里塞图片资源。
/// （白圆给水弹用，白方块给Boss血条用；都是 1x1 世界单位）
/// </summary>
public static class ShapeSprites
{
    private static Sprite circle;
    private static Sprite square;

    /// <summary>白色实心圆（抗锯齿），1x1 世界单位</summary>
    public static Sprite Circle()
    {
        if (circle != null) return circle;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1) * 0.5f;
        float r = size * 0.5f - 1f;
        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - d);   //边缘1像素做抗锯齿
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        circle = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        circle.name = "运行时生成的圆";
        return circle;
    }

    /// <summary>白色实心方块，1x1 世界单位</summary>
    public static Sprite Square()
    {
        if (square != null) return square;

        int size = 4;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(pixels);
        tex.Apply();

        square = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        square.name = "运行时生成的方块";
        return square;
    }
}
