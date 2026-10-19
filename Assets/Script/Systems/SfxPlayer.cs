//这个脚本挂在 GameScene 里的 GameManager 上（挂在任意一个"一直存在"的物体上都行）
//作用：统一播放游戏音效（怪物/玩家/装置/UI），并且跨场景常驻
//需要组件：不用手动加，脚本会自动给自己加两个 AudioSource
//别的脚本怎么调用：SfxPlayer.instance.PlayEnemyAttack();
//
//【重点】跨场景常驻 + 自动补：
//   1. 进游戏时当前场景里没有 SfxPlayer，它会自己创建一个，并且跨场景不销毁；
//   2. 音效格子没拖文件时，自动从 Assets/Resources/Audio/ 里按名字加载。
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    //别的脚本用这个拿到播放器（全局只有一份）
    public static SfxPlayer instance;

    [Header("怪物音效")]
    public AudioClip enemyAttackClip;//怪物远程攻击
    [Header("玩家音效")]
    public AudioClip hurtClip;//玩家受伤
    [Header("装置/UI 音效")]
    public AudioClip repairLoopClip;//装置修理中（进入范围播一遍，离开暂停）
    public AudioClip repairDoneClip;//装置修好
    public AudioClip placeClip;//放置物品（模块装进插槽）
    public AudioClip clickClip;//UI 点击

    [Header("播放器（留空会自动加）")]
    public AudioSource oneShotSource;//放"响一下就完"的音效
    public AudioSource loopSource;//放"装置修理中"的音效（要能暂停）

    [Header("音量")]
    public float volume = 1f;

    //=========== 自动常驻：场景加载完后如果还没有播放器，就自己造一个 ===========
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (instance != null) return;

        GameObject go = new GameObject("SfxPlayer(自动创建)");
        go.AddComponent<SfxPlayer>();
        Debug.Log("[音效] 当前场景没有 SfxPlayer，已自动创建一个（音效从 Resources/Audio 加载）");
    }

    void Awake()
    {
        if (instance != null && instance != this)//已经有常驻的了
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (oneShotSource == null) oneShotSource = gameObject.AddComponent<AudioSource>();
        if (loopSource == null) loopSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        loopSource.loop = false;   //修理音效由 DeviceRepair 控制（进入范围循环播、离开/修好立刻停）

        //格子空着就从 Resources 里按名字加载
        if (enemyAttackClip == null) enemyAttackClip = Resources.Load<AudioClip>("Audio/怪物远程攻击");
        if (hurtClip == null) hurtClip = Resources.Load<AudioClip>("Audio/受伤");
        if (repairLoopClip == null) repairLoopClip = Resources.Load<AudioClip>("Audio/装置修理中");
        if (repairDoneClip == null) repairDoneClip = Resources.Load<AudioClip>("Audio/装置被修好");
        if (placeClip == null) placeClip = Resources.Load<AudioClip>("Audio/放置物品");
        if (clickClip == null) clickClip = Resources.Load<AudioClip>("Audio/点击音效");

        Debug.Log("[音效] 加载情况 → 怪攻击:" + Show(enemyAttackClip) + " 受伤:" + Show(hurtClip)
                + " 修理中:" + Show(repairLoopClip) + " 修好:" + Show(repairDoneClip)
                + " 放置:" + Show(placeClip) + " 点击:" + Show(clickClip));

        if (FindObjectOfType<AudioListener>() == null)
            Debug.LogWarning("[音效] 当前场景里没有 AudioListener！声音听不见");
    }

    static string Show(AudioClip c) { return c != null ? c.name : "【空!】"; }

    //==================== 临时诊断按键（上架版已关闭；想恢复就删掉下面的 /* 和 */ ）====================
    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) { Debug.Log("[音效] 测试 O：怪物远程攻击"); PlayEnemyAttack(); }
        if (Input.GetKeyDown(KeyCode.U)) { Debug.Log("[音效] 测试 U：装置修理中（循环播放）"); PlayRepairSound(); }
        if (Input.GetKeyDown(KeyCode.I)) { Debug.Log("[音效] 测试 I：暂停修理声 + 装置修好"); StopRepairSound(); PlayRepairDone(); }
        if (Input.GetKeyDown(KeyCode.Alpha7)) { Debug.Log("[音效] 测试 7：玩家受伤"); PlayHurt(); }
        if (Input.GetKeyDown(KeyCode.Alpha8)) { Debug.Log("[音效] 测试 8：放置物品"); PlayPlace(); }
        if (Input.GetKeyDown(KeyCode.Alpha9)) { Debug.Log("[音效] 测试 9：UI 点击"); PlayClick(); }
    }
    */

    //==================== 给别的脚本调用的方法 ====================

    public void PlayEnemyAttack() { PlayOneShot(enemyAttackClip); }//怪物远程攻击
    public void PlayHurt() { PlayOneShot(hurtClip); }//玩家受伤
    public void PlayPlace() { PlayOneShot(placeClip); }//放置物品（模块装进插槽）
    public void PlayClick() { PlayOneShot(clickClip); }//UI 点击
    public void PlayRepairDone() { PlayOneShot(repairDoneClip); }//装置修好

    //进入修理范围：开始循环播放"装置修理中"音效（在范围内就一直播，重复调用不会叠加）
    public void PlayRepairSound()
    {
        if (repairLoopClip == null) return;
        loopSource.clip = repairLoopClip;
        loopSource.loop = true;
        if (loopSource.isPlaying == false) loopSource.Play();
    }

    //修好/设备销毁：彻底停掉
    public void StopRepairSound()
    {
        if (loopSource.isPlaying) loopSource.Stop();
        loopSource.clip = null;
    }

    void PlayOneShot(AudioClip clip)
    {
        if (clip == null) { Debug.LogWarning("[音效] 要播的音效是空的（没拖、Resources 也没加载到）"); return; }
        oneShotSource.PlayOneShot(clip, volume);
    }
}