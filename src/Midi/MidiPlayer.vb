' ============================================================================
'  MidiPlayer.vb
'  通过 Win32 API (winmm.dll) 播放 MIDI 文件
'
'  使用 mciSendString API 进行 MIDI 文件的播放、暂停、停止等控制
'  使用 mciGetErrorString 获取错误信息
' ============================================================================
Option Strict On
Option Explicit On

Imports System.Runtime.InteropServices
Imports System.Text

Namespace Midi

    ''' <summary>
    ''' MIDI 播放器，基于 Win32 MCI (Media Control Interface) API
    ''' </summary>
    Public Class MidiPlayer
        Implements IDisposable

        <DllImport("winmm.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function mciSendString(
            <MarshalAs(UnmanagedType.LPTStr)> command As String,
            <MarshalAs(UnmanagedType.LPTStr)> returnValue As StringBuilder,
            returnLength As Integer,
            hwndCallback As IntPtr) As Integer
        End Function

        <DllImport("winmm.dll", CharSet:=CharSet.Auto)>
        Private Shared Function mciGetErrorString(
            fdwError As Integer,
            <MarshalAs(UnmanagedType.LPTStr)> lpszErrorText As StringBuilder,
            cchErrorText As Integer) As Boolean
        End Function

        <DllImport("winmm.dll")>
        Private Shared Function timeBeginPeriod(uPeriod As UInteger) As Integer
        End Function

        <DllImport("winmm.dll")>
        Private Shared Function timeEndPeriod(uPeriod As UInteger) As Integer
        End Function

        ' 通知回调常量
        Private Const MCI_NOTIFY As UInteger = &H1
        Private Const MCI_WAIT As UInteger = &H2
        Private Const MCI_OPEN_ELEMENT As UInteger = &H200
        Private Const MCI_OPEN_TYPE As UInteger = &H2000
        Private Const MCI_OPEN_TYPE_ID As UInteger = &H1000

        ' 通知消息常量
        Public Const MM_MCINOTIFY As Integer = &H3B9
        Public Const MCI_NOTIFY_SUCCESSFUL As Integer = 1
        Public Const MCI_NOTIFY_SUPERSEDED As Integer = 2
        Public Const MCI_NOTIFY_ABORTED As Integer = 4
        Public Const MCI_NOTIFY_FAILURE As Integer = 8

        Private _alias As String = "midi_" & Guid.NewGuid().ToString("N")
        Private _isOpen As Boolean = False
        Private _currentFile As String = ""
        Private _isPlaying As Boolean = False
        Private _isPaused As Boolean = False

        ''' <summary>播放完成事件</summary>
        Public Event PlaybackFinished(sender As Object, e As EventArgs)

        ''' <summary>播放错误事件</summary>
        Public Event PlaybackError(sender As Object, message As String)

        ''' <summary>当前是否正在播放</summary>
        Public ReadOnly Property IsPlaying As Boolean
            Get
                Return _isPlaying
            End Get
        End Property

        ''' <summary>当前是否暂停</summary>
        Public ReadOnly Property IsPaused As Boolean
            Get
                Return _isPaused
            End Get
        End Property

        ''' <summary>当前打开的 MIDI 文件路径</summary>
        Public ReadOnly Property CurrentFile As String
            Get
                Return _currentFile
            End Get
        End Property

        ''' <summary>
        ''' 打开 MIDI 文件
        ''' </summary>
        Public Function Open(filePath As String) As Boolean
            If _isOpen Then
                CloseFile()
            End If

            If Not IO.File.Exists(filePath) Then
                RaiseEvent PlaybackError(Me, "文件不存在: " & filePath)
                Return False
            End If

            ' 提高定时器精度
            timeBeginPeriod(1)

            ' 使用 MCI 打开 MIDI 文件
            Dim cmd As String = String.Format("open ""{0}"" type sequencer alias {1}",
                                              filePath, _alias)
            Dim err As Integer = mciSendString(cmd, Nothing, 0, IntPtr.Zero)
            If err <> 0 Then
                RaiseEvent PlaybackError(Me, "打开 MIDI 文件失败: " & GetErrorString(err))
                timeEndPeriod(1)
                Return False
            End If

            _isOpen = True
            _currentFile = filePath
            _isPlaying = False
            _isPaused = False
            Return True
        End Function

        ''' <summary>
        ''' 开始播放（从头开始）
        ''' </summary>
        Public Function Play() As Boolean
            If Not _isOpen Then
                RaiseEvent PlaybackError(Me, "未打开 MIDI 文件")
                Return False
            End If

            ' 先停止当前播放
            If _isPlaying Then
                mciSendString(String.Format("stop {0}", _alias), Nothing, 0, IntPtr.Zero)
            End If

            ' 使用 seek 到起点
            mciSendString(String.Format("seek {0} to start", _alias), Nothing, 0, IntPtr.Zero)

            ' 播放（不使用 notify，由调用方轮询或定时器检查）
            Dim err As Integer = mciSendString(String.Format("play {0}", _alias),
                                               Nothing, 0, IntPtr.Zero)
            If err <> 0 Then
                RaiseEvent PlaybackError(Me, "播放失败: " & GetErrorString(err))
                Return False
            End If

            _isPlaying = True
            _isPaused = False
            Return True
        End Function

        ''' <summary>
        ''' 从当前位置继续播放
        ''' </summary>
        Public Function ResumePlay() As Boolean
            If Not _isOpen OrElse Not _isPaused Then Return False

            Dim err As Integer = mciSendString(String.Format("play {0}", _alias),
                                               Nothing, 0, IntPtr.Zero)
            If err <> 0 Then
                RaiseEvent PlaybackError(Me, "继续播放失败: " & GetErrorString(err))
                Return False
            End If

            _isPlaying = True
            _isPaused = False
            Return True
        End Function

        ''' <summary>暂停播放</summary>
        Public Function Pause() As Boolean
            If Not _isOpen OrElse Not _isPlaying Then Return False

            Dim err As Integer = mciSendString(String.Format("pause {0}", _alias),
                                               Nothing, 0, IntPtr.Zero)
            If err <> 0 Then
                ' 某些设备不支持 pause，尝试 stop
                mciSendString(String.Format("stop {0}", _alias), Nothing, 0, IntPtr.Zero)
            End If

            _isPlaying = False
            _isPaused = True
            Return True
        End Function

        ''' <summary>停止播放</summary>
        Public Function [Stop]() As Boolean
            If Not _isOpen Then Return False

            mciSendString(String.Format("stop {0}", _alias), Nothing, 0, IntPtr.Zero)
            mciSendString(String.Format("seek {0} to start", _alias), Nothing, 0, IntPtr.Zero)

            _isPlaying = False
            _isPaused = False
            Return True
        End Function

        ''' <summary>关闭文件</summary>
        Public Sub CloseFile()
            If _isOpen Then
                mciSendString(String.Format("stop {0}", _alias), Nothing, 0, IntPtr.Zero)
                mciSendString(String.Format("close {0}", _alias), Nothing, 0, IntPtr.Zero)
                timeEndPeriod(1)
                _isOpen = False
                _isPlaying = False
                _isPaused = False
                _currentFile = ""
            End If
        End Sub

        ''' <summary>获取当前播放位置（毫秒）</summary>
        Public Function GetPositionMs() As Integer
            If Not _isOpen Then Return 0
            Dim sb As New StringBuilder(256)
            mciSendString(String.Format("status {0} position", _alias), sb, 256, IntPtr.Zero)
            Dim result As Integer = 0
            Integer.TryParse(sb.ToString(), result)
            Return result
        End Function

        ''' <summary>获取总长度（毫秒）</summary>
        Public Function GetLengthMs() As Integer
            If Not _isOpen Then Return 0
            Dim sb As New StringBuilder(256)
            mciSendString(String.Format("status {0} length", _alias), sb, 256, IntPtr.Zero)
            Dim result As Integer = 0
            Integer.TryParse(sb.ToString(), result)
            Return result
        End Function

        ''' <summary>检查是否播放完毕</summary>
        Public Function IsFinished() As Boolean
            If Not _isOpen OrElse Not _isPlaying Then Return False
            Dim sb As New StringBuilder(256)
            mciSendString(String.Format("status {0} mode", _alias), sb, 256, IntPtr.Zero)
            Dim mode As String = sb.ToString()
            Return mode.Equals("stopped", StringComparison.OrdinalIgnoreCase)
        End Function

        ''' <summary>设置音量 (0-1000)</summary>
        Public Sub SetVolume(volume As Integer)
            If Not _isOpen Then Return
            If volume < 0 Then volume = 0
            If volume > 1000 Then volume = 1000
            mciSendString(String.Format("setaudio {0} volume to {1}", _alias, volume),
                          Nothing, 0, IntPtr.Zero)
        End Sub

        ''' <summary>获取 MCI 错误描述</summary>
        Private Function GetErrorString(errorCode As Integer) As String
            Dim sb As New StringBuilder(512)
            If mciGetErrorString(errorCode, sb, 512) Then
                Return sb.ToString()
            Else
                Return "未知错误 (代码: " & errorCode & ")"
            End If
        End Function

        ''' <summary>释放资源</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            CloseFile()
        End Sub

    End Class

End Namespace
