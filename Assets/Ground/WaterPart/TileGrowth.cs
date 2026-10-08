using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自蔓延地块：一圈一圈地长，形状永远是均匀的菱形。
/// </summary>
public class TileGrowth : MonoBehaviour
{
    [Header("要蔓延的预制体（把地块自己的预制体拖进来）")]
    public GameObject tilePrefab;
    public bool growOnStart = true;
    [Header("这些层会挡住水的蔓延（勾 DeepWater / 其他不想被淹的层）")]
    public LayerMask blockLayers;
    [Header("我是第几代（种子保持 0，不用手改）")]
    public int depth = 0;
    [Header("长几圈（1 = 上下左右各一个，2 = 再外一圈…）")]
    public int maxDepth = 3;
    [Header("每圈之间的间隔（秒）")]
    public float time = 0.2f;

    // 上下左右
    private static readonly Vector2Int[] FourDirs =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
    };

    public Vector2Int GridPos { get; private set; }

    private void Awake()
    {
        GridPos = new Vector2Int(Mathf.RoundToInt(transform.position.x),
                                 Mathf.RoundToInt(transform.position.y));
    }

    private void Start()
    {
        // 只有种子负责启动整条蔓延，克隆体不自己启动
        if (growOnStart && depth == 0) StartCoroutine(GrowRings());
    }

    /// <summary>一圈一圈地长</summary>
    private IEnumerator GrowRings()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("tilePrefab 没填！", this);
            yield break;
        }
        if (transform.parent == null)
        {
            Debug.LogError("地块必须是某个父物体的子物体（例如 Map）。", this);
            yield break;
        }

        List<TileGrowth> frontier = new List<TileGrowth> { this };   // 当前这一圈

        for (int ring = 0; ring < maxDepth; ring++)
        {
            List<TileGrowth> next = new List<TileGrowth>();          // 下一圈

            // 把这一圈每个格子的四方向都长出来（只生，不递归）
            for (int i = 0; i < frontier.Count; i++)
                frontier[i].SpawnNeighbours(next);

            if (next.Count == 0) break;      // 周围全满了 → 结束

            frontier = next;
            yield return new WaitForSeconds(time);           // 歇一下再长下一圈
        }
    }

    /// <summary>把上下左右四格里空着的都生成出来，新格子记进 next</summary>
    private void SpawnNeighbours(List<TileGrowth> next)
    {
        for (int i = 0; i < FourDirs.Length; i++)
        {
            Vector2Int target = GridPos + FourDirs[i];

            if (ExistsAt(target)) continue;          // 已经有人 → 跳过

            TileGrowth child = SpawnAt(target);
            if (child != null) next.Add(child);
        }
    }

    private TileGrowth SpawnAt(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x, pos.y, 0f);

        GameObject go = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform.parent);
        go.name = TileName(pos);         // 立刻命名，判重靠它

        TileGrowth child = go.GetComponent<TileGrowth>();
        if (child == null) return null;

        child.GridPos = pos;
        child.depth = depth + 1;       // 只做标记，不参与递归
        return child;
    }

    private bool ExistsAt(Vector2Int pos)
    {
        // ① 已经有同名地块了 → 拦住（原来的逻辑，不动）
        if (transform.parent.Find(TileName(pos)) != null) return true;

        // ② 那一格上有深水/障碍物 → 也拦住（新加的）
        Vector2 point = new Vector2(pos.x, pos.y);
        if (Physics2D.OverlapPoint(point, blockLayers) != null) return true;

        return false;
    }

    private static string TileName(Vector2Int pos)
    {
        return $"Tile_{pos.x}_{pos.y}";
    }
}