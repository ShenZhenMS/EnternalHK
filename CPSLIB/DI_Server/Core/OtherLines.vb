Public Class OtherLines
    



    Private _Debug As CPSLIB.Debug
    Private _CPSException As CPSException
    Private _htLines As Hashtable
  

    Public Const FieldPrefix As String = "<{0}>{1}</{0}>"

    Public Sub New()
        _Debug = New CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSException()
        _htLines = New Hashtable
      
    End Sub
#Region "Property"
    Public Sub setValue(ByVal strFieldName As String, ByVal value As Object)
        _htLines(strFieldName) = value
    End Sub
    Public Function getValue(ByVal strFieldName As String) As Object
        Return _htLines(strFieldName)

    End Function
#End Region
   

    

#Region "Operation"
    Public Sub Clear()
        _htLines.Clear()
    End Sub

   

    Public Function GenerateLine() As String
        Dim _Cmd As String = String.Empty

        If Not _htLines Is Nothing Then
            For Each o As Object In _htLines.Keys
                _Cmd = _Cmd & String.Format(FieldPrefix, o.ToString, _htLines(o.ToString))
            Next
        End If

        Return String.Format(DIServer.DI_Object.DocRowXML, _Cmd)
    End Function
#End Region

#Region "Property"
   
#End Region

End Class
