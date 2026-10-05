using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemymove_Ghost : MonoBehaviour
{
    public enum EnemyState
    { chase,attack,hurt,dead}
    private Rigidbody2D rb;
    public Transform player;
    private int facingDirection = -1;
    [Header("速度")]
    public float moveSpeed;
    [Header("总生命")]
    public int maxHp = 10;
    public int currentHp;
    private EnemyState enemyState;
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
}
