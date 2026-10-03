//这个脚本挂在水弹预制体上（水弹预制体放在 Assets/Prefabs/Bullets 里）
//作用：水弹一直往前飞；打中带 Enemy 标签的敌人就打印日志并销毁；飞太久也销毁
//需要组件：同一个物体上要有 Rigidbody2D（Gravity Scale = 0）
//          和 CircleCollider2D，并且把 Collider 的 Is Trigger 勾上
using UnityEngine;

public class WaterBullet : MonoBehaviour
{
    public float speed = 14f;//飞行速度（单位/秒）
    public float lifeTime = 2.5f;//最长存活时间（秒），到时间自动销毁
    public Rigidbody2D rb;//刚体组件（Inspector 里留空的话，会自动找自己身上的）
    public float aliveTime;//运行时的存活计时，不用手动改

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0f;
    }

    void Update()
    {
        //----情况一：飞太久，销毁----
        aliveTime = aliveTime + Time.deltaTime;

        if (aliveTime >= lifeTime)
        {
            Debug.Log("水弹飞了 " + lifeTime + " 秒还没打到东西，自动销毁");
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        //沿自己的"右方向"飞；发射它的 PlayerShoot 已经把它转向鼠标方向了
        //注意：必须用 velocity 让物理系统来移动，不能写 transform.Translate
        rb.velocity = transform.right * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //----情况二：打中敌人，销毁----
        //用标签判断打中的是不是敌人（敌人的标签必须是 Enemy）
        if (other.CompareTag("Enemy") == false)
        {
            return;
        }

        //通知敌人"你被打中了"，敌人自己决定扣血还是死亡
        other.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);

        Debug.Log("水弹打中敌人：" + other.name + "，销毁");

        Destroy(gameObject);
    }
}
