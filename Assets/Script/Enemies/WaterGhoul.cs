using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterGhoul : MonoBehaviour
{
    public enum EnemyState
    { chase,attack,hurt,dead}
    private Rigidbody2D rb;
    private int facingDirection = -1;
    [Header("速度")]
    public float moveSpeed;
    [Header("总生命")]
    public float maxHp = 10;
    public float currentHp;
    private EnemyState enemyState;
    public float damage = 1f;
    Transform _player;
    // Start is called before the first frame update
    void Start()
    {
        currentHp = maxHp;
        if (PlayerMove.Instance != null)
            _player = PlayerMove.Instance.transform;
        rb =GetComponent<Rigidbody2D>();
    }

    [Header("攻击")]
    public float attackRange = 0f;      // 贴上这个距离就开始反复造成伤害
    public float attackInterval = 1f;     // 每隔多少秒造成一次伤害
    private float attackTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (_player == null) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        chase();                          // 移动逻辑不变：走过去、贴上、停住
        // 贴着玩家 → 按冷却反复造成伤害
        // 为什么用距离而不是碰撞回调：碰撞的 Enter 只触发一次，Stay 在刚体睡着后可能不再触发；
        // "贴着"是一个持续状态，用距离判断最稳。
        if (Vector2.Distance(transform.position, _player.position) <= attackRange && attackTimer <= 0f)
        {
            attackTimer = attackInterval;

            PlayerHealth ph = _player.GetComponent<PlayerHealth>();
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
        if (_player.position.x > transform.position.x && facingDirection == -1 ||
                _player.position.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }
        Vector2 direction = (_player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }
    // 【原来的 OnTriggerEnter2D 已删除】
    // 原因：Enter 只在"开始重叠"那一帧触发一次，贴上玩家后就不再触发，
    //       表现为"只掉一次血，然后两个一起站着"。
    // 现在改由 Update 里的"距离 + 冷却"负责伤害。
    // ⚠️ 不要再加回来，否则会和冷却判断叠加，造成双倍伤害。
    // 被玩家水弹打中时调用
    // 为什么加这个：水弹会通过 SendMessage("OnHit", 伤害值) 来通知敌人。
    public void OnHit(float finalDamage)
    {
        currentHp -= finalDamage;
        Debug.Log("水鬼受到伤害：" + finalDamage + "，剩余血量：" + currentHp);

        if (currentHp <= 0f)
        {
            Debug.Log("水鬼死亡！");
            Destroy(gameObject);
        }
    }
}
