// 模块面板的总控，挂在 GameManager 上。
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ModulePanelUI : MonoBehaviour
{

    // ===== 静态入口 =====
    // 用一个"私有字段"存真正的实例
    private static ModulePanelUI instance;

    // 对外暴露的"属性"：别人取的时候，如果发现是空的，就自己去场景里找一遍
    // 为什么要这样：原来只靠 Awake() 登记一次，一旦 Awake 没跑到（物体没激活、
    // 组件被禁用、组件变成Missing Script），Instance 就永远是 null，
    // 而且你很难查。改成"取的时候自动补救"就不会再出现这个问题。
    public static ModulePanelUI Instance
    {
        get
        {
            if (instance == null)
            {
                // 在场景里找一个（找不到就还是 null，调用方自己判空）
                instance = FindObjectOfType<ModulePanelUI>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    [Header("整个面板（控制显示与隐藏）")]
    public GameObject panel;

    [Header("三选一：3张候选卡")]
    public ModuleCell[] candidateCells;

    [Header("两段式界面：这些是候选阶段的显示区")]
    public GameObject candidateRow;      // 三张卡所在的那一排
    public GameObject equipRow;          // 模块链那一排
    public GameObject backpackRow;
    public GameObject rewardRow;
    public GameObject discardRow;        // 废弃格
    public GameObject continueButton;    // 返回键

    // 现在处在哪个阶段
    private bool inCandidateStage = false;

    [Header("面板里所有格子（13个，顺序不限）")]
    public ModuleCell[] cells;

    [Header("详细信息区（默认隐藏）")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoNameText;
    public TextMeshProUGUI infoDescText;

    // ===== 选中状态 =====
    // 【重要】选中的真相只存在这两个字段里，格子自己不能存。
    // 用 (ModuleArea)(-1) 表示"什么都没选中"（枚举里没有这个值）
    private ModuleArea selectedArea = (ModuleArea)(-1);
    private int selectedIndex = -1;

    // ===================== 打开 / 关闭 =====================
    public void Open()
    {
        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;        // 暂停游戏（照 GameOverPanel 的写法）
        ClearSelection();           // 清掉上次残留的选中状态
        RefreshAll();
        RefreshAllText();           // 再显式刷一遍文字，保证打开面板时文字一定是最新的
        EnterCandidateStage();
    }
    // 第一阶段：显示3张候选卡，隐藏整理界面
    private void EnterCandidateStage()
    {
        inCandidateStage = true;

        // 抽3张不重复的
        List<string> picked = ModuleBook.CreateRandomList(3);
        for (int i = 0; i < ModulePanelData.candidates.Length; i++)
        {
            ModulePanelData.candidates[i] = (i < picked.Count) ? picked[i] : "";
        }

        // 显隐：只显示候选行
        SetStageVisible(true);

        ClearSelection();
        RefreshAll();   // 刷新时会把候选卡的文字和图都填上
    }

    // 第二阶段：隐藏候选卡，显示整理界面
    private void EnterArrangeStage()
    {
        inCandidateStage = false;
        SetStageVisible(false);

        ClearSelection();
        RefreshAll();
    }

    // 统一管显隐
    private void SetStageVisible(bool candidateStage)
    {
        if (candidateRow != null) candidateRow.SetActive(candidateStage);
        if (equipRow != null) equipRow.SetActive(!candidateStage);
        if (backpackRow != null) backpackRow.SetActive(!candidateStage);
        if (rewardRow != null) rewardRow.SetActive(!candidateStage);
        if (discardRow != null) discardRow.SetActive(!candidateStage);
        if (continueButton != null) continueButton.SetActive(!candidateStage);
    }
    /// <summary>
    /// 重新走一遍所有格子的"刷新文字"。
    /// 为什么单独来一个：保证"打开面板"这个动作本身就能把文字填对，
    /// 不依赖别的函数有没有顺手调用。以后数据变了也可以直接调它。
    /// </summary>
    private void RefreshAllText()
    {
        Debug.Log(">>> RefreshAllText：面板打开的刷新开始，cells = "
                  + (cells == null ? "null" : cells.Length.ToString()));

        if (cells == null) return;

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] == null) continue;
            cells[i].RefreshTextNow();
        }
    }

    public void Close()
    {
        // 【新】奖励区还有东西 → 不许关
        string reward = ModulePanelData.Read(ModuleArea.Reward, 0);
        if (string.IsNullOrEmpty(reward) == false)
        {
            Debug.Log("奖励区还有模块，先处理掉才能继续");
            return;   // 直接返回，不关面板
        }
        // 【第二个集成点】让枪重新读一遍模块。
        // 没有这一步，玩家在面板里换的顺序不会生效！
        if (PlayerShoot.Instance != null)
        {
            PlayerShoot.Instance.RefreshModules();
        }
        else
        {
            Debug.LogWarning("找不到 PlayerShoot.Instance，模块顺序不会生效！");
        }

        if (panel != null) panel.SetActive(false);

        Time.timeScale = 1f;        // 恢复时间流动
    }

    // ===================== 刷新 =====================
    public void RefreshAll()
    {
        if (cells != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null) continue;

                cells[i].Refresh();      // 刷新文字 + 视觉

                // 刷新高亮：告诉每个格子"你是不是选中的那个"
                bool isSelected = HasSelection()
                                  && cells[i].area == selectedArea
                                  && cells[i].index == selectedIndex;
                cells[i].SetSelected(isSelected);
            }
            // 候选卡也要刷新
            if (candidateCells != null)
            {
                for (int i = 0; i < candidateCells.Length; i++)
                {
                    if (candidateCells[i] == null) continue;
                    candidateCells[i].Refresh();
                    candidateCells[i].SetSelected(false);
                }
            }
        }

        RefreshInfoPanel();
    }

    private void RefreshInfoPanel()
    {
        // 没选中 → 隐藏信息区
        if (HasSelection() == false)
        {
            if (infoPanel != null) infoPanel.SetActive(false);
            return;
        }

        if (infoPanel != null) infoPanel.SetActive(true);

        string moduleName = ModulePanelData.Read(selectedArea, selectedIndex);

        if (string.IsNullOrEmpty(moduleName))
        {
            if (infoNameText != null) infoNameText.text = "";
            if (infoDescText != null) infoDescText.text = "";
            return;
        }

        string display;
        string desc;
        ModuleBook.GetInfo(moduleName, out display, out desc);

        if (infoNameText != null) infoNameText.text = display;
        if (infoDescText != null) infoDescText.text = desc;
    }

    // ===================== 选中状态 =====================
    private bool HasSelection()
    {
        return selectedIndex >= 0;
    }

    private void SetSelection(ModuleArea area, int index)
    {
        selectedArea = area;
        selectedIndex = index;
        RefreshAll();
    }

    private void ClearSelection()
    {
        selectedArea = (ModuleArea)(-1);
        selectedIndex = -1;
        RefreshAll();
    }

    // ===================== 点击处理 =====================
    /// <summary>玩家点了某个格子。由每个格子的 Button 的 OnClick 调用。</summary>
    public void OnCellClicked(ModuleCell cell)
    {
        if (cell == null)
        {
            // 【新】候选阶段：点卡片 = 选它
            if (inCandidateStage)
            {
                OnCandidateClicked(cell);
                return;
            }

            Debug.LogWarning("OnCellClicked 收到的格子是 null，检查 Button 的参数槽有没有拖 ModuleCell");
            return;
        }

        ModuleArea area = cell.area;
        int index = cell.index;

        // ① 点的是废弃格 → 走丢弃流程
        if (cell.IsDiscard())
        {
            OnDiscardClicked();
            return;
        }

        // ② 现在没有选中任何东西
        if (HasSelection() == false)
        {
            // 点空格子：什么都不做
            if (string.IsNullOrEmpty(cell.GetModuleName())) return;

            // 点有模块的格子：选中它
            SetSelection(area, index);
            return;
        }

        // ③ 已经选中了某张卡

        // 点的是同一张卡 → 取消选中
        if (selectedArea == area && selectedIndex == index)
        {
            ClearSelection();
            return;
        }

        // 点的是另一个格子 → 移动或交换
        SwapOrMove(area, index);
    }

    private void OnCandidateClicked(ModuleCell cell)
    {
        string picked = ModulePanelData.Read(ModuleArea.Candidate, cell.index);

        // 点到空位 → 什么都不做
        if (string.IsNullOrEmpty(picked)) return;

        // ① 放进奖励区（奖励区只有1格，index 0）
        ModulePanelData.Write(ModuleArea.Reward, 0, picked);

        // ② 另外两张清空（不管玩家选的是哪张，三张全部清掉，因为选中的已经复制进奖励区了）
        for (int i = 0; i < ModulePanelData.candidates.Length; i++)
        {
            ModulePanelData.candidates[i] = "";
        }

        Debug.Log("三选一：选了 " + picked);

        // ③ 进入整理阶段
        EnterArrangeStage();
    }

    /// <summary>把选中的模块移动或交换到目标格子</summary>
    private void SwapOrMove(ModuleArea targetArea, int targetIndex)
    {
        string fromName = ModulePanelData.Read(selectedArea, selectedIndex);
        string toName = ModulePanelData.Read(targetArea, targetIndex);

        if (string.IsNullOrEmpty(toName))
        {
            // 目标为空 → 移动
            ModulePanelData.Write(targetArea, targetIndex, fromName);   // 先建立新的
            ModulePanelData.Write(selectedArea, selectedIndex, "");     // 再清空旧的
            Debug.Log("移动：" + fromName + " → " + targetArea + "[" + targetIndex + "]");
        }
        else
        {
            // 目标有模块 → 交换
            ModulePanelData.Write(targetArea, targetIndex, fromName);
            ModulePanelData.Write(selectedArea, selectedIndex, toName);
            Debug.Log("交换：" + fromName + " ←→ " + toName);
        }

        ClearSelection();
    }

    /// <summary>玩家点了废弃格</summary>
    private void OnDiscardClicked()
    {
        // 【重要】没有选中任何东西 → 什么都不做
        // 这个判断不能省！否则空点一下就会莫名其妙删掉一个模块
        if (HasSelection() == false)
        {
            Debug.Log("废弃格：请先选中一个模块");
            return;
        }

        string discarded = ModulePanelData.Read(selectedArea, selectedIndex);
        ModulePanelData.Write(selectedArea, selectedIndex, "");

        Debug.Log("废弃了模块：" + discarded);

        ClearSelection();
    }
}