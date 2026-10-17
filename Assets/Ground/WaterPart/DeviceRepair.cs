using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceRepair : MonoBehaviour
{
    [Header("修复范围")]
    [Tooltip("玩家进到这个半径内才开始修（世界单位，1 = 一格）。")]
    public float repairRadius = 2f;
    [Header("玩家标签")]
    public string playerTag = "Player";
    [Header("修复所需时间")]
    public float repairSeconds = 5f;
    [SerializeField] private bool resetOnLeave = true;
    [SerializeField] private bool resetWhenDamaged = true;
    [Header("拖这片水的父物体（记得打勾）")]
    public Transform waterRoot;
    [SerializeField] private bool destroyWhenRepaired = true;
    [Header("进度条显示")]
    public bool showBarOnScreen = true;
    [Tooltip("只有正在修的时候才显示（不修就不显示，画面干净）。")]
    public bool showBarOnlyWhenRepairing = true;
    [Header("测试用，按 H = 假装玩家受伤")]
    public bool debugKey = true;
    [Header("修理好的设备")]
    public GameObject CompleteDevice;
    [Header("修理声音）")]
    public AudioSource repairSound;
    [Header("修理完成声音")]
    public AudioSource finishSound;
    /// <summary>修复进度：0 = 没开始，1 = 修好了。</summary>
    public float Progress01 { get; private set; }
    
    /// <summary>是否已经修好。</summary>
    public bool IsRepaired { get; private set; }
    
    /// <summary>玩家此刻在不在范围里。</summary>
    public bool PlayerInRange { get; private set; }
    
    /// <summary>登记所有水设备。</summary>
    static readonly List<DeviceRepair> AllDevices = new List<DeviceRepair>();
    private Transform _player;          // 找到的玩家（缓存起来，不要每帧去找）
    private bool _reportedMissingTag;   // 标签不存在时只报一次错
    public static event System.Action<DeviceRepair> Repaired;
    void OnEnable()
    {
        if (!AllDevices.Contains(this)) AllDevices.Add(this);   // 开机登记
    }

    void OnDisable()
    {
        AllDevices.Remove(this);                                // 销毁/关闭时注销
        StopRepairSoundLoop();   //新增：销毁时停掉修理音效
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRepaired) return;
        if (debugKey && Input.GetKeyDown(KeyCode.H)) NotifyPlayerDamaged();
        if (_player == null) _player = FindPlayer();
        if(!IsPlayerInRange())
        {
            
            PlayerInRange = false;
            StopRepairSoundLoop();   //新增：离开范围停止修理音效
            if (resetOnLeave)
                ResetProgress();
            return;
        }
        AddProgress(Time.deltaTime / Mathf.Max(0.01f, repairSeconds));
    }
    public static void NotifyPlayerDamaged()
    {
        for (int i = AllDevices.Count - 1; i >= 0; i--)
        {
            DeviceRepair d = AllDevices[i];

            if (d == null) { AllDevices.RemoveAt(i); continue; }
            if (!d.resetWhenDamaged) continue;

            d.ResetProgress();
        }
    }//角色受伤重置进度
    public void ResetProgress()//重置进度
    {
        if (IsRepaired) return;
        bool hadProgress = Progress01 > 0.0001f;

        Progress01 = 0f;
        StopRepairSoundLoop();   //新增：进度清零时停止修理音效

        if (hadProgress) Debug.Log(  "修复进度清零", this);
    }
    public void AddProgress(float delta)
    {
        if (IsRepaired || delta <= 0f) return;
        Progress01 = Mathf.Clamp01(Progress01 + delta);
        StartRepairSoundLoop();   //新增：修理中循环播放修理音效
        if (Progress01 >= 1f) Complete();
       
    }//加进度
    void Complete()
    {
        IsRepaired = true;
        StopRepairSoundLoop();   //停掉修理音效
        PlayFinishSound();       //播放"装置修好"音效
        Progress01 = 1f;
        Debug.Log("[设备] " + name + " 修复完成！", this);

        ClearWater();            // 这片水全部消失

        // 生成"修好后的外观"（没配就跳过，别抛异常）
        if (CompleteDevice != null)
        {
            GameObject done = Instantiate(CompleteDevice, transform.position, Quaternion.identity, transform.parent);
            done.name = "Fixed_" + name;
        }
        else
        {
            Debug.LogWarning("[设备] 没配 CompleteDevice，修好了但看不见变化。", this);
        }

        // ★ 只广播一次（原来这里写了两遍，靠 DeviceChaos 的 cooldown 才没炸）
        if (Repaired != null) Repaired(this);

        // 修好 → 发一个模块（三选一）并进入整理界面
        if (ModulePanelUI.Instance != null)
        {
            ModulePanelUI.Instance.Open();
        }
        else
        {
            Debug.LogWarning("装置修好了，但场景里找不到 ModulePanelUI，模块发不出去！");
        }

        // 修好之后这个"损坏设备"就没用了 → 销毁（放在广播之后，保证别人拿得到位置）
        if (destroyWhenRepaired) Destroy(gameObject);
    }//修理完成
    void ClearWater()
    {
        if (waterRoot == null) return; 
        waterRoot.BroadcastMessage("StopGrowing", SendMessageOptions.DontRequireReceiver);
        int removed = 0;
        for (int i = waterRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(waterRoot.GetChild(i).gameObject);
            removed++;
        }

        Debug.Log("[设备] " + name + " 修好，已清掉它的水 " + removed + " 格。", this);
    }//完成修理后来清除这个设备生成的水
    //=== 新增：装置音效（Inspector 里配了 AudioSource 就用它，没配就用全局 SfxPlayer）===
    void StartRepairSoundLoop()
    {
        if (repairSound != null) { repairSound.loop = true; if (repairSound.isPlaying == false) repairSound.Play(); return; }
        if (SfxPlayer.instance != null) SfxPlayer.instance.PlayRepairLoop();
    }
    void StopRepairSoundLoop()
    {
        if (repairSound != null) { if (repairSound.isPlaying) repairSound.Stop(); return; }
        if (SfxPlayer.instance != null) SfxPlayer.instance.StopRepairLoop();
    }
    void PlayFinishSound()
    {
        if (finishSound != null) { finishSound.Play(); return; }
        if (SfxPlayer.instance != null) SfxPlayer.instance.PlayRepairDone();
    }
    bool IsPlayerInRange()//用于测定角色与设备距离以判断是否进入修理范围内
    {
        if (_player == null) return false;
        Vector2 diff = (Vector2)_player.position - (Vector2)transform.position;
        float r = Mathf.Max(0.01f, repairRadius);
        return diff.sqrMagnitude <= r * r;
    }
    Transform FindPlayer()//找玩家的位置
    {
        if (string.IsNullOrEmpty(playerTag)) return null;

        try
        {
            GameObject go = GameObject.FindWithTag(playerTag);
            return go != null ? go.transform : null;
        }
        catch
        {
            if (!_reportedMissingTag)
            {
                _reportedMissingTag = true;
                Debug.LogError("工程里没有 \"" + playerTag + "\" 这个标签，找不到玩家。", this);
            }
            return null;
        }
    }
    void OnGUI()
    {
        if (Time.timeScale == 0f) return;   // 游戏暂停（面板开着）时不画进度条
        if (!showBarOnScreen) return;
        if (Camera.main == null) return;
        if (showBarOnlyWhenRepairing && (Progress01 <= 0f || IsRepaired)) return;

        // 世界坐标 → 屏幕坐标。
        // 注意：屏幕坐标左下角是 (0,0)，而 OnGUI 的左上角是 (0,0)，所以 y 要翻过来。
        Vector3 screenPos = Camera.main.WorldToScreenPoint(
            transform.position + Vector3.up * 1.2f);

        if (screenPos.z < 0f) return;      // 在相机背后就不画

        const float width = 90f;
        const float height = 12f;

        Rect back = new Rect(screenPos.x - width * 0.5f,
                             Screen.height - screenPos.y - height * 0.5f,
                             width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.6f);      // 底：半透明黑
        GUI.Box(back, GUIContent.none);

        GUI.color = new Color(0.3f, 1f, 0.4f, 0.95f); // 前：绿色进度
        GUI.Box(new Rect(back.x, back.y, back.width * Progress01, back.height),
                GUIContent.none);

        GUI.color = Color.white;                      // 用完还原，否则影响别的 GUI
    }//进度UI

}

