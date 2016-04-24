Public Class DIServer_GRPO : Inherits CPSLIB.DIServer.Document

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _PurConfig As PurchaseConfig
    Dim _Purchase As PurchaseForWMS
    Dim _htDocStatus As Hashtable



    Dim _Setting As Settings

    Private _Message As String
    
    Dim _diServerConn As CPSLIB.DIServer.DIServerConnection


    Private _hasError As Boolean
    Public Property HasError() As Boolean
        Get
            Return _hasError
        End Get
        Set(ByVal value As Boolean)
            _hasError = value
        End Set
    End Property

    Private _ErrMsg As String
    Public Property ErrorMessage() As String
        Get
            Return _ErrMsg
        End Get
        Set(ByVal value As String)
            _ErrMsg = value
        End Set
    End Property



    Public Shared Fld_Hdr_DocDate As String = "DocDate"
    Public Shared Fld_Hdr_DocDueDate As String = "DocDueDate"
    Public Shared Fld_Hdr_WMSEntry As String = "U_WMSEntry"
    Public Shared Fld_Hdr_ASNNum As String = "U_ASNNum"
    Public Shared Fld_Hdr_Comments As String = "Comments"
    Public Shared Fld_Row_BaseEntry As String = "BaseEntry"
    Public Shared Fld_Row_BaseLine As String = "BaseLine"
    Public Shared Fld_Row_BaseType As String = "BaseType"
    Public Shared Fld_Row_Quantity As String = "Quantity"
    Public Shared Fld_Row_ItemCode As String = "ItemCode"
    Public Shared Fld_Row_DocObjectCode As String = "DocObjectCode"
    Public Shared FLD_DTL_WMSDraftEntry As String = "U_WMSDraftEntry"
    Public Shared Fld_DTL_WMSDraftLine As String = "U_WMSDraftLine"
    Public Shared FLD_Row_WhsCode As String = "WarehouseCode"
    Public Shared Fld_Row_UnitPrice As String = "UnitPrice"
    Public Shared FLD_Row_PriceBeforeDiscount As String = "PriceBefDi"
    Public Shared Fld_Row_Warehouse As String = "WhsCode"

    Dim _htHUDF As Hashtable
    Dim _htDUDF As Hashtable
    Dim _SAPPurchase As PurchaseForSAP
    'setRowsValue(Fld_Row_BaseEntry, _PrevDocEntry)
    '                  setRowsValue(Fld_Row_BaseLine, _PrevLineNum)
    '                  setRowsValue(Fld_Row_BaseType, "22")
    '                  setRowsValue(Fld_Row_Quantity, _cumQty)
    '                  setRowsValue(Fld_Row_ItemCode, _PrevItemCode)

    Public Sub New(ByVal _Setting As Settings, ByVal _diServerConn As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_diServerConn, SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes)
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _SAPPurchase = New PurchaseForSAP(_Setting, Nothing)

        Me._diServerConn = _diServerConn
        
        _PurConfig = New PurchaseConfig(_Setting)
        If _PurConfig.isDraft Then
            MyBase.BOObjectType = SAPbobsCOM.BoObjectTypes.oDrafts

        End If
        _Purchase = New PurchaseForWMS(_Setting, Nothing)
        _htDocStatus = New Hashtable


    End Sub

    Public Function Generate(ByVal _ReceiveEntry As String) As Boolean
        Dim _ret As Boolean = True
        Dim _dt As DataTable
        _Debug.Write("Generating Document: " & _ReceiveEntry)
        Try
            _Debug.Write(_ReceiveEntry, "ReceiveEntry")
            _dt = _Purchase.OpenPurchaseOrder(_ReceiveEntry)
            _hasError = False
            _ErrMsg = String.Empty
            If _dt Is Nothing = False Then
                For Each _dr As DataRow In _dt.Rows

                    _htDocStatus.Add(_dr(Purchase.Fld_DocEntry), Generate(_dr(Purchase.Fld_DocEntry), _ReceiveEntry))
                Next
            Else

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return Not _hasError
    End Function

    Private Function Generate(ByVal _DocEntry As Integer, ByVal _ReceiveEntry As String) As String
        Dim _ret As String
        Dim _dtLine As DataTable
        Dim _drLine As DataRow()

        
        Dim _isFirst As Boolean = True
        Dim _PrevDocEntry As Integer = -1
        Dim _PrevLineNum As Integer = -1
        Dim _PrevDraftEntry As Integer = -1
        Dim _PrevDraftLine As Integer = -1
        Dim _PrevItemCode As String = ""
        Dim _cumQty As Decimal = 0
        Dim _LineNum As Integer = 0
        Dim _dtDraftLine As DataTable
        Dim _dtDraftHeader As DataTable
        Dim _SAPConnection As SAPSQLConnections
        _htHUDF = _SAPPurchase.GetUDF("OPDN")
        _htDUDF = _SAPPurchase.GetUDF("PDN1")
        _Debug.Write("Generate")

        Try
            _SAPConnection = New SAPSQLConnections(_Setting)
            NewDocument()
            _dtLine = _Purchase.OpenPurchaseOrder(_DocEntry, _ReceiveEntry)
            _drLine = _dtLine.Select("1 = 1", String.Format("{0} asc, {1} asc", Purchase.Fld_DocEntry, Purchase.Fld_LineNum))

            _isFirst = True

            ' fill business partner class
            _PrevLineNum = -1


            _LineNum = 0
            Dim _BatchRow As CPSLIB.DIServer.BatchNumbers
            For Each _dr As DataRow In _drLine
                _BatchRow = New CPSLIB.DIServer.BatchNumbers
                If _isFirst Then


                    '_htHeaderFieldMapping("Comments") = "Comments"
                    '_htHeaderFieldMapping("DocDueDate") = "DocDueDate"
                    ''_htHeaderFieldMapping("TaxDate") = "TaxDate"
                    '_htHeaderFieldMapping("DocDate") = "DocDate"

                    '_htHeaderFieldMapping("Address") = "Address"
                    '_htHeaderFieldMapping("JrnlMemo") = "JournalMemo"
                    '_htHeaderFieldMapping("SlpCode") = "SalesPersonCode"
                    '_htHeaderFieldMapping("DocCur") = "DocCurrency"
                    '_htHeaderFieldMapping("OwnerCode") = "DocumentsOwner"

                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "Comments")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "DocDueDate")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "TaxDate")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "DocDate")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "Address")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "JrnlMemo")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "SlpCode")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "DocCur")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "OwnerCode")
                    SetHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry), "DiscPrcnt")
                    'SetAllHeaderStandardField("ODRF", _dr(Purchase.Fld_DraftEntry))
                    'SetValue(Fld_Hdr_DocDate, DateTime.Now.ToString("yyyyMMdd"))
                    'SetValue(Fld_Hdr_DocDueDate, DateTime.Now.ToString("yyyyMMdd"))
                    SetValue(Fld_Hdr_WMSEntry, _dr(Purchase.Fld_ReceiveEntry))
                    SetValue(Fld_Hdr_ASNNum, _dr(Purchase.Fld_ASNNum))
                    SetValue(Fld_Hdr_Comments, _dr(Purchase.Fld_Remark))

                    If _PurConfig.isDraft Then
                        SetValue(Fld_Row_DocObjectCode, "20")
                    End If


                End If

                _isFirst = False
                If _PrevLineNum <> _dr(Purchase.Fld_LineNum) Then

                    If _PrevLineNum <> -1 Then
                        _dtDraftLine = _SAPPurchase.GetDraftLine(_PrevDraftEntry, _PrevDraftLine)
                        ' Add new Row
                        setRowsValue(Fld_Row_BaseEntry, _PrevDocEntry)
                        setRowsValue(Fld_Row_BaseLine, _PrevLineNum)
                        setRowsValue(Fld_Row_BaseType, "22")
                        setRowsValue(Fld_Row_Quantity, _cumQty)
                        setRowsValue(Fld_Row_ItemCode, _PrevItemCode)
                        If _dtDraftLine.Rows.Count > 0 Then
                            For Each o As Object In _htDUDF.Keys
                                Try
                                    If IsDBNull(_dtDraftLine(0)(o.ToString)) = False Then
                                        If _dtDraftLine(0)(o.ToString).ToString <> String.Empty Then
                                            setRowsValue(o.ToString, _dtDraftLine(0)(o.ToString))
                                        End If
                                    End If
                                Catch ex As Exception
                                    _CPSException.ExecuteHandle(ex, "set UDF: " & o.ToString)
                                End Try
                            Next
                        End If
                        setRowsValue(FLD_DTL_WMSDraftEntry, _PrevDraftEntry)
                        setRowsValue(Fld_DTL_WMSDraftLine, _PrevDraftLine)

                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode2")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode3")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode4")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode5")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod2")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod3")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod4")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod5")
                        SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "DiscPrcnt")
                        'SetALLLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine)
                        ' Draft Price
                        setRowsValue(Fld_Row_UnitPrice, _dtDraftLine(0)(FLD_Row_PriceBeforeDiscount))
                        ' Draft WhsCode
                        'setRowsValue(FLD_Row_WhsCode, _dtDraftLine(0)(Fld_Row_Warehouse))
                        
                        AddRow()
                        _cumQty = 0
                        _LineNum = _LineNum + 1
                    End If

                End If


                _cumQty = _cumQty + _dr(Purchase.Fld_Quantity)
                _PrevLineNum = _dr(Purchase.Fld_LineNum)
                _PrevDocEntry = _dr(Purchase.Fld_DocEntry)
                _PrevItemCode = _dr(Purchase.Fld_ItemCode)
                _PrevDraftEntry = _dr(Purchase.Fld_DraftEntry)
                _PrevDraftLine = _dr(Purchase.Fld_DraftLine)
                If _dr(Purchase.Fld_BatchNumber) <> String.Empty Then
                    _BatchRow.BatchNumber = _dr(Purchase.Fld_BatchNumber)
                    _BatchRow.ExpDate = Convert.ToDateTime(_dr(Purchase.Fld_ExpireDate)).ToString("yyyyMMdd")
                    _BatchRow.ManufacturingDate = Convert.ToDateTime(_dr(Purchase.Fld_MfrDate)).ToString("yyyyMMdd")
                    _BatchRow.Quantity = _dr(Purchase.Fld_Quantity)
                    '_BatchRow.Location = _dr(Purchase.Fld_LocCode)
                    MyBase.setBatchNumberRow(_BatchRow)

                End If
                'Karrson: 20150308
                setRowsValue(FLD_Row_WhsCode, _dr(Purchase.Fld_WhsCode))
            Next
            _Debug.Write("Check Point A")
            If _PrevLineNum >= 0 Then
                _dtDraftLine = _SAPPurchase.GetDraftLine(_PrevDraftEntry, _PrevDraftLine)
                setRowsValue(Fld_Row_BaseEntry, _PrevDocEntry)
                setRowsValue(Fld_Row_BaseLine, _PrevLineNum)
                setRowsValue(Fld_Row_BaseType, "22")
                setRowsValue(Fld_Row_Quantity, _cumQty)
                setRowsValue(Fld_Row_ItemCode, _PrevItemCode)
                If _dtDraftLine.Rows.Count > 0 Then

                    For Each o As Object In _htDUDF.Keys
                        Try
                            If IsDBNull(_dtDraftLine(0)(o.ToString)) = False Then
                                If _dtDraftLine(0)(o.ToString).ToString <> String.Empty Then
                                    setRowsValue(o.ToString, _dtDraftLine(0)(o.ToString))
                                End If
                            End If
                        Catch ex1 As Exception
                            _CPSException.ExecuteHandle(ex1, "set UDF: " & o.ToString)
                        End Try
                    Next
                End If
                setRowsValue(FLD_DTL_WMSDraftEntry, _PrevDraftEntry)
                setRowsValue(Fld_DTL_WMSDraftLine, _PrevDraftLine)
                'SetALLLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine)
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode2")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode3")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode4")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "OcrCode5")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod2")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod3")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod4")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "CogsOcrCod5")
                SetLineStandardField("DRF1", _PrevDraftEntry, _PrevDraftLine, "DiscPrcnt")
                ' Draft Price
                setRowsValue(Fld_Row_UnitPrice, _dtDraftLine(0)(FLD_Row_PriceBeforeDiscount))
                ' Draft WhsCode
                'setRowsValue(FLD_Row_WhsCode, _dtDraftLine(0)(Fld_Row_Warehouse))
                AddRow()
                _Debug.Write("Check Point B")

                _dtDraftHeader = _SAPPurchase.GetDraftHeader(_PrevDraftEntry)
                _Debug.Write("Check Point B1")
                If _dtDraftHeader.Rows.Count > 0 Then
                    _Debug.WriteTable(_dtDraftHeader, "DTDraftHeader")
                    For Each o As Object In _htHUDF.Keys
                        _Debug.Write(o.ToString, "UDF Header")
                        Try
                            If IsDBNull(_dtDraftHeader(0)(o.ToString)) = False Then
                                If _dtDraftHeader(0)(o.ToString).ToString <> String.Empty Then
                                    setValue(o.ToString, _dtDraftHeader(0)(o.ToString))
                                End If
                            End If
                        Catch ex1 As Exception
                            _CPSException.ExecuteHandle(ex1, "set UDF: " & o.ToString)
                        End Try
                    Next
                End If
                _Debug.Write("Check Point C")

                If MyBase.Post(Command.AddObject) = CommandStatus.Fail Then
                    _hasError = True
                    _ErrMsg = MyBase.CmdMessage
                    _Debug.Write(MyBase.CmdMessage)
                    _ret = MyBase.CmdMessage
                    _Purchase.UpdateErrorStatus(_DocEntry, "-1", MyBase.CmdMessage.Replace("'", "''"))
                Else

                    _ret = String.Empty
                    _Purchase.UpdateSuccessStatus(_DocEntry, MyBase.NewEntry, String.Empty)

                    _Debug.Write("Updating Draft QTY")
                    'Update Draft GRPO
                    For j As Integer = 0 To _dtLine.Rows.Count - 1
                        _Debug.Write(String.Format("EXEC CPS_SP_UPDATE_DRAFT_QTY '{0}','{1}','{2}','{3}'", _
                                                                 MyBase.NewEntry, _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_ASNNum), _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_LineNum)), "Update Draft Quantity")
                        _SAPPurchase.ExecuteUpdate(String.Format("EXEC CPS_SP_UPDATE_DRAFT_QTY '{0}','{1}','{2}','{3}'", _
                                                                 MyBase.NewEntry, _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_ASNNum), _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                                 _dtLine.Rows(j).Item(Purchase.Fld_LineNum)))
                    Next
                End If




            Else
                _hasError = True
                _ErrMsg = "Internal Error"
                _Purchase.UpdateErrorStatus(_DocEntry, "-1", "Internal Error.")
                _ret = "Internal Error"
            End If

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            _hasError = True
            _ErrMsg = ex.Message
            ' Update Failure Status
            _Purchase.UpdateErrorStatus(_DocEntry, "-1", ex.Message.Replace("'", "''"))
            _ret = ex.Message
        End Try



        '    'GRPO_Doc.DocumentStatus = B1WebService.PurchaseDeliveryNotesServiceRef.DocumentDocumentStatus.bost_Open
        '    'GRPO_Doc.DocumentStatusSpecified = True
        '    GRPO_Doc.DocDate = Date.Now
        '    GRPO_Doc.CardCode = dt2.Rows(0).Item("CardCode")
        '    docLine = New B1WebService.PurchaseDeliveryNotesServiceRef.DocumentDocumentLine
        '    GRPO_Doc.DocumentLines = Array.CreateInstance(docLine.GetType(), dt2.Rows.Count)
        '    GRPO_Doc.U_()
        '    For i As Integer = 0 To dt2.Rows.Count - 1
        '        docLine = Nothing
        '        docLine = New B1WebService.PurchaseDeliveryNotesServiceRef.DocumentDocumentLine

        '        ''Define Invoice Details
        '        docLine.LineStatus = B1WebService.PurchaseDeliveryNotesServiceRef.DocumentDocumentLineLineStatus.bost_Open
        '        docLine.ItemCode = dt2.Rows(i).Item("ItemCode")
        '        docLine.Quantity = dt2.Rows(i).Item("Quantity")
        '        docLine.QuantitySpecified = True

        '        docLine.WarehouseCode = dt2.Rows(i).Item("WhsCode")
        '        docLine.UnitPrice = dt2.Rows(i).Item("Price")
        '        docLine.UnitPriceSpecified = True


        '        GRPO_Doc.DocumentLines.SetValue(docLine, i)
        '    Next

        '    DocParams = PurchaseDeliveryNoteService.Add(GRPO_Doc)

        '    docLine = Nothing
        '    GRPO_Doc = Nothing
        '    DocEntry_XElement.SetValue(DocParams.DocEntry.ToString)

        'Catch ex As Exception
        '    retVal = ex.Message.ToString()
        '    ErrorMessage_XElement.SetValue(retVal)
        'End Try
        Return _ret
    End Function

    'Public Function Create(ByVal _dt As DataTable) As Boolean
    '    ' Validate weather PO with same warehouse or not
    '    Dim _SAPPurchase As PurchaseForSAP
    '    Dim _drRow() As DataRow
    '    Dim _PrevDocEntry As String = String.Empty
    '    Dim _PrevLineNunm As String = String.Empty
    '    Dim _PrevWhsCode As String = String.Empty

    '    Try


    '        _dt.Columns.Add(Purchase.Fld_SameWhse)
    '        _dt.Columns.Add(Purchase.Fld_Processed)
    '        _SAPPurchase = New PurchaseForSAP(_Setting, Nothing)
    '        For Each _dr As DataRow In _dt.Rows

    '            If _SAPPurchase.isSameWarehouse(Settings.DBNull(_dr(Purchase.Fld_DocEntry), "-1"), Settings.DBNull(_dr(Purchase.Fld_LineNum), "-1"), _dr(Purchase.Fld_WhsCode)) Then
    '                _dr(Purchase.Fld_SameWhse) = "Y"
    '                _dr(Purchase.Fld_Processed) = "Y"
    '            Else
    '                _dr(Purchase.Fld_SameWhse) = "N"
    '                _dr(Purchase.Fld_Processed) = "Y"
    '            End If
    '        Next

    '        ' Generate with same warehouse 
    '        _drRow = _dt.Select(String.Format("{0} = 'Y'", Purchase.Fld_SameWhse))
    '        If _drRow Is Nothing = False Then
    '            If _drRow.Length > 0 Then
    '                If Create_InProcess(_drRow) = False Then

    '                End If
    '            End If
    '        End If
    '        ' Generate without same warehouse

    '        _drRow = _dt.Select(String.Format("{0} = 'Y'", Purchase.Fld_SameWhse, "{0} asc,{1} asc,{2} asc"))

    '        If _drRow Is Nothing = False Then
    '            If _drRow.Length > 0 Then
    '                Do While _drRow.Length > 0

    '                    For Each _dr As DataRow In _drRow

    '                        _PrevDocEntry = _dr(Purchase.Fld_DocEntry)
    '                        _PrevLineNunm = _dr(Purchase.Fld_LineNum)
    '                        _PrevWhsCode = _dr(Purchase.Fld_WhsCode)
    '                    Next

    '                Loop
    '            End If

    '        End If

    '    Catch ex As Exception
    '        'Me.Message = ex.Message
    '        _CPSException.ExecuteHandle(ex)
    '        Return False
    '    End Try
    'End Function

    'Public Function Create_InProcess(ByVal _dr As DataRow()) As Boolean
    '    Dim _ret As Boolean = True
    '    _Debug.Write("Createing GRPO")
    '    Dim _dt_DocEntry As DataTable
    '    Dim _dt_DocLine As DataTable
    '    Dim _dt_BatchNum As DataTable
    '    Dim _msgHeader As GRPO.MsgHeader
    '    Dim oDoc As GRPO.Document

    '    Dim _TargetEntry As String
    '    Dim docLine As GRPO.DocumentDocumentLine
    '    Dim docLineBatch As GRPO.DocumentDocumentLineBatchNumber
    '    Dim DocParams As GRPO.DocumentParams
    '    Dim mNumPerMsg As DataTable
    '    Dim mSql As String
    '    Dim _alDocumentLine As ArrayList
    '    Dim _alDocumentLineBatch As ArrayList
    '    _alDocumentLine = New ArrayList
    '    _alDocumentLineBatch = New ArrayList
    '    Dim SAPConnection As SAPSQLConnections

    '    SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


    '    Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
    '                                    "From [dbo].[POR1] " & _
    '                                    "Where DocEntry = {0} and LineNum = {1}"

    '    _dt_DocEntry = _Purchase.ExecuteDatatable(String.Format("Select distinct {0}, {1}, {2} " & _
    '                                                           "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                           "Where ISNULL(TRXSTATUS,'') = '' ", _
    '                                                           Purchase.Fld_DocEntry, _
    '                                                           Purchase.Fld_DocDueDate, _
    '                                                           Purchase.Fld_CardCode))

    '    _Debug.WriteTable(_dt_DocEntry, "Distinct CPS_TBL_OPOR")
    '    For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
    '        _Debug.Write(_dt_DocEntry.Rows(i)(Purchase.Fld_DocEntry), "DocEntry")
    '        Try
    '            oDoc = Nothing
    '            _GRPO = Nothing
    '            _msgHeader = Nothing
    '            _TargetEntry = ""
    '            _alDocumentLineBatch.Clear()
    '            _alDocumentLine.Clear()
    '            _GRPO = New GRPO.PurchaseDeliveryNotesService
    '            _msgHeader = New GRPO.MsgHeader
    '            _msgHeader.SessionID = MyBase.SessionID
    '            _msgHeader.ServiceName = B1WebService.GRPO.MsgHeaderServiceName.PurchaseDeliveryNotesService
    '            _msgHeader.ServiceNameSpecified = True
    '            _GRPO.MsgHeaderValue = _msgHeader
    '            oDoc = New GRPO.Document

    '            oDoc.DocDate = Today.Date
    '            oDoc.DocDueDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
    '            oDoc.CardCode = _dt_DocEntry.Rows(i).Item(Purchase.Fld_CardCode)

    '            _dt_DocLine = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, sum({3}) as 'Quantity' " & _
    '                                                               "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                               "Where {4} = {5} and isNull({9},'') = ''  Group by {6},{7},{8}", _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               Purchase.Fld_LineNum, _
    '                                                               Purchase.Fld_ItemCode, _
    '                                                               Purchase.Fld_Quantity, _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry),
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               Purchase.Fld_LineNum,
    '                                                               Purchase.Fld_ItemCode, Purchase.Fld_TrxStatus))
    '            _Debug.WriteTable(_dt_DocLine, "Line Table")
    '            For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
    '                _alDocumentLineBatch.Clear()
    '                docLine = Nothing
    '                docLine = New GRPO.DocumentDocumentLine
    '                docLine.BaseTypeSpecified = True
    '                docLine.BaseType = "22"
    '                docLine.BaseEntrySpecified = True
    '                docLine.BaseEntry = _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry)
    '                docLine.BaseLineSpecified = True
    '                docLine.BaseLine = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)
    '                docLine.ItemCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_ItemCode)
    '                docLine.QuantitySpecified = True
    '                docLine.Quantity = _dt_DocLine.Rows(j).Item(Purchase.Fld_Quantity)
    '                docLine.WarehouseCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_WhsCode)
    '                'Karrson: Add WMS Number to UDF

    '                _Debug.Write(docLine.Quantity, "Item Quantity")
    '                _dt_BatchNum = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, {3} " & _
    '                                                               "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                               "Where {4} = {5} and {6} = {7} and isNull({8},'') = ''", _
    '                                                               Purchase.Fld_BatchNumber, _
    '                                                               Purchase.Fld_Quantity, _
    '                                                               Purchase.Fld_ExpireDate, _
    '                                                               Purchase.Fld_MfrDate, _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
    '                                                               Purchase.Fld_LineNum, _
    '                                                               _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum), Purchase.Fld_TrxStatus))


    '                _Debug.WriteTable(_dt_BatchNum, "Batch Table")
    '                For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
    '                    If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)) Then

    '                        mSql = String.Format(sqlGetNumPerMsg, _
    '                                             _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
    '                                             _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum))
    '                        mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

    '                        docLineBatch = Nothing
    '                        docLineBatch = New GRPO.DocumentDocumentLineBatchNumber
    '                        'docLineBatch.BaseLineNumberSpecified = True
    '                        'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)

    '                        docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)

    '                        docLineBatch.ManufacturingDateSpecified = True
    '                        docLineBatch.ManufacturingDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_MfrDate)
    '                        docLineBatch.ExpiryDateSpecified = True
    '                        docLineBatch.ExpiryDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_ExpireDate)

    '                        docLineBatch.QuantitySpecified = True
    '                        docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(Purchase.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
    '                        _Debug.Write(docLine.ItemCode, "Item Code")
    '                        _Debug.Write(docLineBatch.BatchNumber, "Batch Number")
    '                        _Debug.Write(docLineBatch.Quantity, "Batch Quantity")
    '                        _alDocumentLineBatch.Add(docLineBatch)
    '                    End If
    '                Next
    '                If _alDocumentLineBatch.Count > 0 Then
    '                    _Debug.Write("Batches")
    '                    For Each o As GRPO.DocumentDocumentLineBatchNumber In _alDocumentLineBatch.ToArray
    '                        _Debug.Write(o.BaseLineNumber, "Base Line Number")
    '                        _Debug.Write(o.BatchNumber, "Batch Number")
    '                        _Debug.Write(o.Quantity, "Quantity")
    '                    Next
    '                    docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
    '                End If

    '                _alDocumentLine.Add(docLine)
    '            Next
    '            If _alDocumentLine.Count > 0 Then
    '                _Debug.Write("Document Lines")
    '                For Each o As GRPO.DocumentDocumentLine In _alDocumentLine.ToArray
    '                    _Debug.Write(o.BaseEntry, "BaseEntry")
    '                    _Debug.Write(o.BaseLine, "BaseLine")
    '                    _Debug.Write(o.ItemCode, "ItemCode")
    '                    _Debug.Write(o.Quantity, "Quantity")
    '                Next
    '            End If
    '            oDoc.DocumentLines = MyBase.GRPOtoDocumentLineArray(_alDocumentLine)
    '            _Debug.Write("Create GRPO")


    '            DocParams = _GRPO.Add(oDoc)
    '            If DocParams.DocEntry > 0 Then
    '                ' Update Success Status
    '                _Debug.Write("Update Success Status")
    '                _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), DocParams.DocEntry, String.Empty)
    '                _TargetEntry = DocParams.DocEntry.ToString
    '            End If


    '            'If _PurConfig.isDraft Then
    '            '    _Debug.Write("create to draft document")

    '            '    _GRPO.SaveDraftToDocument()
    '            '    _Debug.Write("Update Success Status")
    '            '    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), -1, String.Empty)
    '            'Else
    '            '    _Debug.Write("create to actrual document")


    '            'End If
    '        Catch ex As Exception
    '            _ret = False
    '            _Debug.Write("Update Error Status")
    '            _Purchase.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), "-1", ex.Message.Replace("'", "''"))
    '            _Message = ex.Message
    '        End Try



    '    Next

    '    Return _ret
    'End Function

    '--------------MK Development-------------------------
    'Public Function Create_GRPO() As Boolean
    '    Dim _ret As Boolean = True
    '    _Debug.Write("Createing GRPO")
    '    Dim _dt_DocEntry As DataTable
    '    Dim _dt_DocLine As DataTable
    '    Dim _dt_BatchNum As DataTable
    '    Dim _msgHeader As GRPO.MsgHeader
    '    Dim oDoc As GRPO.Document

    '    Dim _TargetEntry As String
    '    Dim docLine As GRPO.DocumentDocumentLine
    '    Dim docLineBatch As GRPO.DocumentDocumentLineBatchNumber
    '    Dim DocParams As GRPO.DocumentParams
    '    Dim mNumPerMsg As DataTable
    '    Dim mSql As String
    '    Dim _alDocumentLine As ArrayList
    '    Dim _alDocumentLineBatch As ArrayList
    '    _alDocumentLine = New ArrayList
    '    _alDocumentLineBatch = New ArrayList
    '    Dim SAPConnection As SAPSQLConnections

    '    SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


    '    Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
    '                                    "From [dbo].[POR1] " & _
    '                                    "Where DocEntry = {0} and LineNum = {1}"

    '    _dt_DocEntry = _Purchase.ExecuteDatatable(String.Format("Select distinct {0}, {1}, {2},{3} " & _
    '                                                           "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                           "Where ISNULL(TRXSTATUS,'') = '' ", _
    '                                                           Purchase.Fld_DocEntry, _
    '                                                           Purchase.Fld_DocDueDate, _
    '                                                           Purchase.Fld_CardCode,
    '                                                           Purchase.Fld_WhsCode))

    '    _Debug.WriteTable(_dt_DocEntry, "Distinct CPS_TBL_OPOR")
    '    For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
    '        _Debug.Write(_dt_DocEntry.Rows(i)(Purchase.Fld_DocEntry), "DocEntry")
    '        Try
    '            oDoc = Nothing
    '            _GRPO = Nothing
    '            _msgHeader = Nothing
    '            _TargetEntry = ""
    '            _alDocumentLineBatch.Clear()
    '            _alDocumentLine.Clear()
    '            _GRPO = New GRPO.PurchaseDeliveryNotesService
    '            _msgHeader = New GRPO.MsgHeader
    '            _msgHeader.SessionID = MyBase.SessionID
    '            _msgHeader.ServiceName = B1WebService.GRPO.MsgHeaderServiceName.PurchaseDeliveryNotesService
    '            _msgHeader.ServiceNameSpecified = True
    '            _GRPO.MsgHeaderValue = _msgHeader
    '            oDoc = New GRPO.Document

    '            oDoc.DocDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
    '            'oDoc.DocDate = DateTime.Now
    '            oDoc.DocDueDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
    '            'oDoc.DocDueDate = DateTime.Now
    '            oDoc.CardCode = _dt_DocEntry.Rows(i).Item(Purchase.Fld_CardCode)

    '            _dt_DocLine = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2},{10}, sum({3}) as 'Quantity' " & _
    '                                                               "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                               "Where {4} = {5} and isNull({9},'') = ''  Group by {6},{7},{8},{10}", _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               Purchase.Fld_LineNum, _
    '                                                               Purchase.Fld_ItemCode, _
    '                                                               Purchase.Fld_Quantity, _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry),
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               Purchase.Fld_LineNum,
    '                                                               Purchase.Fld_ItemCode, Purchase.Fld_TrxStatus, Purchase.Fld_WhsCode))
    '            _Debug.WriteTable(_dt_DocLine, "Line Table")
    '            For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
    '                _alDocumentLineBatch.Clear()
    '                docLine = Nothing
    '                docLine = New GRPO.DocumentDocumentLine
    '                docLine.BaseTypeSpecified = True
    '                docLine.BaseType = "22"
    '                docLine.BaseEntrySpecified = True
    '                docLine.BaseEntry = _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry)
    '                docLine.BaseLineSpecified = True
    '                docLine.BaseLine = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)
    '                docLine.ItemCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_ItemCode)
    '                docLine.QuantitySpecified = True
    '                docLine.Quantity = _dt_DocLine.Rows(j).Item(Purchase.Fld_Quantity)
    '                docLine.WarehouseCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_WhsCode)
    '                'Karrson: Add WMS Number to UDF

    '                _Debug.Write(docLine.Quantity, "Item Quantity")
    '                _dt_BatchNum = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, {3} " & _
    '                                                               "From [dbo].[CPS_TBL_OPOR] " & _
    '                                                               "Where {4} = {5} and {6} = {7} and isNull({8},'') = ''", _
    '                                                               Purchase.Fld_BatchNumber, _
    '                                                               Purchase.Fld_Quantity, _
    '                                                               Purchase.Fld_ExpireDate, _
    '                                                               Purchase.Fld_MfrDate, _
    '                                                               Purchase.Fld_DocEntry, _
    '                                                               _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
    '                                                               Purchase.Fld_LineNum, _
    '                                                               _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum), Purchase.Fld_TrxStatus))


    '                _Debug.WriteTable(_dt_BatchNum, "Batch Table")
    '                For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
    '                    If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)) Then

    '                        mSql = String.Format(sqlGetNumPerMsg, _
    '                                             _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
    '                                             _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum))
    '                        mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

    '                        docLineBatch = Nothing
    '                        docLineBatch = New GRPO.DocumentDocumentLineBatchNumber
    '                        'docLineBatch.BaseLineNumberSpecified = True
    '                        'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)

    '                        docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)

    '                        docLineBatch.ManufacturingDateSpecified = True
    '                        docLineBatch.ManufacturingDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_MfrDate)
    '                        docLineBatch.ExpiryDateSpecified = True
    '                        docLineBatch.ExpiryDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_ExpireDate)

    '                        docLineBatch.QuantitySpecified = True
    '                        docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(Purchase.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
    '                        _Debug.Write(docLine.ItemCode, "Item Code")
    '                        _Debug.Write(docLineBatch.BatchNumber, "Batch Number")
    '                        _Debug.Write(docLineBatch.Quantity, "Batch Quantity")
    '                        _alDocumentLineBatch.Add(docLineBatch)
    '                    End If
    '                Next
    '                If _alDocumentLineBatch.Count > 0 Then
    '                    _Debug.Write("Batches")
    '                    For Each o As GRPO.DocumentDocumentLineBatchNumber In _alDocumentLineBatch.ToArray
    '                        _Debug.Write(o.BaseLineNumber, "Base Line Number")
    '                        _Debug.Write(o.BatchNumber, "Batch Number")
    '                        _Debug.Write(o.Quantity, "Quantity")
    '                    Next
    '                    docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
    '                End If

    '                _alDocumentLine.Add(docLine)
    '            Next
    '            If _alDocumentLine.Count > 0 Then
    '                _Debug.Write("Document Lines")
    '                For Each o As GRPO.DocumentDocumentLine In _alDocumentLine.ToArray
    '                    _Debug.Write(o.BaseEntry, "BaseEntry")
    '                    _Debug.Write(o.BaseLine, "BaseLine")
    '                    _Debug.Write(o.ItemCode, "ItemCode")
    '                    _Debug.Write(o.Quantity, "Quantity")
    '                Next
    '            End If
    '            oDoc.DocumentLines = MyBase.GRPOtoDocumentLineArray(_alDocumentLine)
    '            _Debug.Write("Create GRPO")


    '            DocParams = _GRPO.Add(oDoc)
    '            If DocParams.DocEntry > 0 Then
    '                ' Update Success Status
    '                _Debug.Write("Update Success Status")
    '                _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), DocParams.DocEntry, String.Empty)
    '                _TargetEntry = DocParams.DocEntry.ToString
    '            End If


    '            'If _PurConfig.isDraft Then
    '            '    _Debug.Write("create to draft document")

    '            '    _GRPO.SaveDraftToDocument()
    '            '    _Debug.Write("Update Success Status")
    '            '    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), -1, String.Empty)
    '            'Else
    '            '    _Debug.Write("create to actrual document")


    '            'End If
    '        Catch ex As Exception
    '            _ret = False
    '            _Debug.Write("Update Error Status")
    '            _Purchase.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), "-1", ex.Message.Replace("'", "''"))
    '            _Message = ex.Message
    '        End Try



    '    Next

    '    Return _ret

    'End Function

End Class

