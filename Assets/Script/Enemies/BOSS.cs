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


    private bool dead = false;
    void Update()
    {
        chase();
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
    void OnTriggerEnter2D(Collider2D other)  //攻击
    {
        if (other.CompareTag("Player"))     //撞到的tag是不是player
        {
            Debug.Log("玩家被击中！");
            //新增：真的扣玩家血（调用玩家身上 PlayerHealth 脚本的 TakeDamage）
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        }
    }

    private void Start()
    {
        currentHp = maxHp;
        Debug.Log("[Boss] 出场，血量 " + maxHp);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
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