# VRTrainJourney2026 本窗口任务与改动日志

记录日期：2026-06-04  
项目路径：`D:\Apps\WorkSpace\JesinProg\VRDev\VRTrainJourney2026`

本文档用于记录本窗口内围绕视频播放、BGM、韩语语音播报、素材绑定、Editor 预览异常与项目清理所完成的工作。写法偏向交接说明，方便后续自己、Unity AI 或其他协作者快速理解当前状态。

---

## 1. 本轮工作的核心目标

本轮工作的主线是：在不破坏现有 XR、URP、OpenXR 和核心视频播放逻辑的前提下，把项目从“只有三站视频顺序播放”推进到“视频、BGM、韩语语音播报可以共同完成一次完整 Quest 2 体验”。

最终确认的音频范围是：

- 保留：三站 BGM。
- 保留：韩语语音播报。
- 取消：环境音、列车底噪、风声、水声、鸟鸣等环境层。

也就是说，项目当前正式音频素材只需要：

- `3` 段 BGM；
- `4` 段韩语语音播报；
- 不再需要环境音素材。

---

## 2. 已完成的音频系统接入

### 2.1 新增音频配置结构

新增脚本：

`Assets/Scripts/Journey/StationAudioProfile.cs`

它的作用是把每一站的音频内容做成 Inspector 可配置数据，而不是把音频文件名硬编码在流程脚本里。

每站可以配置：

- 站点名称；
- BGM Clip；
- 韩语语音 Clip；
- 音量；
- 淡入淡出时间；
- 语音延迟播放时间；
- 是否循环 BGM。

说明：脚本中仍保留了部分 ambience 字段，这是为了兼容之前已经建立的结构和 Inspector 序列化数据。但根据当前项目决策，`Ambience Clip` 保持为空，不再作为正式功能使用。

### 2.2 新增音频控制器

新增脚本：

`Assets/Scripts/Journey/JourneyAudioController.cs`

它的职责是监听 `JourneySequenceController` 的事件，然后自动播放对应的 BGM 和韩语语音。

当前逻辑是：

- 监听 `StationStarted`；
- 监听 `JourneyCompleted`；
- 监听 `PlaybackError`；
- 每站开始时切换对应 BGM；
- 每站开始后按配置延迟播放韩语语音；
- 第三站结束后播放终点语音，并淡出音频；
- 如果视频暂停，音频同步暂停；
- 如果视频继续，音频同步继续；
- 如果出现播放错误，音频停止或淡出，避免声音残留。

重点限制：

- 音频系统不直接控制 `VideoPlayer`；
- 音频系统只跟随 `JourneySequenceController` 的状态和事件；
- 后续加入手柄控制时，也应该继续调用 `JourneySequenceController` 的公开接口，而不是绕过它。

### 2.3 新增自动配置菜单

新增脚本：

`Assets/Scripts/Editor/JourneyAudioSystemSetup.cs`

Unity 菜单：

`Tools/VR Train Journey/Configure Audio System`

它的作用是帮助在场景中的 `JourneySystem` 上自动添加：

- `JourneyAudioController`；
- BGM AudioSource；
- 预留 Ambience AudioSource；
- Voice AudioSource；
- 三站基础配置数组。

注意：它不会自动生成音频素材，也不会伪造音频资源。真实素材仍然需要放入项目目录后绑定。

---

## 3. 当前音频素材目录与绑定关系

### 3.1 BGM 素材目录

正式 BGM 放置位置：

`Assets/Audio/BGM/`

当前使用的文件名：

- `BGM_Station01_GoldenVillage.wav`
- `BGM_Station02_FjordView.wav`
- `BGM_Station03_AuroraFlowerField.wav`

### 3.2 韩语语音目录

正式韩语语音放置位置：

`Assets/Audio/Voice/`

当前使用的文件名：

- `Voice_KO_Station01_GoldenVillage.wav`
- `Voice_KO_Station02_FjordView.wav`
- `Voice_KO_Station03_AuroraFlowerField.wav`
- `Voice_KO_JourneyCompleted.wav`

### 3.3 已绑定到场景

这些音频已经绑定到：

`Assets/Scenes/SampleScene.unity`

绑定对象是场景中的：

`JourneySystem`

其上挂载：

`JourneyAudioController`

后续如果替换素材，推荐保持相同文件名和路径。这样 Unity 通常会保留引用关系，不需要重新手动拖拽。

---

## 4. 韩语语音播报内容

项目面向韩国老人体验者，因此正式导入 Unity 的语音播报语言必须是韩语。中文只作为内部语义说明，不作为最终音频语言。

### 第一站：Golden Village

中文语义：

列车即将出发。现在进入金色乡村站，请放松欣赏温暖的乡村风景。

韩语 TTS 文案：

```text
이번 열차가 곧 출발합니다.
이제 황금빛 시골 마을 역으로 들어갑니다.
편안히 앉으셔서, 창밖의 따뜻한 시골 풍경을 천천히 감상해 주세요.
```

### 第二站：Fjord View

中文语义：

金色乡村站已到达。接下来前往峡湾观景站，窗外会看到山谷、瀑布和宽阔水面。

韩语 TTS 文案：

```text
황금빛 시골 마을 역에 도착했습니다.
다음 목적지는 피오르 전망 역입니다.
창밖을 바라보시면, 산골짜기와 폭포, 그리고 넓은 물길이 천천히 펼쳐집니다.
```

### 第三站：Aurora Flower Field

中文语义：

峡湾观景站已到达。接下来前往极光花田站，请慢慢欣赏夜空、柔光与花田。

韩语 TTS 文案：

```text
피오르 전망 역에 도착했습니다.
다음 목적지는 오로라 꽃밭 역입니다.
천천히 숨을 고르시면서, 밤하늘 아래의 부드러운 빛과 꽃밭을 감상해 주세요.
```

### 旅程结束

中文语义：

极光花田站已到达。本次旅程即将结束，请继续坐稳并慢慢休息。

韩语 TTS 文案：

```text
오로라 꽃밭 역에 도착했습니다.
이번 여행은 곧 마무리됩니다.
계속 편안히 앉아 계시고, 천천히 쉬어 주세요.
```

---

## 5. 视频文件处理记录

### 5.1 重新编码过的视频

为了尝试解决 Unity Editor 预览中第二站或第三站偶发卡住的问题，曾使用 `ffmpeg` 对三段正式视频进行了重新编码。

当前正式使用的视频仍位于：

`Assets/Videos/`

文件名保持不变：

- `Station01_GoldenVillage.mp4`
- `Station02_FjordView.mp4`
- `Station03_AuroraFlowerField.mp4`

重新编码后的目标格式：

- H.264；
- 1280x720；
- 30 fps；
- Quest 2 可播放；
- 文件名保持不变；
- 原来的 Unity `.meta` 引用不主动破坏。

### 5.2 原始视频备份已移出项目

原始视频备份目录原本在：

`Assets/Videos/_OriginalBackup_20260603`

后来根据项目清理需求，已经从项目中剪切到桌面：

`C:\Users\25142\Desktop\_OriginalBackup_20260603`

这意味着项目内已经不再保留这份备份目录，当前项目只保留正式使用的视频。

---

## 6. Unity Editor 预览卡住问题记录

### 6.1 问题现象

在 Unity Editor 的 Game 预览窗口中，三站视频播放偶尔会出现以下现象：

- 第一段正常播放；
- 第二段或第三段可能卡在第一帧；
- Console 中可能显示 `VideoPlayer.isPlaying=True`；
- 但 `time=0.00`、`frame=0` 或 `frame=-1` 长时间不前进；
- 音频仍然可以正常播放。

### 6.2 当前判断

由于 Quest 2 实机打包测试已经可以跑完整流程，因此当前把这个问题记录为：

Unity Editor 预览环境下的偶发 VideoPlayer 解码或播放头推进问题。

它目前不是 Quest 2 实机体验的阻塞项。

### 6.3 当前处理原则

已经明确不继续为了这个 Editor-only 问题大幅修改：

- XR 设置；
- URP 设置；
- OpenXR 设置；
- RenderTexture 架构；
- 核心视频播放流程；
- 三站顺序播放结构。

如果未来确实需要提升 Editor 预览稳定性，应另开独立任务处理，例如：

- 多 `VideoPlayer` 预热池；
- Editor 专用调试播放模式；
- 更细的视频资源加载诊断；
- 或专门为 Editor 做低风险兜底逻辑。

在当前阶段，不建议继续围绕这个问题改主流程。

---

## 7. 已更新的项目文档

本轮工作中同步更新过以下文档：

- `Assets/Docs/VRTrainJourney_音频系统设计与接入记录.md`
- `Assets/Docs/UnityAI_ProjectExecutionGuide.md`
- `Assets/Docs/VRTrainJourney_核心视频播放系统接入记录.md`
- `Assets/Docs/VRTrainJourney_项目承接说明.md`
- `Assets/Docs/项目企划书.md`

主要更新内容包括：

- 音频系统从“BGM、环境音、语音”调整为“BGM、韩语语音”；
- 明确环境音取消；
- 明确 Unity AI 后续不得修改视频系统内部逻辑；
- 记录 Editor 预览卡住但 Quest 2 实机可跑通的状态；
- 记录音频资源应如何绑定；
- 记录后续手柄控制应该接到现有旅程控制接口。

---

## 8. 当前剩余工作

从体验功能角度看，当前主要剩余两块：

### 8.1 手柄控制播放

建议新增独立输入控制脚本，让 Quest 2 手柄按钮调用：

- `JourneySequenceController.StartJourney()`
- `JourneySequenceController.TogglePause()`
- `JourneySequenceController.SkipToNextStation()`

不要让手柄输入脚本直接控制：

- `VideoPlayer`
- `AudioSource`
- RenderTexture

这样视频、BGM 和韩语语音仍然会通过现有事件系统自动同步。

### 8.2 最终视角流程确认

需要在 Quest 2 上确认：

- 初始视角是否正对车窗和主画面；
- 三站切换时黑色淡入淡出是否自然；
- BGM 是否不会盖住韩语播报；
- 语音出现时机是否舒服；
- 结束时是否停在预期状态；
- 老年体验者观看时是否容易理解、不过度刺激、不容易晕。

---

## 9. 后续协作注意事项

如果后续 Unity Editor 状态、Quest 2 连接状态、Build 状态会影响判断，应优先询问使用者当前状态，不要在不确定的情况下绕很大弯路。

尤其是以下情况，应先确认：

- Unity Editor 是否打开；
- 当前场景是否已经保存；
- Quest 2 是否连接；
- 是否允许重新 Build；
- 是否允许安装 APK；
- 是否允许改视频系统主流程；
- 是否允许删除或移动大型素材。

当前项目的正确方向是：优先保证 Quest 2 实机体验稳定，不为了 Editor 预览偶发现象过度扰动已经能跑通的主流程。
