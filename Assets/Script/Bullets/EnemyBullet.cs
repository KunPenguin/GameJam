using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 3f;
    public Rigidbody2D rb;

    //新修正，新加入Onhit，但与原来差不多
    public float finalDamage = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  //拿rigidbody的插件，存进rb
        Destroy(gameObject, lifeTime);     //存在时间
    }

    void FixedUpdate()                    //射击的方向，朝向玩家
    {
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D other)   //碰撞评定，主要检测tag
    {
        // 撞到玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家被击中！");
            other.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver); //新加入，控制台发消息
            Destroy(gameObject);
        }
    }
}