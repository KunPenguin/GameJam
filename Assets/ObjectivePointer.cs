using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ObjectivePointer : MonoBehaviour
{
    [Header("目的地")]
    public Transform target;
    [Tooltip("角色")]
    public Transform player;
    [Header("位置")]
    [Tooltip("图标离屏幕边缘留多少像素。")]
    public float edgePadding = 40f;
    [Header("外观")]
    [Tooltip("箭头类图标：让它跟着方向旋转。")]
    public bool rotateToDirection = true;
    [Tooltip("旋转修正：贴图默认朝【上】填 -90，朝【右】填 0。")]
    public float rotationOffset = -90f;
    [Header("行为")]
    [Tooltip("目的地被销毁时，图标也一起销毁。手动摆图标时建议勾上。")]
    public bool destroyWhenTargetMissing = true;
    // Start is called before the first frame update
    [HideInInspector] public int stackIndex;
    [HideInInspector] public float stackOffset;
    private RectTransform _rect;
    private CanvasGroup _group;
    private Canvas _canvas;
    private Camera _cam;
    bool _hadTarget;
    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _rect.anchorMin = new Vector2(0.5f, 0.5f);
        _rect.anchorMax = new Vector2(0.5f, 0.5f);
        _rect.pivot = new Vector2(0.5f, 0.5f);

        // 隐藏用 CanvasGroup（没有就自动加）
        _group = GetComponent<CanvasGroup>();
        if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;      // 别挡住鼠标点击
    }
    public void SetTarget(Transform t)
    {
        target = t;
    }
    // Update is called once per frame
    void Update()
    {
        // 目的地被销毁 → 图标也没用了（只有"曾经有过目标"才自杀，避免刚生成就判空）
        if (target != null) _hadTarget = true;
        if (target == null && _hadTarget && destroyWhenTargetMissing)
        {
            Destroy(gameObject);
            return;
        }

        if (_cam == null) _cam = Camera.main;          // 切场景后相机可能变

        if (player == null)                            // 找一次就缓存，别每帧找
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        Refresh();
    }
    void Refresh()
    {
        // --- 第 1 步：保险检查，缺东西就藏起来 ---
        if (_cam == null || _canvas == null || _rect == null) { SetVisible(false); return; }
        if (target == null || player == null) { SetVisible(false); return; }

        // --- 第 2 步：世界坐标 → 屏幕坐标（像素，左下角是原点）---
        Vector3 ts = _cam.WorldToScreenPoint(target.position);
        Vector3 ps = _cam.WorldToScreenPoint(player.position);

        Vector2 to = new Vector2(ts.x, ts.y);
        Vector2 from = new Vector2(ps.x, ps.y);

        // 在相机背后的点，投影是"镜像"的，绕屏幕中心翻一下方向才对
        bool targetBehind = ts.z < 0f;
        bool playerBehind = ps.z < 0f;

        if (targetBehind) { to.x = Screen.width - to.x; to.y = Screen.height - to.y; }
        if (playerBehind) { from.x = Screen.width - from.x; from.y = Screen.height - from.y; }

        // --- 第 3 步：目的地在画面里？→ 藏起来 ---
        if (!targetBehind && IsOnScreen(to))
        {
            SetVisible(false);
            return;
        }

        // --- 第 4 步：算方向（角色 → 目的地）---
        Vector2 dir = to - from;
        if (dir.sqrMagnitude < 0.0001f) { SetVisible(false); return; }   // 两点重合
        dir.Normalize();

        // --- 第 5 步：从角色出发沿 dir 走，求穿出屏幕边的那一点 ---
        Vector2 iconScreen = EdgePoint(from, dir);

        // --- 第 5.5 步：多个图标挤在一起时沿边缘错开（stackOffset = 0 就跳过）---
        if (stackOffset > 0f && stackIndex > 0)
        {
            Vector2 tangent = new Vector2(-dir.y, dir.x);            // 垂直于 dir = 沿边缘方向
            iconScreen += tangent * (stackOffset * stackIndex);

            iconScreen.x = Mathf.Clamp(iconScreen.x, edgePadding, Screen.width - edgePadding);
            iconScreen.y = Mathf.Clamp(iconScreen.y, edgePadding, Screen.height - edgePadding);
        }

        // --- 第 6 步：屏幕坐标 → Canvas 里的位置 ---
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        _rect.anchoredPosition = (iconScreen - screenCenter) / _canvas.scaleFactor;

        // --- 第 7 步：让图标朝外 ---
        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _rect.localRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }

        SetVisible(true);
    }
    bool IsOnScreen(Vector2 p)
    {
        return p.x >= 0f && p.x <= Screen.width && p.y >= 0f && p.y <= Screen.height;
    }
    Vector2 EdgePoint(Vector2 from, Vector2 dir)
    {
        float left = edgePadding;
        float right = Screen.width - edgePadding;
        float bottom = edgePadding;
        float top = Screen.height - edgePadding;

        float t = float.MaxValue;        // 需要走的距离

        if (dir.x > 0.0001f) t = Mathf.Min(t, (right - from.x) / dir.x);
        else if (dir.x < -0.0001f) t = Mathf.Min(t, (left - from.x) / dir.x);
        // dir.x ≈ 0 → 这条边永远碰不到，跳过

        if (dir.y > 0.0001f) t = Mathf.Min(t, (top - from.y) / dir.y);
        else if (dir.y < -0.0001f) t = Mathf.Min(t, (bottom - from.y) / dir.y);

        if (t == float.MaxValue || t < 0f) t = 0f;    // 兜底

        return from + dir * t;
    }
    void SetVisible(bool show)
    {
        // ★ 只改透明度，不 SetActive(false)！
        //   关了物体，这个脚本的 Update 也会停 → 图标再也没机会自己出现。
        _group.alpha = show ? 1f : 0f;
    }
}
