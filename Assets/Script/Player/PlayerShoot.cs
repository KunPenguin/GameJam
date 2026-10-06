//这个脚本挂在 GameScene 里的 Player 物体上（一般和 PlayerMove 挂在同一个物体上）
//作用：1.按住鼠标左键 → 触发"一系列函数"（模块链，现在先用 FireByModules 一个函数代表） → 最后调用开火函数 Fire
//      2.开火函数 Fire(...) 可以接收一整套数值（穿透/吸血/子弹大小/热力值/弹射/波数/每波个数/散射角），并把这些值传给子弹
//需要组件：不需要额外组件；但要先把水弹预制体拖到下面 Inspector 的 bulletPrefab 上
//          另外建议同一个物体上挂上 PlayerHealth（吸血要回血用），没挂也不会报错，只是吸不了血
using UnityEngine;
using System.Collections.Generic;//为了使用列表功能而引入

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;//水弹预制体（必填！把 Assets/Prefabs/Bullets 里的水弹预制体拖到这里）
    public Transform firePoint;//枪口位置（可以不填，不填就从玩家中心发射）
    public float fireInterval = 0.15f;//射速：隔多少秒打一发，越小打得越快
    public float timer;//运行时的冷却计时，不用手动改
    
    [Header("基础数值模板")]//将AttackContext里的数值引到PlayerShoot的inspector面板上
    public AttackContext baseContext = new AttackContext();

    //原本存在于此的9个变量的初始化被删了，因为AttackContext脚本将承担这个工作

    //当前玩家装备的模块列表
    //此处不用手动调整，UI做好后，这个列表直接由UI管理
    public List<WeaponModule> equippedModules = new List<WeaponModule>();

    //临时测试用开火模组，真正的模组列表会随UI补充
    private void Start()
    {
        equippedModules.Add(new Test_FireModule());   // 开火
    }

    void Update()
    {
        //计时器一直往下减，减到 0 以下就可以再打一发
        timer = timer - Time.deltaTime;

        //没按住鼠标左键就不打
        if (Input.GetMouseButton(0) == false) return;

        //还在冷却中就不打
        if (timer > 0f) return;

        timer = fireInterval;

        //按下左键 → 先走"一系列函数"（模块链）→ 最后由它调用开火函数 Fire
        FireByModules();
    }

    //====================================================================
    //"一系列函数"的入口：代表【模块链】
    //====================================================================
    public void FireByModules()
    {
        // 更新列表状态，保证每一次点鼠标发射，都从零开始累积状态，防止上一枪的属性残留到这一枪。
        AttackContext ctx = baseContext.Clone();

        // 2. 依次遍历所有模块
        foreach (var module in equippedModules)
        {
            module.Apply(ctx);

            // 3. 遇到开火节点，就用当前累积的状态发射一次
            if (ctx.isFireTrigger)
            {
                Fire(ctx);//将ctx里的值一个一个填入fire中
                ctx.isFireTrigger = false;
            }
        }
    }

    //====================================================================
    //开火函数：一次发射
    //模块（或任何别的脚本）想打出自己的效果时，直接调用这个函数就行，例如：
    //    shoot.Fire(2f, 0.1f, 0.5f, 0f, 3, 1, 5, 30f);
    //意思就是：穿透2、吸血10%、子弹大50%、热力值0、弹射3次、1波、每波5发、散射30度
    //====================================================================
    /// <summary>
    /// 这是一个管控开火时最终属性的函数，无特殊情况只管调用，要改先去跟队长讨论一下
    /// </summary>
    /// <param name="ctx"></param>
    public void Fire(AttackContext ctx)
    {
        //忘了拖预制体时给个提示，免得以为是脚本坏了
        if (bulletPrefab == null)
        {
            Debug.LogWarning("还没有把水弹预制体拖到 PlayerShoot 的 bulletPrefab 上！");
            return;
        }

        //1.算出"玩家 → 鼠标"的方向，并转成角度
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDir = mouseWorld - transform.position;
        aimDir = aimDir.normalized;
        float aimAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        //2.哪怕波数与每波子弹数为0甚至为负也至少打一发
        if (ctx.bulletsPerWave < 1) ctx.bulletsPerWave = 1;
        if (ctx.waveCount < 1) ctx.waveCount = 1;

        //3.子弹从哪儿出生（没填枪口就用玩家自己的位置）
        Vector3 startPosition = transform.position;
        if (firePoint != null)
        {
            startPosition = firePoint.position;
        }

        //4."垂直于瞄准方向"的方向：把瞄准方向转 90 度就是它
        //  用它把每一波错开，打出上下几排
        Vector2 sideDir = new Vector2(-aimDir.y, aimDir.x);

        //5.一波一波地生成子弹
        for (int wave = 0; wave < ctx.waveCount; wave++)
        {
            //这一波相对"正中间那一波"的偏移量（波数=1 时偏移就是 0）
            float offset = (wave - (ctx.waveCount - 1) * 0.5f) * ctx.waveSpacing;
            Vector3 wavePosition = startPosition + (Vector3)(sideDir * offset);

            //这一波里的每一发子弹
            for (int i = 0; i < ctx.bulletsPerWave; i++)
            {
                //散射：把这一波的子弹在 spreadAngle 里均匀铺开
                float t = 0f;
                if (ctx.bulletsPerWave > 1)
                {
                    t = (float)i / (ctx.bulletsPerWave - 1) - 0.5f;//算出来是 -0.5 到 +0.5
                }
                float angle = aimAngle + t * ctx.spreadAngle;

                SpawnOneBullet(wavePosition, angle, ctx);
            }
        }

        Debug.Log("发射！波数 " + ctx.waveCount + "，每波 " + ctx.bulletsPerWave + " 发，散射 " + ctx.spreadAngle + " 度，这一次共 " + (ctx.waveCount * ctx.bulletsPerWave) + " 发");
    }

    //生成一颗水弹，并把这一枪的数值交给它
    void SpawnOneBullet(Vector3 position, float angle, AttackContext ctx)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0f, 0f, angle));

        WaterBullet bulletScript = bullet.GetComponent<WaterBullet>();
        if (bulletScript == null)
        {
            Debug.LogWarning("水弹预制体上没有挂 WaterBullet 脚本！");
            return;
        }

        //把数值传给这颗子弹；吸血要回血，所以顺便把玩家身上的 PlayerHealth 也交给它
        bulletScript.Setup(ctx, GetComponent<PlayerHealth>());
    }
}
