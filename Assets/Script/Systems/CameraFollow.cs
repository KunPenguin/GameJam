using UnityEngine;

// 相机跟随玩家，并限制在地图边界内。
// 挂载：挂在 Main Camera 上。
// 需要：把 Player 拖进 target；把地图边界框拖进 mapBounds。
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标（把 Player 拖进来）")]
    public Transform target;

    [Header("平滑时间：0 = 硬跟随最跟手，0.2 = 稍微拖一下更柔")]
    public float smoothTime = 0.15f;

    [Header("地图边界（拖一个挂了 BoxCollider2D 的物体进来）")]
    public BoxCollider2D mapBounds;

    private Camera cam;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraFollow：这个物体上没有 Camera 组件！");
        }
        if (target == null)
        {
            Debug.LogWarning("CameraFollow：target 没拖，相机不会跟随。");
        }
        if (mapBounds == null)
        {
            Debug.LogWarning("CameraFollow：mapBounds 没拖，相机不会被限制在地图内。");
        }
    }

    // 必须写在 LateUpdate，不是 Update
    //    因为 Update 里玩家的位置可能还没算完，相机会晚一帧、画面会抖
    private void LateUpdate()
    {
        if (target == null) return;

        // ① 算出相机想去的位置（保持原来的 z，2D 相机 z 必须是负的）
        Vector3 want = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z);

        // ② 平滑过去
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position, want, ref velocity, smoothTime);

        // ③ 限制在地图边界内
        if (cam != null && mapBounds != null)
        {
            // 相机能看到的范围（半宽 / 半高）
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            Bounds b = mapBounds.bounds;

            float minX = b.min.x + halfW;
            float maxX = b.max.x - halfW;
            float minY = b.min.y + halfH;
            float maxY = b.max.y - halfH;

            // ⚠ 如果"视野比地图还大"，min 会大于 max，Clamp 就会出错。
            //    所以这种情况下直接取地图中心（宁可不跟随，也不要乱跑）。
            if (minX > maxX) smoothed.x = b.center.x;
            else smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);

            if (minY > maxY) smoothed.y = b.center.y;
            else smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        transform.position = smoothed;
    }

    // 在 Scene 视图里把"允许相机移动的范围"画出来，方便调
    private void OnDrawGizmosSelected()
    {
        if (cam == null || mapBounds == null) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Bounds b = mapBounds.bounds;

        float minX = b.min.x + halfW;
        float maxX = b.max.x - halfW;
        float minY = b.min.y + halfH;
        float maxY = b.max.y - halfH;

        Gizmos.color = Color.cyan;

        if (minX > maxX || minY > maxY)
        {
            // 视野比地图大 → 相机只能待在中心
            Gizmos.DrawWireSphere(new Vector3(b.center.x, b.center.y, 0f), 0.5f);
            return;
        }

        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}