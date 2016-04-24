Public Class WS_StockTransfer : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _StockTransfer As StockTransfer.StockTransferService
    Dim _InventoryTransactionConfig As InventoryTransactionConfig
    Dim _InventoryTransaction As InventoryTransactionForWMS

    Dim _htDocStatus As Hashtable

    Private _isError As Boolean

    Private _Message As String
    Public Overrides Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Public Overrides Property isError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property


    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        If MyBase.isConnected = False Then
            isError = True
            Message = MyBase.Message
        End If
        _InventoryTransactionConfig = New InventoryTransactionConfig(_Setting)
        _InventoryTransaction = New InventoryTransactionForWMS(_Setting, Nothing)
        _htDocStatus = New Hashtable
    End Sub

    Public Function Create_InventoryTransfer() As Boolean


        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _msgHeader As StockTransfer.MsgHeader
        Dim oDoc As StockTransfer.StockTransfer
        Dim _ret As Boolean
        Dim _TargetEntry As String
        Dim docLine As StockTransfer.StockTransferStockTransferLine
        Dim docLineBatch As StockTransfer.StockTransferStockTransferLineBatchNumber
        Dim DocParams As StockTransfer.StockTransferParams
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList
        _alDocumentLine = New ArrayList
        _alDocumentLineBatch = New ArrayList
        Dim SAPConnection As SAPSQLConnections

        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[WTQ1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        mSql = String.Format("Select distinct {0},{1},{2},{3} " & _
                             "From [dbo].[CPS_TBL_OWTR] " & _
                             "Where ISNULL(TRXSTATUS,'') ='' and FrmWhsCode <> ToWhsCode ", _
                             InventoryTransaction.Fld_DocEntry, _
                             InventoryTransaction.Fld_DocDate, _
                             InventoryTransaction.Fld_FrmWhsCode, _
                             InventoryTransaction.Fld_ReceiveEntry)

        _dt_DocEntry = _InventoryTransaction.ExecuteDatatable(mSql)

        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            Try
                oDoc = Nothing
                _StockTransfer = Nothing
                _msgHeader = Nothing
                _TargetEntry = ""
                _alDocumentLineBatch.Clear()
                _alDocumentLine.Clear()
                _StockTransfer = New StockTransfer.StockTransferService
                _msgHeader = New StockTransfer.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = StockTransfer.MsgHeaderServiceName.StockTransferService
                _msgHeader.ServiceNameSpecified = True
                _StockTransfer.MsgHeaderValue = _msgHeader
                oDoc = New StockTransfer.StockTransfer

                oDoc.DocDate = _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_DocDate)
                oDoc.DocDateSpecified = True
                oDoc.U_WMSEntry = _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_ReceiveEntry)
                oDoc.FromWarehouse = _dt_DocEntry.Rows(i).Item(InventoryTransaction.Fld_FrmWhsCode)
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
                    _alDocumentLineBatch.Clear()
                    docLine = Nothing
                    docLine = New StockTransfer.StockTransferStockTransferLine


                    If _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry) >= 0 Then
                        docLine.BaseTypeSpecified = True
                        docLine.BaseType = 5
                        docLine.BaseEntrySpecified = True
                        docLine.BaseEntry = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry)
                    End If
                    If _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum) >= 0 Then
                        docLine.BaseLineSpecified = True
                        docLine.BaseLine = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum)
                    End If

                    docLine.ItemCode = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ItemCode)
                    docLine.QuantitySpecified = True
                    docLine.Quantity = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_BatchQuantity)
                    docLine.WarehouseCode = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ToWhsCode)
                    docLine.U_WMSEntry = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ReceiveEntry)
                    docLine.U_WMSLineNumSpecified = True
                    docLine.U_WMSLineNum = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_ReceiveLineNum)
                    docLine.UseBaseUnits = StockTransfer.StockTransferStockTransferLineUseBaseUnits.tYES
                    docLine.UseBaseUnitsSpecified = True

                    _dt_BatchNum = _InventoryTransaction.ExecuteDatatable(String.Format("Select {0}, {1} " & _
                                                                   "From [dbo].[CPS_TBL_OWTR] " & _
                                                                   "Where {2} = '{3}' and {4} = '{5}' and isNull({6},'') = ''", _
                                                                   InventoryTransaction.Fld_BatchNum, _
                                                                   InventoryTransaction.Fld_BatchQuantity, _
                                                                  _InventoryTransactionConfig.KeyField, _
                                                                   _dt_DocLine.Rows(j).Item(_InventoryTransactionConfig.KeyField), _
                                                                   _InventoryTransactionConfig.KeyLineField, _
                                                                   _dt_DocLine.Rows(j).Item(_InventoryTransactionConfig.KeyLineField), InventoryTransaction.Fld_TrxStatus))

                
                    For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
                        If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)) Then

                            If Settings.DBNull(_dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)) <> String.Empty Then

                                docLineBatch = Nothing
                                docLineBatch = New StockTransfer.StockTransferStockTransferLineBatchNumber
                                'docLineBatch.BaseLineNumberSpecified = True
                                'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum)
                                docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchNum)
                                docLineBatch.QuantitySpecified = True


                                If docLine.BaseEntrySpecified Then
                                    mSql = String.Format(sqlGetNumPerMsg, _
                                                         _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_DocEntry), _
                                                         _dt_DocLine.Rows(j).Item(InventoryTransaction.Fld_LineNum))
                                    mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

                                    docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchQuantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
                                Else
                                    docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(InventoryTransaction.Fld_BatchQuantity)
                                End If

                                _alDocumentLineBatch.Add(docLineBatch)
                            End If

                        End If
                    Next
                    If _alDocumentLineBatch.Count > 0 Then
                        docLine.BatchNumbers = MyBase.StockTransfertoDocumentLineBatchArray(_alDocumentLineBatch)
                    End If

                    _alDocumentLine.Add(docLine)
                Next
                oDoc.StockTransferLines = MyBase.StockTransfertoDocumentLineArray(_alDocumentLine)
                _Debug.Write("Ready to post")
                _Debug.Write(oDoc.StockTransferLines.Count, "Number of Line Records")

                DocParams = _StockTransfer.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _ret = True
                    _isError = False
                    _InventoryTransaction.UpdateSuccessStatus(_InventoryTransactionConfig.KeyField, _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField), DocParams.DocEntry, String.Empty)
                    _TargetEntry = DocParams.DocEntry.ToString
                End If

            Catch ex As Exception
                _ret = False
                _InventoryTransaction.UpdateErrorStatus(_InventoryTransactionConfig.KeyField, _dt_DocEntry.Rows(i).Item(_InventoryTransactionConfig.KeyField), "-1", ex.Message)
                _Message = ex.Message
                _isError = True
            End Try



        Next

        '_TargetEntry = Left(_TargetEntry, _TargetEntry.Length - 1)

        Return _ret


    End Function
End Class


