using UnityEngine;

/// <summary>
/// BOSS 指引：在 UI 上生成一个红色箭头图标，指向场景里的 BOSS。
///   · BOSS 还没生成 / 被销毁后重生 → 每隔 rescanInterval 秒自动重新找一次
///   · 图标预制体用 BossPointer.prefab（同款箭头，Image 颜色是红的）
///
/// 用法：挂到场景里任意常驻物体上 →
///        Pointer Prefab 拖 BossPointer.prefab
///        UI Parent     拖 Canvas
/// </summary>
public class BossPointerSpawner : MonoBehaviour
{
    [Header("红色 BOSS 指引图标预制体")]
    public GameObject pointerPrefab;

    [Header("图标挂在哪个 UI 父物体下（拖 Canvas）")]
    public Transform uiParent;

    [Header("多久重新查找一次 BOSS（秒）")]
    public float rescanInterval = 1f;

    ObjectivePointer _pointer;
    Transform _boss;
    float _timer;

    void Start()
    {
        if (pointerPrefab == null)
        {
            Debug.LogWarning("[BOSS指引] 没填 Pointer Prefab，生成不了图标。", this);
            return;
        }

        if (uiParent == null)
        {
            Debug.LogWarning("[BOSS指引] 没填 UI Parent（应该拖 Canvas），生成不了图标。", this);
            return;
        }

        GameObject go = Instantiate(pointerPrefab, uiParent);
        go.name = "Pointer_Boss";

        _pointer = go.GetComponent<ObjectivePointer>();
        if (_pointer == null)
        {
            Debug.LogWarning("[BOSS指引] 图标预制体上没有 ObjectivePointer 组件。", this);
            Destroy(go);
            return;
        }

        // 目标由我们自己换，别让它因为"目标消失"就自杀
        _pointer.destroyWhenTargetMissing = false;

        FindBoss();
        _pointer.target = _boss;
    }

    void Update()
    {
        if (_pointer == null) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        _timer = Mathf.Max(0.2f, rescanInterval);

        // BOSS 还没出现 / 已经被销毁 → 重新找
        if (_boss == null)
        {
            FindBoss();
            _pointer.target = _boss;
        }
    }

    /// <summary>在场景里找 BOSS（包括未激活的）。</summary>
    void FindBoss()
    {
        bool hadBoss = _boss != null;

        // ⚠️ 这里原来找的是 BOSS（老脚本）。Boss 换成新的 BossHealth 之后，
        //    找 BOSS 就永远找不到，红色指引箭头会一直隐藏，所以改成找 BossHealth。
        BossHealth boss = FindObjectOfType<BossHealth>(true);
        _boss = boss != null ? boss.transform : null;

        if (hadBoss != (_boss != null))
        {
            if (_boss != null) Debug.Log("[BOSS指引] 已锁定 BOSS：" + _boss.name, this);
            else Debug.Log("[BOSS指引] 场景里现在没有 BOSS，图标会先隐藏。", this);
        }
    }
}
