using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 任务指引图标（修复版）：贴在屏幕边缘指向目的地；目的地进入画面就隐藏。
///
/// 这一版修掉的问题：
///   1) 图标会跑出屏幕 —— 位置现在一定会被夹在屏幕内（并且不再往外偏半个图标）；
///   2) 图标消失后再也不回来 —— 隐藏改用 CanvasGroup.alpha，不再用 SetActive，
///      否则本脚本的 Update 会被停掉，它就没机会把自己显示回来；
///   3) 目标还没设定就自杀 —— 只有"曾经指过某个目标、后来那个目标被销毁"才会删自己；
///   4) 找不到 Player 就整个罢工 —— 现在找不到只是隐藏，并给出原因；
///   5) 每帧 GetComponent / 重复找相机 —— 改成缓存 + 相机空了自动重取。
///
/// 摆放要求：图标必须是 Canvas 的直接子物体（不是的话 Console 会警告，位置会偏）。
/// </summary>
public class ObjectivePointer : MonoBehaviour
{
    [Header("谁指向谁")]
    [Tooltip("目的地。手动摆时在这里拖；用管理器时由管理器赋值。")]
    public Transform target;

    [Tooltip("角色。留空 = 自动按 Player 标签找。")]
    public Transform player;

    [Header("位置")]
    [Tooltip("图标中心离屏幕边缘多少像素。建议大于图标宽度的一半，否则会被裁掉一部分。")]
    public float edgePadding = 60f;

    [Header("外观")]
    [Tooltip("箭头类图标：跟着方向旋转。")]
    public bool rotateToDirection = true;

    [Tooltip("旋转修正：贴图默认朝上填 -90，朝右填 0。")]
    public float rotationOffset = -90f;

    [Header("行为")]
    [Tooltip("目的地被销毁时，图标也一起销毁（手动摆图标时建议勾上）。")]
    public bool destroyWhenTargetMissing = true;

    [Tooltip("打开后会在 Console 打印「为什么隐藏」，排查用。")]
    public bool logReason = false;

    // 由管理器赋值（多图标错开用），不在 Inspector 显示
    [HideInInspector] public int stackIndex;
    [HideInInspector] public float stackOffset;

    RectTransform _rect;
    CanvasGroup _group;
    Canvas _canvas;
    Camera _cam;
    bool _hadTarget;
    bool _warnedParent;
    bool _wasVisible;

    // ------------------------------------------------------------------
    // 初始化
    // ------------------------------------------------------------------
    void Awake()
    {
        _rect = GetComponent<RectTransform>();

        // 位置算法假设"锚点、中心点都在正中" —— 这里强制设好，省得 Inspector 配错
        if (_rect != null)
        {
            _rect.anchorMin = new Vector2(0.5f, 0.5f);
            _rect.anchorMax = new Vector2(0.5f, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
        }

        // 显示/隐藏改用 CanvasGroup（没有就自动加一个）
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;   // 别挡住鼠标点击
    }

    // ------------------------------------------------------------------
    // 每帧
    // ------------------------------------------------------------------
    void Start()
    {
        // 启动时把自身状况打一条，方便一眼看出"哪个图标缺目标 / 不在 Canvas 下"
        Debug.Log("[指引] 图标启动：" + name
                  + " | target=" + (target != null ? target.name : "空")
                  + " | player=" + (player != null ? player.name : "空")
                  + " | 父物体=" + (transform.parent != null ? transform.parent.name : "无")
                  + " | 激活=" + gameObject.activeSelf, this);
    }

    void Update()
    {
        if (target != null) _hadTarget = true;

        // 目的地被销毁（设备修好了）→ 图标也没用了。只有"曾经有过目标"才自杀，
        // 避免刚生成、还没赋值时就把自己删掉。
        if (target == null && _hadTarget && destroyWhenTargetMissing)
        {
            Destroy(gameObject);
            return;
        }

        if (_cam == null) _cam = Camera.main;                       // 切场景后相机可能变
        if (_canvas == null) _canvas = GetComponentInParent<Canvas>();

        if (player == null)
        {
            try
            {
                GameObject go = GameObject.FindGameObjectWithTag("Player");
                if (go != null) player = go.transform;
            }
            catch
            {
                // 工程里没有 Player 标签时 FindGameObjectWithTag 会抛异常，忽略即可
            }
        }

        Refresh();
    }

    // ------------------------------------------------------------------
    // 核心：算图标该在哪
    // ------------------------------------------------------------------
    void Refresh()
    {
        // ---------- 1. 保险检查 ----------
        if (_cam == null) { Hide("没有相机（Camera.main 为空）"); return; }
        if (_canvas == null) { Hide("不在 Canvas 下"); return; }
        if (_rect == null) { Hide("没有 RectTransform"); return; }
        if (target == null) { Hide("target 还没设"); return; }
        if (player == null) { Hide("找不到 Player（标签或引用）"); return; }

        // 图标不是 Canvas 的直接子物体 → 位置会偏，提醒一次
        if (!_warnedParent && _rect.parent != _canvas.transform)
        {
            _warnedParent = true;
            Debug.LogWarning("[指引] " + name + " 不是 Canvas 的直接子物体，位置会算偏，建议把它拖到 Canvas 下。", this);
        }

        // ---------- 2. 世界坐标 → 屏幕坐标（像素，左下角为原点）----------
        Vector3 ts = _cam.WorldToScreenPoint(target.position);
        Vector3 ps = _cam.WorldToScreenPoint(player.position);

        Vector2 to = new Vector2(ts.x, ts.y);
        Vector2 from = new Vector2(ps.x, ps.y);

        // 在相机背后的点，投影是"镜像"的，绕屏幕中心翻一下方向才对
        bool targetBehind = ts.z < 0f;
        bool playerBehind = ps.z < 0f;
        if (targetBehind) { to.x = Screen.width - to.x; to.y = Screen.height - to.y; }
        if (playerBehind) { from.x = Screen.width - from.x; from.y = Screen.height - from.y; }

        // ---------- 3. 目的地在画面里 → 隐藏 ----------
        if (!targetBehind && IsOnScreen(to)) { Hide("目的地在画面内"); return; }

        // ---------- 4. 方向（角色 → 目的地）----------
        Vector2 dir = to - from;
        if (dir.sqrMagnitude < 0.0001f) { Hide("角色和目的地重合"); return; }
        dir.Normalize();

        // ---------- 5. 射线起点夹回画面里（角色可能在屏幕外）----------
        float pad = Mathf.Max(0f, edgePadding);
        from.x = Mathf.Clamp(from.x, pad, Mathf.Max(pad, Screen.width - pad));
        from.y = Mathf.Clamp(from.y, pad, Mathf.Max(pad, Screen.height - pad));

        // ---------- 6. 求"穿出屏幕边"的那一点 ----------
        Vector2 iconScreen = EdgePoint(from, dir, pad);

        // ---------- 6.5 多个图标挤在一起时沿边缘错开 ----------
        if (stackOffset > 0f && stackIndex > 0)
        {
            Vector2 tangent = new Vector2(-dir.y, dir.x);        // 垂直于方向的"沿边缘"方向
            iconScreen += tangent * (stackOffset * stackIndex);
            iconScreen.x = Mathf.Clamp(iconScreen.x, pad, Mathf.Max(pad, Screen.width - pad));
            iconScreen.y = Mathf.Clamp(iconScreen.y, pad, Mathf.Max(pad, Screen.height - pad));
        }

        // ---------- 7. 屏幕坐标 → Canvas 本地坐标（Unity 自带，自动处理缩放）----------
        RectTransform canvasRect = _canvas.transform as RectTransform;
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            iconScreen,
            _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _cam,
            out localPoint);

        _rect.anchoredPosition = localPoint;      // 注意：不要再往外加半个图标尺寸，那会把图标推出屏幕

        // ---------- 8. 朝向 ----------
        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _rect.localRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }

        Show();
    }

    // ------------------------------------------------------------------
    // 小工具
    // ------------------------------------------------------------------

    /// <summary>屏幕坐标（像素）在画面里吗？</summary>
    bool IsOnScreen(Vector2 p)
    {
        return p.x >= 0f && p.x <= Screen.width && p.y >= 0f && p.y <= Screen.height;
    }

    /// <summary>
    /// 从 from 沿 dir（单位向量）走，求穿出屏幕矩形的那一点。
    /// 分别算"碰到左右边要走多远""碰到上下边要走多远"，取更近的那个；结果再夹一次，保证不出界。
    /// </summary>
    Vector2 EdgePoint(Vector2 from, Vector2 dir, float pad)
    {
        float left = pad;
        float right = Screen.width - pad;
        float bottom = pad;
        float top = Screen.height - pad;

        float t = float.MaxValue;

        if (dir.x > 0.0001f) t = Mathf.Min(t, (right - from.x) / dir.x);
        else if (dir.x < -0.0001f) t = Mathf.Min(t, (left - from.x) / dir.x);

        if (dir.y > 0.0001f) t = Mathf.Min(t, (top - from.y) / dir.y);
        else if (dir.y < -0.0001f) t = Mathf.Min(t, (bottom - from.y) / dir.y);

        if (t == float.MaxValue || t < 0f) t = 0f;

        Vector2 p = from + dir * t;

        p.x = Mathf.Clamp(p.x, pad, Mathf.Max(pad, Screen.width - pad));
        p.y = Mathf.Clamp(p.y, pad, Mathf.Max(pad, Screen.height - pad));

        return p;
    }

    void Show()
    {
        if (_group != null) _group.alpha = 1f;

        // 只在"从隐藏变显示"时打一条，方便排查（不会刷屏）
        if (!_wasVisible)
        {
            _wasVisible = true;
            Debug.Log("[指引] " + name + " 显示（目标：" + (target != null ? target.name : "空") + "）", this);
        }
    }

    void Hide(string reason)
    {
        if (_group != null) _group.alpha = 0f;

        // 只在"从显示变隐藏"时打一条
        if (_wasVisible)
        {
            _wasVisible = false;
            Debug.Log("[指引] " + name + " 隐藏：" + reason, this);
        }
        else if (logReason)
        {
            Debug.Log("[指引] " + name + " 隐藏：" + reason, this);
        }
    }
}
