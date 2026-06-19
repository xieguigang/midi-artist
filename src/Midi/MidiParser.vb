' ============================================================================
'  MidiParser.vb
'  解析 MIDI txt 描述文件，生成 MidiFileData 中间结构
'
'  txt 文件格式：
'    # 行首为 # 表示注释
'    TRACKS: <数量>
'    TPQN: <每四分音符 tick 数>
'    TEMPO: <BPM>
'
'    [TRACK]
'    NAME: <轨道名>
'    CHANNEL: <0-15>
'    INSTRUMENT: <0-127>
'    VOLUME: <0-127>
'    [EVENTS]
'    <事件指令> ...
'
'    轨道之间用空行分隔
'
'  支持的事件指令：
'    NOTE_ON        <音符> <力度> <delta>
'    NOTE_OFF       <音符> <力度> <delta>
'    PROGRAM_CHANGE <程序号> <delta>
'    CONTROL_CHANGE <控制器> <值> <delta>
'    PITCH_BEND     <值-8192~8191> <delta>
'    TEMPO          <BPM> <delta>
'    END_TRACK
' ============================================================================
Option Strict On
Option Explicit On

Imports System.IO
Imports System.Globalization

Namespace Midi

    Public Class MidiParser

        ''' <summary>解析过程中产生的日志信息</summary>
        Public ReadOnly Log As New List(Of String)

        Private _lineNumber As Integer = 0

        ''' <summary>
        ''' 解析 txt 文件，返回 MidiFileData
        ''' </summary>
        Public Function Parse(filePath As String) As MidiFileData
            If Not File.Exists(filePath) Then
                Throw New FileNotFoundException("找不到 txt 文件: " & filePath)
            End If

            Dim lines As String() = File.ReadAllLines(filePath)
            Return ParseLines(lines)
        End Function

        ''' <summary>
        ''' 解析文本行集合
        ''' </summary>
        Public Function ParseLines(lines As IEnumerable(Of String)) As MidiFileData
            Dim data As New MidiFileData()
            _lineNumber = 0

            Dim currentTrack As MidiTrack = Nothing
            Dim inEventsSection As Boolean = False

            For Each rawLine As String In lines
                _lineNumber += 1
                Dim line As String = rawLine.Trim()

                ' 跳过空行与注释
                If line = "" OrElse line.StartsWith("#") Then
                    Continue For
                End If

                Try
                    ' 区段标记
                    If line.StartsWith("[") AndAlso line.EndsWith("]") Then
                        Dim section As String = line.Substring(1, line.Length - 2).Trim().ToUpperInvariant()
                        Select Case section
                            Case "TRACK"
                                ' 开始新轨道
                                currentTrack = New MidiTrack()
                                data.Tracks.Add(currentTrack)
                                inEventsSection = False
                            Case "EVENTS"
                                inEventsSection = True
                            Case Else
                                Log.Add(String.Format("第 {0} 行: 未知区段 [{1}]，已忽略", _lineNumber, section))
                        End Select
                        Continue For
                    End If

                    ' 如果在事件区段，解析事件
                    If inEventsSection AndAlso currentTrack IsNot Nothing Then
                        ParseEvent(line, currentTrack)
                    ElseIf currentTrack IsNot Nothing Then
                        ' 轨道头部属性
                        ParseTrackProperty(line, currentTrack)
                    Else
                        ' 全局属性
                        ParseGlobalProperty(line, data)
                    End If
                Catch ex As Exception
                    Log.Add(String.Format("第 {0} 行错误: {1}", _lineNumber, ex.Message))
                End Try
            Next

            ' 校验
            If data.Tracks.Count = 0 Then
                Log.Add("警告: 未找到任何 [TRACK] 区段")
            End If

            data.TrackCount = data.Tracks.Count

            Log.Add(String.Format("解析完成: 共 {0} 条轨道，TPQN={1}，TEMPO={2}",
                                  data.TrackCount, data.Tpqn, data.Tempo))
            Return data
        End Function

        ''' <summary>解析全局属性</summary>
        Private Sub ParseGlobalProperty(line As String, data As MidiFileData)
            Dim parts As String() = SplitKeyValue(line)
            Dim key As String = parts(0).ToUpperInvariant()
            Dim value As String = parts(1)

            Select Case key
                Case "TRACKS"
                    data.TrackCount = Integer.Parse(value, CultureInfo.InvariantCulture)
                Case "TPQN"
                    data.Tpqn = Integer.Parse(value, CultureInfo.InvariantCulture)
                    If data.Tpqn <= 0 Then
                        Throw New FormatException("TPQN 必须为正数")
                    End If
                Case "TEMPO"
                    data.Tempo = Integer.Parse(value, CultureInfo.InvariantCulture)
                    If data.Tempo <= 0 Then
                        Throw New FormatException("TEMPO 必须为正数")
                    End If
                Case Else
                    Log.Add(String.Format("第 {0} 行: 未知全局属性 {1}", _lineNumber, key))
            End Select
        End Sub

        ''' <summary>解析轨道属性</summary>
        Private Sub ParseTrackProperty(line As String, track As MidiTrack)
            Dim parts As String() = SplitKeyValue(line)
            Dim key As String = parts(0).ToUpperInvariant()
            Dim value As String = parts(1)

            Select Case key
                Case "NAME"
                    track.Name = value
                Case "CHANNEL"
                    track.Channel = ParseByte(value, 0, 15, "CHANNEL")
                Case "INSTRUMENT"
                    track.Instrument = ParseByte(value, 0, 127, "INSTRUMENT")
                Case "VOLUME"
                    track.Volume = ParseByte(value, 0, 127, "VOLUME")
                Case Else
                    Log.Add(String.Format("第 {0} 行: 未知轨道属性 {1}", _lineNumber, key))
            End Select
        End Sub

        ''' <summary>解析事件行</summary>
        Private Sub ParseEvent(line As String, track As MidiTrack)
            ' 按空白拆分
            Dim tokens As String() = line.Split(New Char() {" "c, ControlChars.Tab},
                                                StringSplitOptions.RemoveEmptyEntries)
            If tokens.Length = 0 Then Return

            Dim cmd As String = tokens(0).ToUpperInvariant()
            Dim ev As New MidiEvent()
            ev.Channel = track.Channel

            Select Case cmd
                Case "NOTE_ON"
                    ' NOTE_ON <音符> <力度> <delta>
                    If tokens.Length < 4 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.NoteOn
                    ev.Data1 = CByte(NoteNames.NameToNumber(tokens(1)))
                    ev.Data2 = ParseByte(tokens(2), 0, 127, "力度")
                    ev.DeltaTime = Integer.Parse(tokens(3), CultureInfo.InvariantCulture)

                Case "NOTE_OFF"
                    ' NOTE_OFF <音符> <力度> <delta>
                    If tokens.Length < 4 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.NoteOff
                    ev.Data1 = CByte(NoteNames.NameToNumber(tokens(1)))
                    ev.Data2 = ParseByte(tokens(2), 0, 127, "力度")
                    ev.DeltaTime = Integer.Parse(tokens(3), CultureInfo.InvariantCulture)

                Case "PROGRAM_CHANGE", "PROGRAM", "PATCH"
                    ' PROGRAM_CHANGE <程序号> <delta>
                    If tokens.Length < 3 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.ProgramChange
                    ev.Data1 = ParseByte(tokens(1), 0, 127, "程序号")
                    ev.Data2 = 0
                    ev.DeltaTime = Integer.Parse(tokens(2), CultureInfo.InvariantCulture)

                Case "CONTROL_CHANGE", "CONTROL", "CC"
                    ' CONTROL_CHANGE <控制器> <值> <delta>
                    If tokens.Length < 4 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.ControlChange
                    ev.Data1 = ParseByte(tokens(1), 0, 127, "控制器号")
                    ev.Data2 = ParseByte(tokens(2), 0, 127, "控制器值")
                    ev.DeltaTime = Integer.Parse(tokens(3), CultureInfo.InvariantCulture)

                Case "PITCH_BEND", "PITCH"
                    ' PITCH_BEND <值-8192~8191> <delta>
                    If tokens.Length < 3 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.PitchBend
                    Dim bend As Integer = Integer.Parse(tokens(1), CultureInfo.InvariantCulture)
                    If bend < -8192 OrElse bend > 8191 Then
                        Throw New FormatException("PITCH_BEND 值范围 -8192 ~ 8191")
                    End If
                    Dim v As Integer = bend + 8192
                    ev.Data1 = CByte(v And &H7F)        ' LSB
                    ev.Data2 = CByte((v >> 7) And &H7F) ' MSB
                    ev.DeltaTime = Integer.Parse(tokens(2), CultureInfo.InvariantCulture)

                Case "TEMPO"
                    ' TEMPO <BPM> <delta>  (Meta 事件)
                    If tokens.Length < 3 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.MetaEvent
                    ev.MetaType = MetaEventType.SetTempo
                    Dim bpm As Integer = Integer.Parse(tokens(1), CultureInfo.InvariantCulture)
                    If bpm <= 0 Then Throw New FormatException("TEMPO 必须为正数")
                    ' 微秒/四分音符 = 60,000,000 / BPM
                    Dim microsPerQuarter As Integer = 60000000 \ bpm
                    ev.MetaData = New Byte() {
                        CByte((microsPerQuarter >> 16) And &HFF),
                        CByte((microsPerQuarter >> 8) And &HFF),
                        CByte(microsPerQuarter And &HFF)
                    }
                    ev.DeltaTime = Integer.Parse(tokens(2), CultureInfo.InvariantCulture)

                Case "END_TRACK", "END"
                    ev.Type = MidiEventType.MetaEvent
                    ev.MetaType = MetaEventType.EndOfTrack
                    ev.MetaData = New Byte(-1) {}
                    ev.DeltaTime = 0

                Case "TEXT"
                    ' TEXT <内容> <delta>
                    If tokens.Length < 3 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.MetaEvent
                    ev.MetaType = MetaEventType.TextEvent
                    Dim text As String = line.Substring(line.IndexOf(" "c) + 1)
                    ' 最后一个 token 是 delta，去掉
                    Dim lastSpace As Integer = text.LastIndexOf(" "c)
                    Dim deltaStr As String = text.Substring(lastSpace + 1).Trim()
                    text = text.Substring(0, lastSpace).Trim()
                    ev.MetaData = System.Text.Encoding.UTF8.GetBytes(text)
                    ev.DeltaTime = Integer.Parse(deltaStr, CultureInfo.InvariantCulture)

                Case "TRACK_NAME", "TRACKNAME"
                    ' TRACK_NAME <名称> <delta>
                    If tokens.Length < 3 Then ThrowInvalidArgs(cmd)
                    ev.Type = MidiEventType.MetaEvent
                    ev.MetaType = MetaEventType.TrackName
                    Dim text As String = line.Substring(line.IndexOf(" "c) + 1)
                    Dim lastSpace As Integer = text.LastIndexOf(" "c)
                    Dim deltaStr As String = text.Substring(lastSpace + 1).Trim()
                    text = text.Substring(0, lastSpace).Trim()
                    ev.MetaData = System.Text.Encoding.UTF8.GetBytes(text)
                    ev.DeltaTime = Integer.Parse(deltaStr, CultureInfo.InvariantCulture)

                Case Else
                    Throw New FormatException("未知事件指令: " & cmd)
            End Select

            track.Events.Add(ev)
        End Sub

        ''' <summary>拆分 "KEY: VALUE" 形式的行</summary>
        Private Function SplitKeyValue(line As String) As String()
            Dim idx As Integer = line.IndexOf(":"c)
            If idx < 0 Then
                Throw New FormatException("属性行缺少冒号: " & line)
            End If
            Dim key As String = line.Substring(0, idx).Trim()
            Dim value As String = line.Substring(idx + 1).Trim()
            Return {key, value}
        End Function

        ''' <summary>解析字节范围</summary>
        Private Function ParseByte(s As String, min As Integer, max As Integer, name As String) As Byte
            Dim v As Integer = Integer.Parse(s, CultureInfo.InvariantCulture)
            If v < min OrElse v > max Then
                Throw New FormatException(String.Format("{0} 范围 {1}-{2}，得到 {3}", name, min, max, v))
            End If
            Return CByte(v)
        End Function

        Private Sub ThrowInvalidArgs(cmd As String)
            Throw New FormatException("指令 " & cmd & " 参数数量不正确")
        End Sub

    End Class

End Namespace
