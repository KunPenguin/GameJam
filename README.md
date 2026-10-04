# 玩家移动 + 水弹（程序说明与搭建步骤）

这份文档说的是**玩家部分**：WASD 移动、按住左键连发水弹、水弹的穿透/弹射/吸血等属性、玩家血量。
搭建步骤在第三节，照着点鼠标就行，**不用改代码**。

> 目录约定：脚本放在 `Assets/Script/`（单数）下对应的子文件夹里，
> 预制体放在 `Assets/Prefabs/` 下（子弹放 `Prefabs/Bullets`、玩家放 `Prefabs/Player`）。

---

## 一、脚本清单

| 文件 | 干啥的 | 挂在谁身上 | 需要什么组件 |
| --- | --- | --- | --- |
| `Script/Player/PlayerMove.cs` | WASD 移动 | 玩家 Player | Rigidbody2D（Gravity Scale = 0） |
| `Script/Player/PlayerShoot.cs` | 按住左键朝鼠标连发；提供带参数的 `Fire(...)` | 玩家 Player | 无（要拖水弹预制体进去） |
| `Script/Player/PlayerHealth.cs` | 玩家血量：扣血、吸血回血、每次变化都 Debug.Log | 玩家 Player | 无 |
| `Script/Bullets/WaterBullet.cs` | 飞行、穿透、弹射、吸血、命中 Enemy 打日志、超时/用完销毁 | 水弹预制体 | Rigidbody2D（Gravity Scale = 0）+ CircleCollider2D（勾 Is Trigger） |

---

## 二、所有能调的数值

### PlayerMove

| 变量 | 意思 |
| --- | --- |
| `Move Speed` | 移速（默认 5） |

### PlayerShoot（普攻的默认值，也是 `Fire(...)` 的默认来源）

| 变量 | 意思 | 默认 |
| --- | --- | --- |
| `Bullet Prefab` | 水弹预制体（**必填**） | 空 |
| `Fire Point` | 枪口位置（不填就从玩家中心发射） | 空 |
| `Fire Interval` | 射速：隔多少秒打一发 | 0.15 |
| `Damage` | 伤害（现在只用来算吸血） | 1 |
| `Pierce` | 穿透：能多穿几个敌人，**能命中的敌人数 = 穿透 + 1** | 0 |
| `Lifesteal` | 吸血：0.1 表示造成伤害的 10% 变成回血 | 0 |
| `Bullet Size` | 子弹大小加成：0.5 表示变成 1.5 倍大 | 0 |
| `Heat` | 热力值：-100 ~ +100（暂时只是存着，还没有效果） | 0 |
| `Bounce` | 弹射：穿透用完之后还能弹几次 | 0 |
| `Bullets Per Wave` | 每波子弹个数：一次横向射出几发（霰弹枪那样铺开） | 1 |
| `Wave Count` | 子弹波数：一次发射几波，几波沿垂直方向错开 | 1 |
| `Spread Angle` | 散射角度（总角度，度）：每波子弹在这个角度里均匀铺开 | 0 |
| `Wave Spacing` | 每波之间的间距（世界单位） | 0.3 |

### WaterBullet

| 变量 | 意思 |
| --- | --- |
| `Speed` | 弹速（默认 14） |
| `Life Time` | 最长活多久（默认 2.5 秒） |
| `Damage` / `Pierce Left` / `Lifesteal` / `Bullet Size` / `Heat` / `Bounce Left` | **由 PlayerShoot 在生成时写入**，是这一枪的数值，运行时看得到，不用手改 |
| `Player Health` | 玩家的血量脚本（吸血用，由 PlayerShoot 传进来） |

### PlayerHealth

| 变量 | 意思 | 默认 |
| --- | --- | --- |
| `Max Hp` | 最大血量 | 20 |
| `Current Hp` | 当前血量（运行时看） | 20 |

---

## 三、搭建步骤

### 第 1 步：做玩家（在 GameScene 里）

1. 打开 `Assets/Scenes/GameScene.scene`；
2. 菜单 **GameObject → 2D Object → Sprites → Circle**；
3. 改名成 `Player`，**Tag 选 `Player`**；
4. **Add Component → Rigidbody 2D** → `Gravity Scale` 改 **0**、`Collision Detection` 改 **Continuous**；
5. **Add Component → Circle Collider 2D** → `Radius` 改成 `0.3`（**不要**勾 Is Trigger）；
6. **Add Component → Player Move**、**Player Shoot**、**Player Health** 三个都加上；
7. 拖进 `Assets/Prefabs/Player/` 存成预制体，`Ctrl + S` 保存场景。

### 第 2 步：做水弹预制体

1. 菜单 **GameObject → 2D Object → Sprites → Square**，改名 `WaterBullet`，`Scale` 改成 `(0.2, 0.2, 1)`；
2. **Add Component → Rigidbody 2D** → `Gravity Scale` 改 **0**、`Collision Detection` 改 **Continuous**；
3. **Add Component → Circle Collider 2D** → **勾上 `Is Trigger`**；
4. **Add Component → Water Bullet**；
5. 拖进 `Assets/Prefabs/Bullets/` 存成预制体，然后把 Hierarchy 里那个临时的删掉；
6. 选中 `Player`，在 **Player Shoot** 的 `Bullet Prefab` 那一栏拖入刚做的预制体。

### 第 3 步：测试

按 **Play**（记得先用鼠标点一下 Game 窗口）：

- `W A S D` 移动；
- 按住鼠标左键连发，子弹朝鼠标方向飞；
- Console 里能看到 `发射！波数 1，每波 1 发...`、`水弹飞了 2.5 秒还没打到东西，自动销毁`。

**测穿透/弹射/吸血**要场上有几个敌人，可以临时摆几个靶子：

1. 菜单 **GameObject → 2D Object → Sprites → Square**，改名 `靶子1`；
2. **Tag 选 `Enemy`**，**Add Component → Circle Collider 2D**（`Radius` 0.5）；
3. 复制两份，摆成一排（横向间距 2 左右）；
4. 选中 Player，把 **Player Shoot** 上的 `Pierce` 改成 `2`、`Bounce` 改成 `3`、`Lifesteal` 改成 `0.5`；
5. Play 朝靶子开枪，Console 里应该按这个顺序出现：

```
水弹打中敌人：靶子1
水弹穿透，继续飞（还能再命中 2 个敌人）
水弹打中敌人：靶子2
水弹穿透，继续飞（还能再命中 1 个敌人）
水弹打中敌人：靶子3
水弹弹射，转向下一个敌人（还能弹 2 次）
...
吸血回复 0.5 点，当前血量 20 / 20
```

> 穿透 2 + 弹射 3 = 这一发子弹最多能打中 **6 个**敌人（先穿 3 个，穿透用完后还能弹 3 次）。

**测血量**：现在还没有怪，可以临时用键盘测。打开 `PlayerHealth.cs`，在 `Update` 里加这几行（测完删掉）：

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.H)) TakeDamage(1f);   // 按 H 扣 1 血
}
```

按 Play 后连续按 H，Console 会打印 `玩家受到 1 点伤害，当前血量 19 / 20`，血量归零会打印 `玩家血量归零`。

---

## 四、模块怎么用这个射击函数

`PlayerShoot` 里的 `Fire(...)` 是给模块链调用的，参数顺序固定：

```csharp
//                   穿透  吸血  子弹大小 热力值 弹射 波数 每波个数 散射角
shoot.Fire(          2f,   0.1f,  0.5f,   0f,   3,   1,    5,     30f);
```

举个"霰弹枪"的例子：`Fire(0f, 0f, 0f, 0f, 0, 1, 10, 40f)` = 一次射出 10 发、散开 40 度、没有穿透和弹射。
"大狙"的例子：`Fire(5f, 0f, 0.5f, 0f, 0, 1, 1, 0f)` = 一发、能穿 6 个敌人、子弹大一圈。
（伤害现在取的是 PlayerShoot 上的 `Damage`；以后做武器如果要"每把武器伤害不同"，把 damage 也加进 Fire 的参数里就行。）

规则汇总：

| 属性 | 规则 |
| --- | --- |
| 穿透 | 能命中的敌人数 = 穿透 + 1；还有穿透就继续往前飞，不销毁 |
| 弹射 | 穿透用完后，命中敌人不销毁，转向**最近的另一个敌人**继续打；次数 = 弹射值 |
| 吸血 | 每次命中回复 = `Damage × Lifesteal`（血量是小数，回血不能超过上限） |
| 子弹大小 | 生成时把 Scale 乘上 `(1 + 子弹大小)`，碰撞体跟着一起变大 |
| 热力值 | 只是记录在子弹上，暂时没有效果（等元素系统） |
| 每波个数 + 散射角 | 一波内均匀铺开：10 发 + 40 度 = 每发间隔约 4.4 度 |
| 波数 | 同时发射几波，几波沿"垂直于瞄准方向"错开 `Wave Spacing` 距离 |

---

## 五、和统一要求的对照

| 统一要求 | 我们怎么做的 |
| --- | --- |
| 玩家 Tag 必须是 `Player` | 第 1 步第 3 点手动设置 |
| 敌人 Tag 必须是 `Enemy` | 标签已经加进工程；靶子/敌人自己挂上 |
| 水弹用 `CompareTag("Enemy")` 判断命中 | `WaterBullet.OnTriggerEnter2D` |
| 命中后用 `SendMessage("OnHit")` 通知敌人 | 已写好（**不带参数**，符合统一要求） |
| 子弹用 `Rigidbody2D.velocity` 飞，碰撞体勾 `Is Trigger` | `WaterBullet.FixedUpdate` 用 `rb.velocity`；Is Trigger 见第 2 步第 3 点 |
| 不许用 `transform.Translate` | 没有使用 |
| 所有变量用 `public` 放 Inspector | 全部 public，带中文注释 |
| 脚本开头写清"干什么 / 挂谁 / 要什么组件" | 四个脚本开头都有 |
| 脚本放 `Assets/Script/` 子文件夹、脚本名=文件名 | `Script/Player/`、`Script/Bullets/` |

---

## 六、跳过 / 还没做的部分

- `Fire(...)` 的参数里**没有伤害**（按目前的要求）；伤害取 PlayerShoot 上的 `Damage`。以后武器要各自伤害时再加。
- **热力值**只是存在子弹上，蒸汽/冰冻等效果还没做。
- 玩家血量归零后只是打了条日志（`dead = true`），**还没有接上 GameOverPanel**。
  要接的话在 `PlayerHealth` 里加一个 `public GameOverPanel gameOverPanel;`，血量归零时调用 `gameOverPanel.Show();` 就行。
- 敌人脚本、敌方子弹不是这部分（由别人负责）。
- 旧的"纯代码自动生成"那套脚本已经删掉，不会再影响手动搭建。

---

## 七、常见问题

**按 Play 角色往下掉** → Player 的 Rigidbody 2D `Gravity Scale` 要改成 0。

**按住左键没反应** → 先看 Console 有没有"还没有把水弹预制体拖到..."；再确认 `Project Settings → Player → Active Input Handling = Input Manager (Old)`。

**打中靶子没反应** → 靶子的 Tag 是不是 `Enemy`（**区分大小写**）；水弹的 Collider 有没有勾 `Is Trigger`。

**子弹穿过敌人** → 水弹预制体的 Rigidbody 2D → `Collision Detection` 改成 `Continuous`。

**子弹穿了一次就没了** → `Pierce` 还是 0（默认 0 表示只打 1 个）；想要穿就调大。

**吸血没效果** → 玩家身上要有 `Player Health` 组件，并且 `Lifesteal` 要大于 0。

**报错说 Camera.main 是空的** → 场景里要有 Tag 为 `MainCamera` 的相机（GameScene 里已有，别删）。

**改了预制体数值别的场景没变** → Inspector 顶部点 `Overrides` → `Apply All`。
