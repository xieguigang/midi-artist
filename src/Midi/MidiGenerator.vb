' ============================================================================
'  MidiGenerator.vb
'  将 MidiFileData 写入标准 MIDI 二进制文件（.mid，格式 1）
'
'  MIDI 文件结构：
'    Header chunk:  "MThd" + len(4) + format(2) + tracks(2) + division(2)
'    Track chunk:   "MTrk" + len(4) + events...
'
'  每个 event = delta_time(VLQ) + 状态字节 + 数据字节
'  每条轨道以 EndOfTrack meta 事件结尾
' ============================================================================
Option Strict On
Option Explicit On

Imports System.IO
Imports System.Text

Namespace Midi

    Public Class MidiGenerator

        ''' <summary>生成过程中产生的日志</summary>
        Public ReadOnly Log As New List(Of String)

        ''' <summary>
        ''' 将 MidiFileData 写入 MIDI 文件
        ''' </summary>
        Public Sub Write(data As MidiFileData, outputPath As String)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Tracks.Count = 0 Then Throw New InvalidOperationException("没有可写入的轨道")

            Using fs As New FileStream(outputPath, FileMode.Create, FileAccess.Write)
                Using bw As New BinaryWriter(fs, Encoding.ASCII)
                    ' ===== 写 Header Chunk =====
                    bw.Write(Encoding.ASCII.GetBytes("MThd"))
                    bw.Write(BeUInt32(6))                       ' header length = 6
                    bw.Write(BeUInt16(1))                       ' format = 1 (多轨道)
                    bw.Write(BeUInt16(CUShort(data.Tracks.Count)))
                    bw.Write(BeUInt16(CUShort(data.Tpqn)))

                    ' ===== 写每个 Track Chunk =====
                    For i As Integer = 0 To data.Tracks.Count - 1
                        Dim track As MidiTrack = data.Tracks(i)
                        Dim trackBytes As Byte() = BuildTrackBytes(track, data, i)
                        bw.Write(Encoding.ASCII.GetBytes("MTrk"))
                        bw.Write(BeUInt32(CUInt(trackBytes.Length)))
                        bw.Write(trackBytes)
                    Next
                End Using
            End Using

            Log.Add(String.Format("已生成 MIDI 文件: {0}", outputPath))
            Log.Add(String.Format("  轨道数: {0}", data.Tracks.Count))
            Log.Add(String.Format("  TPQN: {0}", data.Tpqn))
        End Sub

        ''' <summary>构建一条轨道的字节流</summary>
        Private Function BuildTrackBytes(track As MidiTrack, data As MidiFileData, trackIndex As Integer) As Byte()
            Using ms As New MemoryStream()
                Using bw As New BinaryWriter(ms, Encoding.ASCII)

                    ' 1) 写 TrackName meta 事件（如果有名称）
                    If Not String.IsNullOrEmpty(track.Name) Then
                        WriteMetaEvent(bw, 0, MetaEventType.TrackName,
                                       Encoding.UTF8.GetBytes(track.Name))
                    End If

                    ' 2) 写初始 Tempo meta 事件（仅第一条轨道）
                    If trackIndex = 0 Then
                        Dim microsPerQuarter As Integer = 60000000 \ data.Tempo
                        WriteMetaEvent(bw, 0, MetaEventType.SetTempo, New Byte() {
                            CByte((microsPerQuarter >> 16) And &HFF),
                            CByte((microsPerQuarter >> 8) And &HFF),
                            CByte(microsPerQuarter And &HFF)
                        })
                    End If

                    ' 3) 写 Program Change（如果轨道配置了乐器）
                    If track.Instrument <> 0 OrElse True Then
                        ' 始终写入 Program Change，确保乐器设置生效
                        WriteChannelMessage(bw, 0, MidiEventType.ProgramChange,
                                            track.Channel, track.Instrument, 0)
                    End If

                    ' 4) 写 Control Change：Volume (CC7) 和 Expression (CC11)
                    WriteChannelMessage(bw, 0, MidiEventType.ControlChange,
                                        track.Channel, 7, track.Volume)
                    WriteChannelMessage(bw, 0, MidiEventType.ControlChange,
                                        track.Channel, 11, 127)

                    ' 5) 写用户事件
                    Dim lastStatus As Byte = 0
                    For Each ev As MidiEvent In track.Events
                        WriteEvent(bw, ev, lastStatus)
                        If ev.Type <> MidiEventType.MetaEvent Then
                            lastStatus = CByte(ev.Type Or (ev.Channel And &HF))
                        End If
                    Next

                    ' 6) 写 EndOfTrack（如果用户未显式写）
                    Dim hasEnd As Boolean = False
                    If track.Events.Count > 0 Then
                        Dim last As MidiEvent = track.Events(track.Events.Count - 1)
                        If last.Type = MidiEventType.MetaEvent AndAlso
                           last.MetaType = MetaEventType.EndOfTrack Then
                            hasEnd = True
                        End If
                    End If
                    If Not hasEnd Then
                        WriteMetaEvent(bw, 0, MetaEventType.EndOfTrack, New Byte(-1) {})
                    End If

                End Using
                Return ms.ToArray()
            End Using
        End Function

        ''' <summary>写单个事件（支持 running status 优化）</summary>
        Private Sub WriteEvent(bw As BinaryWriter, ev As MidiEvent, ByRef lastStatus As Byte)
            ' 写 delta time（VLQ）
            WriteVarLen(bw, CUInt(ev.DeltaTime))

            If ev.Type = MidiEventType.MetaEvent Then
                ' Meta 事件：0xFF + type + VLQ length + data
                bw.Write(CByte(&HFF))
                bw.Write(CByte(ev.MetaType))
                Dim len As Integer = If(ev.MetaData IsNot Nothing, ev.MetaData.Length, 0)
                WriteVarLen(bw, CUInt(len))
                If ev.MetaData IsNot Nothing AndAlso ev.MetaData.Length > 0 Then
                    bw.Write(ev.MetaData)
                End If
                lastStatus = 0  ' meta 事件后不能 running status
            Else
                Dim status As Byte = CByte(ev.Type Or (ev.Channel And &HF))

                ' Running status 优化：如果状态字节与上次相同，可省略
                ' 为兼容性考虑，这里不启用 running status，始终写状态字节
                bw.Write(status)

                Select Case ev.Type
                    Case MidiEventType.NoteOn, MidiEventType.NoteOff,
                         MidiEventType.NoteAftertouch, MidiEventType.ControlChange,
                         MidiEventType.PitchBend
                        ' 2 数据字节
                        bw.Write(ev.Data1)
                        bw.Write(ev.Data2)
                    Case MidiEventType.ProgramChange, MidiEventType.ChannelAftertouch
                        ' 1 数据字节
                        bw.Write(ev.Data1)
                End Select

                lastStatus = status
            End If
        End Sub

        ''' <summary>写通道消息（带 delta time）</summary>
        Private Sub WriteChannelMessage(bw As BinaryWriter, delta As Integer,
                                        type As MidiEventType, channel As Byte,
                                        data1 As Byte, data2 As Byte)
            WriteVarLen(bw, CUInt(delta))
            bw.Write(CByte(type Or (channel And &HF)))
            Select Case type
                Case MidiEventType.NoteOn, MidiEventType.NoteOff,
                     MidiEventType.NoteAftertouch, MidiEventType.ControlChange,
                     MidiEventType.PitchBend
                    bw.Write(data1)
                    bw.Write(data2)
                Case MidiEventType.ProgramChange, MidiEventType.ChannelAftertouch
                    bw.Write(data1)
            End Select
        End Sub

        ''' <summary>写 Meta 事件（带 delta time）</summary>
        Private Sub WriteMetaEvent(bw As BinaryWriter, delta As Integer,
                                   metaType As MetaEventType, payload As Byte())
            WriteVarLen(bw, CUInt(delta))
            bw.Write(CByte(&HFF))
            bw.Write(CByte(metaType))
            Dim len As Integer = If(payload IsNot Nothing, payload.Length, 0)
            WriteVarLen(bw, CUInt(len))
            If payload IsNot Nothing AndAlso payload.Length > 0 Then
                bw.Write(payload)
            End If
        End Sub

        ''' <summary>写变长整数（MIDI VLQ）</summary>
        Private Sub WriteVarLen(bw As BinaryWriter, value As UInteger)
            Dim buffer As Byte() = New Byte(3) {}
            Dim idx As Integer = 0
            buffer(0) = CByte(value And &H7F)
            value >>= 7
            While value > 0
                idx += 1
                buffer(idx) = CByte((value And &H7F) Or &H80)
                value >>= 7
            End While
            ' 反向写出
            For i As Integer = idx To 0 Step -1
                bw.Write(buffer(i))
            Next
        End Sub

        ' ===== 大端字节序辅助 =====
        Private Function BeUInt16(v As UShort) As Byte()
            Return New Byte() {CByte((v >> 8) And &HFF), CByte(v And &HFF)}
        End Function

        Private Function BeUInt32(v As UInteger) As Byte()
            Return New Byte() {
                CByte((v >> 24) And &HFF),
                CByte((v >> 16) And &HFF),
                CByte((v >> 8) And &HFF),
                CByte(v And &HFF)
            }
        End Function

    End Class

End Namespace
