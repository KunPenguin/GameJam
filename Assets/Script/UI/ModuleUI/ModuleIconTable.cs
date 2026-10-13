// "模块种类 → logo图"的对照表，挂在 GameManager 上。
// 为什么放在这里而不是写死进代码：美术会陆续给新图，
// 在 Inspector 里填，新加模块时不用改代码。
using UnityEngine;

public class ModuleIconTable : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        [Tooltip("必须和 ModuleBook.types 里的字符串完全一样")]
        public string moduleName;   // 例如 Test_PierceModule

        [Tooltip("这个模块的logo图")]
        public Sprite logo;
    }

    public Entry[] entries;

    /// <summary>查这个模块的logo图；查不到返回null</summary>
    public Sprite GetLogo(string moduleName)
    {
        if (entries == null) return null;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] == null) continue;
            if (entries[i].moduleName == moduleName) return entries[i].logo;
        }
        return null;   // 查不到，让调用方决定怎么办
    }

    private void Start()
    {
        // 自检：ModuleBook里有、但这里没填logo的，报出来
        // 否则你只会在界面上看到一个空白的logo位，不知道哪里漏了
        for (int i = 0; i < ModuleBook.types.Length; i++)
        {
            if (GetLogo(ModuleBook.types[i]) == null)
            {
                Debug.LogWarning("ModuleIconTable 里没填logo：" + ModuleBook.types[i]);
            }
        }
    }
}