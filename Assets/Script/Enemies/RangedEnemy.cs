// 标记组件：挂在"站着不动、会原地堆叠"的远程敌人预制体上。
// 刷怪器靠它识别"这块地附近是不是已经有远程怪了"。
// 为什么用标记组件、而不是判断具体类型（GetComponent<WaterWisp>() 之类）：
//   以后新增远程怪，只要挂上这个组件就自动生效，不用回来改刷怪器的代码。
using UnityEngine;

public class RangedEnemy : MonoBehaviour
{
}