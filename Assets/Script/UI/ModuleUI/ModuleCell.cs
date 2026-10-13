// 挂在每一个格子上（共13个）。
// 职责：记住自己是谁 + 按当前状态显示对应的图。
// 点击逻辑不在这里，由 ModulePanelUI 统一处理。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;   // 鼠标事件接口
using UnityEngine.UI;

public class ModuleCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ===== 这个格子是哪个区、第几格 =====
    [Header("这个格子属于哪个区")]
    public ModuleArea area = ModuleArea.Equip;

    [Header("这个格子是第几格（从0开始）")]
    public int index = 0;

    [Header("这个格子长什么样")]
    public ModuleCellKind kind = ModuleCellKind.EquipCell;

    // ===== 文字 =====
    [Header("格子里的文字（调试用，可以留空）")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    [Header("横条上的模块名字")]
    public TextMeshProUGUI barNameText;

    [Header("废弃格的固定文字（只有废弃格填）")]
    public string discardLabel = "废弃";

    // ===== 图 =====
    [Header("底座图（挂ModuleCell的根物体上的Image）")]
    public Image baseImage;

    [Header("logo图（只有模块链格有）")]
    public Image logoImage;

    [Header("静态圆形（背包格/奖励格用）")]
    public Image circleImage;

    [Header("动图物体（只有模块链格有，挂FrameAnimator）")]
    public GameObject circleAnim;

    [Header("横条图（只有模块链格有）")]
    public Image barImage;

    // ===== 查logo的对照表 =====
    [Header("模块logo对照表（挂在GameManager上，拖进来）")]
    public ModuleIconTable iconTable;

    // ===== 贴图 =====
    [Header("底座：常态（底座A / 底座B常态 / 废弃格常态）")]
    public Sprite baseNormal;

    [Header("底座：悬停（底座B悬停 / 废弃格悬停；底座A留空）")]
    public Sprite baseHover;

    [Header("横条：开火模块那套（常态/悬停/选中）")]
    public CellSpriteSet barFire;

    [Header("横条：非开火模块那套（常态/悬停/选中）")]
    public CellSpriteSet barSupport;

    [Header("圆形：常态/悬停/选中（背包格、奖励格用）")]
    public CellSpriteSet circleState;

    // ===== 运行时状态 =====
    private bool hovered = false;    // 鼠标在不在这格上（由IPointerEnter/Exit改）
    private bool selected = false;   // 是不是当前选中的格（由SetSelected改）

    // ===================== 鼠标事件 =====================
    // 【注意】这两个方法名必须一字不差，写错不会报错但永远不触发
    public void OnPointerEnter(PointerEventData e) { hovered = true; UpdateVisual(); }
    public void OnPointerExit(PointerEventData e) { hovered = false; UpdateVisual(); }

    // ===================== 对外接口 =====================
    /// <summary>由 ModulePanelUI.RefreshAll() 调用：刷新文字 + 视觉</summary>
    public void Refresh()
    {
        RefreshText();
        UpdateVisual();
    }

    /// <summary>由 ModulePanelUI 调用：告诉这个格子"你是不是当前选中的"</summary>
    public void SetSelected(bool value)
    {
        selected = value;
        UpdateVisual();
    }

    /// <summary>这个格子现在放着什么模块名字（空格子返回""）</summary>
    public string GetModuleName()
    {
        if (area == ModuleArea.Discard) return "";
        return ModulePanelData.Read(area, index);
    }

    /// <summary>是不是废弃格</summary>
    public bool IsDiscard()
    {
        return area == ModuleArea.Discard;
    }

    // ===================== 内部：刷新文字 =====================
    private void RefreshText()
    {
        // 废弃格：显示固定文字
        if (area == ModuleArea.Discard)
        {
            if (nameText != null) nameText.text = discardLabel;
            if (descText != null) descText.text = "";
            return;
        }

        string moduleName = ModulePanelData.Read(area, index);

        // 空格子：文字清空
        if (string.IsNullOrEmpty(moduleName))
        {
            if (nameText != null) nameText.text = "";
            if (descText != null) descText.text = "";
            if (barNameText != null) barNameText.text = "";
            return;
        }

        // 有模块：查名字和说明
        string display;
        string desc;
        bool ok = ModuleBook.GetInfo(moduleName, out display, out desc);

        if (ok == false) display = "未知模块";

        if (nameText != null) nameText.text = display;
        if (descText != null) descText.text = desc;
        if (barNameText != null) barNameText.text = display;
    }

    // ===================== 内部：刷新视觉 =====================
    private void UpdateVisual()
    {
        if (kind == ModuleCellKind.EquipCell) { UpdateEquipCell(); return; }
        if (kind == ModuleCellKind.BackpackCell) { UpdateBackpackCell(); return; }
        if (kind == ModuleCellKind.RewardCell) { UpdateRewardCell(); return; }
        if (kind == ModuleCellKind.DiscardCell) { UpdateDiscardCell(); return; }
    }

    // 模块链格：底座A + logo + 圆形动图 + 横条
    private void UpdateEquipCell()
    {
        string moduleName = ModulePanelData.Read(area, index);
        bool hasModule = string.IsNullOrEmpty(moduleName) == false;

        // 底座A：永远显示，只有常态图
        SetSprite(baseImage, baseNormal);

        // 没有模块 → 只留底座A
        if (hasModule == false)
        {
            SetVisible(logoImage.gameObject, false);
            SetVisible(circleImage.gameObject, false);
            SetVisible(circleAnim, false);
            SetVisible(barImage.gameObject, false);
            SetVisible(barNameText.gameObject, false);
            return;
        }

        // 有模块
        SetVisible(logoImage.gameObject, true);
        if (logoImage != null)
        {
            logoImage.sprite = (iconTable != null) ? iconTable.GetLogo(moduleName) : null;
        }

        // 圆形：显示动图（图由FrameAnimator换，这里只负责显隐）
        SetVisible(circleImage.gameObject, false);
        SetVisible(circleAnim, true);

        // 横条：先按"是不是开火模块"选一套，再按 选中/悬停/常态 选一张
        SetVisible(barImage.gameObject, true);
        if (barImage != null)
        {
            bool isFire = ModuleBook.IsFireType(moduleName);
            barImage.sprite = Pick(isFire ? barFire : barSupport);
        }

        SetVisible(barNameText.gameObject, true);
    }

    // 背包格：底座B + 圆形
    private void UpdateBackpackCell()
    {
        string moduleName = ModulePanelData.Read(area, index);
        bool hasModule = string.IsNullOrEmpty(moduleName) == false;

        if (hasModule)
        {
            // 有模块：只显示圆形
            SetVisible(baseImage.gameObject, false);
            SetVisible(circleImage.gameObject, true);
            SetSprite(circleImage, Pick(circleState));
        }
        else
        {
            // 空格：只显示底座B（悬停时换悬停图）
            SetVisible(circleImage.gameObject, false);
            SetVisible(baseImage.gameObject, true);
            SetSprite(baseImage, hovered ? baseHover : baseNormal);
        }
    }

    // 奖励格：只有圆形
    private void UpdateRewardCell()
    {
        SetVisible(circleImage.gameObject, true);
        SetSprite(circleImage, Pick(circleState));
    }

    // 废弃格：一张图，两态
    private void UpdateDiscardCell()
    {
        SetVisible(baseImage.gameObject, true);
        SetSprite(baseImage, hovered ? baseHover : baseNormal);
    }

    // ===================== 小工具 =====================
    // 选图：优先级 选中 > 悬停 > 常态
    private Sprite Pick(CellSpriteSet s)
    {
        if (s == null) return null;
        if (selected && s.selected != null) return s.selected;
        if (hovered && s.hover != null) return s.hover;
        return s.normal;
    }

    // 显隐（先判空，物体不存在也不崩）
    private void SetVisible(GameObject go, bool visible)
    {
        if (go != null) go.SetActive(visible);
    }

    // 换图（先判空）
    private void SetSprite(Image img, Sprite s)
    {
        if (img != null) img.sprite = s;
    }
}