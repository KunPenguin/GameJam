// 一个元素的三种状态图，捆成一组，Inspector 里会折叠成一行，方便填。
// 为什么叫 CellSpriteSet 而不叫 SpriteState：
//   UnityEngine.UI 里已经有一个 SpriteState 了，
//   同名会让编译器和读代码的人都困惑，换个名字最省事。
using UnityEngine;

[System.Serializable]
public class CellSpriteSet
{
    public Sprite normal;      // 常态
    public Sprite hover;       // 悬停
    public Sprite selected;    // 选中
}