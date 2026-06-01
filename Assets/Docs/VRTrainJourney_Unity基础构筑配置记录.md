# VRTrainJourney Unity 基础构筑配置记录

## 1. 文档用途

本文档记录 VRTrainJourney 项目从 Unity 初始配置到 Quest 2 最小 XR 场景真机验证通过的完整过程。

记录范围仅包括：

- Unity 基础设置。
- XR 与 Android 依赖安装。
- Quest 2 构建参数。
- 移动端 URP 性能基线。
- Windows 环境问题排查。
- Quest 2 USB 调试连接。
- 最小 XR 场景构建与头部追踪验证。

本文档不包含车厢建模、正式场景制作、交互逻辑、脚本开发和视频播放系统。这些内容应在后续开发阶段单独处理。

## 2. 当前基础环境

| 项目 | 当前配置 |
| --- | --- |
| Unity 编辑器 | `6000.3.16f1 LTS` |
| 渲染管线 | `URP 17.3.0` |
| 目标设备 | `Meta Quest 2` |
| 目标平台 | `Android` |
| XR 标准 | `OpenXR 1.16.1` |
| 构建架构 | `ARM64` |
| Android 图形接口 | 仅保留 `Vulkan` |
| Android 包名 | `com.jesyn.vrtrainjourney2026` |
| 调试 APK | `Builds/Android/VRTrainJourney2026_Debug.apk` |

## 3. 按实际顺序完成的配置步骤

### 3.1 安装并确认基础 XR 依赖

在 `Window > Package Management > Package Manager`（窗口 > 包管理 > 包管理器）中确认项目已经安装以下依赖：

| Package（安装包） | 版本 | 用途 |
| --- | --- | --- |
| `Input System` | `1.19.0` | 使用 Unity 新输入系统读取头显和控制器输入。 |
| `Universal RP` | `17.3.0` | 为 Quest 2 提供适合移动端优化的 URP 渲染管线。 |
| `XR Interaction Toolkit` | `3.3.2` | 提供 XR Origin、交互管理器和后续交互组件。 |
| `XR Plug-in Management` | `4.5.4` | 管理 Android 与电脑端使用的 XR Loader。 |
| `OpenXR Plugin` | `1.16.1` | 使用统一 OpenXR 标准支持 Quest 2。 |

安装后打开 `Window > General > Console`（窗口 > 常规 > 控制台），确认没有红色编译错误。

**动机：** 在继续配置 XR 之前先保证依赖安装完整且项目可以正常编译。后续任何 XR 设置都依赖这些包。

### 3.2 处理 XR Interaction Toolkit 示例资源

配置过程中曾经导入：

- `Starter Assets`（起步资源）
- `XR Device Simulator`（XR 设备模拟器）

当前阶段已经删除这两组示例资源，但保留了 `XR Interaction Toolkit` 安装包本身。

**动机：**

- `Starter Assets` 提供预设输入、交互 Prefab 和 locomotion 示例，正式开发交互功能时可能重新导入。
- `XR Device Simulator` 用于没有连接头显时，通过鼠标和键盘模拟 XR 设备，后续调试时也可能重新导入。
- 它们并非永久无用，只是在“基础配置与真机连通验证”阶段不属于必需项。

`Assets/XRI/Settings` 与 `Assets/XR` 下由 Unity 自动生成的设置资源应保留。

### 3.3 将构建平台切换到 Android

打开：

`File > Build Profiles`（文件 > 构建配置）

在左侧选择 `Android`（安卓），将其切换为 `Active`（已激活）。

**动机：** Quest 2 独立运行模式使用 Android APK。只有激活 Android 平台后，Unity 才会显示并校验 Quest 2 真机需要的参数。

### 3.4 关闭 Build Profiles 中的 Diagnostics Data

在 `Build Profiles`（构建配置）窗口中，将：

`Diagnostics Data`（诊断数据）

设置为：

`Disabled`（禁用）

**动机：** 关闭 Unity 自动崩溃诊断数据上传，消除与本项目无关的 Debug Symbols 黄色提醒。该操作不会禁用：

- `Console`（控制台）
- `Android Logcat`（安卓日志查看器）
- `Development Build`（开发版本）
- `Script Debugging`（脚本调试）
- `Profiler`（性能分析器）

### 3.5 配置 Android OpenXR

打开：

`Edit > Project Settings > XR Plug-in Management`  
（编辑 > 项目设置 > XR 插件管理）

点击上方安卓机器人图标 `Android`（安卓），勾选：

`OpenXR`

**动机：** 让构建出的 Android APK 使用 OpenXR Loader，在 Quest 2 上以独立 VR 应用运行。

### 3.6 处理 Android OpenXR 初始验证提示

打开：

`Edit > Project Settings > XR Plug-in Management > Project Validation`  
（编辑 > 项目设置 > XR 插件管理 > 项目验证）

在安卓机器人图标下处理以下提示：

1. 在 OpenXR 的交互配置中添加：
   `Oculus Touch Controller Profile`（Oculus Touch 控制器配置）
2. 对以下迁移建议点击 `Fix`（修复）：
   - 使用 `InputSystem.XR.PoseControl`
   - 使用 `StickControl` thumbsticks

**动机：**

- Quest 2 使用 Touch 控制器，需要明确启用对应 OpenXR 交互配置。
- 两项迁移修复用于适配当前 Input System 类型，避免未来版本升级时产生兼容问题。

### 3.7 处理 Windows Smart App Control 对 Burst 的拦截

在 OpenXR 自动修复触发重新编译时，Windows 安全中心曾拦截 Unity 生成的 Burst JIT 临时 DLL，Console 中出现红色错误。

日志中可见类似内容：

```text
Unexpected error in Burst compilation
Unable to load unmanaged library
Library\BurstCache\JIT\*.dll
```

当前电脑已关闭：

`Smart App Control`（智能应用控制）

之后 Burst 编译恢复正常。

**动机：** 这是 Windows 环境的代码完整性拦截，不是 Unity XR 配置错误。解决后可以继续保留 Burst 编译能力。该步骤只在同类错误出现时需要处理，不是每台电脑都必须执行。

### 3.8 配置电脑端 OpenXR，供 Quest Link / Air Link 调试

仍在：

`Edit > Project Settings > XR Plug-in Management`  
（编辑 > 项目设置 > XR 插件管理）

点击顶部电脑图标 `Standalone`（电脑独立平台），勾选：

`OpenXR`

随后在电脑图标对应的 OpenXR 配置中添加：

`Oculus Touch Controller Profile`（Oculus Touch 控制器配置）

**动机：**

- 安卓机器人图标控制 Quest 2 独立 APK。
- 电脑图标控制 Unity 编辑器 Play Mode 以及 Quest Link / Air Link 电脑端调试。
- 两者用途不同，可以同时启用。

### 3.9 配置 Android Player Settings

打开：

`Edit > Project Settings > Player`  
（编辑 > 项目设置 > 播放器）

切换到安卓机器人图标，确认并设置：

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Package Name`（包名） | `com.jesyn.vrtrainjourney2026` | 作为 Android 应用的唯一标识。 |
| `Scripting Backend`（脚本后端） | `IL2CPP` | 适合 Quest 2 Android 构建，并满足 OpenXR 与发布要求。 |
| `Active Input Handling`（活动输入处理） | `Input System Package (New)` | 使用新输入系统读取 XR 设备。 |
| `Target Architectures`（目标架构） | 仅 `ARM64` | Quest 2 OpenXR 独立应用使用 ARM64。 |
| `Minimum API Level`（最低 API 级别） | `Android 7.1 Nougat (API level 25)` | 当前验证通过，保留默认基线。 |
| `Target API Level`（目标 API 级别） | `Automatic (highest installed)` | 使用当前环境已安装的最高 API。 |

### 3.10 Android 图形接口仅保留 Vulkan

在：

`Edit > Project Settings > Player > Android > Other Settings > Rendering`  
（编辑 > 项目设置 > 播放器 > 安卓 > 其他设置 > 渲染）

执行：

1. 保持 `Auto Graphics API`（自动选择图形接口）不勾选。
2. 在 `Graphics APIs`（图形接口）列表中移除 `OpenGLES3`。
3. 仅保留 `Vulkan`。

**动机：** OpenXR 官方包将 Meta Quest Android ARM64 的首选图形接口标记为 Vulkan。固定接口可以避免设备回退到 OpenGLES3，并为后续 Quest Vulkan 优化保留一致的运行环境。

页面中：

`Texture Compression Targeting is disabled`

属于信息提示。当前纹理压缩格式仅保留 `ASTC`，符合 Quest 2 使用场景，不需要额外处理。

### 3.11 启用 Meta Quest Support

打开：

`Edit > Project Settings > XR Plug-in Management > OpenXR`  
（编辑 > 项目设置 > XR 插件管理 > OpenXR）

点击安卓机器人图标，在 `OpenXR Feature Groups`（OpenXR 功能组）中勾选：

`Meta Quest Support`（Meta Quest 支持）

确认目标设备列表中包含：

`Quest 2`

**动机：** OpenXR 通用 Loader 负责标准 XR 接入，`Meta Quest Support` 负责生成适合 Quest 系列设备运行的 APK，并启用 Meta Quest 专用选项。

### 3.12 调整 OpenXR 延迟优化

在 Android OpenXR 页面中，将：

`Latency Optimization`（延迟优化）

从：

`Prioritize Rendering`（优先渲染）

调整为：

`Prioritize Input Polling`（优先输入轮询）

**动机：** 当前 OpenXR 包在启用 `Meta Quest Support` 后，官方验证规则会建议优先缩短输入轮询到画面提交之间的延迟，使交互反馈更及时。

### 3.13 识别并保留 SSAO 可选提醒

启用 `Meta Quest Support` 后，Android `Project Validation` 页面仍会显示一条黄色提醒：

```text
Using the Screen Space Ambient Occlusion render feature results in
significant performance overhead when the application is running natively
on device.
```

原因是 Unity 会扫描项目内所有可能使用的 URP Renderer。模板自带的：

`PC_Renderer`（电脑端渲染器）

仍然包含：

`Screen Space Ambient Occlusion`（屏幕空间环境光遮蔽，SSAO）

而 Quest 2 Android 实际使用的：

`Mobile_Renderer`（移动端渲染器）

没有添加 SSAO Renderer Feature。

**处理结论：** 当前保留此黄色提醒，不删除 `PC_Renderer` 中的 SSAO。它不是 Android 必修错误，也不会阻碍打包和真机运行。后续如果希望让验证页面完全干净，再单独决定是否移除电脑端 SSAO。

### 3.14 设置 Quest 2 移动端 URP 基线

在 `Project`（项目）窗口中选择：

`Assets > Settings > Mobile_RPAsset`

完成以下设置：

#### Rendering（渲染）

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Depth Texture`（深度纹理） | 关闭 | 避免没有实际需求时生成额外深度纹理。 |
| `Opaque Texture`（不透明纹理） | 关闭 | 避免额外复制不透明颜色纹理。 |

#### Quality（质量）

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `HDR`（高动态范围） | 关闭 | 降低移动 VR 渲染成本。 |
| `Anti Aliasing (MSAA)`（多重采样抗锯齿） | `2x` | 在 Quest 2 上兼顾边缘质量与性能。 |
| `Render Scale`（渲染比例） | `0.8` | 保持模板移动端的轻量基线。 |
| `Upscaling Filter`（放大滤镜） | `Automatic`（自动） | 暂时由 Unity 选择适合的放大方式。 |
| `LOD Cross Fade`（LOD 渐变切换） | 关闭 | 降低移动端渐变过渡成本。 |

`Render Scale` 低于 `1` 时，Inspector 中可能出现摄像机深度相关黄色信息条。当前属于信息提示，不是错误。

#### Lighting（光照）

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Main Light > Cast Shadows`（主光源 > 投射阴影） | 关闭 | 在建模前优先确保 Quest 2 帧率稳定。 |
| `Additional Lights`（附加光源） | `Per Vertex`（逐顶点） | 保留车厢灯光能力，同时降低逐像素计算成本。 |
| `Additional Lights > Cast Shadows`（附加光源 > 投射阴影） | 关闭 | 避免额外实时阴影成本。 |
| `Reflection Probes > Probe Blending`（反射探针 > 探针混合） | 关闭 | 降低移动端反射探针计算成本。 |
| `Reflection Probes > Box Projection`（反射探针 > 盒体投影） | 关闭 | 暂不启用室内反射高级能力。 |
| `Reflection Probes > Probe Atlas`（反射探针 > 探针图集） | 关闭 | 当前会跟随前置选项联动变灰，保持关闭即可。 |

#### Shadows（阴影）

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Max Distance`（最大阴影距离） | `20` | 为以后按需恢复少量实时阴影预留适合车厢尺度的上限。 |
| `Cascade Count`（阴影级联数量） | `1` | 限制阴影复杂度。 |
| `Soft Shadows`（柔和阴影） | 关闭 | 降低移动端阴影采样成本。 |

#### Adaptive Performance（自适应性能）

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Use Adaptive Performance`（使用自适应性能） | 关闭 | 当前未接入 Quest 专用自适应性能提供器，关闭只是明确配置意图。 |

`Use Adaptive Performance` 不是必修项，也不是故障修复项。以后如果专门接入设备适配提供器，可以重新启用。

### 3.15 设置 Mobile Renderer

在 `Project`（项目）窗口中选择：

`Assets > Settings > Mobile_Renderer`

确认并设置：

| 设置项 | 当前值 | 动机 |
| --- | --- | --- |
| `Rendering Path`（渲染路径） | `Forward`（前向渲染） | 适合 Quest 2 移动端 VR 基线。 |
| `Depth Priming Mode`（深度预处理模式） | `Disabled`（关闭） | 避免不必要的预处理成本。 |
| `Post-processing > Enabled`（后处理 > 启用） | 关闭 | 关闭模板中的 Bloom、Vignette 等后处理通道，降低真机成本。 |
| `Renderer Features`（渲染器功能） | 空 | Android 移动端未启用 SSAO 等额外功能。 |

后处理资源仍可保留在项目中。以后确实需要某个效果时，应结合 Quest 2 真机帧率逐项恢复，不应一次性全部打开。

### 3.16 复查 Android Project Validation

重新打开：

`Edit > Project Settings > XR Plug-in Management > Project Validation`  
（编辑 > 项目设置 > XR 插件管理 > 项目验证）

点击安卓机器人图标，保持：

`Show all`（显示全部）

不勾选。

验证结果：

- 没有必须修复的 Android XR 错误。
- 仅保留电脑端 `PC_Renderer` SSAO 引发的可选黄色提醒。

**动机：** 用 Unity 自带规则确认 Android OpenXR 必修项已经完成。`Show all` 勾选后会列出通过项，默认不勾选更适合观察仍待处理的问题。

### 3.17 安装 Android Logcat

打开：

`Window > Package Management > Package Manager`  
（窗口 > 包管理 > 包管理器）

在 `Unity Registry`（Unity 注册表）中搜索并安装：

`Android Logcat`（安卓日志查看器）`1.4.7`

安装后可以通过：

`Window > Analysis > Android Logcat`  
（窗口 > 分析 > 安卓日志查看器）

打开真机日志窗口。

**动机：** Quest 2 上如果出现黑屏、启动失败或崩溃，Android Logcat 比 Unity 编辑器 Console 更适合定位设备端问题。

### 3.18 开启 Quest 2 开发者模式

为了让 Quest 2 接受 USB 调试，需要：

1. 使用绑定 Quest 2 的 Meta 账号登录 Meta Horizon 开发者后台。
2. 创建一个 `Organization`（开发者组织）。
3. 在手机端 `Meta Horizon` 应用中进入设备设置。
4. 开启 `Developer Mode`（开发者模式）。
5. 重启 Quest 2。

**动机：** Meta 要求账号具备开发者身份后，Quest 2 才允许通过 USB 安装和调试未发布 APK。开发者组织只是后台工作空间，不等于注册公司。

### 3.19 完成 Quest 2 USB 调试授权

使用支持数据传输的 USB-C 线将 Quest 2 连接电脑。

戴上头显，在系统弹窗中：

1. 勾选 `Always allow from this computer`（始终允许此电脑）。
2. 点击 `Allow`（允许）。

ADB 检测结果由：

```text
unauthorized
```

变为：

```text
device product:hollywood model:Quest_2
```

**动机：** `unauthorized` 表示电脑已经识别设备，但头显还没有允许 USB 调试。变为 `device` 后，Unity 才可以自动安装 APK 和读取设备日志。

### 3.20 在 Unity 中选择 Quest 2 运行设备

打开：

`File > Build Profiles`（文件 > 构建配置）

确认：

1. 左侧 `Android`（安卓）显示 `Active`（已激活）。
2. 点击 `Run Device > Refresh`（运行设备 > 刷新）。
3. 在下拉列表中选择 `Oculus Quest 2`。

**动机：** 明确指定 APK 构建完成后自动安装到当前连接的 Quest 2。

### 3.21 完成第一次空场景 APK 构建

在 `Build Profiles`（构建配置）中：

1. 勾选 `Development Build`（开发版本）。
2. 保持以下选项不勾选：
   - `Autoconnect Profiler`（自动连接性能分析器）
   - `Deep Profiling Support`（深度性能分析）
   - `Script Debugging`（脚本调试）
3. 点击 `Build And Run`（构建并运行）。
4. 将 APK 保存为：

```text
Builds/Android/VRTrainJourney2026_Debug.apk
```

首次构建已经成功：

- APK 正常生成。
- APK 正常安装到 Quest 2。
- Unity Console 没有构建错误。
- Quest 2 可以进入 Unity 应用画面。

**动机：** 先验证 Unity、IL2CPP、Android SDK、ADB、OpenXR 和 Quest 2 安装链路是否连通，再继续搭建 XR 场景。

Android Logcat 中可能持续出现黄色或红色系统日志。它会混合输出 Quest 2 操作系统和后台服务信息，颜色本身不等于项目崩溃。排查时应优先按应用包名：

```text
com.jesyn.vrtrainjourney2026
```

过滤日志。

### 3.22 调整 Quest 2 系统地面高度

首次运行时，Quest 2 系统边界中的地面高度曾经偏高。

在头显中进入：

`Quick Settings > Boundary`（快捷设置 > 边界）

重新执行：

`Set Floor Level`（设置地面高度）

并使用：

`Stationary Boundary`（固定边界）

**动机：** VRTrainJourney 是坐姿体验。正确的系统地面高度有助于后续判断座椅高度、相机位置和车厢空间尺度。该设置属于 Quest 2 系统边界，不是 Unity 项目参数。

### 3.23 创建最小 XR Origin 测试场景

第一次 APK 运行时，模板场景仍然使用普通固定相机，因此画面角度无法正确表现头显姿态。

在 Unity `Hierarchy`（层级）窗口中：

1. 删除模板自带的 `Main Camera`（主摄像机）。
2. 点击：

   `GameObject > XR > XR Origin (VR)`  
   （游戏对象 > XR > XR 原点（虚拟现实））

Unity 自动创建：

- `XR Origin (VR)`（XR 原点）
- `Camera Offset`（摄像机偏移）
- 新的 `Main Camera`（主摄像机）
- `Tracked Pose Driver`（追踪姿态驱动器）
- `XR Interaction Manager`（XR 交互管理器）

**动机：** 普通 Unity Camera 不会自动读取头显位置与旋转。`XR Origin (VR)` 中的新相机会通过 `Tracked Pose Driver` 读取 Quest 2 姿态，使画面跟随头部运动。

### 3.24 完成第二次真机验证

再次执行：

`File > Build Profiles > Build And Run`  
（文件 > 构建配置 > 构建并运行）

覆盖原有调试 APK。

真机验证结果：

- Quest 2 可以正常进入 Unity 应用。
- 左右转头时画面跟随自然。
- 抬头与低头时视角变化平滑。
- 以模板天空中的太阳为参照物，观察到头部追踪稳定。

**结论：** 建模前的 Unity 基础配置、Android 构建链路、OpenXR 接入、Quest 2 真机连接和最小头部追踪验证均已完成。

## 4. 当前最终配置摘要

### 4.1 Android 与 XR

| 设置项 | 当前结果 |
| --- | --- |
| Android 构建平台 | `Active` |
| OpenXR Android Loader | 已启用 |
| OpenXR Standalone Loader | 已启用 |
| Meta Quest Support | 已启用 |
| Oculus Touch Controller Profile for Android | 已启用 |
| Oculus Touch Controller Profile for Standalone | 已启用 |
| Latency Optimization for Android | `Prioritize Input Polling` |
| Android 图形接口 | 仅 `Vulkan` |
| Android 架构 | 仅 `ARM64` |
| Input System | `Input System Package (New)` |
| Scripting Backend | `IL2CPP` |

### 4.2 Quest 2 移动端 URP

| 设置项 | 当前结果 |
| --- | --- |
| HDR | 关闭 |
| MSAA | `2x` |
| Render Scale | `0.8` |
| Depth Texture | 关闭 |
| Opaque Texture | 关闭 |
| Rendering Path | `Forward` |
| Depth Priming | `Disabled` |
| 移动端后处理 | 关闭 |
| 主光源实时阴影 | 关闭 |
| 附加光源 | `Per Vertex` |
| 附加光源实时阴影 | 关闭 |
| 反射探针混合 | 关闭 |
| 反射探针盒体投影 | 关闭 |
| LOD Cross Fade | 关闭 |
| 阴影距离预留值 | `20` |
| 阴影级联 | `1` |
| 柔和阴影 | 关闭 |
| Use Adaptive Performance | 关闭 |

## 5. 当前可以保留的提示

### 5.1 Project Validation 中的 SSAO 黄色提醒

来源：

`PC_Renderer`（电脑端渲染器）中的 SSAO。

当前处理：

保留，不影响 Quest 2 Android 运行。移动端 `Mobile_Renderer` 中不存在 SSAO Renderer Feature。

### 5.2 Render Scale 的摄像机深度信息条

来源：

`Render Scale = 0.8`

当前处理：

保留。属于信息提示，不是错误。

### 5.3 Android Logcat 中的系统黄色和红色日志

来源：

Quest 2 系统服务、后台应用和 Android 运行环境。

当前处理：

不能仅凭颜色判断项目失败。排查时按包名过滤，并关注明确的崩溃栈、`FATAL EXCEPTION` 或 Unity 应用启动失败信息。

## 6. 后续开发时需要重新评估的项目

以下选项当前关闭是为了得到稳定的建模前基线，不代表永久禁用：

- 实时阴影。
- 后处理。
- 反射探针高级能力。
- LOD 渐变切换。
- 自适应性能提供器。
- `Starter Assets`（起步资源）。
- `XR Device Simulator`（XR 设备模拟器）。

车厢模型、材质、灯光和交互逻辑完成后，应以 Quest 2 真机帧率与画面效果为依据，逐项评估是否恢复。

## 7. 建模前状态

当前 `SampleScene`（示例场景）已经具备最小 XR 运行结构：

```text
SampleScene
├── Directional Light
├── Global Volume
├── XR Interaction Manager
└── XR Origin (VR)
    └── Camera Offset
        └── Main Camera
```

可以从此状态开始进入正式车厢建模与场景开发阶段。
