# 玩家移动 + 水弹（程序说明与搭建步骤）

这份文档说的是**玩家部分**：WASD 移动、按住左键连发水弹、水弹命中 Enemy 标签的敌人后打印日志并销毁。
照着第三节搭一遍就能测，**不用改任何代码**。

> 目录约定：本工程的脚本放在 `Assets/Script/`（单数）下对应的子文件夹里，
> 预制体放在 `Assets/Prefabs/` 下（子弹放 `Prefabs/Bullets`、玩家放 `Prefabs/Player`）。

---

## 一、这次做了哪 4 件事

| 任务 | 状态 | 在哪个脚本里 |
| --- | --- | --- |
| 1. 角色 WASD 移动 | ✅ | `Assets/Script/Player/PlayerMove.cs` |
| 2. 按住鼠标左键连发子弹，朝鼠标方向飞 | ✅ | `Assets/Script/Player/PlayerShoot.cs` |
| 3. 子弹命中带 `Enemy` 标签的敌人 → **Console 打印日志**并销毁这发子弹 | ✅ | `Assets/Script/Bullets/WaterBullet.cs` |
| 4. 弹速、移速、射速等变量都是 `public`，能在 Inspector 里改 | ✅ | 三个脚本（见第二节表格） |

敌人和敌方子弹**不是这部分**，由其他人负责。这部分只需要"打中敌人时有反应"，敌人做出来就能直接用。

---

## 二、脚本清单

| 文件 | 干啥的 | 挂在谁身上 | 需要什么组件 |
| --- | --- | --- | --- |
| `Script/Player/PlayerMove.cs` | WASD 移动 | 玩家物体 Player | Rigidbody2D（Gravity Scale = 0） |
| `Script/Player/PlayerShoot.cs` | 按住左键朝鼠标连发水弹 | 玩家物体 Player | 无（但要把水弹预制体拖进 `bulletPrefab`） |
| `Script/Bullets/WaterBullet.cs` | 飞行、超时销毁、命中 Enemy 销毁 | 水弹预制体 | Rigidbody2D（Gravity Scale = 0）+ CircleCollider2D（勾 Is Trigger） |

每个脚本 Inspector 上能改的东西：

| 脚本 | 变量 | 意思 |
| --- | --- | --- |
| PlayerMove | `Move Speed` | 移速（默认 5） |
| PlayerShoot | `Bullet Prefab` | 水弹预制体（**必填**） |
| PlayerShoot | `Fire Point` | 枪口位置（可不填） |
| PlayerShoot | `Fire Interval` | 射速，隔多少秒一发（默认 0.15，约每秒 6.7 发） |
| WaterBullet | `Speed` | 弹速（默认 14） |
| WaterBullet | `Life Time` | 子弹最长活多久（默认 2.5 秒） |

> 另外每个脚本还有 `rb`、`timer`、`aliveTime` 几个变量也是 public，
> 那些是**运行时的内部状态，不用手动改**（`rb` 留空会自动找自己身上的 Rigidbody2D）。

---

## 三、搭建步骤（照着做，约 5 分钟）

### 第 1 步：做玩家（在 GameScene 里）

1. 打开 `Assets/Scenes/GameScene.scene`；
2. 菜单 **GameObject → 2D Object → Sprites → Circle**（会自动带一个 Sprite Renderer，
   用内置的白色圆形先顶着；美术出图后把 `Assets/Art/Characters` 里的图拖到 `Sprite` 那一栏就行）；
3. 把这个物体改名成 `Player`；
4. 选中它，Inspector **最上面 Tag 那一栏选 `Player`**（统一要求，必须设）；
5. **Add Component → Rigidbody 2D**，把 **Gravity Scale 改成 0**；
6. **Add Component → Circle Collider 2D**，`Radius` 改成 `0.3` 左右（挨打范围）；
7. **Add Component → Player Move**，再 **Add Component → Player Shoot**；
8. 把 Player 拖进 Project 窗口的 `Assets/Prefabs/Player/` 文件夹，做成预制体（团队约定），
   然后 `Ctrl + S` 保存场景。

### 第 2 步：做水弹预制体

1. 菜单 **GameObject → 2D Object → Sprites → Square**，改名成 `WaterBullet`；
2. 把它的 `Scale` 改成 `(0.2, 0.2, 1)`（小一点，水弹不该跟人一样大）；
3. **Add Component → Rigidbody 2D**：**Gravity Scale 改成 0**，
   **Collision Detection 改成 Continuous**（子弹飞得快也不会穿过敌人）；
4. **Add Component → Circle Collider 2D**：**把 `Is Trigger` 勾上**（统一要求，必须勾）；
5. **Add Component → Water Bullet**；
6. 把这个物体拖进 Project 窗口的 `Assets/Prefabs/Bullets/` 文件夹，生成预制体；
7. 生成后，把 **Hierarchy 里那个临时的 WaterBullet 删掉**；
8. 回到 Hierarchy 选中 `Player`，在 **Player Shoot** 组件里，把 `Assets/Prefabs/Bullets/WaterBullet.prefab`
   拖到 **Bullet Prefab** 那一栏。

> **最容易忘的一步**：`Bullet Prefab` 不拖东西，按左键是打不出子弹的
> （Console 会提示"还没有把水弹预制体拖到 PlayerShoot 的 bulletPrefab 上！"）。

### 第 3 步：测试

按 **Play**（在 GameScene 里，或者从 StartScene 点"开始游戏"进去）：

- `W A S D` 移动；
- 按住鼠标左键连发，子弹朝鼠标方向飞；
- Console 里会看到：

```
发射水弹，方向 = (0.97, -0.24)
水弹飞了 2.5 秒还没打到东西，自动销毁
```

**验证"打中敌人"**（敌人还没做，可以先造个靶子）：

1. 在 GameScene 里菜单 **GameObject → 2D Object → Sprites → Square**，改名 `靶子`；
2. 它的 **Tag 选 `Enemy`**（这个标签已经加进工程了，下拉里能找到）；
3. **Add Component → Circle Collider 2D**，`Radius` 改成 `0.5`；
4. 把它挪到玩家前面，按 Play 朝它开枪，Console 会出现：

```
水弹打中敌人：靶子，销毁
```

测完把靶子删掉。（子弹还会顺手发一条 `OnHit` 消息给靶子，靶子没有这个方法也不会报错。）

---

## 四、和统一要求的对照

| 统一要求 | 我们怎么做的 |
| --- | --- |
| 玩家 Tag 必须是 `Player` | 第 1 步第 4 点手动设置 |
| 敌人 Tag 必须是 `Enemy` | 已经帮你在 Project Settings 里加好这个标签；敌人的物体自己挂上即可 |
| 水弹用 `CompareTag("Enemy")` 判断命中 | `WaterBullet.cs` 的 `OnTriggerEnter2D` |
| 命中后用 `SendMessage("OnHit")` 通知敌人 | `WaterBullet.cs` 里已写好，敌人脚本等着接 |
| 子弹用 `Rigidbody2D.velocity` 飞，碰撞体勾 `Is Trigger` | `WaterBullet.cs` 用 `rb.velocity`；Is Trigger 见第 2 步第 4 点 |
| 不许用 `transform.Translate` | 三个脚本都没用 |
| 所有变量用 `public` 放 Inspector | 三个脚本全部 public（带中文注释说明） |
| 脚本开头写清"干什么 / 挂谁 / 要什么组件" | 三个脚本开头都有 3 行中文注释 |
| 脚本放 `Assets/Script/` 的子文件夹，脚本名=文件名 | `Script/Player/`、`Script/Bullets/` |

---

## 五、改数值 / 改预制体

**场景里的对象**：选中它，在 Inspector 里改，改完 `Ctrl + S`。

**预制体**：Project 窗口双击 `Assets/Prefabs/Bullets/WaterBullet.prefab` 进去改，改完 `Ctrl + S`。

**在场景里改了预制体的数值**：Inspector 顶部会显示 `Overrides`，点它 → **Apply All**，
才会作用到所有用到这个预制体的地方；不点只对当前场景里这一个生效（最容易踩的坑）。

| 想改什么 | 去哪儿改 |
| --- | --- |
| 移速 | Player → Player Move → `Move Speed` |
| 射速 | Player → Player Shoot → `Fire Interval` |
| 弹速 | WaterBullet 预制体 → Water Bullet → `Speed` |
| 子弹飞多久/射程 | WaterBullet 预制体 → Water Bullet → `Life Time`（射程 ≈ 速度 × 时间） |

---

## 六、常见问题

**按 Play 角色往下掉**
Player 的 Rigidbody 2D → `Gravity Scale` 要改成 **0**。

**按住左键没反应**
先看 Console 有没有"还没有把水弹预制体拖到..."；再确认
`Project Settings → Player → Active Input Handling = Input Manager (Old)`（本工程就是这个）。

**子弹看不见 / 不动**
预制体没加 Sprite Renderer 或 Sprite 是空的；不动一般是没加 Rigidbody 2D 或没挂 Water Bullet 脚本。

**打中靶子没反应**
靶子的 Tag 是不是 `Enemy`（**区分大小写**）；水弹的 Collider 有没有勾 `Is Trigger`。

**子弹穿过敌人**
水弹预制体的 Rigidbody 2D → `Collision Detection` 改成 `Continuous`。

**报错说 Camera.main 是空的**
场景里要有 Tag 为 `MainCamera` 的相机（GameScene 里已经有，别删）。

**改了预制体数值，别的场景没变**
Inspector 顶部点 `Overrides` → `Apply All`。
