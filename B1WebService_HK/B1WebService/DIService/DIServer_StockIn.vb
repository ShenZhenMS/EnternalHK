Public Class DIServer_StockIn : Inherits CPSLIB.DIServer.Document
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable

    Dim _InventoryTranConfig As InventoryInoutConfig

    Dim _InventoryTran As InventoryInoutForWMS
    Dim _SAPInventory As InventoryInoutForSAP
    Dim _htKeyValue As Hashtable
    Dim _htDocStatus As Hashtable
    Dim _diConn As CPSLIB.DIServer.DIServerConnection
    Public Shared ObjType As String = "59"
    Private _isError As Boolean
    Dim _Setting As Settings
    Dim _isDraft As Boolean

    Public Shared FLD_HDR_DocObjectCode As String = "DocObjectCode"
    Public Shared FLD_HDR_DocDate As String = "DocDate"
    Public Shared FLD_HDR_Series As String = "Series"
    Public Shared FLD_HDR_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_HDR_WMSUser As String = "U_WMSUser"
    Public Shared FLD_DTL_Quantity As String = "Quantity"
    Public Shared FLD_DTL_ItemCode As String = "ItemCode"
    Public Shared FLD_DTL_WhsCode As String = "WarehouseCode"
    Public Shared FLD_DTL_AccountCode As String = "AccountCode"
    Public Shared FLD_DTL_UnitPrice As String = "Price"
    Public Shared FLD_DTL_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_DTL_WMSLineNum As String = "U_WMSLine"


    Public Shared FLD_DTL_COSTINGCODE As String = "CostingCode"
    Public Shared FLD_DTL_COSTINGCODE2 As String = "CostingCode2"
    Public Shared FLD_DTL_COSTINGCODE3 As String = "CostingCode3"
    Public Shared FLD_DTL_COSTINGCODE4 As String = "CostingCode4"
    Public Shared FLD_DTL_COSTINGCODE5 As String = "CostingCode5"

    Public Shared FLD_HDR_Remark2 As String = "Comments"
    Public Shared FLD_DTL_PriceBefDi As String = "UnitPrice"

    Dim Fld_HDR_COUNTER As String = "U_COUNTER"
    Dim Fld_HDR_KEEPER As String = "U_Keeper"

    Public Sub New(ByVal _Setting As Settings, ByVal _diConn As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_diConn, SAPbobsCOM.BoObjectTypes.oInventoryGenEntry)

        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._Setting = _Setting
        
        _SAPInventory = New InventoryInoutForSAP(_Setting, Nothing)
        _InventoryTranConfig = New InventoryInoutConfig(_Setting)
        _InventoryTran = New InventoryInoutForWMS(_Setting, Nothing, InventoryInoutForWMS._DocumentType.GR)
        _htDocStatus = New Hashtable
    End Sub

    Public Sub New(ByVal _Setting As Settings, ByVal _diConn As CPSLIB.DIServer.DIServerConnection, ByVal _isDraft As Boolean)
        MyBase.New(_diConn, SAPbobsCOM.BoObjectTypes.oDrafts)
        Me._isDraft = True
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._Setting = _Setting

        _SAPInventory = New InventoryInoutForSAP(_Setting, Nothing)
        _InventoryTranConfig = New InventoryInoutConfig(_Setting)
        _InventoryTran = New InventoryInoutForWMS(_Setting, Nothing, InventoryInoutForWMS._DocumentType.GR)
        _htDocStatus = New Hashtable
    End Sub


    'this function add by jerry
    Public Function Start(ByVal _o As String, ByVal _dt As DataTable) As Boolean
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty
        Dim _COASetting As COASetting
        Dim _LineQuantity As Decimal
        Dim _BatchRow As CPSLIB.DIServer.BatchNumbers
        Dim _htCOADefaultDr As Hashtable
        Dim _GLAccount As String
        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
            If _drRow.Length > 0 Then
                NewDocument()
                If _isDraft Then
                    SetValue(FLD_HDR_DocObjectCode, "59")
                End If

                SetValue(FLD_HDR_DocDate, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"))
                SetValue(FLD_HDR_Series, _SAPInventory.DocSeries(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries)))
                SetValue(FLD_HDR_WMSEntry, _drRow(0)(Inventory_Inout.Fld_ReceiveEntry))
                SetValue(FLD_HDR_WMSUser, _drRow(0)(Inventory_Inout.Fld_WMSUser))

                'add counter & keeper logic
                Dim whsCode As String = IIf(_drRow(0).Item(Inventory_Inout.Fld_WhsCode) = Nothing, "", _drRow(0).Item(Inventory_Inout.Fld_WhsCode))
                If whsCode = "C-00-001" Then
                    SetValue(Fld_HDR_COUNTER, IIf(_drRow(0).Item(Inventory_Inout.Fld_LocCode) = Nothing, "", _
                                                  _drRow(0).Item(Inventory_Inout.Fld_LocCode)))
                ElseIf whsCode = "K-OP-001" Then
                  SetValue(Fld_HDR_KEEPER, IIf(_drRow(0).Item(Inventory_Inout.Fld_LocCode) = Nothing, "", _
                                                _drRow(0).Item(Inventory_Inout.Fld_LocCode)))
                End If


                If Not IsDBNull(_drRow(0)(Inventory_Inout.Fld_Remarks)) Then
                    SetValue(FLD_HDR_Remark2, _drRow(0)(Inventory_Inout.Fld_Remarks))
                End If

                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow
                    If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line
                            setRowsValue(FLD_DTL_Quantity, _LineQuantity)
                            setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)
                            AddRow()
                            _LineQuantity = 0
                        End If
                    End If

                    If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
                        _BatchRow = New CPSLIB.DIServer.BatchNumbers
                        _BatchRow.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
                        _BatchRow.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetSalesItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        setBatchNumberRow(_BatchRow)
                    End If
                    setRowsValue(FLD_DTL_WhsCode, dr(InventoryInoutForWMS.Fld_WhsCode))
                    _Debug.Write("Get Inventory account.")
                    _GLAccount = _SAPInventory.GLAccount(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), dr(Inventory_Inout.Fld_DocSeries), dr(Inventory_Inout.Fld_ItemCode))
                    setRowsValue(FLD_DTL_AccountCode, _GLAccount)
                    _htCOADefaultDr = _SAPInventory.GetCOADefaultDR(_GLAccount)
                    If Not _htCOADefaultDr Is Nothing Then
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim1Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE, _htCOADefaultDr(SAPSQLConnections.Fld_Dim1Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim2Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE2, _htCOADefaultDr(SAPSQLConnections.Fld_Dim2Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim3Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE3, _htCOADefaultDr(SAPSQLConnections.Fld_Dim3Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim4Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE4, _htCOADefaultDr(SAPSQLConnections.Fld_Dim4Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim5Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE5, _htCOADefaultDr(SAPSQLConnections.Fld_Dim5Relvnt))
                        End If
                    End If
                    ' Validate COA to input correcponding cost code in GI.
                    _COASetting = New COASetting(_Setting, _GLAccount)
                    If _COASetting.CostCode1Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE, _COASetting.GetDepartmentCode(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode2Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE2, _COASetting.GetBrand(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode3Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE3, _COASetting.getCounter(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode4Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE4, _COASetting.getLocation(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode5Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE5, _COASetting.getTeam(dr(InventoryInoutForWMS.Fld_ItemCode)))
                    End If

                    setRowsValue(FLD_DTL_PriceBefDi, _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode), dr(Inventory_Inout.Fld_WhsCode)))
                    setRowsValue(FLD_DTL_UnitPrice, _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode), dr(Inventory_Inout.Fld_WhsCode)))
                    setRowsValue(FLD_DTL_WMSEntry, dr(Inventory_Inout.Fld_ReceiveEntry))
                    setRowsValue(FLD_DTL_WMSLineNum, dr(Inventory_Inout.Fld_ReceiveLineNum))


                    _LineQuantity = _LineQuantity + dr(Inventory_Inout.Fld_Quantity)

                    _PrevLineNum = dr(_InventoryTranConfig.KeyLineField)
                    _PrevItemCode = dr(InventoryInoutForWMS.Fld_ItemCode)
                Next
                setRowsValue(FLD_DTL_Quantity, _LineQuantity)
                setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)
                AddRow()
                _Debug.Write("create to actrual document")

                If MyBase.Post(Command.AddObject) = CommandStatus.Fail Then
                    _Debug.Write("Update Fail Status")
                    _ret = False
                    _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", CmdMessage)
                Else
                    _Debug.Write("Update Success Status")
                    _ret = True
                    _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, NewEntry, String.Empty)
                End If



            End If
        Catch ex As Exception
            _ret = False
            _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function




    Public Function Start(ByVal _o As String) As Boolean
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty
        Dim _COASetting As COASetting
        
        Dim _LineQuantity As Decimal
        Dim DocParams As InventoryReceive.DocumentParams
        Dim _BatchRow As CPSLIB.DIServer.BatchNumbers


        Dim _htCOADefaultDr As Hashtable
        Dim _GLAccount As String
        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
            If _drRow.Length > 0 Then
                NewDocument()
                If _isDraft Then
                    setValue(Fld_HDR_DocObjectCode, "59")
                End If
                
                setValue(Fld_HDR_DocDate, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"))
                SetValue(FLD_HDR_Series, _SAPInventory.DocSeries(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries)))
                setValue(Fld_HDR_WMSEntry, _drRow(0)(Inventory_Inout.Fld_ReceiveEntry))
                setValue(Fld_HDR_WMSUser, _drRow(0)(Inventory_Inout.Fld_WMSUser))
                
                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line

                            
                            setRowsValue(FLD_DTL_Quantity, _LineQuantity)
                            setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)

                            'docLine.BatchNumbers = InventoryReceipttoDocumentLineBatchArray(_alDocumentLIneBatch)
                            '_alDocumentLine.Add(docLine)
                            '_alDocumentLIneBatch.Clear()
                            AddRow()
                            _LineQuantity = 0

                        End If


                    End If

                    
                    If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
                        _BatchRow = New CPSLIB.DIServer.BatchNumbers
                        _BatchRow.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
                        _BatchRow.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetSalesItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        setBatchNumberRow(_BatchRow)
                    End If

                    



                    setRowsValue(FLD_DTL_WhsCode, dr(InventoryInoutForWMS.Fld_WhsCode))
                    _Debug.Write("Get Inventory account.")
                    _GLAccount = _SAPInventory.GLAccount(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), dr(Inventory_Inout.Fld_DocSeries), dr(Inventory_Inout.Fld_ItemCode))
                    setRowsValue(FLD_DTL_AccountCode, _GLAccount)
                    _htCOADefaultDr = _SAPInventory.GetCOADefaultDR(_GLAccount)
                    If Not _htCOADefaultDr Is Nothing Then
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim1Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE, _htCOADefaultDr(SAPSQLConnections.Fld_Dim1Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim2Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE2, _htCOADefaultDr(SAPSQLConnections.Fld_Dim2Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim3Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE3, _htCOADefaultDr(SAPSQLConnections.Fld_Dim3Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim4Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE4, _htCOADefaultDr(SAPSQLConnections.Fld_Dim4Relvnt))
                        End If
                        If _htCOADefaultDr(SAPSQLConnections.Fld_Dim5Relvnt) <> "" Then
                            setRowsValue(FLD_DTL_COSTINGCODE5, _htCOADefaultDr(SAPSQLConnections.Fld_Dim5Relvnt))
                        End If
                    End If
                    ' Validate COA to input correcponding cost code in GI.
                    _COASetting = New COASetting(_Setting, _GLAccount)
                    If _COASetting.CostCode1Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE, _COASetting.GetDepartmentCode(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode2Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE2, _COASetting.GetBrand(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode3Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE3, _COASetting.getCounter(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode4Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE4, _COASetting.getLocation(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If
                    If _COASetting.CostCode5Mandatory Then
                        setRowsValue(FLD_DTL_COSTINGCODE5, _COASetting.getTeam(dr(InventoryInoutForWMS.Fld_ItemCode)))

                    End If

                    setRowsValue(FLD_DTL_UnitPrice, _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode), dr(Inventory_Inout.Fld_WhsCode)))
                    setRowsValue(Fld_DTL_WMSEntry, dr(Inventory_Inout.Fld_ReceiveEntry))
                    setRowsValue(Fld_DTL_WMSLineNum, dr(Inventory_Inout.Fld_ReceiveLineNum))


                    _LineQuantity = _LineQuantity + dr(Inventory_Inout.Fld_Quantity)

                    _PrevLineNum = dr(_InventoryTranConfig.KeyLineField)
                    _PrevItemCode = dr(InventoryInoutForWMS.Fld_ItemCode)
                Next
                setRowsValue(Fld_DTL_Quantity, _LineQuantity)
                setRowsValue(Fld_DTL_ItemCode, _PrevItemCode)
                AddRow()

                
                
                
                _Debug.Write("create to actrual document")

                If MyBase.Post(Command.AddObject) = CommandStatus.Fail Then
                    _Debug.Write("Update Fail Status")
                    _ret = False
                    _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", CmdMessage)
                Else
                    _Debug.Write("Update Success Status")
                    _ret = True
                    _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, NewEntry, String.Empty)
                End If

                

            End If
        Catch ex As Exception
            _ret = False
            _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function StartDraft2(ByVal _o As String, ByVal dataTb As DataTable) As Boolean
        Me._dt = dataTb
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
                oDoc.DocObjectCode = WS_StockIn.ObjType
                _msgHeader = New DocDraft.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
                _msgHeader.ServiceNameSpecified = True
                _StockIn.MsgHeaderValue = _msgHeader

                oDoc.DocDate = Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate))
                oDoc.Series = _SAPInventory.DocSeries(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries))
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
                        docLineBatch.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetPurchaseItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        _alDocumentLIneBatch.Add(docLineBatch)
                    End If
                    docLine.WarehouseCode = dr(InventoryInoutForWMS.Fld_WhsCode)
                    docLine.PriceSpecified = True
                    docLine.AccountCode = _SAPInventory.GLAccount(DIServer_StockIn.ObjType, Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyyMMdd"), dr(Inventory_Inout.Fld_DocSeries), dr(Inventory_Inout.Fld_ItemCode))

                    docLine.Price = _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode), dr(Inventory_Inout.Fld_WhsCode))

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


    'this function added by Jerry for generate Draft
    Public Function StartDraft(ByVal _o As String,byval _dt As DataTable) As Boolean
        'Dim wsStockIn As New WS_StockIn(Me._Setting)
        'wsStockIn.StartDraft(_o, _dt)

        _Debug.Write(String.Format("creating draft document: key value : {0}", _o))
        Dim _ret As Boolean = True
        Dim _drrow As DataRow()
        Dim _prevlinenum As String = String.Empty
        Dim _previtemcode As String = String.Empty

        Dim _msgheader As DocDraft.MsgHeader
        Dim _stockin As DocDraft.DraftsService

        Dim odoc As DocDraft.Document
        Dim docline As DocDraft.DocumentDocumentLine
        Dim doclinebatch As DocDraft.DocumentDocumentLineBatchNumber
        Dim _aldocumentline As ArrayList
        Dim _aldocumentlinebatch As ArrayList
        Dim _linequantity As Decimal
        Dim docparams As DocDraft.DocumentParams
        _aldocumentline = New ArrayList
        _aldocumentlinebatch = New ArrayList

        _aldocumentlinebatch.Clear()
        _aldocumentline.Clear()
        Try
            _drrow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
            If _drrow.Length > 0 Then
                _stockin = New DocDraft.DraftsService
                odoc = New DocDraft.Document
                odoc.DocObjectCode = DIServer_StockIn.ObjType
                _msgheader = New DocDraft.MsgHeader
                _msgheader.SessionID = MyBase.SessionID
                _msgheader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
                _msgheader.ServiceNameSpecified = True
                _stockin.MsgHeaderValue = _msgheader

                odoc.DocDate = Convert.ToDateTime(_drrow(0)(Inventory_Inout.Fld_DocDate))
                'odoc.series = _sapinventory.docseries(diserver_stockin.objtype, odoc.docdate.tostring("yyyymmdd"), _drrow(0)(inventory_inout.fld_docseries)
                odoc.Series = _SAPInventory.DocSeries(DIServer_StockIn.ObjType, Convert.ToDateTime(_drrow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyymmdd"), _drrow(0)(Inventory_Inout.Fld_DocSeries))
                odoc.SeriesSpecified = True
                odoc.U_WMSEntry = _drrow(0)(Inventory_Inout.Fld_ReceiveEntry)
                odoc.U_WMSUser = _drrow(0)(Inventory_Inout.Fld_WMSUser)
                odoc.DocDateSpecified = True
                ' udf for receive entry and wms user
                _linequantity = 0
                For Each dr In _drrow

                    If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _prevlinenum Then
                        If _prevlinenum <> String.Empty Then
                            ' add new line
                            docline.Quantity = _linequantity
                            docline.QuantitySpecified = True
                            docline.ItemCode = _previtemcode

                            docline.BatchNumbers = DraftDocumentLineBatchArray(_aldocumentlinebatch)
                            _aldocumentline.Add(docline)
                            _aldocumentlinebatch.Clear()
                            _linequantity = 0

                        End If
                        docline = New DocDraft.DocumentDocumentLine

                    End If
                    doclinebatch = Nothing
                    doclinebatch = New DocDraft.DocumentDocumentLineBatchNumber
                    If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
                        doclinebatch.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
                        doclinebatch.QuantitySpecified = True
                        doclinebatch.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetPurchaseItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
                        _aldocumentlinebatch.Add(doclinebatch)
                    End If
                    docline.WarehouseCode = dr(InventoryInoutForWMS.Fld_WhsCode)
                    docline.PriceSpecified = True



                    'docline.accountcode = _sapinventory.glaccount(ws_stockin.objtype, odoc.docdate.tostring("yyyymmdd"), dr(inventoryinoutforwms.fld_docseries))
                    docline.AccountCode = _SAPInventory.GLAccount(DIServer_StockIn.ObjType, Convert.ToDateTime(_drrow(0)(Inventory_Inout.Fld_DocDate)).ToString("yyyymmdd"), dr(Inventory_Inout.Fld_DocSeries), dr(Inventory_Inout.Fld_ItemCode))
                    'docline.price = _sapinventory.itemcost(dr(inventory_inout.fld_itemcode))
                    docline.Price = _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode), dr(Inventory_Inout.Fld_WhsCode))

                    docline.U_WMSEntry = dr(Inventory_Inout.Fld_ReceiveEntry)

                    docline.U_WMSLineNum = dr(Inventory_Inout.Fld_ReceiveLineNum)
                    docline.U_WMSLineNumSpecified = True
                    _linequantity = _linequantity + dr(Inventory_Inout.Fld_Quantity)

                    _prevlinenum = dr(_InventoryTranConfig.KeyLineField)
                    _previtemcode = dr(InventoryInoutForWMS.Fld_ItemCode)
                Next

                docline.Quantity = _linequantity
                docline.QuantitySpecified = True
                docline.ItemCode = _previtemcode

                If _aldocumentlinebatch.Count > 0 Then
                    docline.BatchNumbers = DraftDocumentLineBatchArray(_aldocumentlinebatch)
                End If

                _aldocumentline.Add(docline)

                odoc.DocumentLines = DraftDocumentLineArray(_aldocumentline)


                docparams = _stockin.Add(odoc)
                If docparams.DocEntry > 0 Then
                    ' update success status
                    _Debug.Write("update success status")
                    _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, docparams.DocEntry, String.Empty)
                    _ret = True
                End If
            End If
        Catch ex As Exception
            _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret

        
    End Function

    'Public Function StartDraft(ByVal _o As String) As Boolean
    '    _Debug.Write(String.Format("Creating Draft Document: Key Value : {0}", _o))
    '    Dim _ret As Boolean = True
    '    Dim _drRow As DataRow()
    '    Dim _PrevLineNum As String = String.Empty
    '    Dim _PrevItemCode As String = String.Empty

    '    Dim _msgHeader As DocDraft.MsgHeader
    '    Dim _StockIn As DocDraft.DraftsService

    '    Dim oDoc As DocDraft.Document
    '    Dim docLine As DocDraft.DocumentDocumentLine
    '    Dim docLineBatch As DocDraft.DocumentDocumentLineBatchNumber
    '    Dim _alDocumentLine As ArrayList
    '    Dim _alDocumentLIneBatch As ArrayList
    '    Dim _LineQuantity As Decimal
    '    Dim DocParams As DocDraft.DocumentParams
    '    _alDocumentLine = New ArrayList
    '    _alDocumentLIneBatch = New ArrayList

    '    _alDocumentLIneBatch.Clear()
    '    _alDocumentLine.Clear()




    '    Try
    '        _drRow = _dt.Select(String.Format("{0} = '{1}'", _InventoryTranConfig.KeyField, _o.ToString), String.Format("{0} asc", _InventoryTranConfig.KeyField))
    '        If _drRow.Length > 0 Then
    '            _StockIn = New DocDraft.DraftsService
    '            oDoc = New DocDraft.Document
    '            oDoc.DocObjectCode = WS_StockIn.ObjType
    '            _msgHeader = New DocDraft.MsgHeader
    '            _msgHeader.SessionID = MyBase.SessionID
    '            _msgHeader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
    '            _msgHeader.ServiceNameSpecified = True
    '            _StockIn.MsgHeaderValue = _msgHeader

    '            oDoc.DocDate = Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate))
    '            oDoc.Series = _SAPInventory.DocSeries(WS_StockIn.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(Inventory_Inout.Fld_DocSeries))
    '            oDoc.SeriesSpecified = True
    '            oDoc.U_WMSEntry = _drRow(0)(Inventory_Inout.Fld_ReceiveEntry)
    '            oDoc.U_WMSUser = _drRow(0)(Inventory_Inout.Fld_WMSUser)
    '            oDoc.DocDateSpecified = True
    '            ' UDF for Receive Entry and WMS User
    '            _LineQuantity = 0
    '            For Each dr In _drRow

    '                If Settings.DBNull(dr(_InventoryTranConfig.KeyLineField)) <> _PrevLineNum Then
    '                    If _PrevLineNum <> String.Empty Then
    '                        ' Add new Line
    '                        docLine.Quantity = _LineQuantity
    '                        docLine.QuantitySpecified = True
    '                        docLine.ItemCode = _PrevItemCode

    '                        docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
    '                        _alDocumentLine.Add(docLine)
    '                        _alDocumentLIneBatch.Clear()
    '                        _LineQuantity = 0

    '                    End If
    '                    docLine = New DocDraft.DocumentDocumentLine

    '                End If
    '                docLineBatch = Nothing
    '                docLineBatch = New DocDraft.DocumentDocumentLineBatchNumber
    '                If IsDBNull(dr(InventoryInoutForWMS.Fld_BatchNum)) = False Then
    '                    docLineBatch.BatchNumber = dr(Inventory_Inout.Fld_BatchNum)
    '                    docLineBatch.QuantitySpecified = True
    '                    docLineBatch.Quantity = dr(Inventory_Inout.Fld_Quantity) * _SAPInventory.GetPurchaseItemPerBaseUnit(dr(Inventory_Inout.Fld_ItemCode))
    '                    _alDocumentLIneBatch.Add(docLineBatch)
    '                End If
    '                docLine.WarehouseCode = dr(InventoryInoutForWMS.Fld_WhsCode)
    '                docLine.PriceSpecified = True
    '                docLine.AccountCode = _SAPInventory.GLAccount(WS_StockIn.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), dr(InventoryInoutForWMS.Fld_DocSeries))

    '                docLine.Price = _SAPInventory.ItemCost(dr(Inventory_Inout.Fld_ItemCode))

    '                docLine.U_WMSEntry = dr(Inventory_Inout.Fld_ReceiveEntry)

    '                docLine.U_WMSLineNum = dr(Inventory_Inout.Fld_ReceiveLineNum)
    '                docLine.U_WMSLineNumSpecified = True
    '                _LineQuantity = _LineQuantity + dr(Inventory_Inout.Fld_Quantity)

    '                _PrevLineNum = dr(_InventoryTranConfig.KeyLineField)
    '                _PrevItemCode = dr(InventoryInoutForWMS.Fld_ItemCode)
    '            Next

    '            docLine.Quantity = _LineQuantity
    '            docLine.QuantitySpecified = True
    '            docLine.ItemCode = _PrevItemCode

    '            If _alDocumentLIneBatch.Count > 0 Then
    '                docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
    '            End If

    '            _alDocumentLine.Add(docLine)

    '            oDoc.DocumentLines = DraftDocumentLineArray(_alDocumentLine)


    '            DocParams = _StockIn.Add(oDoc)
    '            If DocParams.DocEntry > 0 Then
    '                ' Update Success Status
    '                _Debug.Write("Update Success Status")

    '                _InventoryTran.UpdateSuccessStatus(_InventoryTranConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
    '                _ret = True


    '            End If

    '        End If
    '    Catch ex As Exception
    '        _InventoryTran.UpdateErrorStatus(_InventoryTranConfig.KeyField, _o, "-1", ex.Message)
    '        _CPSException.ExecuteHandle(ex)
    '    End Try
    '    Return _ret
    'End Function


    Public Function DraftDocumentLineArray(ByVal _al As ArrayList) As DocDraft.DocumentDocumentLine()
        Dim l(_al.Count - 1) As DocDraft.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), DocDraft.DocumentDocumentLine)
        Next
        Return l
    End Function

    Public Function DraftDocumentLineBatchArray(ByVal _al As ArrayList) As DocDraft.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As DocDraft.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), DocDraft.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function



    'Public Function Start(ByVal _dt As DataTable) As Boolean
    '    Me._dt = _dt
    '    Dim _drEntry As DataRow()
    '    Dim _ret As Boolean = True
    '    _htKeyValue = New Hashtable
    '    _htDocStatus = New Hashtable
    '    Dim ret As Boolean = True
    '    Try
    '        If _dt.Rows.Count > 0 Then
    '            For Each dr As DataRow In _dt.Rows
    '                '<<<<<Jerry Remove<<<<<<<<<<<<<<<<<<<<
    '                '_htKeyValue(dr(_InventoryTranConfig.KeyField)) = "Y"
    '                '<<<<<Jerry Add<<<<<<<<<<<<<<<<<<<<<<<
    '                'Jerry Changed
    '                If Not IsDBNull(dr(Inventory_Inout.Fld_isDraft)) And dr(Inventory_Inout.Fld_isDraft) = "N" Then
    '                    _htKeyValue(dr(_InventoryTranConfig.KeyField)) = "Y"
    '                Else
    '                    _htKeyValue(dr(_InventoryTranConfig.KeyField)) = "N"
    '                End If
    '                '<<<<<<<<<<<<<<<<<<<<<<<<<<<<<


    '            Next

    '        Else

    '            Return False
    '        End If

    '        If _htKeyValue.Count > 0 Then
    '            For Each o As Object In _htKeyValue.Keys
    '                '<<<Jerry remove<<<<<<<<<<<<<<<<<
    '                '_ret = Start(o)
    '                'If Not _ret Then
    '                '    Exit For
    '                'End If
    '                ''_htDocStatus(o) = Start(o)
    '                '<<<<Jerry Add<<<<<<<<<<<<<<
    '                If _htKeyValue(o) = "Y" Then
    '                    _htDocStatus(o) = Start(o)
    '                Else
    '                    _htDocStatus(o) = StartDraft(o)
    '                End If
    '                '<<<<<<<<<<<<<<<<<<
    '            Next

    '        End If
    '    Catch ex As Exception
    '        _ret = False
    '        _CPSException.ExecuteHandle(ex)
    '    End Try

    '    Return _ret



    'End Function
End Class
