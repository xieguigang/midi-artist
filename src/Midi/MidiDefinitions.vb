' ============================================================================
'  MidiDefinitions.vb
'  MIDI 核心定义：音符映射、事件类型、数据结构
' ============================================================================
Option Strict On
Option Explicit On

Namespace Midi

    ''' <summary>
    ''' MIDI 事件类型枚举
    ''' </summary>
    Public Enum MidiEventType As Byte
        NoteOff = &H80
        NoteOn = &H90
        NoteAftertouch = &HA0
        ControlChange = &HB0
        ProgramChange = &HC0
        ChannelAftertouch = &HD0
        PitchBend = &HE0
        MetaEvent = &HFF
    End Enum

    ''' <summary>
    ''' Meta 事件子类型
    ''' </summary>
    Public Enum MetaEventType As Byte
        SequenceNumber = &H0
        TextEvent = &H1
        CopyrightNotice = &H2
        TrackName = &H3
        InstrumentName = &H4
        Lyric = &H5
        Marker = &H6
        CuePoint = &H7
        EndOfTrack = &H2F
        SetTempo = &H51
        SmpteOffset = &H54
        TimeSignature = &H58
        KeySignature = &H59
        SequencerSpecific = &H7F
    End Enum

    ''' <summary>
    ''' 单个 MIDI 事件（解析后的中间表示）
    ''' </summary>
    Public Class MidiEvent
        ''' <summary>距上一事件的 tick 数（增量时间）</summary>
        Public Property DeltaTime As Integer
        ''' <summary>事件类型</summary>
        Public Property Type As MidiEventType
        ''' <summary>通道 0-15</summary>
        Public Property Channel As Byte
        ''' <summary>数据字节 1（如音符编号、控制器号）</summary>
        Public Property Data1 As Byte
        ''' <summary>数据字节 2（如力度、控制器值）</summary>
        Public Property Data2 As Byte
        ''' <summary>Meta 事件子类型（仅 MetaEvent 时有效）</summary>
        Public Property MetaType As MetaEventType
        ''' <summary>Meta 事件原始数据</summary>
        Public Property MetaData As Byte()

        Public Sub New()
            MetaData = New Byte(-1) {}
        End Sub
    End Class

    ''' <summary>
    ''' 音轨数据
    ''' </summary>
    Public Class MidiTrack
        ''' <summary>轨道名称</summary>
        Public Property Name As String = ""
        ''' <summary>MIDI 通道 0-15</summary>
        Public Property Channel As Byte = 0
        ''' <summary>乐器（程序）编号 0-127</summary>
        Public Property Instrument As Byte = 0
        ''' <summary>音量 0-127</summary>
        Public Property Volume As Byte = 100
        ''' <summary>事件列表</summary>
        Public Property Events As New List(Of MidiEvent)
    End Class

    ''' <summary>
    ''' 整个 MIDI 文件描述
    ''' </summary>
    Public Class MidiFileData
        ''' <summary>轨道数</summary>
        Public Property TrackCount As Integer = 0
        ''' <summary>每四分音符 tick 数（TPQN）</summary>
        Public Property Tpqn As Integer = 480
        ''' <summary>初始速度（BPM）</summary>
        Public Property Tempo As Integer = 120
        ''' <summary>轨道列表</summary>
        Public Property Tracks As New List(Of MidiTrack)
    End Class

    ''' <summary>
    ''' 音符名称与 MIDI 编号之间的转换工具
    ''' </summary>
    Public NotInheritable Class NoteNames

        Private Sub New()
        End Sub

        ''' <summary>12 个半音名称（与 MIDI 编号 mod 12 对应）</summary>
        Public Shared ReadOnly SemitoneNames As String() = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        }

        ''' <summary>
        ''' 将音符名称（如 "C4", "F#5"）转换为 MIDI 编号
        ''' 约定：中央 C 为 C4，对应 MIDI 编号 60
        ''' </summary>
        Public Shared Function NameToNumber(name As String) As Integer
            If String.IsNullOrWhiteSpace(name) Then
                Throw New FormatException("音符名称为空")
            End If

            Dim s As String = name.Trim().ToUpperInvariant()

            ' 提取音名（1 或 2 个字符：字母 + 可选的 # 或 b）
            If s.Length < 2 Then
                Throw New FormatException("无效的音符名称: " & name)
            End If

            Dim noteChar As Char = s(0)
            Dim semitoneIndex As Integer
            Dim pos As Integer = 1

            Select Case noteChar
                Case "C"c : semitoneIndex = 0
                Case "D"c : semitoneIndex = 2
                Case "E"c : semitoneIndex = 4
                Case "F"c : semitoneIndex = 5
                Case "G"c : semitoneIndex = 7
                Case "A"c : semitoneIndex = 9
                Case "B"c : semitoneIndex = 11
                Case Else
                    Throw New FormatException("无效的音名: " & noteChar)
            End Select

            ' 处理升号 # 或降号 b
            If pos < s.Length AndAlso s(pos) = "#"c Then
                semitoneIndex += 1
                pos += 1
            ElseIf pos < s.Length AndAlso s(pos) = "b"c Then
                semitoneIndex -= 1
                pos += 1
            End If

            ' 剩余部分应为八度数
            Dim octaveStr As String = s.Substring(pos)
            Dim octave As Integer
            If Not Integer.TryParse(octaveStr, octave) Then
                Throw New FormatException("无效的八度: " & octaveStr)
            End If

            ' MIDI 编号 = (八度 + 1) * 12 + 半音索引
            ' C-1 = 0, C0 = 12, C4 = 60（中央 C）
            Dim midiNumber As Integer = (octave + 1) * 12 + semitoneIndex

            If midiNumber < 0 OrElse midiNumber > 127 Then
                Throw New FormatException(
                    String.Format("音符 {0} 超出 MIDI 范围 (0-127)，得到 {1}", name, midiNumber))
            End If

            Return midiNumber
        End Function

        ''' <summary>
        ''' 将 MIDI 编号转换为音符名称
        ''' </summary>
        Public Shared Function NumberToName(number As Integer) As String
            If number < 0 OrElse number > 127 Then
                Throw New ArgumentOutOfRangeException(NameOf(number))
            End If
            Dim semitone As Integer = number Mod 12
            Dim octave As Integer = number \ 12 - 1
            Return SemitoneNames(semitone) & octave.ToString()
        End Function

    End Class

End Namespace
