using UnityEngine;

public class ModulePanelUI : MonoBehaviour
{
    public static ModulePanelUI Instance;
    
    //物体一被创建就登记自己
    private void Awake()
    {
        Instance = this;
    }

    [Header("整个面板（控制显示与隐藏）")]
    public GameObject panel;

    [Header("面板里所有格子")]
    public ModuleCell[] cells;

    /// <summary>
    /// 开启面板
    /// </summary>
    public void Open()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        RefreshAll();
    }

    /// <summary>
    ///关闭面板
    /// </summary>
    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }


    public void RefreshAll()
    {
        if (cells == null) return;

        for(int i =0;i<cells.Length;i++)
        {
            if (cells[i] == null) continue;
            cells[i].Refresh();
        }
    }
}


