// 格子长什么样（外观类型）。
// 为什么和 ModuleArea 分开：
//   ModuleArea 管"数据从哪读"，ModuleCellKind 管"用哪套贴图"。
//   两者目前一一对应，但将来可以不一样
//   （比如美术想让奖励格也用底座A，那只改 kind，不动数据逻辑）。
public enum ModuleCellKind
{
    EquipCell,      // 模块链格：底座A + logo + 圆形动图 + 横条
    BackpackCell,   // 背包格：底座B + 圆形
    RewardCell,     // 奖励格：只有圆形
    DiscardCell,    // 废弃格：一张图
    CandidateCell,   // 【新增】三选一的候选卡片
}