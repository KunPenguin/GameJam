using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaytoDevice : MonoBehaviour
{
   
    [HideInInspector] public Transform target; // 由管理器赋值的隐藏变量
    public Transform player;                   // 角色位置
    public float edgePadding = 50f;            // 距离屏幕边缘的内边距

    private Camera mainCam;
    private Canvas canvas;

    void Start()
    {
        mainCam = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        player = GameObject.FindGameObjectWithTag("Player").transform; // 假设你的角色有Player标签，或者让管理器统一赋值
    }

    void Update()
    {
        // 💡 核心改动1：如果目标被销毁了，自己也销毁
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (player == null) return;

        // 1. 将目的地世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);
        bool isBehind = screenPos.z < 0;

        // 2. 判断目的地是否在屏幕内
        if (screenPos.x >= 0 && screenPos.x <= Screen.width &&
            screenPos.y >= 0 && screenPos.y <= Screen.height && !isBehind)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 3. 处理目的地已经在相机背后的情况（反转坐标）
        if (isBehind)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
        }

        // 4. 计算边缘位置和偏移
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 screenPos2D = new Vector2(screenPos.x, screenPos.y);
        Vector2 direction = (screenPos2D - screenCenter).normalized;

        float widthRadius = (Screen.width / 2f) - edgePadding;
        float heightRadius = (Screen.height / 2f) - edgePadding;

        float scaleX = Mathf.Abs(direction.x) > 0.001f ? Mathf.Abs(widthRadius / direction.x) : float.MaxValue;
        float scaleY = Mathf.Abs(direction.y) > 0.001f ? Mathf.Abs(heightRadius / direction.y) : float.MaxValue;
        float scale = Mathf.Min(scaleX, scaleY);

        Vector2 edgePosition = screenCenter + direction * scale;

        // 5. UI 坐标转换
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            edgePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCam,
            out localPoint);

        // 6. 让图标完整贴合屏幕边缘
        float offsetX = direction.x > 0.1f ? (GetComponent<RectTransform>().rect.width / 2f) : (direction.x < -0.1f ? -(GetComponent<RectTransform>().rect.width / 2f) : 0);
        float offsetY = direction.y > 0.1f ? (GetComponent<RectTransform>().rect.height / 2f) : (direction.y < -0.1f ? -(GetComponent<RectTransform>().rect.height / 2f) : 0);

        GetComponent<RectTransform>().anchoredPosition = localPoint + new Vector2(offsetX, offsetY);
    }
}