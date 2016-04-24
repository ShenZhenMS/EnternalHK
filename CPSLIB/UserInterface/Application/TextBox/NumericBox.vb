Public Class NumericBox : Inherits Windows.Forms.TextBox

    Public Sub New()
        MyBase.New()

    End Sub
    Private Function TrapKey(ByVal KCode As String) As Boolean
        If (KCode >= 48 And KCode <= 57) Or KCode = 8 Then
            TrapKey = False
        Else
            TrapKey = True
        End If
    End Function

    Private Sub TextBox1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        e.Handled = TrapKey(Asc(e.KeyChar))
    End Sub
End Class
