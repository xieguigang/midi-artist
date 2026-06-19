' ============================================================================
'  MainForm.Designer.vb
'  主窗体设计器自动生成代码
' ============================================================================
Namespace Midi

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Friend Class MainForm
        Inherits System.Windows.Forms.Form

        'Required designer variable.
        Private components As System.ComponentModel.IContainer = Nothing

        'Clean up any resources being used.
        Protected Overrides Sub Dispose(disposing As Boolean)
            Try
                If disposing AndAlso (components IsNot Nothing) Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()

            Me.lblTxtFile = New System.Windows.Forms.Label()
            Me.txtTxtFile = New System.Windows.Forms.TextBox()
            Me.btnBrowseTxt = New System.Windows.Forms.Button()

            Me.lblMidiFile = New System.Windows.Forms.Label()
            Me.txtMidiFile = New System.Windows.Forms.TextBox()
            Me.btnBrowseMidi = New System.Windows.Forms.Button()

            Me.btnGenerate = New System.Windows.Forms.Button()
            Me.btnPlay = New System.Windows.Forms.Button()
            Me.btnPause = New System.Windows.Forms.Button()
            Me.btnStop = New System.Windows.Forms.Button()
            Me.btnEditTxt = New System.Windows.Forms.Button()

            Me.lblStatus = New System.Windows.Forms.Label()
            Me.lblPosition = New System.Windows.Forms.Label()
            Me.progressBar = New System.Windows.Forms.ProgressBar()

            Me.grpFile = New System.Windows.Forms.GroupBox()
            Me.grpControl = New System.Windows.Forms.GroupBox()
            Me.grpLog = New System.Windows.Forms.GroupBox()

            Me.txtLog = New System.Windows.Forms.TextBox()
            Me.btnClearLog = New System.Windows.Forms.Button()

            Me.playTimer = New System.Windows.Forms.Timer(Me.components)

            Me.grpFile.SuspendLayout()
            Me.grpControl.SuspendLayout()
            Me.grpLog.SuspendLayout()
            Me.SuspendLayout()

            ' ===== lblTxtFile =====
            Me.lblTxtFile.AutoSize = True
            Me.lblTxtFile.Location = New System.Drawing.Point(12, 25)
            Me.lblTxtFile.Text = "MIDI 描述文件 (.txt):"

            ' ===== txtTxtFile =====
            Me.txtTxtFile.Location = New System.Drawing.Point(140, 22)
            Me.txtTxtFile.Size = New System.Drawing.Size(490, 21)
            Me.txtTxtFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or
                                          System.Windows.Forms.AnchorStyles.Left) Or
                                          System.Windows.Forms.AnchorStyles.Right),
                                          System.Windows.Forms.AnchorStyles)

            ' ===== btnBrowseTxt =====
            Me.btnBrowseTxt.Location = New System.Drawing.Point(636, 20)
            Me.btnBrowseTxt.Size = New System.Drawing.Size(85, 25)
            Me.btnBrowseTxt.Text = "浏览..."
            Me.btnBrowseTxt.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or
                                            System.Windows.Forms.AnchorStyles.Right),
                                            System.Windows.Forms.AnchorStyles)

            ' ===== lblMidiFile =====
            Me.lblMidiFile.AutoSize = True
            Me.lblMidiFile.Location = New System.Drawing.Point(12, 56)
            Me.lblMidiFile.Text = "输出 MIDI 文件 (.mid):"

            ' ===== txtMidiFile =====
            Me.txtMidiFile.Location = New System.Drawing.Point(140, 53)
            Me.txtMidiFile.Size = New System.Drawing.Size(490, 21)
            Me.txtMidiFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or
                                            System.Windows.Forms.AnchorStyles.Left) Or
                                            System.Windows.Forms.AnchorStyles.Right),
                                            System.Windows.Forms.AnchorStyles)

            ' ===== btnBrowseMidi =====
            Me.btnBrowseMidi.Location = New System.Drawing.Point(636, 51)
            Me.btnBrowseMidi.Size = New System.Drawing.Size(85, 25)
            Me.btnBrowseMidi.Text = "另存为..."
            Me.btnBrowseMidi.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or
                                             System.Windows.Forms.AnchorStyles.Right),
                                             System.Windows.Forms.AnchorStyles)

            ' ===== grpFile =====
            Me.grpFile.Controls.Add(Me.lblTxtFile)
            Me.grpFile.Controls.Add(Me.txtTxtFile)
            Me.grpFile.Controls.Add(Me.btnBrowseTxt)
            Me.grpFile.Controls.Add(Me.lblMidiFile)
            Me.grpFile.Controls.Add(Me.txtMidiFile)
            Me.grpFile.Controls.Add(Me.btnBrowseMidi)
            Me.grpFile.Location = New System.Drawing.Point(12, 12)
            Me.grpFile.Size = New System.Drawing.Size(740, 90)
            Me.grpFile.Text = "文件"
            Me.grpFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or
                                        System.Windows.Forms.AnchorStyles.Left) Or
                                        System.Windows.Forms.AnchorStyles.Right),
                                        System.Windows.Forms.AnchorStyles)

            ' ===== btnGenerate =====
            Me.btnGenerate.Location = New System.Drawing.Point(15, 25)
            Me.btnGenerate.Size = New System.Drawing.Size(120, 38)
            Me.btnGenerate.Text = "生成 MIDI"
            Me.btnGenerate.BackColor = System.Drawing.Color.LightSteelBlue
            Me.btnGenerate.FlatStyle = System.Windows.Forms.FlatStyle.Flat

            ' ===== btnPlay =====
            Me.btnPlay.Location = New System.Drawing.Point(145, 25)
            Me.btnPlay.Size = New System.Drawing.Size(100, 38)
            Me.btnPlay.Text = "播放"
            Me.btnPlay.BackColor = System.Drawing.Color.LightGreen
            Me.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat

            ' ===== btnPause =====
            Me.btnPause.Location = New System.Drawing.Point(255, 25)
            Me.btnPause.Size = New System.Drawing.Size(100, 38)
            Me.btnPause.Text = "暂停"
            Me.btnPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat

            ' ===== btnStop =====
            Me.btnStop.Location = New System.Drawing.Point(365, 25)
            Me.btnStop.Size = New System.Drawing.Size(100, 38)
            Me.btnStop.Text = "停止"
            Me.btnStop.BackColor = System.Drawing.Color.LightSalmon
            Me.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat

            ' ===== btnEditTxt =====
            Me.btnEditTxt.Location = New System.Drawing.Point(475, 25)
            Me.btnEditTxt.Size = New System.Drawing.Size(120, 38)
            Me.btnEditTxt.Text = "编辑 txt 文件"
            Me.btnEditTxt.FlatStyle = System.Windows.Forms.FlatStyle.Flat

            ' ===== lblStatus =====
            Me.lblStatus.AutoSize = True
            Me.lblStatus.Location = New System.Drawing.Point(610, 33)
            Me.lblStatus.Text = "就绪"
            Me.lblStatus.ForeColor = System.Drawing.Color.DarkBlue
            Me.lblStatus.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or
                                         System.Windows.Forms.AnchorStyles.Right),
                                         System.Windows.Forms.AnchorStyles)

            ' ===== progressBar =====
            Me.progressBar.Location = New System.Drawing.Point(15, 70)
            Me.progressBar.Size = New System.Drawing.Size(590, 18)
            Me.progressBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or
                                            System.Windows.Forms.AnchorStyles.Left) Or
                                            System.Windows.Forms.AnchorStyles.Right),
                                            System.Windows.Forms.AnchorStyles)

            ' ===== lblPosition =====
            Me.lblPosition.AutoSize = True
            Me.lblPosition.Location = New System.Drawing.Point(610, 72)
            Me.lblPosition.Text = "00:00 / 00:00"
            Me.lblPosition.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or
                                           System.Windows.Forms.AnchorStyles.Right),
                                           System.Windows.Forms.AnchorStyles)

            ' ===== grpControl =====
            Me.grpControl.Controls.Add(Me.btnGenerate)
            Me.grpControl.Controls.Add(Me.btnPlay)
            Me.grpControl.Controls.Add(Me.btnPause)
            Me.grpControl.Controls.Add(Me.btnStop)
            Me.grpControl.Controls.Add(Me.btnEditTxt)
            Me.grpControl.Controls.Add(Me.lblStatus)
            Me.grpControl.Controls.Add(Me.progressBar)
            Me.grpControl.Controls.Add(Me.lblPosition)
            Me.grpControl.Location = New System.Drawing.Point(12, 108)
            Me.grpControl.Size = New System.Drawing.Size(740, 100)
            Me.grpControl.Text = "操作"
            Me.grpControl.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or
                                           System.Windows.Forms.AnchorStyles.Left) Or
                                           System.Windows.Forms.AnchorStyles.Right),
                                           System.Windows.Forms.AnchorStyles)

            ' ===== txtLog =====
            Me.txtLog.Location = New System.Drawing.Point(15, 22)
            Me.txtLog.Size = New System.Drawing.Size(620, 200)
            Me.txtLog.Multiline = True
            Me.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.txtLog.ReadOnly = True
            Me.txtLog.Font = New System.Drawing.Font("Consolas", 9.0F)
            Me.txtLog.BackColor = System.Drawing.Color.Black
            Me.txtLog.ForeColor = System.Drawing.Color.LightGreen
            Me.txtLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or
                                        System.Windows.Forms.AnchorStyles.Bottom) Or
                                        System.Windows.Forms.AnchorStyles.Left) Or
                                        System.Windows.Forms.AnchorStyles.Right),
                                        System.Windows.Forms.AnchorStyles)

            ' ===== btnClearLog =====
            Me.btnClearLog.Location = New System.Drawing.Point(645, 22)
            Me.btnClearLog.Size = New System.Drawing.Size(80, 30)
            Me.btnClearLog.Text = "清空日志"
            Me.btnClearLog.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or
                                           System.Windows.Forms.AnchorStyles.Right),
                                           System.Windows.Forms.AnchorStyles)

            ' ===== grpLog =====
            Me.grpLog.Controls.Add(Me.txtLog)
            Me.grpLog.Controls.Add(Me.btnClearLog)
            Me.grpLog.Location = New System.Drawing.Point(12, 214)
            Me.grpLog.Size = New System.Drawing.Size(740, 240)
            Me.grpLog.Text = "日志"
            Me.grpLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or
                                        System.Windows.Forms.AnchorStyles.Bottom) Or
                                        System.Windows.Forms.AnchorStyles.Left) Or
                                        System.Windows.Forms.AnchorStyles.Right),
                                        System.Windows.Forms.AnchorStyles)

            ' ===== playTimer =====
            Me.playTimer.Interval = 200
            Me.playTimer.Enabled = False

            ' ===== MainForm =====
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0F, 12.0F)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(764, 461)
            Me.Controls.Add(Me.grpFile)
            Me.Controls.Add(Me.grpControl)
            Me.Controls.Add(Me.grpLog)
            Me.MinimumSize = New System.Drawing.Size(780, 500)
            Me.Text = "MIDI 音乐生成工具 - VB.NET"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.0F)

            Me.grpFile.ResumeLayout(False)
            Me.grpFile.PerformLayout()
            Me.grpControl.ResumeLayout(False)
            Me.grpControl.PerformLayout()
            Me.grpLog.ResumeLayout(False)
            Me.grpLog.PerformLayout()
            Me.ResumeLayout(False)
        End Sub

        Friend WithEvents lblTxtFile As System.Windows.Forms.Label
        Friend WithEvents txtTxtFile As System.Windows.Forms.TextBox
        Friend WithEvents btnBrowseTxt As System.Windows.Forms.Button
        Friend WithEvents lblMidiFile As System.Windows.Forms.Label
        Friend WithEvents txtMidiFile As System.Windows.Forms.TextBox
        Friend WithEvents btnBrowseMidi As System.Windows.Forms.Button
        Friend WithEvents btnGenerate As System.Windows.Forms.Button
        Friend WithEvents btnPlay As System.Windows.Forms.Button
        Friend WithEvents btnPause As System.Windows.Forms.Button
        Friend WithEvents btnStop As System.Windows.Forms.Button
        Friend WithEvents btnEditTxt As System.Windows.Forms.Button
        Friend WithEvents lblStatus As System.Windows.Forms.Label
        Friend WithEvents lblPosition As System.Windows.Forms.Label
        Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
        Friend WithEvents grpFile As System.Windows.Forms.GroupBox
        Friend WithEvents grpControl As System.Windows.Forms.GroupBox
        Friend WithEvents grpLog As System.Windows.Forms.GroupBox
        Friend WithEvents txtLog As System.Windows.Forms.TextBox
        Friend WithEvents btnClearLog As System.Windows.Forms.Button
        Friend WithEvents playTimer As System.Windows.Forms.Timer

    End Class

End Namespace
