//这个脚本挂在 GameScene 里的 Player 物体上（一般和 PlayerMove 挂在同一个物体上）
//作用：1.按住鼠标左键 → 触发"一系列函数"（模块链，现在先用 FireByModules 一个函数代表） → 最后调用开火函数 Fire
//      2.开火函数 Fire(...) 可以接收一整套数值（穿透/吸血/子弹大小/热力值/弹射/波数/每波个数/散射角），并把这些值传给子弹
//需要组件：不需要额外组件；但要先把水弹预制体拖到下面 Inspector 的 bulletPrefab 上
//          另外建议同一个物体上挂上 PlayerHealth（吸血要回血用），没挂也不会报错，只是吸不了血
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;//水弹预制体（必填！把 Assets/Prefabs/Bullets 里的水弹预制体拖到这里）
    public Transform firePoint;//枪口位置（可以不填，不填就从玩家中心发射）
    public float fireInterval = 0.15f;//射速：隔多少秒打一发，越小打得越快
    public float timer;//运行时的冷却计时，不用手动改

    //========== 下面这 9 个是"这一枪"的数值，按住左键发射时就按这里的值 ==========
    //（它们将来代表"模块链跑完之后"的最终数值：模块会在这里做加法/乘法改动）
    public int damage = 1;//伤害（现在只用来算吸血；通知敌人还是发 OnHit()，不带参数）
    public float pierce = 0f;//穿透：能多穿几个敌人，能命中的敌人数 = 穿透 + 1
    public float lifesteal = 0f;//吸血：0.1 表示"造成伤害的 10%"变成回血
    public float bulletSize = 0f;//子弹大小加成：0.5 表示变成 1.5 倍大
    public float heat = 0f;//热力值：-100 到 +100，现在只是存着，还没有实际效果
    public int bounce = 0;//弹射：穿透用完之后，还能弹几次去攻击别的敌人
    public int bulletsPerWave = 1;//每波子弹个数（一次横向射出几发，像霰弹枪）
    public int waveCount = 1;//子弹波数（一次发射几波，几波之间沿垂直方向错开）
    public float spreadAngle = 0f;//散射角度（总角度，单位：度；每波子弹在这个角度里均匀铺开）
    public float waveSpacing = 0.3f;//每波之间的间距（世界单位）

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
    //
    //现在整条模块链还没做，所以先用这一个函数把它的影响压缩在一起；
    //以后每做一个模块，就在这个函数里（或者用模块脚本）改某个数值，
    //改完再交给 Fire 发射。
    //
    //这个函数是 public 的，所以别的脚本也可以直接调用它来测试。
    //====================================================================
    public void FireByModules()
    {
        //这里把"这一枪"的最终数值交给开火函数。
        //现在直接取 Inspector 上的变量，方便你在面板上调；
        //以后模块链就是在这里把数值改掉（例如霰弹模块让 bulletsPerWave、spreadAngle 变大）。
        Fire(pierce, lifesteal, bulletSize, heat, bounce, waveCount, bulletsPerWave, spreadAngle);

        //------------------------------------------------------------------
        //【想临时写死一套数值试效果？】把上面那行最前面加 // 注释掉，
        //再把下面这行前面的 // 去掉，改成你想要的数字即可。
        //参数顺序：穿透, 吸血, 子弹大小, 热力值, 弹射, 波数, 每波个数, 散射角
        //举例：穿透2、吸血10%、子弹大30%、热力值0、弹射2次、1波、每波5发、散射30度
        //Fire(2f, 0.1f, 0.3f, 0f, 2, 1, 5, 30f);
        //------------------------------------------------------------------
    }

    //====================================================================
    //开火函数：一次发射
    //模块（或任何别的脚本）想打出自己的效果时，直接调用这个函数就行，例如：
    //    shoot.Fire(2f, 0.1f, 0.5f, 0f, 3, 1, 5, 30f);
    //意思就是：穿透2、吸血10%、子弹大50%、热力值0、弹射3次、1波、每波5发、散射30度
    //====================================================================
    public void Fire(float pierce, float lifesteal, float bulletSize, float heat, int bounce, int waveCount, int bulletsPerWave, float spreadAngle)
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

        //2.把不合理的数字挡一下，避免出 0 发子弹或者 0 波
        if (bulletsPerWave < 1) bulletsPerWave = 1;
        if (waveCount < 1) waveCount = 1;

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
        for (int wave = 0; wave < waveCount; wave++)
        {
            //这一波相对"正中间那一波"的偏移量（波数=1 时偏移就是 0）
            float offset = (wave - (waveCount - 1) * 0.5f) * waveSpacing;
            Vector3 wavePosition = startPosition + (Vector3)(sideDir * offset);

            //这一波里的每一发子弹
            for (int i = 0; i < bulletsPerWave; i++)
            {
                //散射：把这一波的子弹在 spreadAngle 里均匀铺开
                float t = 0f;
                if (bulletsPerWave > 1)
                {
                    t = (float)i / (bulletsPerWave - 1) - 0.5f;//算出来是 -0.5 到 +0.5
                }
                float angle = aimAngle + t * spreadAngle;

                SpawnOneBullet(wavePosition, angle, pierce, lifesteal, bulletSize, heat, bounce);
            }
        }

        Debug.Log("发射！波数 " + waveCount + "，每波 " + bulletsPerWave + " 发，散射 " + spreadAngle + " 度，这一次共 " + (waveCount * bulletsPerWave) + " 发");
    }

    //生成一颗水弹，并把这一枪的数值交给它
    void SpawnOneBullet(Vector3 position, float angle, float pierce, float lifesteal, float bulletSize, float heat, int bounce)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0f, 0f, angle));

        WaterBullet bulletScript = bullet.GetComponent<WaterBullet>();
        if (bulletScript == null)
        {
            Debug.LogWarning("水弹预制体上没有挂 WaterBullet 脚本！");
            return;
        }

        //把数值传给这颗子弹；吸血要回血，所以顺便把玩家身上的 PlayerHealth 也交给它
        bulletScript.Setup(damage, pierce, lifesteal, bulletSize, heat, bounce, GetComponent<PlayerHealth>());
    }
}
