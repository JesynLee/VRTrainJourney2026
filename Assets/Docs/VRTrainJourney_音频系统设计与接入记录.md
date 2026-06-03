# VR Train Journey 音频系统设计与接入记录

记录日期：2026-06-03  
适用工程：`VRTrainJourney2026`  
当前阶段：核心视频播放系统已完成，本轮接入 BGM、环境音和韩语语音播报的可配置播放系统。

---

## 1. 接入原则

音频系统只监听旅程流程，不反向控制视频系统。

必须遵守：

- `JourneySequenceController` 仍然是三站旅程主流程。
- 音频系统监听 `StationStarted`、`JourneyCompleted`、`PlaybackError`。
- 音频系统不直接修改 `VideoPlayer`、RenderTexture、URP 材质或 OpenXR 设置。
- 暂停和继续通过读取 `JourneySequenceController.State` 同步。
- 正式语音播报必须使用韩语，中文只作为内部语义说明。

日志前缀：

```text
[VRTrainJourney.Audio]
```

---

## 2. 新增脚本结构

```text
Assets/
  Scripts/
    Journey/
      StationAudioProfile.cs
      JourneyAudioController.cs

    Editor/
      JourneyAudioSystemSetup.cs
```

### 2.1 StationAudioProfile.cs

用途：每站音频配置。

可配置内容：

- 站点名。
- BGM 音频。
- 环境音音频。
- 韩语语音播报音频。
- BGM 音量。
- 环境音音量。
- 语音音量。
- 淡入时间。
- 淡出时间。
- 语音延迟时间。
- BGM 是否循环。
- 环境音是否循环。

说明：

- `StationAudioProfile` 使用 `[Serializable]`，直接显示在 `JourneyAudioController` 的 Inspector 中。
- 后续替换音频素材时，只需要替换对应 `AudioClip`，不需要修改流程代码。

### 2.2 JourneyAudioController.cs

用途：运行时音频控制器。

职责：

- 持有三个 `AudioSource`：`BGM`、`Ambience`、`Voice`。
- 监听 `JourneySequenceController.StationStarted(int stationIndex)`。
- 按站点索引读取 `StationAudioProfile`。
- 每站播放对应 BGM、环境音和韩语语音。
- 语音播放时自动压低 BGM 和环境音。
- 视频暂停时同步暂停音频。
- 视频继续时同步恢复音频。
- 第三站完成后整体淡出音频。
- 视频报错时停止音频。

### 2.3 JourneyAudioSystemSetup.cs

用途：Unity 编辑器配置入口。

菜单路径：

```text
Tools/VR Train Journey/Configure Audio System
```

执行后：

- 查找现有 `JourneySystem`。
- 检查 `JourneySequenceController` 是否存在。
- 添加或复用 `JourneyAudioController`。
- 添加或复用三个 `AudioSource`。
- 写入三站默认 `StationAudioProfile`。
- 不创建假的音频素材，不伪装真实资源。

---

## 3. 三站音频设计

### 3.1 第一站：Golden Village / 金色乡村站

情绪目标：温暖、熟悉、安全、被欢迎。

| 项目 | 建议 |
| --- | --- |
| BGM 风格 | 慢速、温暖、轻弦乐、柔和钢琴，可少量手风琴点缀。 |
| 环境音方向 | 低速列车行驶声、轻微轨道节奏、午后微风、远处鸟鸣。 |
| 出现时机 | 第一站视频开始后 BGM 和环境音淡入，语音延迟约 1.2 秒进入。 |
| 音量建议 | BGM `0.24`，环境音 `0.18`，语音 `0.95`。 |
| 淡入淡出 | 淡入约 `2.0s`，离站淡出约 `2.0s`。 |

中文语义：

```text
本次列车即将出发。现在我们将进入金色乡村站。请放松坐好，慢慢欣赏窗外温暖的乡村风景。
```

韩语 TTS 文案：

```text
이번 열차가 곧 출발합니다.
이제 황금빛 시골 마을 역으로 들어갑니다.
편안히 앉으셔서, 창밖의 따뜻한 시골 풍경을 천천히 감상해 주세요.
```

### 3.2 第二站：Fjord View / 峡湾观景站

情绪目标：开阔、震撼但稳定、真实世界的辽阔感。

| 项目 | 建议 |
| --- | --- |
| BGM 风格 | 宽广、稳定、低冲击的氛围管弦乐或柔和 pad，不使用强鼓点。 |
| 环境音方向 | 列车低频行驶声、峡湾风声、远处瀑布水声、空间感较大的山谷回响。 |
| 出现时机 | 第二站开始时淡入，语音延迟约 1.2 秒进入。 |
| 音量建议 | BGM `0.22`，环境音 `0.18`，语音 `0.95`。 |
| 淡入淡出 | 淡入约 `2.25s`，离站淡出约 `2.25s`。 |

中文语义：

```text
金色乡村站已经到达。接下来列车将前往峡湾观景站。请看向窗外，山谷、瀑布和宽阔的水面会慢慢展开。
```

韩语 TTS 文案：

```text
황금빛 시골 마을 역에 도착했습니다.
다음 목적지는 피오르 전망 역입니다.
창밖을 바라보시면, 산골짜기와 폭포, 그리고 넓은 물길이 천천히 펼쳐집니다.
```

### 3.3 第三站：Aurora Flower Field / 极光花田站

情绪目标：安静、平和、精神安抚、温柔收尾。

| 项目 | 建议 |
| --- | --- |
| BGM 风格 | 空灵、安静、冥想钢琴、轻柔合成器 pad，避免强节奏。 |
| 环境音方向 | 更轻的列车底噪、夜风、柔和花田风声，不加入刺耳虫鸣。 |
| 出现时机 | 第三站开始时淡入，语音延迟约 1.4 秒进入；终点时整体淡出。 |
| 音量建议 | BGM `0.20`，环境音 `0.15`，语音 `0.95`。 |
| 淡入淡出 | 淡入约 `2.5s`，站内淡出约 `3.0s`，旅程完成淡出约 `4.0s`。 |

中文语义：

```text
峡湾观景站已经到达。接下来列车将前往极光花田站。请慢慢呼吸，欣赏夜空下柔和的光与花田。
```

韩语 TTS 文案：

```text
피오르 전망 역에 도착했습니다.
다음 목적지는 오로라 꽃밭 역입니다.
천천히 숨을 고르시면서, 밤하늘 아래의 부드러운 빛과 꽃밭을 감상해 주세요.
```

终点中文语义：

```text
极光花田站已经到达。本次旅程即将结束。请继续坐稳，慢慢休息。
```

终点韩语 TTS 文案：

```text
오로라 꽃밭 역에 도착했습니다.
이번 여행은 곧 마무리됩니다.
계속 편안히 앉아 계시고, 천천히 쉬어 주세요.
```

---

## 4. 音频素材生成提示词

### 4.1 BGM 生成提示词

第一站 BGM：

```text
Warm slow instrumental background music for a gentle scenic train journey through a golden European countryside, soft strings, warm piano, subtle accordion color, nostalgic but not sad, calm tempo, no drums, no vocals, seamless loop, suitable for elderly VR users.
```

中文解释：

- `Warm slow instrumental background music`：温暖、慢速、纯音乐背景。
- `soft strings`：柔和弦乐。
- `warm piano`：温暖钢琴。
- `subtle accordion color`：轻微手风琴色彩。
- `no drums, no vocals`：不要鼓点和人声，避免干扰韩语播报。
- `seamless loop`：可无缝循环。

第二站 BGM：

```text
Wide cinematic ambient orchestral music for a calm train ride through Norwegian fjords, spacious pads, gentle low strings, slow harmonic movement, majestic but stable, no percussion impact, no vocals, seamless loop, safe and comfortable for elderly VR listeners.
```

中文解释：

- `wide cinematic ambient orchestral music`：开阔的电影感氛围管弦乐。
- `spacious pads`：有空间感的合成器铺底。
- `gentle low strings`：柔和低音弦乐。
- `majestic but stable`：有壮阔感但保持稳定。
- `no percussion impact`：不要强冲击打击乐。

第三站 BGM：

```text
Peaceful ethereal ambient music for a night train passing through an aurora flower field, soft meditation piano, gentle synth pads, slow breathing rhythm, dreamlike but warm, no drums, no vocals, seamless loop, calming ending atmosphere.
```

中文解释：

- `peaceful ethereal ambient music`：平和、空灵的氛围音乐。
- `soft meditation piano`：柔和冥想钢琴。
- `gentle synth pads`：轻柔合成器铺底。
- `slow breathing rhythm`：像慢呼吸一样的节奏感。
- `calming ending atmosphere`：适合收尾的安定氛围。

### 4.2 环境音生成提示词

第一站环境音：

```text
Loopable ambience for a slow scenic train ride in the afternoon countryside, gentle rail rhythm, soft cabin vibration, light warm breeze, distant small birds, very calm, no sudden loud sounds, no speech.
```

第二站环境音：

```text
Loopable ambience for a slow train moving through a Norwegian fjord valley, soft rail movement, low cabin rumble, wide mountain wind, distant waterfall, spacious natural echo, no sudden loud sounds, no speech.
```

第三站环境音：

```text
Loopable ambience for a quiet night train through an aurora flower field, very soft rail bed, gentle night wind, subtle flower field breeze, peaceful open air, no insects close to the ear, no sudden loud sounds, no speech.
```

关键英文说明：

- `loopable ambience`：可循环环境音。
- `soft cabin vibration`：轻微车厢振动。
- `low cabin rumble`：低频车厢行驶声。
- `distant waterfall`：远处瀑布声，不要太近太吵。
- `no sudden loud sounds`：不要突然的大声响，保护老年体验者舒适度。
- `no speech`：不要生成任何语音，避免和韩语播报冲突。

### 4.3 韩语 TTS 生成要求

TTS 参数建议：

```text
Language: Korean
Voice style: warm, polite, calm
Speaking speed: slow
Tone: gentle and reassuring
Audience: elderly listeners
Emotion: peaceful, clear, not theatrical
```

中文解释：

- `Korean`：正式导入 Unity 的语音必须是韩语。
- `warm, polite, calm`：温和、礼貌、平静。
- `slow`：语速偏慢。
- `elderly listeners`：面向老年听众。
- `not theatrical`：不要夸张表演腔。

---

## 5. Unity Inspector 绑定步骤

1. 打开 `Assets/Scenes/SampleScene.unity`。
2. 运行菜单：

```text
Tools/VR Train Journey/Configure Audio System
```

3. 在 Hierarchy 中选择 `JourneySystem`。
4. 确认对象上有：

```text
JourneySequenceController
JourneyAudioController
AudioSource
AudioSource
AudioSource
```

5. 在 `JourneyAudioController` 中绑定：

| Profile Index | 站点 | 需要绑定 |
| --- | --- | --- |
| `0` | `Station01_GoldenVillage` | 第一站 BGM、环境音、韩语语音 |
| `1` | `Station02_FjordView` | 第二站 BGM、环境音、韩语语音 |
| `2` | `Station03_AuroraFlowerField` | 第三站 BGM、环境音、韩语语音 |

6. 建议资源目录：

```text
Assets/Audio/BGM/
Assets/Audio/Ambience/Train/
Assets/Audio/Ambience/Stations/
Assets/Audio/Voice/
```

7. 正式语音文件命名建议：

```text
Voice_KO_Station01_GoldenVillage.wav
Voice_KO_Station02_FjordView.wav
Voice_KO_Station03_AuroraFlowerField.wav
Voice_KO_JourneyCompleted.wav
```

如果单独制作终点语音，后续可将它扩展为单独完成提示 Clip；当前基础系统以第三站语音和完成淡出为主。

---

## 6. Quest 2 测试步骤

### 6.1 Editor 冒烟测试

1. 在 Unity Editor 中进入 Play Mode。
2. 按 `Space` 开始旅程。
3. 确认第一站音频进入。
4. 按 `P` 暂停，确认视频和音频一起暂停。
5. 再按 `P` 继续，确认视频和音频一起恢复。
6. 按 `N` 跳到下一站，确认旧站音频淡出，新站音频进入。
7. 播放到第三站结束，确认音频整体淡出。

### 6.2 Quest 2 真机测试

1. 构建 Development APK。
2. 安装到 Quest 2。
3. 佩戴头显完成三站播放。
4. 使用 Android Logcat 筛选：

```text
[VRTrainJourney.Video]
[VRTrainJourney.Audio]
```

5. 检查：

- 三站视频仍正常顺序播放。
- 每站 BGM、环境音和韩语播报按站点变化。
- 暂停和继续时音频同步。
- 跳站时没有残留上一站循环音。
- 第三站结束后音频淡出。
- 韩语语音不被 BGM 或环境音遮盖。

---

## 7. 后续手柄控制接口

后续加入手柄控制时，手柄脚本仍然只调用视频旅程控制接口：

```text
开始体验    -> JourneySequenceController.StartJourney()
暂停/继续   -> JourneySequenceController.TogglePause()
跳到下一站  -> JourneySequenceController.SkipToNextStation()
```

音频系统不需要被手柄脚本直接调用。

原因：

- `JourneyAudioController` 会监听 `StationStarted` 来切换站点音频。
- `JourneyAudioController` 会读取 `JourneySequenceController.State` 来同步暂停和继续。
- 这样可以避免手柄、视频、音频三套脚本互相交叉控制。

---

## 8. 后续素材导入建议

| 类型 | Unity 导入建议 |
| --- | --- |
| BGM | `Compressed In Memory`，循环点要平滑。 |
| 环境音 | `Compressed In Memory`，优先检查 loop 是否无缝。 |
| 韩语短播报 | `Decompress On Load` 或 `Compressed In Memory` 均可，优先清晰度。 |
| 较长语音 | 可考虑 `Streaming`，但当前项目语音较短，一般不必。 |

音频文件响度建议：

- 语音播报优先清晰，不追求很响。
- BGM 和环境音作为背景，不抢语音。
- 不使用突然高音、爆音、强鼓点或刺耳音效。
