Public Class InventoryTransactionForWMS : Inherits InventoryTransaction

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogInventoryTransfer '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}'," & _
    "'{17}','{18}','{19}','{20}','{21}'"
    Dim _sqlInventoryTransResult As String = "SELECT * FROM CPS_FUNC_INVENTORYRESULT('{0}')"
    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)

        MyBase.New(_Setting, _SAPDIConn)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToInventoryTransferTable(ByVal _dt As DataTable) As Boolean

        Try
            For Each _dr As DataRow In _dt.Rows
                _Debug.Write("ToInventoryTransferTable")
                ToInventoryTransferTable(_dr)
            Next
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Function

    Private Function ToInventoryTransferTable(ByVal _dr As DataRow) As Boolean
        _Debug.Write("Check Point A")
        Dim _sql As String
      

        Try
            _sql = String.Format(_sqlCreateHist,
                                        Settings.DBNull(_dr(Fld_DocEntry), "-1"), _
                                       Settings.DBNull(_dr(Fld_LineNum), "-1"), _
                                       Settings.DBNull(_dr(Fld_DocNum)), _
                                       Convert.ToDateTime(Settings.DBNull(_dr(Fld_DocDate), "1970-01-01")).ToString("yyyyMMdd"), _
                                       Settings.DBNull(_dr(Fld_ItemCode)), _
                                       Settings.DBNull(_dr(Fld_Quantity), "0"), _
                                       Settings.DBNull(_dr(Fld_UOM)), _
                                       Settings.DBNull(_dr(Fld_FrmWhsCode)), _
                                       Settings.DBNull(_dr(Fld_ToWhsCode)), _
                                       Settings.DBNull(_dr(Fld_LineQuantity), "0"), _
                                       Settings.DBNull(_dr(Fld_OldItemCode)), _
                                       Settings.DBNull(_dr(Fld_BatchNum)), _
                                       Settings.DBNull(_dr(Fld_TransferType)), _
                                       Settings.DBNull(_dr(Fld_LocCode)), _
                                       Settings.DBNull(_dr(Fld_BatchQuantity), "0"), _
                                       Settings.DBNull(_dr(Fld_ItemPerUnit), "0"), _
                                       Settings.DBNull(_dr(Fld_ReceiveLineNum), "-1"), _
                                       Settings.DBNull(_dr(Fld_ReceiveEntry)), _
                                       Settings.DBNull(_dr(Fld_Counter)), _
                                       Settings.DBNull(_dr(Fld_FrmLocCode), ""), _
                                       Settings.DBNull(_dr(Fld_ToLocCode), ""), _
                                       Settings.DBNull(_dr(Fld_AllowBTChange), "N")
                                       )

            _Debug.Write(_sql)
            MyBase.ExecuteUpdate(_sql)
            If MyBase.isError Then
                _Debug.Write("Error Found: " & MyBase.ErrorMessage)
                Throw New Exception(MyBase.Message)

            End If
        Catch ex As Exception
            _Debug.Write(ex.Message, "Exception on create inventory transfer record")
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            Return False
        End Try
        Return True
    End Function

    Public Function InventoryTransResult(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlInventoryTransResult, _ReceiveEntry))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _dt
    End Function
End Class
