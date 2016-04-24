Public Class Tools
#Region "String Function"
    Public Shared Function setData(ByVal o As String, ByVal c As Char, ByVal i As Integer) As String
        Dim ret As String = ""
        If o.Length >= i Then
            ret = o
        Else
            ret = o
            For ii As Integer = 1 To i - o.length
                ret = c & ret
            Next
        End If
        Return ret
    End Function

    Public Shared Function ReverseHashTable(ByVal _ht As Hashtable) As Hashtable
        Dim _revht As New Hashtable
        For Each o As Object In _ht.Keys
            _revht(o.ToString) = _ht(o)
        Next
        Return _revht
    End Function
#End Region
End Class
