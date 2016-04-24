Public Class SalesForWMS : Inherits Sales

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogSales '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}'"
    Dim _sqlSalesResult As String = "SELECT * FROM CPS_FUNC_SALESRESULT('{0}')"

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)

        MyBase.New(_Setting, _SAPDIConn)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToSalesTable(ByVal _dt As DataTable) As Boolean
        Dim _ret As Boolean
        Try
            For Each _dr As DataRow In _dt.Rows
                _ret = ToSalesTable(_dr)
            Next
        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)

        End Try
        Return _ret
    End Function

    Private Function ToSalesTable(ByVal _dr As DataRow) As Boolean

        Dim mSql As String
        mSql = String.Format(_sqlCreateHist,
                                        "17",
                                        Settings.DBNull(_dr(Fld_DocEntry), "-1"), _
                                       Settings.DBNull(_dr(Fld_LineNum), "-1"), _
                                       Settings.DBNull(_dr(Fld_DocNum), "-1"), _
                                       Convert.ToDateTime(_dr(Fld_DocDueDate)).ToString("yyyyMMdd"), _
                                       _dr(Fld_CardCode).ToString.Replace("'", "''"), _
                                       _dr(Fld_CardName).ToString.Replace("'", "''"), _
                                       _dr(Fld_ItemCode).ToString.Replace("'", "''"), _
                                       _dr(Fld_ItemName).ToString.Replace("'", "''"), _
                                       _dr(Fld_Quantity).ToString.Replace("'", "''"), _
                                       _dr(Fld_UOM).ToString.Replace("'", "''"), _
                                       _dr(Fld_WhsCode).ToString.Replace("'", "''"), _
                                       _dr(Fld_WhsName).ToString.Replace("'", "''"), _
                                       _dr(Fld_BatchNum).ToString.Replace("'", "''"), _
                                       _dr(Fld_LocCode).ToString.Replace("'", "''"), _
                                       _dr(Fld_ReceiveEntry).ToString.Replace("'", "''"),
                                       _dr(Fld_PickNum).ToString.Replace("'", "''"),
                                       _dr(Fld_ReceiveLineNum),
                                       _dr(Fld_LineQuantity), Convert.ToDateTime(Settings.DBNull(_dr(Fld_NewDocDueDate), "1970-01-01")).ToString("yyyyMMdd"))

        Try
            MyBase.ExecuteUpdate(mSql)
            Return True
        Catch ex As Exception

            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            Return False
        End Try

    End Function

    Public Function SalesResult(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable
        Try
            MyBase.ExecuteDatatable(String.Format(_sqlSalesResult, _ReceiveEntry))
        Catch ex As Exception

        End Try
    End Function
End Class
