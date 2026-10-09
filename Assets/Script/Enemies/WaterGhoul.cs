using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterGhoul : MonoBehaviour
{
    public enum EnemyState
    { chase,attack,hurt,dead}
    private Rigidbody2D rb;
    public Transform player;
    private int facingDirection = -1;
    [Header("速度")]
    public float moveSpeed;
    [Header("总生命")]
    public float maxHp = 10;
    public float currentHp;
    private EnemyState enemyState;
    public float damage = 1f;
    // Start is called before the first frame update
    void Start()
    {
        currentHp = maxHp;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        rb=GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
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
