# VR Train Journey Experience 项目承接说明

我正在开发一个大学课程 Demo 项目，项目名为 **VR Train Journey Experience**。项目面向老年用户，是一个基于 VR 头显的列车旅行体验。之前我使用 Unity 2022 LTS 开发过旧版本，但因为需要使用 Unity 最新 AI 功能，并且旧项目在新引擎中打开后出现材质异常，所以我决定删除旧项目，使用 **Unity 6000.3.16f1 LTS** 重新创建一个干净的新项目。

当前目标不是恢复旧项目，而是基于旧项目经验和项目企划书，重新搭建一个可完成的 VR Demo。

## 当前已确认的核心方向

### 项目定位

- 这是一个课程 Demo，不是正式商业项目。
- 项目由我一个人负责，不再保留原来的团队分工。
- 完成度以“可展示、可运行、逻辑清楚”为优先。
- 不需要复杂首页动画或大型 UI 系统。
- 需要保留必要的 VR 手柄交互，不能完全没有交互。

### 技术栈

新项目使用：

- Unity：`6000.3.16f1 LTS`
- 渲染管线：`URP / Universal Render Pipeline`
- XR 框架：`XR Interaction Toolkit + OpenXR`
- 目标设备：`Meta Quest 2`
- 图形 API：优先 `Vulkan`
- 项目模板：Unity Hub 中的 **Core → Universal 3D**
- 不使用 Built-in Render Pipeline 作为新项目主方案。
- 不使用 HDRP，因为 Quest 2 性能压力过大。

### Unity Hub 创建项目选择

创建新项目时应选择：

- 分类：`Core`
- 模板：`Universal 3D`
- 项目名：`VRTrainJourney`
- 位置：`D:\Apps\WorkSpace\JesinProg\VRDev`
- Source control provider：可暂时不选

不要选择：

- `High Definition 3D`
- `Universal 2D`
- `Sample`
- `Learning`

## 企划书文件位置

项目计划书位于：

`D:\Jesyn_Obsidian_Vault\项目开发日志\VR Train Journey Experience\项目企划以及实时策略\项目企划书.md`

这个文件已经被多次更新，目前版本应为：

`v3.4（Unity 6 URP 与 OpenXR 技术栈确认版）`

计划书里已经整合了以下内容：

- Unity 6 新项目重建原因
- URP + OpenXR + XR Interaction Toolkit 的技术方案
- Quest 2 的性能设置原则
- 三站式视频体验结构
- mentoring 反馈
- 站点语音提示
- 视频切换和过渡逻辑
- VR 手柄交互设计
- 脚本模块设计计划
- 前向二层观景车厢视角逻辑

## 设计理念重点

这个项目不是传统侧窗看风景的列车体验，而是：

**用户坐在列车二层最前端，通过前方大窗观看风景。**

视角逻辑类似开车时看前挡风玻璃：

- 风景从远处向近处推进。
- 用户主要看正前方，而不是侧窗。
- 这种设计是为了提高老年用户的视觉舒适度。
- 二层前端设定用于避免一层驾驶室、控制台等复杂元素。
- 车厢内部只需要基础结构即可，不追求复杂真实内饰。

关于铁轨：

- 旧企划中曾写过“通过裁切避免铁轨和车体结构出现”，这是不合理的。
- 新方案允许 AI 视频中出现铁轨。
- 铁轨是 VR 列车旅行的必要视觉元素。
- 需要避免的是复杂驾驶室、控制设备、过近的车头机械结构，而不是铁轨本身。

真实观景列车：

- 可以作为概念灵感。
- 但不能作为 AI 视频生成的直接参考，因为真实观景列车多数是侧窗观景。
- 本项目强调“二层前向全景观景”。

## 视频体验结构

项目包含 3 个主题视频，也就是 3 个站点：

1. 金色乡村站
2. 峡湾观景站
3. 极光花田站

每段主题视频约 45 秒。

用户反馈重点来自课程合作的老年人福利中心 mentoring：

- 老年用户需要知道“现在到了哪里”。
- 所以每到一个站，需要有语音提示。
- 站点之间需要有过渡感，不能像三个视频硬切。
- 实际项目中语音提示会使用韩语。
- 项目计划书中可以使用中文描述。

视频生成模型已经确定为：

`Seedance 2.0`

## 视频与音频素材组织策略

不要把所有内容提前剪成一个完整长视频。

推荐方式：

- 每个站点视频单独生成和剪辑。
- 语音、BGM、环境音尽量作为独立音频文件导入 Unity。
- Unity 用脚本控制播放顺序、过渡、语音提示、暂停、跳站。

推荐素材结构示例：

```text
Assets/
  Videos/
    Station01_GoldenVillage.mp4
    Station02_FjordView.mp4
    Station03_AuroraFlowerField.mp4

  Audio/
    Voice/
      Station01_Arrived.wav
      Station02_Arrived.wav
      Station03_Arrived.wav

    BGM/
      Journey_BGM.wav

    SFX/
      Train_Ambience_Loop.wav
```

视频剪辑软件目前未决定。  
音频编辑软件目前未决定。  
BGM 是否必须仍可讨论，但目前倾向于需要轻量 BGM 或环境氛围音。

## 画面比例与分辨率

视频窗口可以采用 `16:9`，因为它对应的是车厢前方大窗/视频播放平面的比例，不是 VR 头显本身的显示比例。

推荐：

- 先测试 `1600x900`
- 如果 Quest 2 性能稳定，再使用 `1920x1080`
- 不强制必须一开始就用 `1920x1080`
- 建议 30fps
- H.264 MP4 优先

## VR 舒适度与坐姿视角

项目是固定坐姿体验。

需要一个脚本或逻辑用于：

- 初始化用户座位视角
- 设置 XR Origin 的起始位置
- 提供“居中视角 / Recenter”功能
- 保持自然头部转动

注意：

- 不应该锁死 VR 摄像机旋转。
- 用户仍然应该可以自然转头。
- 固定坐姿脚本不是强行固定头显，而是管理起始位置和居中逻辑。

建议脚本名：

`SeatedViewInitializer` 或 `VRSeatedViewController`

## 手柄交互需求

虽然是 Demo，但需要保留基础手柄交互。

需要支持：

1. 开始体验
2. 居中视角
3. 暂停 / 继续
4. 跳到下一站

这些交互应该用脚本实现。

手柄输入脚本与视频控制脚本有联系，但职责不同：

- `VRInputController`
  - 只负责读取手柄按钮输入。
  - 调用其他控制器的方法。

- `JourneySequenceController`
  - 控制体验流程。
  - 管理三段视频播放顺序。
  - 处理开始、暂停、继续、跳站。

- `FadeTransitionController`
  - 控制站点之间的淡入淡出。
  - 负责过渡感。

- `StationAnnouncementController`
  - 控制站点到达语音提示。

- `AudioMixController`
  - 控制 BGM、环境音、语音提示之间的音量关系。
  - 语音播放时可降低 BGM 音量。

- `SeatedViewInitializer`
  - 负责坐姿视角初始化和居中。

## URP / Quest 2 性能设置原则

新项目创建后，应按照 Quest 2 的移动 VR 性能限制来配置 URP：

- Graphics API：优先 Vulkan
- URP Renderer：Forward Renderer
- 避免 Forward+
- HDR：关闭
- Post-processing：关闭或最低限度
- MSAA：先用 2x，性能允许再尝试 4x
- Render Scale：从 1.0 开始测试
- Opaque Texture：非必要关闭
- Depth Texture：非必要关闭
- Real-time shadows：尽量减少或关闭
- 车厢模型：基础几何体 + 简单材质
- 视频分辨率：先测试 1600x900 或 1920x1080，再根据 Quest 2 实机性能调整

## 新项目创建后的下一步

新窗口需要帮我继续做这些事：

1. 检查新 Unity 项目的文件结构。
2. 确认 `manifest.json` 中是否已经包含：
   - `com.unity.render-pipelines.universal`
   - `com.unity.xr.interaction.toolkit`
   - `com.unity.xr.management`
   - `com.unity.xr.openxr`
3. 如果没有，指导我在 Unity Package Manager 中安装。
4. 指导我配置：
   - Android Build Support
   - XR Plugin Management
   - OpenXR
   - Quest 2 相关设置
   - XR Interaction Toolkit Starter Assets
5. 帮我建立基础项目目录结构。
6. 之后再根据企划书创建必要的 C# 脚本框架。
7. 暂时不要实现复杂功能，先确保项目架构清楚、可运行、适合课程 Demo。

## 工作方式要求

请用中文和我沟通。  
请优先保证方案简单、稳定、能完成。  
不要过度设计。  
如果需要改项目计划书，请先说明会改哪里，再编辑：

`D:\Jesyn_Obsidian_Vault\项目开发日志\VR Train Journey Experience\项目企划以及实时策略\项目企划书.md`

如果需要操作 Unity 项目，请先读取项目文件结构，再给出或执行下一步。

## 给新窗口的开场提示

可以在新窗口直接发送：

```markdown
请你根据这份承接说明继续协助我配置新 Unity 项目。
```
