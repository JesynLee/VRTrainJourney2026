# VR Train Journey Experience：Unity 6 + Quest 2 初步配置决策

> 文档日期：2026-05-31  
> 项目目录：`D:\Apps\WorkSpace\JesinProg\VRDev\VRTrainJourney2026`  
> 目标设备：Meta Quest 2  
> 项目类型：固定坐姿、前向观景、低交互 VR 课程 Demo

---

## 1. 配置目标

第一阶段只建立一个稳定、容易排错的 VR 基础环境：

1. 使用 Unity 6 LTS、URP、OpenXR 和 XR Interaction Toolkit。
2. 能够将一个最小场景打包并运行在 Quest 2 上。
3. 能够追踪头显和 Quest 2 手柄输入。
4. 为后续车厢场景、视频播放、语音提示和四项手柄操作预留清晰结构。
5. 暂时不引入没有实际需求的 Meta 专属 SDK、复杂 UI 或高成本画面效果。

---

## 2. 已确认的技术决策

| 项目 | 决策 | 原因 |
|---|---|---|
| Unity | `6000.3.16f1 LTS` | 当前项目实际版本 |
| 项目模板 | `Core > Universal 3D` | 已创建，适合 URP 新项目 |
| 渲染管线 | URP | 适合 Quest 2 移动 VR，性能配置较容易控制 |
| XR 运行时 | OpenXR | Unity 推荐的长期路线，避免依赖旧版 Oculus XR Plugin |
| 交互框架 | XR Interaction Toolkit | 足够支持基础手柄输入和 XR Origin |
| 输入系统 | Input System Package (New) | XR Interaction Toolkit 的推荐输入路线 |
| 构建平台 | Android | Quest 2 是 Android 独立头显 |
| 图形 API | Vulkan 优先 | 先统一为移动 VR 主路线；完成 Validation 和实机测试后再决定是否保留 OpenGL ES 作为回退 |
| 构建后端 | IL2CPP | 当前项目已经配置 |
| CPU 架构 | ARM64 | 当前项目已经配置 |
| Android 包名 | `com.jesyn.vrtrainjourney2026` | 替换模板默认包名，避免后续 APK 标识混乱 |
| XR 场景 | 固定坐姿 XR Origin | 用户可以自然转头，但不提供自由移动 |
| 视频 | H.264 MP4，先测试 `1600x900`、30 fps | 先保证 Quest 2 播放稳定，再尝试 `1920x1080` |

### 暂时不安装的内容

| 内容 | 当前决策 | 何时再考虑 |
|---|---|---|
| Unity Meta OpenXR | 暂不安装 | 需要 Passthrough、MR 或 Meta 专属扩展时 |
| Meta XR SDK | 暂不安装 | OpenXR + XRI 无法满足明确需求时 |
| Oculus XR Plugin | 不使用 | 旧路线，不应与当前 OpenXR 主方案混用 |
| Hands Interaction Demo | 不导入 | 项目没有手势交互需求 |
| 复杂后处理 | 不启用 | 只有实机性能稳定且确有视觉收益时 |

---

## 3. 当前项目审计结果

### 3.1 已安装依赖

当前 `Packages/manifest.json` 已包含：

```text
com.unity.render-pipelines.universal
com.unity.inputsystem
com.unity.modules.video
```

说明：

- URP 已安装并挂接到 Graphics Settings。
- Input System 已安装。
- Unity 内置 VideoPlayer 模块已存在，不需要额外安装视频插件。

### 3.2 尚未安装的必要依赖

需要通过 Unity Package Manager 安装：

```text
XR Plugin Management
OpenXR Plugin
XR Interaction Toolkit
```

底层依赖让 Unity 自动解析，不要手动逐个添加版本号。

### 3.3 当前移动端 URP Asset 状态

文件：`Assets/Settings/Mobile_RPAsset.asset`

| 设置 | 当前值 | 第一阶段建议 |
|---|---:|---:|
| Renderer | Forward | 保持 |
| HDR | 开启 | 关闭 |
| MSAA | 1x | 调整为 2x |
| Render Scale | `0.8` | 保持，清晰度不足时再提高到 `1.0` |
| Opaque Texture | 关闭 | 保持 |
| Depth Texture | 关闭 | 保持 |
| Main Light Shadows | 开启 | 初期关闭或严格限制 |
| Additional Lights | 开启 | 尽量减少 |

### 3.4 当前 Android 基础状态

| 设置 | 当前状态 | 处理 |
|---|---|---|
| Android 质量等级 | 已指向 `Mobile` | 保持 |
| Scripting Backend | 已是 IL2CPP | 保持 |
| CPU 架构 | 已是 ARM64 | 保持 |
| Active Input Handling | 已是 Input System Package (New) | 保持 |
| Package Name | 仍为 Universal 3D 模板默认值 | 改为 `com.jesyn.vrtrainjourney2026` |

---

## 4. 后续脚本接入顺序

视频最小闭环通过后，再按顺序加入脚本：

```text
JourneySequenceController
StationAnnouncementController
FadeTransitionController
VRInputController
SeatedViewInitializer
AudioMixController
```

四项手柄操作保持简单：

| 操作 | 目标方法 |
|---|---|
| 开始体验 | `JourneySequenceController.StartJourney()` |
| 暂停 / 继续 | `JourneySequenceController.TogglePause()` |
| 下一站 | `JourneySequenceController.SkipToNextStation()` |
| 重新居中 | `SeatedViewInitializer.RecenterView()` |

重新居中只调整起始方向和座位基准，不应锁死头显旋转。

---

## 5. 推荐安装的辅助工具

| 工具 | 是否建议 | 用途 |
|---|---|---|
| XR Device Simulator Sample | 可选 | 不戴头显时做基础输入调试 |
| Android Logcat Package | 建议 | 查看 Quest 2 真机日志 |
| Starter Assets Sample | 必须 | 快速获得 XRI Input Actions 和 Preset |

---

## 6. 第一阶段完成标准

完成下列事项后，再进入三站旅程脚本开发：

- [ ] Android SDK、NDK、OpenJDK 已通过 Unity Hub 安装。
- [ ] XR Plugin Management、OpenXR Plugin、XR Interaction Toolkit 已安装。
- [ ] Starter Assets 已导入。
- [ ] Android OpenXR 已启用。
- [ ] Oculus Touch Controller Profile 已添加。
- [ ] Project Validation 没有红色错误。
- [ ] Android 使用 IL2CPP、ARM64、Input System。
- [ ] 移动端 URP 已关闭 HDR，使用 Forward 和 2x MSAA。
- [ ] XRBootstrapTest 已在 Quest 2 上运行。
- [ ] 头显转动和左右手柄追踪正常。
- [ ] MainJourneyScene 已能播放一段窗外测试视频。

---

## 7. 官方参考资料

- [Unity Manual：Develop for Meta Quest workflow](https://docs.unity3d.com/current/Documentation/Manual/xr-meta-quest-develop.html)
- [Unity Manual：Install Android dependencies](https://docs.unity3d.com/current/Documentation/Manual/android-install-dependencies.html)
- [Unity Manual：XR](https://docs.unity3d.com/current/Documentation/Manual/XR.html)
- [Unity Package Docs：XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest)
- [Unity Package Docs：XR Device Simulator](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest/manual/xr-device-simulator.html)
