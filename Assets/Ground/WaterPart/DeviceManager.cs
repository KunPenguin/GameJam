using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 多目标指引管理器：给每个设备生成一个屏幕边缘图标。
///   · 一开始摆在场景里的设备 → 拖进 Targets 列表，或者干脆靠"自动发现"；
///   · 运行时新生成的设备 → 自动发现（每隔 scanInterval 秒扫一次场景里的 DeviceRepair），
///     出现一个就配一个图标，不需要任何手动操作；
///   · 设备被销毁（修好/消失）→ 对应图标自动销毁，并从列表剔除。
///
/// 图标本身的行为（贴边、隐藏、朝向）在 ObjectivePointer 里。
/// </summary>
public class DeviceManager : MonoBehaviour
{
    [Header("图标预制体（带 Image + ObjectivePointer）")]
    public GameObject pointerPrefab;

    [Header("图标挂在哪个 UI 父物体下（拖 Canvas）")]
    public Transform uiParent;

    [Header("角色（建议留空 = 每个图标自己按 Player 标签找）")]
    public Transform player;

    [Header("一开始就要指引的设备（可选，从场景 Hierarchy 拖）")]
    public List<Transform> targets = new List<Transform>();

    [Header("自动发现新设备（设备身上要有 DeviceRepair）")]
    public bool autoDetect = true;

    [Tooltip("每隔几秒扫描一次场景里有没有新设备")]
    public float scanInterval = 0.5f;

    [Header("最多同时显示几个（0 = 不限）")]
    public int maxIcons = 0;

    [Header("图标挤在一起时沿边缘错开多少像素（0 = 不错开）")]
    public float stackOffset = 0f;

    // 已经配了图标的目的地 / 对应的图标组件，两个列表下标一一对应
    readonly List<Transform> _targets = new List<Transform>();
    readonly List<ObjectivePointer> _icons = new List<ObjectivePointer>();

    float _scanTimer;

    // ------------------------------------------------------------------
    // 启动
    // ------------------------------------------------------------------
    void Start()
    {
        // 这两项没填就一个图标都出不来，先说清楚
        if (pointerPrefab == null) Debug.LogWarning("[指引] DeviceManager 没填 Pointer Prefab，生成不了图标。", this);
        if (uiParent == null) Debug.LogWarning("[指引] DeviceManager 没填 UI Parent（应该拖 Canvas），生成不了图标。", this);

        for (int i = 0; i < targets.Count; i++)
        {
            if (maxIcons > 0 && _icons.Count >= maxIcons) break;

            if (targets[i] == null)
            {
                // 这条就是"明明 3 个设备却只有 2 个图标"的常见原因之一
                Debug.LogWarning("[指引] Targets 第 " + (i + 1) + " 个槽是空的（None），已跳过。", this);
                continue;
            }

            Add(targets[i]);
        }

        int nullSlots = 0;
        for (int i = 0; i < targets.Count; i++) if (targets[i] == null) nullSlots++;

        Debug.Log("[指引] DeviceManager 启动：Targets 共 " + targets.Count + " 个（空槽 " + nullSlots
                  + " 个），已生成图标 " + _icons.Count + " 个；自动发现 = " + autoDetect + "。", this);

        // 启动时先扫一次，场景里已经有 DeviceRepair 但没拖进列表的设备也能立刻出图标
        if (autoDetect) ScanNewDevices();
    }

    // ------------------------------------------------------------------
    // 每帧
    // ------------------------------------------------------------------
    void Update()
    {
        // 1) 目的地被销毁（设备修好了）→ 销毁对应图标，并从列表剔除
        for (int i = _targets.Count - 1; i >= 0; i--)      // 倒着遍历，删元素才安全
        {
            if (_targets[i] != null) continue;

            if (_icons[i] != null) Destroy(_icons[i].gameObject);

            _targets.RemoveAt(i);
            _icons.RemoveAt(i);
        }

        // 2) 定时扫描新出现的设备
        if (!autoDetect) return;

        _scanTimer -= Time.deltaTime;
        if (_scanTimer > 0f) return;

        _scanTimer = Mathf.Max(0.1f, scanInterval);
        ScanNewDevices();
    }

    // ------------------------------------------------------------------
    // 自动发现
    // ------------------------------------------------------------------
    /// <summary>扫一遍场景里的所有 DeviceRepair，没配过图标的就配一个。</summary>
    void ScanNewDevices()
    {
        if (pointerPrefab == null || uiParent == null) return;   // 没配好就别扫，免得刷警告

        DeviceRepair[] all = FindObjectsOfType<DeviceRepair>(true);   // true = 包含未激活的

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == null) continue;

            Transform t = all[i].transform;
            if (t == null || _targets.Contains(t)) continue;      // 已经配过就跳过

            int before = _icons.Count;
            Add(t);

            if (_icons.Count > before)
            {
                Debug.Log("[指引] 自动发现新设备：" + t.name + "，已配图标（现在共 " + _icons.Count + " 个）。", this);
            }
        }
    }

    // ------------------------------------------------------------------
    // 给一个目的地配一个图标
    // ------------------------------------------------------------------
    public void Add(Transform t)
    {
        if (t == null) return;
        if (pointerPrefab == null || uiParent == null) return;
        if (_targets.Contains(t)) return;                  // 同一个目的地只配一次（重复拖会跳过）
        if (maxIcons > 0 && _icons.Count >= maxIcons) return;

        GameObject go = Instantiate(pointerPrefab, uiParent);
        go.name = "Pointer_" + t.name;

        ObjectivePointer pointer = go.GetComponent<ObjectivePointer>();
        if (pointer == null)
        {
            Debug.LogWarning("[指引] 图标预制体上没有 ObjectivePointer 组件，已跳过：" + pointerPrefab.name, this);
            Destroy(go);
            return;
        }

        pointer.target = t;                                // ★ 一个图标 → 一个目的地
        pointer.player = player;                           // 留空也没关系，图标会自己按标签找
        pointer.stackIndex = _icons.Count;
        pointer.stackOffset = stackOffset;

        _targets.Add(t);
        _icons.Add(pointer);
    }
}
