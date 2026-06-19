' ============================================================================
'  MainForm.vb
'  主窗体：MIDI 生成 + 播放控制
' ============================================================================
Option Strict On
Option Explicit On

Imports System.IO
Imports System.Diagnostics
Imports System.Text

Namespace Midi

    Friend Class MainForm

        Private _player As MidiPlayer
        Private _parser As MidiParser
        Private _generator As MidiGenerator
        Private _totalMs As Integer = 0

        Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            _player = New MidiPlayer()
            AddHandler _player.PlaybackFinished, AddressOf OnPlaybackFinished
            AddHandler _player.PlaybackError, AddressOf OnPlaybackError

            _parser = New MidiParser()
            _generator = New MidiGenerator()

            ' 默认填充示例文件路径
            Dim exeDir As String = AppDomain.CurrentDomain.BaseDirectory
            Dim defaultTxt As String = Path.Combine(exeDir, "sample.txt")
            If File.Exists(defaultTxt) Then
                txtTxtFile.Text = defaultTxt
                txtMidiFile.Text = Path.ChangeExtension(defaultTxt, ".mid")
            Else
                txtTxtFile.Text = Path.Combine(exeDir, "sample.txt")
                txtMidiFile.Text = Path.Combine(exeDir, "sample.mid")
            End If

            Log("MIDI 音乐生成工具已就绪")
            Log("提示: 编辑 txt 文件后点击 [生成 MIDI]，再点击 [播放] 试听")
            Log("")
        End Sub

        Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
            If _player IsNot Nothing Then
                _player.Dispose()
            End If
        End Sub

        ' ===== 浏览 txt 文件 =====
        Private Sub btnBrowseTxt_Click(sender As Object, e As EventArgs) Handles btnBrowseTxt.Click
            Using dlg As New OpenFileDialog()
                dlg.Title = "选择 MIDI 描述文件"
                dlg.Filter = "MIDI 描述文件 (*.txt)|*.txt|所有文件 (*.*)|*.*"
                If Not String.IsNullOrEmpty(txtTxtFile.Text) Then
                    dlg.InitialDirectory = Path.GetDirectoryName(txtTxtFile.Text)
                End If
                If dlg.ShowDialog() = DialogResult.OK Then
                    txtTxtFile.Text = dlg.FileName
                    txtMidiFile.Text = Path.ChangeExtension(dlg.FileName, ".mid")
                End If
            End Using
        End Sub

        ' ===== 浏览 MIDI 输出 =====
        Private Sub btnBrowseMidi_Click(sender As Object, e As EventArgs) Handles btnBrowseMidi.Click
            Using dlg As New SaveFileDialog()
                dlg.Title = "保存 MIDI 文件"
                dlg.Filter = "MIDI 文件 (*.mid)|*.mid|所有文件 (*.*)|*.*"
                If Not String.IsNullOrEmpty(txtMidiFile.Text) Then
                    dlg.InitialDirectory = Path.GetDirectoryName(txtMidiFile.Text)
                    dlg.FileName = Path.GetFileName(txtMidiFile.Text)
                End If
                If dlg.ShowDialog() = DialogResult.OK Then
                    txtMidiFile.Text = dlg.FileName
                End If
            End Using
        End Sub

        ' ===== 生成 MIDI =====
        Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
            Dim txtPath As String = txtTxtFile.Text.Trim()
            Dim midPath As String = txtMidiFile.Text.Trim()

            If String.IsNullOrEmpty(txtPath) OrElse Not File.Exists(txtPath) Then
                MessageBox.Show("请选择有效的 txt 描述文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If String.IsNullOrEmpty(midPath) Then
                MessageBox.Show("请指定输出 MIDI 文件路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 如果正在播放，先停止
            If _player.IsPlaying Then
                _player.Stop()
                playTimer.Enabled = False
            End If

            Try
                lblStatus.Text = "解析中..."
                Application.DoEvents()

                Log(String.Format("==== 开始解析: {0} ====", txtPath))
                Dim data As MidiFileData = _parser.Parse(txtPath)
                For Each msg As String In _parser.Log
                    Log("  " & msg)
                Next
                _parser.Log.Clear()

                ' 显示轨道摘要
                For i As Integer = 0 To data.Tracks.Count - 1
                    Dim t As MidiTrack = data.Tracks(i)
                    Log(String.Format("  轨道 {0}: ""{1}"" 通道={2} 乐器={3} 音量={4} 事件数={5}",
                                      i + 1, t.Name, t.Channel, t.Instrument, t.Volume, t.Events.Count))
                Next

                lblStatus.Text = "生成中..."
                Application.DoEvents()

                Log(String.Format("==== 生成 MIDI: {0} ====", midPath))
                _generator.Write(data, midPath)
                For Each msg As String In _generator.Log
                    Log("  " & msg)
                Next
                _generator.Log.Clear()

                Dim fi As New FileInfo(midPath)
                Log(String.Format("  文件大小: {0} 字节", fi.Length))
                Log("生成完成！")
                Log("")
                lblStatus.Text = "生成完成"
                lblPosition.Text = "00:00 / 00:00"
                progressBar.Value = 0

                ' 自动打开新文件
                If _player.CurrentFile <> midPath Then
                    _player.Open(midPath)
                    _totalMs = _player.GetLengthMs()
                End If

            Catch ex As Exception
                Log("[错误] " & ex.Message)
                lblStatus.Text = "生成失败"
                MessageBox.Show(ex.Message, "生成失败", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        ' ===== 播放 =====
        Private Sub btnPlay_Click(sender As Object, e As EventArgs) Handles btnPlay.Click
            Dim midPath As String = txtMidiFile.Text.Trim()
            If String.IsNullOrEmpty(midPath) OrElse Not File.Exists(midPath) Then
                MessageBox.Show("MIDI 文件不存在，请先生成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' 如果文件变化或未打开，重新打开
            If _player.CurrentFile <> midPath Then
                _player.Open(midPath)
                _totalMs = _player.GetLengthMs()
            End If

            If _player.IsPaused Then
                _player.ResumePlay()
            Else
                _player.Play()
            End If

            playTimer.Enabled = True
            lblStatus.Text = "播放中..."
            Log("开始播放: " & midPath)
        End Sub

        ' ===== 暂停 =====
        Private Sub btnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
            If _player.IsPlaying Then
                _player.Pause()
                playTimer.Enabled = False
                lblStatus.Text = "已暂停"
                Log("播放已暂停")
            End If
        End Sub

        ' ===== 停止 =====
        Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
            If _player.IsPlaying OrElse _player.IsPaused Then
                _player.Stop()
                playTimer.Enabled = False
                lblStatus.Text = "已停止"
                progressBar.Value = 0
                lblPosition.Text = "00:00 / " & FormatMs(_totalMs)
                Log("播放已停止")
            End If
        End Sub

        ' ===== 编辑 txt 文件 =====
        Private Sub btnEditTxt_Click(sender As Object, e As EventArgs) Handles btnEditTxt.Click
            Dim txtPath As String = txtTxtFile.Text.Trim()
            If String.IsNullOrEmpty(txtPath) OrElse Not File.Exists(txtPath) Then
                MessageBox.Show("txt 文件不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            Try
                Process.Start(txtPath)
            Catch ex As Exception
                ' 默认编辑器无法启动时尝试 notepad
                Try
                    Process.Start("notepad.exe", """" & txtPath & """")
                Catch
                    Log("[错误] 无法打开编辑器: " & ex.Message)
                End Try
            End Try
        End Sub

        ' ===== 清空日志 =====
        Private Sub btnClearLog_Click(sender As Object, e As EventArgs) Handles btnClearLog.Click
            txtLog.Clear()
        End Sub

        ' ===== 播放定时器：更新进度 =====
        Private Sub playTimer_Tick(sender As Object, e As EventArgs) Handles playTimer.Tick
            If _player Is Nothing Then Return

            If _player.IsFinished() Then
                playTimer.Enabled = False
                _player.Stop()
                lblStatus.Text = "播放完成"
                progressBar.Value = 0
                lblPosition.Text = "00:00 / " & FormatMs(_totalMs)
                Log("播放完成")
                Return
            End If

            Dim pos As Integer = _player.GetPositionMs()
            If _totalMs > 0 Then
                Dim pct As Integer = CInt(Math.Min(100, pos * 100L \ _totalMs))
                progressBar.Value = pct
            End If
            lblPosition.Text = FormatMs(pos) & " / " & FormatMs(_totalMs)
        End Sub

        ' ===== 播放完成回调 =====
        Private Sub OnPlaybackFinished(sender As Object, e As EventArgs)
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, EventArgs)(AddressOf OnPlaybackFinished), sender, e)
            Else
                playTimer.Enabled = False
                lblStatus.Text = "播放完成"
                Log("播放完成事件")
            End If
        End Sub

        Private Sub OnPlaybackError(sender As Object, message As String)
            If Me.InvokeRequired Then
                Me.Invoke(New Action(Of Object, String)(AddressOf OnPlaybackError), sender, message)
            Else
                Log("[播放错误] " & message)
                lblStatus.Text = "播放错误"
            End If
        End Sub

        ' ===== 工具方法 =====
        Private Sub Log(msg As String)
            If txtLog.InvokeRequired Then
                txtLog.Invoke(New Action(Of String)(AddressOf Log), msg)
            Else
                txtLog.AppendText(msg & Environment.NewLine)
                txtLog.SelectionStart = txtLog.TextLength
                txtLog.ScrollToCaret()
            End If
        End Sub

        Private Function FormatMs(ms As Integer) As String
            Dim s As Integer = ms \ 1000
            Dim m As Integer = s \ 60
            s = s Mod 60
            Return String.Format("{0:00}:{1:00}", m, s)
        End Function

    End Class

End Namespace
