' ============================================================================
'  App.vb
'  应用程序入口
' ============================================================================
Option Strict On
Option Explicit On

Namespace Midi

    Friend Module App

        ''' <summary>
        ''' 应用程序主入口
        ''' </summary>
        <STAThread()>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            Try
                Using form As New MainForm()
                    Application.Run(form)
                End Using
            Catch ex As Exception
                MessageBox.Show("程序发生未处理异常: " & ex.Message & Environment.NewLine &
                                Environment.NewLine & ex.StackTrace,
                                "错误", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

    End Module

End Namespace
