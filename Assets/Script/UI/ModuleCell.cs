using TMPro;
using UnityEngine;

public class ModuleCell : MonoBehaviour
{
    [Header("这个格子属于哪个区")]
    public ModuleArea area = ModuleArea.Equip;

    [Header("格子里的模块在第几格(从0开始)")]
    public int index = 0;

    [Header("格子里的模块的名字（拖入子物体）")]
    public TextMeshProUGUI nameText;

    [Header("格子里的模块的说明（可以留空）")]
    public TextMeshProUGUI descText;

    [Header("废弃格的固定文字（只有废弃格填写）")]
    public string discardLabel = "废弃";

    public void Refresh()
    {
        //废弃格：显示固定文字，不读数据
        if (area == ModuleArea.Discard)
        {
            if (nameText != null)
            {
                nameText.text = discardLabel;
            }
            if(descText != null)
            {
                descText.text = "";
            }
            return;
        }
        //其他三个区：显示模块信息
        string moduleName = ModulePanelData.Read(area, index);

        //空插槽：显示空
        if(string.IsNullOrEmpty(moduleName))
        {
            if (nameText != null)
            {
                nameText.text = "";
            }
            if (descText != null)
            {
                descText.text = "";
            }
            return;
        }

        //有模块：用ModuleBook显示模块信息
        string display;
        string desc;
        bool ok = ModuleBook.GetInfo(moduleName, out display, out desc);

        if (nameText != null) nameText.text = display;
        if(descText != null) descText.text = desc;

        //查不到（如名字错误或ModuleBook里没有登记）时给出提示
        if(!ok&&nameText != null)
        {
            nameText.text = "未知模块";
        }
    }
}
