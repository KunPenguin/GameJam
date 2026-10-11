//这个脚本挂在 GameScene 里的 GameManager 上（挂在任意一个"一直存在"的物体上都行）
//作用：统一播放游戏音效：怪物远程攻击、怪物死亡、装置修理中（循环）、装置修好
//需要组件：不用手动加，脚本会自动给自己加两个 AudioSource
//别的脚本怎么调用：SfxPlayer.instance.PlayEnemyAttack();
//
//【重点】这个脚本会"跨场景常驻 + 自动补"：
//   1. 进游戏时如果当前场景里没有 SfxPlayer，它会自己创建一个，并且跨场景不销毁
//      —— 所以像 EnemySpawnTestScene 这种单独按 Play 的测试场景也能听到音效；
//   2. 音效格子没拖文件时，会自动从 Assets/Resources/Audio/ 里按名字加载
//      —— 面板上拖了就用拖的，没拖就用 Resources 里的。
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    //别的脚本用这个拿到播放器（全局只有一份）
    public static SfxPlayer instance;

    [Header("音效文件（拖了就用拖的；留空会自动从 Resources/Audio/ 加载）")]
    public AudioClip enemyAttackClip;//怪物远程攻击
    public AudioClip enemyDeathClip;//怪物死亡
    public AudioClip repairLoopClip;//装置修理中（循环播放）
    public AudioClip repairDoneClip;//装置修好

    [Header("播放器（留空会自动加）")]
    public AudioSource oneShotSource;//放"响一下就完"的音效
    public AudioSource loopSource;//放"一直循环"的修理声

    [Header("音量")]
    public float volume = 1f;

    //=========== 自动常驻：场景加载完后如果还没有播放器，就自己造一个 ===========
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (instance != null) return;//场景里已经有了（它的 Awake 已经跑过）

        GameObject go = new GameObject("SfxPlayer(自动创建)");
        go.AddComponent<SfxPlayer>();
        Debug.Log("[音效] 当前场景没有 SfxPlayer，已自动创建一个（音效从 Resources/Audio 加载）");
    }

    void Awake()
    {
        //已经有常驻的了（比如从别的场景切过来），就不要重复创建
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);   //跨场景不销毁

        //忘了拖就自动加，省得没声音还找不到原因
        if (oneShotSource == null) oneShotSource = gameObject.AddComponent<AudioSource>();
        if (loopSource == null) loopSource = gameObject.AddComponent<AudioSource>();

        oneShotSource.playOnAwake = false;
        loopSource.playOnAwake = false;
        loopSource.loop = true;

        //格子空着就从 Resources 里按名字加载（这样任何场景都有音效）
        if (enemyAttackClip == null) enemyAttackClip = Resources.Load<AudioClip>("Audio/怪物远程攻击");
        if (enemyDeathClip == null) enemyDeathClip = Resources.Load<AudioClip>("Audio/怪物死亡");
        if (repairLoopClip == null) repairLoopClip = Resources.Load<AudioClip>("Audio/装置修理中");
        if (repairDoneClip == null) repairDoneClip = Resources.Load<AudioClip>("Audio/装置被修好");

        //【诊断用】启动时把加载情况打出来，方便查"没声音"
        Debug.Log("[音效] 加载情况 → 攻击:" + (enemyAttackClip != null ? enemyAttackClip.name : "【空!】")
                + "  死亡:" + (enemyDeathClip != null ? enemyDeathClip.name : "【空!】")
                + "  修理中:" + (repairLoopClip != null ? repairLoopClip.name : "【空!】")
                + "  修好:" + (repairDoneClip != null ? repairDoneClip.name : "【空!】")
                + "  AudioSource:" + (oneShotSource != null ? "有" : "无"));

        //顺便看一下场景里有没有 AudioListener（没有的话等于整个世界没声音）
        if (FindObjectOfType<AudioListener>() == null)
        {
            Debug.LogWarning("[音效] 当前场景里没有 AudioListener！声音听不见，请给相机加一个 AudioListener 组件");
        }
    }

    //==================== 临时诊断按键（查完"没声音"的问题后可以整段删掉）====================
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) { Debug.Log("[音效] 测试按键 T：怪物远程攻击"); PlayEnemyAttack(); }
        if (Input.GetKeyDown(KeyCode.Y)) { Debug.Log("[音效] 测试按键 Y：怪物死亡"); PlayEnemyDeath(); }
        if (Input.GetKeyDown(KeyCode.U)) { Debug.Log("[音效] 测试按键 U：装置修理中（循环）"); PlayRepairLoop(); }
        if (Input.GetKeyDown(KeyCode.I)) { Debug.Log("[音效] 测试按键 I：停循环 + 装置修好"); StopRepairLoop(); PlayRepairDone(); }
    }

    //==================== 给别的脚本调用的 5 个方法 ====================

    //怪物远程攻击：远程怪开火时调用
    public void PlayEnemyAttack()
    {
        PlayOneShot(enemyAttackClip);
    }

    //怪物死亡：血扣完的那一刻调用
    public void PlayEnemyDeath()
    {
        PlayOneShot(enemyDeathClip);
    }

    //装置开始修理：循环播放修理声（修理玩法做好后，在"开始读条"时调用）
    public void PlayRepairLoop()
    {
        if (repairLoopClip == null) return;

        loopSource.clip = repairLoopClip;
        loopSource.loop = true;
        if (loopSource.isPlaying == false) loopSource.Play();
    }

    //装置修理中断/结束：停掉循环声
    public void StopRepairLoop()
    {
        if (loopSource.isPlaying) loopSource.Stop();
    }

    //装置修好：响一声（在"修好的一瞬间"调用）
    public void PlayRepairDone()
    {
        PlayOneShot(repairDoneClip);
    }

    //内部用：播一个"响一下就完"的音效（顺手打印一条，方便确认到底有没有被调用）
    void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[音效] 要播的音效是空的（没拖、Resources 也没加载到）");
            return;
        }
        oneShotSource.PlayOneShot(clip, volume);
        Debug.Log("[音效] 播放：" + clip.name + "（音量 " + volume + "，AudioSource 启用=" + oneShotSource.enabled + "）");
    }
}