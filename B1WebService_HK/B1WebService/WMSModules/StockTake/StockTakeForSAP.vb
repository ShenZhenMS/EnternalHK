Public Class StockTakeForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlStatus As String = "SELECT * FROM CPS_FUNC_STOCKTAKERESULT('{0}')"

    
    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function GetResult(ByVal _RefNum As String) As String
        Dim _ret As String
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlStatus, _RefNum.Replace("'", "''")))
            If _dt.Rows.Count > 0 Then
                _ret = _dt.Rows(0)(0)
            Else
                _ret = "Pending"
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            _ret = String.Empty
        End Try
        Return _ret
    End Function
    

End Class
