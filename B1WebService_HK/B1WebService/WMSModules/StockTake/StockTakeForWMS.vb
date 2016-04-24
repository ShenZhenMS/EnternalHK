Public Class StockTakeForWMS : Inherits Purchase

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogStockTake '{0}','{1}','{2}','{3}'"
    Dim _sqlOSLOGTABLE As String = "SELECT * FROM CPS_TBL_StockTake WHERE isNull(TrxStatus,'') not in ('E','F') and 1 = 1 "
    Dim _sqlStockTakeResult As String = "SELECT * FROM CPS_FUNC_STOCKTAKERESULT('{0}')"



    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting, _SAPDIConn)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToLogTable(ByVal _dt As DataTable) As Boolean
        _Debug.WriteTable(_dt, "StockTake Table From XML")
        Dim _ret As Boolean = True
        Try
            For Each _dr As DataRow In _dt.Rows
                If ToLogTable(_dr) = False Then

                End If
            Next
        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _ret
    End Function

    Public Function OSStockTake() As DataTable
        Dim _dt As DataTable = Nothing

        Try

            _dt = MyBase.ExecuteDatatable(_sqlOSLOGTABLE)

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function StockTakeResult(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable = Nothing

        Try

            _dt = MyBase.ExecuteDatatable(String.Format(_sqlStockTakeResult, _ReceiveEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _dt
    End Function

    '--------------MK Development-------------------------
    Private Function ToLogTable(ByVal _dr As DataRow) As Boolean
        Dim _sql As String
        


        Try
            _sql = String.Format(_sqlCreateHist, Settings.DBNull(_dr(StockTake.Fld_WMSENtry)), Settings.DBNull(_dr(StockTake.Fld_RefNum)), Settings.DBNull(_dr(StockTake.Fld_AdjType)), Settings.DBNull(_dr(StockTake.Fld_WMSUser)))

            _Debug.Write(_sql)
            MyBase.ExecuteUpdate(_sql)

            If MyBase.isError Then


                Throw New Exception(MyBase.Message)
            End If

        Catch ex As Exception

            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            Return False
        End Try
        Return True
    End Function

End Class
