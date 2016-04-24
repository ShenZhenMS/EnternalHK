Public Class WS_StockOut : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable

    Dim _StockOut As InventoryIssue.InventoryGenExitService
    Dim _InventoryTranConfig As InventoryInoutConfig

    Dim _InventoryTran As InventoryInoutForWMS
    Dim _SAPInventory As InventoryInoutForSAP
    Dim _htKeyValue As Hashtable
    Dim _htDocStatus As Hashtable
    Public Shared ObjType As String = "60"
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
        _SAPInventory = New InventoryInoutForSAP(_Setting, Nothing)
        _InventoryTranConfig = New InventoryInoutConfig(_Setting)
        _InventoryTran = New InventoryInoutForWMS(_Setting, Nothing, InventoryInoutForWMS._DocumentType.GI)
        _htDocStatus = New Hashtable
    End Sub

    Public Function Start(ByVal _o As String) As Boolean
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty

        Dim _msgHeader As InventoryIssue.MsgHeader
        Dim _StockIn As InventoryIssue.InventoryGenExitService

        Dim oDoc As InventoryIssue.Document
        Dim docLine As InventoryIssue.DocumentDocumentLine
        Dim docLineBatch As InventoryIssue.DocumentDocumentLineBatchNumber
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLIneBatch As ArrayList
        Dim _LineQuantity As Decimal
        Dim DocParams As InventoryIssue.DocumentParams
        _alDocumentLine = New ArrayList
        _alDocumentLIneBatch = New ArrayList

        _alDocumentLIneBatch.Clear()
        _alDocumentLine.Clear()




        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
            If _drRow.Length > 0 Then
                _StockIn = New InventoryIssue.InventoryGenExitService
                oDoc = New InventoryIssue.Document

                _msgHeader = New InventoryIssue.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = InventoryIssue.MsgHeaderServiceName.InventoryGenExitService
                _msgHeader.ServiceNameSpecified = True
                _StockOut.MsgHeaderValue = _msgHeader

                oDoc.DocDate = Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate))
                oDoc.Series = _SAPInventory.DocSeries(WS_StockOut.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries))
                oDoc.SeriesSpecified = True
                oDoc.U_WMSEntry = _drRow(0)(Inventory_Inout.Fld_ReceiveEntry)
                oDoc.U_WMSUser = _drRow(0)(Inventory_Inout.Fld_WMSUser)
                oDoc.DocDateSpecified = True
                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line
                            docLine.Quantity = _LineQuantity
                            docLine.QuantitySpecified = True
                            docLine.ItemCode = _PrevItemCode
                            docLine.BatchNumbers = InventoryIssuetoDocumentLineBatchArray(_alDocumentLIneBatch)
                            _alDocumentLine.Add(docLine)
                            _alDocumentLIneBatch.Clear()
                            _LineQuantity = 0

                        End If
                        docLine = New InventoryIssue.DocumentDocumentLine

                    End If
                    docLineBatch = Nothing
                    docLineBatch = New InventoryIssue.DocumentDocumentLineBatchNumber
                    If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
                        docLineBatch.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
                        docLineBatch.QuantitySpecified = True
                        docLineBatch.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetSalesItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        _alDocumentLIneBatch.Add(docLineBatch)
                    End If
                    docLine.WarehouseCode = dr(InventoryInoutForWMS.Fld_WhsCode)
                    docLine.PriceSpecified = True
                    docLine.AccountCode = _SAPInventory.GLAccount(WS_StockOut.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), dr(Inventory_Inout.Fld_DocSeries))
                    docLine.Price = _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode))

                    docLine.U_WMSEntry = dr(Inventory_Inout.Fld_ReceiveEntry)

                    docLine.U_WMSLineNum = dr(Inventory_Inout.Fld_ReceiveLineNum)
                    docLine.U_WMSLineNumSpecified = True
                    _LineQuantity = _LineQuantity + dr(Inventory_Inout.Fld_Quantity)

                    _PrevLineNum = dr(_InventoryTranConfig.KeyLineField)
                    _PrevItemCode = dr(InventoryInoutForWMS.Fld_ItemCode)
                Next

                docLine.Quantity = _LineQuantity
                docLine.QuantitySpecified = True
                docLine.ItemCode = _PrevItemCode

                If _alDocumentLIneBatch.Count > 0 Then
                    docLine.BatchNumbers = InventoryIssuetoDocumentLineBatchArray(_alDocumentLIneBatch)
                End If

                _alDocumentLine.Add(docLine)

                oDoc.DocumentLines = InventoryIssueDocumentLineArray(_alDocumentLine)

                _Debug.Write("create to actrual document")
                DocParams = _StockIn.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Debug.Write("Update Success Status")

                    _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
                    _ret = True


                End If

            End If
        Catch ex As Exception
            _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function StartDraft(ByVal _o As String) As Boolean
        _Debug.Write(String.Format("Creating Draft Document: Key Value : {0}", _o))
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty

        Dim _msgHeader As DocDraft.MsgHeader
        Dim _StockIn As DocDraft.DraftsService

        Dim oDoc As DocDraft.Document
        Dim docLine As DocDraft.DocumentDocumentLine
        Dim docLineBatch As DocDraft.DocumentDocumentLineBatchNumber
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLIneBatch As ArrayList
        Dim _LineQuantity As Decimal
        Dim DocParams As DocDraft.DocumentParams
        _alDocumentLine = New ArrayList
        _alDocumentLIneBatch = New ArrayList

        _alDocumentLIneBatch.Clear()
        _alDocumentLine.Clear()




        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
            If _drRow.Length > 0 Then
                _StockIn = New DocDraft.DraftsService
                oDoc = New DocDraft.Document
                oDoc.DocObjectCode = WS_StockOut.ObjType
                _msgHeader = New DocDraft.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
                _msgHeader.ServiceNameSpecified = True
                _StockIn.MsgHeaderValue = _msgHeader

                oDoc.DocDate = Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate))
                oDoc.Series = _SAPInventory.DocSeries(WS_StockOut.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries))
                oDoc.SeriesSpecified = True
                oDoc.U_WMSEntry = _drRow(0)(Inventory_Inout.Fld_ReceiveEntry)
                oDoc.U_WMSUser = _drRow(0)(Inventory_Inout.Fld_WMSUser)
                oDoc.DocDateSpecified = True
                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line
                            docLine.Quantity = _LineQuantity
                            docLine.QuantitySpecified = True
                            docLine.ItemCode = _PrevItemCode
                            docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
                            _alDocumentLine.Add(docLine)
                            _alDocumentLIneBatch.Clear()
                            _LineQuantity = 0

                        End If
                        docLine = New DocDraft.DocumentDocumentLine

                    End If
                    docLineBatch = Nothing
                    docLineBatch = New DocDraft.DocumentDocumentLineBatchNumber
                    If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
                        docLineBatch.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
                        docLineBatch.QuantitySpecified = True
                        docLineBatch.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetSalesItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        _alDocumentLIneBatch.Add(docLineBatch)
                    End If
                    docLine.WarehouseCode = dr(InventoryInoutForWMS.Fld_WhsCode)
                    docLine.PriceSpecified = True
                    docLine.AccountCode = _SAPInventory.GLAccount(WS_StockOut.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), dr(InventoryInoutForWMS.Fld_DocSeries))

                    docLine.Price = _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode))

                    docLine.U_WMSEntry = dr(Inventory_Inout.Fld_ReceiveEntry)

                    docLine.U_WMSLineNum = dr(Inventory_Inout.Fld_ReceiveLineNum)
                    docLine.U_WMSLineNumSpecified = True
                    _LineQuantity = _LineQuantity + dr(Inventory_Inout.Fld_Quantity)

                    _PrevLineNum = dr(_InventoryTranConfig.KeyLineField)
                    _PrevItemCode = dr(InventoryInoutForWMS.Fld_ItemCode)
                Next

                docLine.Quantity = _LineQuantity
                docLine.QuantitySpecified = True
                docLine.ItemCode = _PrevItemCode

                If _alDocumentLIneBatch.Count > 0 Then
                    docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
                End If

                _alDocumentLine.Add(docLine)

                oDoc.DocumentLines = DraftDocumentLineArray(_alDocumentLine)


                DocParams = _StockIn.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Debug.Write("Update Success Status")

                    _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
                    _ret = True


                End If

            End If
        Catch ex As Exception
            _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function Start(ByVal _dt As DataTable) As Boolean
        Me._dt = _dt
        Dim _drEntry As DataRow()
        Dim _ret As Boolean = True
        _htKeyValue = New Hashtable
        _htDocStatus = New Hashtable
        Try
            If _dt.Rows.Count > 0 Then
                For Each dr As DataRow In _dt.Rows
                    If Settings.DBNull(dr(Inventory_Inout.Fld_DocEntry)) <> String.Empty Then
                        _htKeyValue(dr(_InventoryTranConfig.KeyField)) = "Y"
                    Else
                        _htKeyValue(dr(_InventoryTranConfig.KeyField)) = "N"
                    End If

                Next

            Else
                _Message = "No data found."
                Return False
            End If
            If _htKeyValue.Count > 0 Then
                For Each o As Object In _htKeyValue.Keys
                    If _htKeyValue(o) = "Y" Then
                        _htDocStatus(o) = Start(o)
                    Else
                        _htDocStatus(o) = StartDraft(o)
                    End If

                Next

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret



    End Function
End Class


