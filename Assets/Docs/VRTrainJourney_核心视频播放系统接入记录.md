# VR Train Journey 核心视频播放系统接入记录

记录日期：2026-06-03  
适用工程：`VRTrainJourney2026`  
当前阶段：核心视频播放系统已接入，后续进入音频、语音播报与基础手柄交互阶段。

---

## 1. 本轮目标

本轮工作的目标是解决前窗幕布无法正常播放本地视频的问题，并建立一套可在 Quest 2 上稳定运行的视频播放链路。

最终方案：

```text
本地 MP4 视频文件
  ↓
VideoClip（视频片段资源）
  ↓
VideoPlayer（视频播放器组件）
  ↓
RenderTexture（渲染纹理，中转画布）
  ↓
URP Unlit Material（URP 无光照材质）
  ↓
FrontVideoScreen（前窗幕布）
```

黑场转场方案：

```text
JourneySequenceController（旅程流程控制）
  ↓
FadeTransitionController（黑场透明度控制）
  ↓
FrontVideoFadeOverlay（黑色遮罩平面）
```

---

## 2. 初始问题原因

一开始将视频直接拖到幕布上无法正常播放，核心原因不是视频一定不兼容，而是 Unity 中缺少完整播放链路。

主要问题包括：

1. `MP4` 在 Unity 中是 `VideoClip`，即“视频片段资源”，不会像普通图片贴图一样自动播放。
2. 幕布对象原本的材质引用失效，`FrontVideoScreen_Placeholder` 指向了工程中不存在的材质资源。
3. 场景中缺少 `VideoPlayer`，也就是“视频播放器组件”。
4. 场景中缺少 `RenderTexture`，也就是“渲染纹理/视频画面中转画布”。
5. 没有处理 `Prepare()`，即“视频预加载完成后再播放”的时序。
6. 没有黑场遮罩，站点之间直接切换容易出现闪烁、黑屏跳变或首帧不同步。

后续 Quest 2 真机日志证明，当前 H.264 视频能够通过 Qualcomm 硬件 AVC 解码器播放，因此最初问题主要是工程播放系统未搭建完整，而不是单纯的视频编码错误。

---

## 3. 引擎与工程设置变更

### 3.1 视频资源整理

视频资源目录：

```text
Assets/Videos/
```

三段视频文件：

```text
Station01_GoldenVillage.mp4
Station02_FjordView.mp4
Station03_AuroraFlowerField.mp4
```

用途：

- 作为 Unity 本地 `VideoClip` 使用。
- 不走 HTTP 流媒体。
- 不使用逐帧图片序列。
- 当前方案优先保证 Quest 2 离线稳定播放。

### 3.2 Git LFS 设置

修改文件：

```text
.gitattributes
```

新增规则：

```text
*.mp4 filter=lfs diff=lfs merge=lfs -text
```

说明：

- `Git LFS` 是 Git Large File Storage，即“大文件存储机制”。
- MP4 文件体积较大，应该由 Git LFS 管理，避免普通 Git 仓库快速膨胀。

### 3.3 RenderTexture 设置

新增资源：

```text
Assets/Art/RenderTextures/RT_FrontVideo_720p.renderTexture
```

设置目标：

| 项目 | 设置 |
| --- | --- |
| 分辨率 | `1280 x 720` |
| 深度缓冲 | 无 |
| 抗锯齿 | 无 |
| Mipmap | 关闭 |
| Wrap Mode（包裹模式） | Clamp（边缘夹紧） |
| Filter Mode（过滤模式） | Point（点采样） |

说明：

- `RenderTexture` 是“渲染纹理”，可以理解为视频播放器输出画面的中转画布。
- `VideoPlayer` 将视频画面输出到该纹理。
- 前窗幕布材质再读取该纹理并显示出来。

### 3.4 前窗视频材质

新增资源：

```text
Assets/Art/Materials/Mat_FrontVideo_Unlit.mat
```

说明：

- 使用 `URP Unlit` 材质。
- `URP` 是 Universal Render Pipeline，即“通用渲染管线”。
- `Unlit` 是“无光照材质”，不会被场景灯光影响。
- 视频屏幕使用无光照材质，可以避免车厢灯光改变视频亮度和色彩。

### 3.5 黑场遮罩材质

新增资源：

```text
Assets/Art/Materials/Mat_FrontVideoFade_Unlit.mat
```

说明：

- 用于站点切换时的黑场淡入淡出。
- 避免直接切换视频造成闪烁、突兀黑屏或首帧不同步。

### 3.6 SampleScene 场景调整

修改场景：

```text
Assets/Scenes/SampleScene.unity
```

主要调整：

1. 将前窗幕布对象整理为 `FrontVideoScreen`。
2. 修复幕布原本失效的材质引用。
3. 将视频材质 `Mat_FrontVideo_Unlit` 绑定到前窗幕布。
4. 关闭前窗幕布的投射阴影和接收阴影。
5. 新增 `FrontVideoFadeOverlay` 作为黑色遮罩平面。
6. 新增 `JourneySystem` 作为旅程播放控制对象。
7. 在 `JourneySystem` 上挂载 `VideoPlayer` 和播放控制脚本。

### 3.7 Unity AI 执行文档更新

修改文件：

```text
Assets/Docs/UnityAI_ProjectExecutionGuide.md
```

更新内容：

- 当前最高优先级切换为“核心视频播放系统接入”。
- 保留 Quest 2 轻量化约束。
- 保留 XR、URP、OpenXR 基线，不允许 Unity AI 擅自改动。
- 增加 Quest 2 真机验证状态。

术语说明：

- `XR`：扩展现实，包括 VR/AR/MR。
- `OpenXR`：开放式 XR 运行接口，是 Quest 2 运行链路的重要基础。
- `URP`：Unity 通用渲染管线。

---

## 4. 新增代码结构

```text
Assets/
  Scripts/
    Journey/
      JourneySequenceController.cs
      FadeTransitionController.cs
      JourneyDebugInput.cs

    Editor/
      JourneyVideoSystemSetup.cs
      JourneyAndroidDevelopmentBuild.cs
```

---

## 5. 脚本职责说明

### 5.1 JourneySequenceController.cs

路径：

```text
Assets/Scripts/Journey/JourneySequenceController.cs
```

职责：

- 管理三段视频的顺序播放。
- 第一站视频预热。
- 等待开始指令。
- 播放、暂停、继续。
- 跳到下一站。
- 每站结束后进入黑场转场。
- 第三站结束后进入完成状态。
- 处理视频准备超时。
- 处理播放器错误。
- 输出统一日志前缀 `[VRTrainJourney.Video]`。

公开接口：

| 接口 | 作用 |
| --- | --- |
| `StartJourney()` | 开始旅程。若旅程已经完成，再次调用会从第一站重启。 |
| `TogglePause()` | 暂停或继续当前视频。 |
| `SkipToNextStation()` | 通过黑场切换到下一站。 |
| `CurrentStationIndex` | 获取当前站点索引。 |
| `State` | 获取当前播放状态。 |
| `StationStarted` | 站点开始事件，供后续旁白、BGM、手柄系统监听。 |
| `JourneyCompleted` | 旅程完成事件。 |
| `PlaybackError` | 播放错误事件。 |

术语说明：

- `Event` 是“事件通知接口”，用于告诉其他系统某件事发生了。
- 后续音频系统应该监听这些事件，而不是直接改视频播放器内部逻辑。

### 5.2 FadeTransitionController.cs

路径：

```text
Assets/Scripts/Journey/FadeTransitionController.cs
```

职责：

- 控制 `FrontVideoFadeOverlay` 的透明度。
- 实现淡出到黑、黑场停留、再淡入画面的过渡。
- 使用 `MaterialPropertyBlock` 修改材质参数。

术语说明：

- `MaterialPropertyBlock` 是“材质属性块”。
- 它可以只修改某个物体的显示参数，不会在运行时复制一份新材质。
- 这种方式更适合 Quest 2，因为它更稳、更轻量。

### 5.3 JourneyDebugInput.cs

路径：

```text
Assets/Scripts/Journey/JourneyDebugInput.cs
```

职责：

- 仅用于 Unity 编辑器和 Development Build 测试。
- 提供键盘开始播放。
- 提供键盘暂停/继续。
- 提供键盘跳到下一站。
- 在 Development Build 中支持自动开始播放，方便 Quest 2 真机验证。

术语说明：

- `Development Build` 是“开发构建版本”，用于测试和抓日志，不是最终发布版。

### 5.4 JourneyVideoSystemSetup.cs

路径：

```text
Assets/Scripts/Editor/JourneyVideoSystemSetup.cs
```

职责：

- 在 Unity 菜单中提供 `Tools/VR Train Journey/Configure Video System`。
- 自动创建或检查 `RenderTexture`。
- 自动创建视频材质和黑场材质。
- 自动绑定前窗幕布。
- 自动创建 `JourneySystem`。
- 自动挂载 `VideoPlayer`、播放控制器、黑场控制器和测试输入脚本。
- 自动绑定三段视频。

术语说明：

- `Editor Script` 是“编辑器脚本”，只用于 Unity 编辑器配置工程。
- 它不是玩家运行时体验的一部分。

### 5.5 JourneyAndroidDevelopmentBuild.cs

路径：

```text
Assets/Scripts/Editor/JourneyAndroidDevelopmentBuild.cs
```

职责：

- 在 Unity 菜单中提供 `Tools/VR Train Journey/Build Quest 2 Development APK`。
- 检查关键资产是否存在。
- 构建 Android Development APK。
- 输出 Quest 2 测试包。

输出路径：

```text
Builds/Android/VRTrainJourney2026_Debug.apk
```

术语说明：

- `APK` 是 Android 安装包。
- Quest 2 底层运行 Android 系统，因此 Unity 项目构建后以 APK 形式安装到设备上。

---

## 6. Quest 2 验证记录

当前已完成：

- APK 成功构建。
- APK 成功安装到 Quest 2。
- 三段本地视频可以按顺序播放。
- 第三站结束后进入完成状态。
- Android Logcat 中可看到 Qualcomm 硬件 AVC 解码器：

```text
OMX.qcom.video.decoder.avc
```

说明：

- `Android Logcat` 是 Android 日志查看工具。
- `AVC` 是 H.264 视频编码的一种常见名称。
- 当前日志说明 Quest 2 能够使用硬件解码器播放当前视频素材。

仍建议后续作为回归测试补做：

1. 连续三轮完整播放，不让头显休眠。
2. 测试暂停、继续、跳站。
3. 接入音频后检查音画同步。
4. 接入手柄后检查交互是否影响视频播放状态。

---

## 7. 后续音频与交互开发规范

### 7.1 音频系统不要反向控制视频系统

视频系统仍然作为旅程主流程。

后续韩语语音播报、BGM 和环境音应该监听：

```text
StationStarted
JourneyCompleted
PlaybackError
```

不要让音频脚本直接修改 `VideoPlayer` 的内部播放逻辑。

### 7.2 每站音频应按站点配置

建议后续建立类似 `StationAudioProfile` 的配置资产。

每站可配置：

- 韩语旁白音频。
- BGM。
- 环境音。
- 音量。
- 淡入时间。
- 淡出时间。
- 是否循环。

这样后续新增站点或替换音频时，不需要改核心播放代码。

### 7.3 语音播报语言规范

正式语音播报必须使用韩语，因为本项目面向韩国老人体验者。

规范：

- 中文只用于项目内部说明、策划文档和语义对照。
- 最终导入 Unity 的语音素材应为韩语音频。
- 后续生成语音素材时，应先写出中文语义，再转写为自然、礼貌、语速偏慢的韩语播报文本。
- 韩语播报应避免过长句子，尽量使用清晰、安定、容易理解的表达。
- 如果使用 `TTS`，即 Text To Speech，“文本转语音”，应选择温和、清楚、适合老年听众的韩语声音。
- BGM 和环境音不涉及语言，可按每站氛围自由设计。

### 7.4 黑场与音频淡入淡出要分开

视觉黑场转场不应该直接等同于音频转场。

建议：

- 视频站点结束前，BGM 可提前淡出。
- 黑场期间可保留环境底噪。
- 下一站开始后，旁白可以延迟进入。
- 不要把视频黑场时长和音频淡入淡出时间强行绑定。

### 7.5 Quest 2 音频资源建议

建议原则：

| 音频类型 | 建议 |
| --- | --- |
| 短提示音 | 可使用 Decompress On Load，即“加载时解压”，反应更快。 |
| BGM | 可使用 Compressed In Memory，即“内存中压缩”，节省内存。 |
| 长旁白 | 可考虑 Streaming，即“流式读取”，减少一次性内存占用。 |
| 环境循环音 | 优先压缩，注意循环点是否平滑。 |

具体设置应在音频文件导入后再按体积和效果调整。

### 7.6 日志规范

视频日志前缀：

```text
[VRTrainJourney.Video]
```

后续音频建议使用：

```text
[VRTrainJourney.Audio]
```

后续手柄交互建议使用：

```text
[VRTrainJourney.Input]
```

这样可以在 Android Logcat 中按系统筛选问题。

### 7.7 不允许擅自改动基础运行链路

后续接入音频、手柄、UI 或虚拟手柄时，仍应遵守：

- 不擅自更换 XR 插件。
- 不擅自更换 OpenXR 基线。
- 不擅自更换 URP 配置。
- 不擅自引入高开销后处理。
- 不在 Quest 2 上增加不必要的实时光照、阴影和复杂透明叠层。

---

## 8. 当前距离完整版的剩余模块

从当前项目状态看，核心视频播放链路已经打通。距离基础完整版，主要还缺：

1. BGM 系统。
2. 韩语语音播报系统。
3. 手柄控制暂停、继续、切换下一站。
4. 可选的虚拟手柄可视化。
5. 最终 Quest 2 完整回归测试。

其中前三项是基础完整版必须项。第四项“虚拟手柄可视化”属于体验增强项，不一定是核心功能必须项。

---

## 9. 虚拟手柄可视化工作量判断

如果只是实现“能用手柄按钮控制暂停和切换下一站”，工作量较小。

原因：

- 当前 `JourneySequenceController` 已经提供 `TogglePause()` 和 `SkipToNextStation()`。
- 后续只需要新增输入绑定脚本，将 Quest 2 手柄按钮映射到这些接口。

如果要在视线中看到虚拟手柄模型，工作量会变大，取决于目标精度。

### 9.1 低成本方案

目标：

- 显示简单的左右手控制器模型。
- 不做复杂手部动画。
- 只用于告诉玩家“手柄在这里”。

预估工作量：

- 小到中等。
- 主要工作在 XR Controller 模型、材质、跟踪绑定和 Quest 2 真机验证。

适合当前阶段。

### 9.2 中等方案

目标：

- 显示 Meta Quest 风格控制器。
- 按钮触发时有简单高亮。
- 暂停/下一站操作时有视觉反馈。

预估工作量：

- 中等。
- 需要输入系统、模型表现、按钮状态反馈和测试。

### 9.3 高成本方案

目标：

- 显示完整虚拟手。
- 支持手势、抓握、指向、按钮动画。
- 与座舱内 UI 或物体交互。

预估工作量：

- 较大。
- 会牵涉 XR Interaction Toolkit、手部追踪、交互射线、UI 事件、性能优化等。

当前不建议一开始就做这个版本。

---

## 10. 推荐后续开发顺序

建议顺序：

1. 接入手柄基础输入：暂停、继续、下一站。
2. 接入 BGM 和站点环境音。
3. 接入韩语语音播报。
4. 做音画同步和 Quest 2 真机回归测试。
5. 再决定是否加入虚拟手柄模型。
6. 若加入虚拟手柄，优先做低成本控制器模型显示，不先做复杂虚拟手。

推荐理由：

- 当前最重要的是让体验流程完整。
- 暂停和下一站交互可以直接复用现有视频控制 API。
- 音频系统可以监听已有旅程事件。
- 虚拟手柄属于表现增强，可以后置，避免过早扩大工程复杂度。
