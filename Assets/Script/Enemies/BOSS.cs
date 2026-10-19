// Boss 专用脚本，挂在 Test_Boss 预制体上。
// 为什么不能复用 WaterGhoul：那样 Boss 死了只会被 Destroy，
// 没有任何地方能喊"玩家赢了"。
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS : MonoBehaviour
{
    [Header("速度")]
    public float moveSpeed;
    private Rigidbody2D rb;              // chase() 里要改它的 velocity
    public Transform player;             // chase() 里要追它
    private int facingDirection = -1;    // Flip() 里记朝向
    [Header("血量")]
    public float maxHp = 300f;
    private float currentHp;
    public float damage = 1f;

    [Header("打赢后要弹的面板（把 GameManager 拖进来）")]
    public GameOverPanel gameOverPanel;


    [Header("攻击")]
    public float attackRange = 5.5f;      // 贴上这个距离就开始反复造成伤害（Boss 体型很大，所以值也大）
    public float attackInterval = 1f;     // 每隔多少秒造成一次伤害
    private float attackTimer = 0f;

    private bool dead = false;
    void Update()
    {
        if (dead) return;

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        chase();                          // 移动逻辑不变：走过去、贴上、停住

        if (player == null) return;

        // 贴着玩家 → 按冷却反复造成伤害（和近战怪同一套做法）
        // 为什么用距离而不是碰撞回调：Enter 只触发一次，Stay 在刚体睡着后可能不再触发；
        // "贴着"是一个持续状态，用距离判断最稳。
        if (Vector2.Distance(transform.position, player.position) <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackInterval;

            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
        }
    }
    void Flip()
    {
        facingDirection = facingDirection * -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
    void chase()
    {
        if (player.position.x > transform.position.x && facingDirection == -1 ||
                player.position.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }
    // 【原来的 OnTriggerEnter2D 已删除】
    // 原因同 WaterGhoul：Enter 只在"开始重叠"那一帧触发一次，贴上玩家后就不再触发，
    //       表现为"只掉一次血，然后两个一起站着"。
    // 现在改由 Update 里的"距离 + 冷却"负责伤害。
    // ⚠️ 不要再加回来，否则会和冷却判断叠加，造成双倍伤害。

    private void Start()
    {
        currentHp = maxHp;
        Debug.Log("[Boss] 出场，血量 " + maxHp);
        player = PlayerMove.T;      // 已经是 Transform，不用再 GetComponent，也不用遍历场景
        if (player == null) Debug.LogWarning("[BOSS] 没找到玩家（PlayerMove 还没登记，或场景里没有玩家）", this);
        rb = GetComponent<Rigidbody2D>();

        // 预制体不能引用场景物体 → 这里自己找一次（Inspector 拖了就优先用拖的）
        if (gameOverPanel == null)
        {
            gameOverPanel = FindObjectOfType<GameOverPanel>();
        }

        if (gameOverPanel == null)
        {
            Debug.LogWarning("[Boss] 场景里找不到 GameOverPanel，胜利界面弹不出来！");
        }
    }

    // ⚠️ 方法名和参数必须和水弹里的 SendMessage("OnHit", 伤害) 完全一致
    public void OnHit(float finalDamage)
    {
        if (dead) return;

        currentHp -= finalDamage;
        Debug.Log("[Boss] 受到 " + finalDamage + " 点伤害，剩余 " + currentHp);

        if (currentHp <= 0f)
        {
            dead = true;
            Debug.Log("[Boss] 被击败！");

            if (gameOverPanel != null)
            {
                gameOverPanel.ShowWin();
            }
            else
            {
                // 让"忘了拖引用"直接可见，而不是静默什么都不发生
                Debug.LogWarning("[Boss] gameOverPanel 没拖，胜利界面弹不出来！");
            }

            Destroy(gameObject);
        }
    }
}