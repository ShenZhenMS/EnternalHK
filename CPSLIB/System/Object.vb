
Public Class CPSObject
    Public Shared Sub Release(ByVal o As Object)
        Try
            If Not o Is Nothing Then
                System.Runtime.InteropServices.Marshal.ReleaseComObject(o)
            End If

        Catch ex As Exception

        End Try
    End Sub
End Class


