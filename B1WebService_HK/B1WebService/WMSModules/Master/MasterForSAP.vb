Public Class MasterForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    
    
    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub


#Region "Batch Information"
    Public Function UpdateBatchInfo(ByVal _ItemCode As String, ByVal _BatchNum As String, ByVal _WhsCode As String, ByVal _MfrDate As String, ByVal _ExpDate As String)
        Dim _ret As Boolean = True
      
        Dim _sql As String = "Update OBTN set MnfDate = '{0}', ExpDate = '{1}' Where Itemcode = '{2}' and DistNumber = '{3}' "
        Try
            If MyBase.ExecuteUpdate(String.Format(_sql, Convert.ToDateTime(Settings.DBNull(_MfrDate, "1970-01-01")).ToString("yyyyMMdd"), _
                                               Convert.ToDateTime(Settings.DBNull(_ExpDate, "1970-01-01")).ToString("yyyyMMdd"), _
                                               _ItemCode, _BatchNum)) > 0 Then
                _ret = True
            Else
                _ret = False
            End If

        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret
    End Function
#End Region
End Class
