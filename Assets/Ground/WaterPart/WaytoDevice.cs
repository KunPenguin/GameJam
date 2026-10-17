/// <summary>
/// 旧的指引图标类名，逻辑全部继承自 ObjectivePointer。
/// 这样做的好处：你场景/预制体上已经挂着的 WaytoDevice 组件**不用重新挂**，
/// 但行为会跟着 ObjectivePointer 一起修好（屏幕内夹边界、不再 SetActive 隐藏等）。
///
/// 以后新建图标建议直接用 ObjectivePointer。
/// </summary>
public class WaytoDevice : ObjectivePointer
{
}
