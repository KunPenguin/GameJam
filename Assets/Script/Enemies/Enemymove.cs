using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove_Ghost : MonoBehaviour
{
    private Rigidbody2D rb;
    public Transform player;
    [Header("速度")]
    public float moveSpeed;
    [Header("总生命")]
    public int maxHp = 10;
    public int currentHp;
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
        if (player == null) return;
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction* moveSpeed;
        if(currentHp<=0)
        {
            Destroy(gameObject);
        }
    }
}
