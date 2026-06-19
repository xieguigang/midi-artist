# MIDI 音乐生成工具 (VB.NET)

一个使用 VB.NET 编写的 MIDI 音乐生成工具，通过解析自定义格式的 txt 描述文件生成标准 MIDI 二进制文件，并通过 Win32 API (winmm.dll) 播放。

## 功能特性

- 解析自定义 txt 格式描述文件
- 生成标准 MIDI 格式 1 二进制文件 (.mid)
- 通过 Win32 MCI API 播放 MIDI 文件
- 支持多轨道、多通道、多种 MIDI 事件
- 实时播放进度显示
- 内置日志输出，便于调试
- 一键打开 txt 文件进行编辑

## 项目结构

```
MidiGenerator/
├── MidiGenerator.sln              # Visual Studio 解决方案
├── MidiGenerator.vbproj           # VB.NET 项目文件
├── App.vb                         # 应用程序入口
├── MainForm.vb                    # 主窗体代码
├── MainForm.Designer.vb           # 主窗体设计器代码
├── sample.txt                     # 示例 MIDI 描述文件
├── Midi/
│   ├── MidiDefinitions.vb         # MIDI 核心定义（音符映射、事件类型）
│   ├── MidiParser.vb              # txt 文件解析器
│   ├── MidiGenerator.vb           # MIDI 二进制文件生成器
│   └── MidiPlayer.vb              # Win32 API MIDI 播放器
└── My Project/
    ├── AssemblyInfo.vb            # 程序集信息
    └── Application.myapp          # 应用程序配置
```

## 编译与运行

### 方式一：使用 Visual Studio

1. 用 Visual Studio 2019/2022 打开 `MidiGenerator.sln`
2. 按 F5 调试运行，或 Ctrl+Shift+B 编译
3. 需要 .NET Framework 4.8 或更高版本

### 方式二：使用命令行

```cmd
# 使用 MSBuild 编译
msbuild MidiGenerator.vbproj /p:Configuration=Release

# 或使用 vbc 编译器（需要手动指定引用）
vbc /target:winexe /out:MidiGenerator.exe ^
    /reference:System.dll ^
    /reference:System.Drawing.dll ^
    /reference:System.Windows.Forms.dll ^
    App.vb MainForm.vb MainForm.Designer.vb ^
    Midi\MidiDefinitions.vb Midi\MidiParser.vb ^
    Midi\MidiGenerator.vb Midi\MidiPlayer.vb
```

## 使用方法

1. 启动程序后，默认加载 `sample.txt` 示例文件
2. 点击「浏览...」选择自己的 txt 描述文件
3. 点击「生成 MIDI」按钮，生成 .mid 文件
4. 点击「播放」按钮试听
5. 如需修改，点击「编辑 txt 文件」打开编辑器
6. 修改保存后，重新点击「生成 MIDI」→「播放」

## txt 文件格式

详见随附的 Word 文档《MIDI txt 文件格式说明.docx》。

简单示例：

```
TRACKS: 2
TPQN: 480
TEMPO: 120

[TRACK]
NAME: 主旋律
CHANNEL: 0
INSTRUMENT: 0
VOLUME: 100

[EVENTS]
NOTE_ON C4 80 0
NOTE_OFF C4 0 480
NOTE_ON D4 80 0
NOTE_OFF D4 0 480

[TRACK]
NAME: 伴奏
CHANNEL: 1
INSTRUMENT: 33
VOLUME: 80

[EVENTS]
NOTE_ON C3 80 0
NOTE_OFF C3 0 960
```

## 技术要点

- **MIDI 文件格式**：标准 MIDI 格式 1（多轨道），支持变长编码（VLQ）
- **Win32 API**：使用 `mciSendString` 进行 MIDI 文件播放控制
- **音符命名**：采用科学音高记号，中央 C = C4 = MIDI 60
- **时间单位**：TPQN（Ticks per Quarter Note），默认 480
