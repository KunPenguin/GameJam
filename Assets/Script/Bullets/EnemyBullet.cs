using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class EnemyBullet : MonoBehaviour    //依旧填数值，欢迎来抓虫，有啥不对的，请马上来qq私我，我马上改
{
    //数值你们来调，我先随便填一点
    public float speed = 5f;                //子弹速度
    public float lifeTime = 3f;             //子弹的存在时间
    public Rigidbody2D rb;                  //物理组件
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();   //这一步获取组件，方便后续的rb
        Destroy(gameObject, lifeTime);      //实现子弹有存在时间的效果
    }

    // Update is called once per frame
    void FixedUpdate()                     //fixed可以使update有固定的时间间隔，对后续有好处（确信)
    {
        rb.velocity = transform.right * speed;   //移动的代码
    }
    void OnTriggerEnter2D(Collider2D other)  //这一段是碰撞
    {
        if (other.CompareTag("Player"))     //撞到的tag是不是player
        {
            Debug.Log("玩家被击中！");
            Destroy(gameObject);            //大概是写完了，有啥不对马上q我
        }
    }
}
