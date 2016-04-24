Public Class DIServer_StockTransfer : Inherits CPSLIB.DIServer.StockTransfer
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _DIConn As CPSLIB.DIServer.DIServerConnection
    Dim _Setting As Settings
    Dim _InventoryTransaction As InventoryTransactionForWMS
    Dim _InventoryTransactionConfig As InventoryTransactionConfig

    Public Shared FLD_HDR_DocDate As String = "DocDate"
    Public Shared FLD_HDR_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_HDR_FromWhse As String = "FromWarehouse"
    Public Shared FLD_HDR_Series As String = "Series"
    Public Shared FLD_DTL_BaseType As String = "BaseType"
    Public Shared FLD_DTL_BaseLine As String = "BaseLine"
    Public Shared FLD_DTL_ItemCode As String = "ItemCode"
    Public Shared FLD_DTL_Quantity As String = "Quantity"
    Public Shared FLD_DTL_BaseEntry As String = "BaseEntry"
    Public Shared FLD_DTL_Warehouse As String = "WarehouseCode"
    Public Shared FLD_DTL_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_DTL_WMSLineNum As String = "U_WMSLine"
    Public Shared FLD_DTL_UseBaseUnits As String = "UseBaseUnits"
    Public Shared FLD_HDR_FrmLocCode As String = "U_FrmLocCode"
    Public Shared FLD_HDR_ToLocCode As String = "U_ToLocCode"
    Public Shared FLD_HDR_AllowBTChange As String = "U_AllowBTChange"

    Public Sub New(ByVal _Setting As Settings, ByVal _DIConn As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_DIConn)
        Me._DIConn = _DIConn
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _InventoryTransaction = New InventoryTransactionForWMS(_Setting, Nothing)
        _InventoryTransactionConfig = New InventoryTransactionConfig(_Setting)

    End Sub


    Public Function Create(ByVal _WMSDocNum As String) As Boolean
        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _ret As Boolean
        Dim _TargetEntry As String
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim SAPConnection As SAPSQLConnections
        Dim _batchNumber As CPSLIB.DIServer.BatchNumbers
        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)
        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[WTQ1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        mSql = String.Format("Select distinct {0},{1},{2},{3},{6},{7},{8} " & _
                             "From [dbo].[CPS_TBL_OWTR] " & _
                             "Where ISNULL(TRXSTATUS,'') ='' " & _
                             "and FrmWhsCode <> ToWhsCode and {4} = '{5}' ", _
                             InventoryTransaction.Fld_DocEntry, _
                             InventoryTransaction.Fld_DocDate, _
                             InventoryTransaction.Fld_FrmWhsCode, _
                             InventoryTransaction.Fld_ReceiveEntry, _
                             InventoryTransaction.Fld_ReceiveEntry, _
                             _WMSDocNum, _
                             InventoryTransaction.Fld_FrmLocCode, _
                             InventoryTransaction.Fld_ToLocCode, _
                             InventoryTransaction.Fld_AllowBTChange)

        _dt_DocEntry = _InventoryTransaction.ExecuteDatatable(mSql)

        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            Try
                _TargetEntry = ""
                NewDocument()
                SetValue(FLD_HDR_DocDate, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_DocDate))
                SetValue(FLD_HDR_WMSEntry, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_ReceiveEntry))
                SetValue(FLD_HDR_FromWhse, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_FrmWhsCode))
                SetValue(FLD_HDR_Series, SAPConnection.GetSeries(_dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_DocEntry), "OWTQ"))
                setUDF("OWTQ", _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_DocEntry))
                'Add From To Logic By Jerry
                SetValue(FLD_HDR_FrmLocCode, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_FrmLocCode))
                SetValue(FLD_HDR_ToLocCode, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_ToLocCode))
                SetValue(FLD_HDR_AllowBTChange, _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_AllowBTChange))

                mSql = String.Format("Select {0}, {1}, {2},{3},{12},{13}, sum({4}) as 'BatchQuantity' " & _
                                                                   "From [dbo].[CPS_TBL_OWTR] " & _
                                                                   "Where {5} = '{6}' and isNull({11},'') = '' and FrmWhsCode <> ToWhsCode  Group by {7},{8},{9},{10},{12},{13}", _
                                                                   InventoryTransaction.Fld_DocEntry, _
                                                                   InventoryTransaction.Fld_LineNum, _
                                                                   InventoryTransaction.Fld_ItemCode, _
                                                                   InventoryTransaction.Fld_ToWhsCode, _
                                                                   InventoryTransaction.Fld_BatchQuantity, _
                                                                   _InventoryTransactionConfig.KeyField, _
                                                                   _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField),
                                                                   InventoryTransaction.Fld_DocEntry, _
                                                                   InventoryTransaction.Fld_LineNum, _
                                                                   InventoryTransaction.Fld_ItemCode, _
                                                                   InventoryTransaction.Fld_ToWhsCode, InventoryTransaction.Fld_TrxStatus, InventoryTransaction.Fld_ReceiveEntry, InventoryTransaction.Fld_ReceiveLineNum)

                _dt_DocLine = _InventoryTransaction.ExecuteDatatable(mSql)
                _Debug.WriteTable(_dt_DocLine, "DataTable")
                For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
                    If _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry) >= 0 Then
                        setRowsValue(FLD_DTL_BaseType, "1250000001")
                        setRowsValue(FLD_DTL_BaseEntry, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry))
                        setUDF("WTQ1", _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry), _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum))
                    End If
                    If _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum) >= 0 Then
                        setRowsValue(FLD_DTL_BaseLine, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum))
                    End If
                    setRowsValue(FLD_DTL_ItemCode, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ItemCode))
                    setRowsValue(FLD_DTL_Quantity, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_BatchQuantity))
                    setRowsValue(FLD_DTL_Warehouse, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ToWhsCode))
                    setRowsValue(FLD_DTL_WMSEntry, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ReceiveEntry))
                    setRowsValue(FLD_DTL_WMSLineNum, _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ReceiveLineNum))
                    setRowsValue(FLD_DTL_UseBaseUnits, "tYES")
                    _dt_BatchNum = _InventoryTransaction.ExecuteDatatable(String.Format("Select {0}, {1} " & _
                                                                   "From [dbo].[CPS_TBL_OWTR] " & _
                                                                   "Where {2} = '{3}' and {4} = '{5}' and isNull({6},'') = ''", _
                                                                   InventoryTransaction.Fld_BatchNum, _
                                                                   InventoryTransaction.Fld_BatchQuantity, _
                                                                  _InventoryTransactionConfig.KeyField, _
                                                                   _dt_DocLine.Rows(j).Item(_InventoryTransactionConfig.KeyField), _
                                                                   _InventoryTransactionConfig.KeyLineField, _
                                                                   _dt_DocLine.Rows(j).Item(_InventoryTransactionConfig.KeyLineField), _
                                                                   InventoryTransaction.Fld_TrxStatus))
                    For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
                        If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)) Then

                            If Settings.DBNull(_dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)) <> String.Empty Then

                                _batchNumber = New CPSLIB.DIServer.BatchNumbers
                                _batchNumber.BatchNumber = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)
                                If _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry) > 0 Then
                                    mSql = String.Format(sqlGetNumPerMsg, _
                                                            _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry), _
                                                            _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum))
                                    mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)
                                    _batchNumber.Quantity = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchQuantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
                                Else
                                    _batchNumber.Quantity = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchQuantity)
                                End If
                                setBatchNumberRow(_batchNumber)
                            End If
                        End If
                    Next
                    AddRow()
                Next
                _Debug.Write("Ready to post")
                If MyBase.Post(Command.AddObject) = CommandStatus.Fail Then
                    _ret = False
                    _InventoryTransaction.UpdateErrorStatus(_InventoryTransactionConfig.KeyField, _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField), "-1", MyBase.CmdMessage)
                Else
                    _ret = True
                    _TargetEntry = NewEntry
                    _InventoryTransaction.UpdateSuccessStatus(_InventoryTransactionConfig.KeyField, _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField), NewEntry, String.Empty)
                End If
            Catch ex As Exception
                _ret = False
                _InventoryTransaction.UpdateErrorStatus(_InventoryTransactionConfig.KeyField, _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField), "-1", ex.Message)
            End Try
        Next
        Return _ret
    End Function
End Class
