Public Class WS_GRPO : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _GRPO As GRPO.PurchaseDeliveryNotesService
    Dim _PurConfig As PurchaseConfig
    Dim _Purchase As PurchaseForWMS
    Dim _htDocStatus As Hashtable

    Private _isError As Boolean
    Dim _Setting As Settings

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
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        If MyBase.isConnected = False Then
            isError = True
            Message = MyBase.Message
        End If
        _PurConfig = New PurchaseConfig(_Setting)
        _Purchase = New PurchaseForWMS(_Setting, Nothing)
        _htDocStatus = New Hashtable
    End Sub

    Public Function Generate(ByVal _ReceiveEntry As String) As Boolean
        Dim _ret As Boolean = True
        Dim _dt As DataTable
        _Debug.Write("Generating Document: " & _ReceiveEntry)
        Try
            _dt = _Purchase.OpenPurchaseOrder(_ReceiveEntry)
            If _dt Is Nothing = False Then
                For Each _dr As DataRow In _dt.Rows
                    _htDocStatus.Add(_dr(Purchase.Fld_DocEntry), Generate(_dr(Purchase.Fld_DocEntry), _ReceiveEntry))
                Next
            Else

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Function

    Private Function Generate(ByVal _DocEntry As Integer, ByVal _ReceiveEntry As String) As String
        Dim _msgHeader As GRPO.MsgHeader
        Dim oDoc As GRPO.Document
        Dim _ret As String
        Dim docLine As GRPO.DocumentDocumentLine
        Dim docLineBatch As GRPO.DocumentDocumentLineBatchNumber
        Dim DocParams As GRPO.DocumentParams
        Dim _dtLine As DataTable
        Dim _drLine As DataRow()

        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList

        Dim _isFirst As Boolean = True
        Dim _PrevDocEntry As Integer = -1
        Dim _PrevLineNum As Integer = -1
        Dim _PrevItemCode As String = ""
        Dim _cumQty As Decimal = 0
        Dim _LineNum As Integer = 0
        Try
            _dtLine = _Purchase.OpenPurchaseOrder(_DocEntry, _ReceiveEntry)
            _drLine = _dtLine.Select("1 = 1", String.Format("{0} asc, {1} asc", Purchase.Fld_DocEntry, Purchase.Fld_LineNum))

            _GRPO = New GRPO.PurchaseDeliveryNotesService
            _msgHeader = New GRPO.MsgHeader
            _msgHeader.SessionID = MyBase.SessionID
            _msgHeader.ServiceName = B1WebService.GRPO.MsgHeaderServiceName.PurchaseDeliveryNotesService
            _msgHeader.ServiceNameSpecified = True
            _GRPO.MsgHeaderValue = _msgHeader




            _isFirst = True

            ' fill business partner class
            oDoc = New GRPO.Document
            _PrevLineNum = -1

            _alDocumentLine = New ArrayList
            _LineNum = 0
            For Each _dr As DataRow In _drLine
                docLineBatch = New GRPO.DocumentDocumentLineBatchNumber

                If _isFirst Then
                    oDoc.DocDate = _dr(Purchase.Fld_DocDate)
                    oDoc.DocDueDate = _dr(Purchase.Fld_DocDueDate)

                End If

                _isFirst = False
                If _PrevLineNum <> _dr(Purchase.Fld_LineNum) Then

                    If _PrevLineNum <> -1 Then
                        ' Add new Row

                        docLine.BaseTypeSpecified = True
                        docLine.BaseType = "22"
                        docLine.BaseEntrySpecified = True
                        docLine.BaseEntry = _PrevDocEntry
                        docLine.BaseLineSpecified = True
                        docLine.BaseLine = _PrevLineNum
                        docLine.QuantitySpecified = True
                        docLine.Quantity = _cumQty
                        docLine.ItemCode = _PrevItemCode
                        docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
                        _alDocumentLine.Add(docLine)
                        _LineNum = _LineNum + 1
                    End If
                    _alDocumentLineBatch = New ArrayList
                    docLine = New GRPO.DocumentDocumentLine
                End If


                _cumQty = _cumQty + _dr(Purchase.Fld_Quantity)
                _PrevLineNum = _dr(Purchase.Fld_LineNum)
                _PrevDocEntry = _dr(Purchase.Fld_DocEntry)
                _PrevItemCode = _dr(Purchase.Fld_ItemCode)

                docLineBatch.BaseLineNumberSpecified = True
                docLineBatch.BaseLineNumber = _LineNum

                docLineBatch.BatchNumber = _dr(Purchase.Fld_BatchNumber)

                docLineBatch.ManufacturingDateSpecified = True
                docLineBatch.ManufacturingDate = _dr(Purchase.Fld_MfrDate)
                docLineBatch.ExpiryDateSpecified = True
                docLineBatch.ExpiryDate = _dr(Purchase.Fld_ExpireDate)


                docLineBatch.Location = _dr(Purchase.Fld_LocCode)
                'docLineBatch.U_AlcoLvl = _dr(Purchase.Fld_Alcohollv)
                docLineBatch.QuantitySpecified = True
                docLineBatch.Quantity = _dr(Purchase.Fld_Quantity)

                _alDocumentLineBatch.Add(docLineBatch)

            Next
            If _PrevLineNum >= 0 Then

                docLine.BaseTypeSpecified = True
                docLine.BaseType = "22"
                docLine.BaseEntrySpecified = True
                docLine.BaseEntry = _PrevDocEntry
                docLine.BaseLineSpecified = True
                docLine.BaseLine = _PrevLineNum

                docLine.QuantitySpecified = True
                docLine.Quantity = _cumQty

                docLine.ItemCode = _PrevItemCode
                docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
                _alDocumentLine.Add(docLine)
                oDoc.DocumentLines = MyBase.GRPOtoDocumentLineArray(_alDocumentLine)

                If _PurConfig.isDraft Then
                    _GRPO.SaveDraftToDocument()
                    _Purchase.UpdateSuccessStatus(_DocEntry, -1, String.Empty)
                Else
                    DocParams = _GRPO.Add(oDoc)
                    If DocParams.DocEntry > 0 Then
                        ' Update Success Status
                        _Purchase.UpdateSuccessStatus(_DocEntry, DocParams.DocEntry, String.Empty)
                        _ret = ""
                    End If

                End If



            Else
                _Purchase.UpdateErrorStatus(_DocEntry, "-1", "Internal Error.")
                _ret = "Internal Error"
            End If

        Catch ex As Exception
            ' Update Failure Status
            _Purchase.UpdateErrorStatus(_DocEntry, "-1", ex.Message)
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
    End Function

    Public Function Create(ByVal _dt As DataTable) As Boolean
        ' Validate weather PO with same warehouse or not
        Dim _SAPPurchase As PurchaseForSAP
        Dim _drRow() As DataRow
        Dim _PrevDocEntry As String = String.Empty
        Dim _PrevLineNunm As String = String.Empty
        Dim _PrevWhsCode As String = String.Empty

        Try

        
        _dt.Columns.Add(Purchase.Fld_SameWhse)
        _dt.Columns.Add(Purchase.Fld_Processed)
        _SAPPurchase = New PurchaseForSAP(_Setting, Nothing)
            For Each _dr As DataRow In _dt.Rows

                If _SAPPurchase.isSameWarehouse(Settings.DBNull(_dr(Purchase.Fld_DocEntry), "-1"), Settings.DBNull(_dr(Purchase.Fld_LineNum), "-1"), _dr(Purchase.Fld_WhsCode)) Then
                    _dr(Purchase.Fld_SameWhse) = "Y"
                    _dr(Purchase.Fld_Processed) = "Y"
                Else
                    _dr(Purchase.Fld_SameWhse) = "N"
                    _dr(Purchase.Fld_Processed) = "Y"
                End If
            Next

        ' Generate with same warehouse 
        _drRow = _dt.Select(String.Format("{0} = 'Y'", Purchase.Fld_SameWhse))
            If _drRow Is Nothing = False Then
                If _drRow.Length > 0 Then
                    If Create_InProcess(_drRow) = False Then

                    End If
                End If
            End If
            ' Generate without same warehouse

            _drRow = _dt.Select(String.Format("{0} = 'Y'", Purchase.Fld_SameWhse, "{0} asc,{1} asc,{2} asc"))

            If _drRow Is Nothing = False Then
                If _drRow.Length > 0 Then
                    Do While _drRow.Length > 0

                        For Each _dr As DataRow In _drRow

                            _PrevDocEntry = _dr(Purchase.Fld_DocEntry)
                            _PrevLineNunm = _dr(Purchase.Fld_LineNum)
                            _PrevWhsCode = _dr(Purchase.Fld_WhsCode)
                        Next

                    Loop
                End If

            End If

        Catch ex As Exception
            Me.Message = ex.Message
            _CPSException.ExecuteHandle(ex)
            Return False
        End Try
    End Function

    Public Function Create_InProcess(ByVal _dr As DataRow()) As Boolean
        Dim _ret As Boolean = True
        _Debug.Write("Createing GRPO")
        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _msgHeader As GRPO.MsgHeader
        Dim oDoc As GRPO.Document

        Dim _TargetEntry As String
        Dim docLine As GRPO.DocumentDocumentLine
        Dim docLineBatch As GRPO.DocumentDocumentLineBatchNumber
        Dim DocParams As GRPO.DocumentParams
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList
        _alDocumentLine = New ArrayList
        _alDocumentLineBatch = New ArrayList
        Dim SAPConnection As SAPSQLConnections

        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[POR1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        _dt_DocEntry = _Purchase.ExecuteDatatable(String.Format("Select distinct {0}, {1}, {2} " & _
                                                               "From [dbo].[CPS_TBL_OPOR] " & _
                                                               "Where ISNULL(TRXSTATUS,'') = '' ", _
                                                               Purchase.Fld_DocEntry, _
                                                               Purchase.Fld_DocDueDate, _
                                                               Purchase.Fld_CardCode))

        _Debug.WriteTable(_dt_DocEntry, "Distinct CPS_TBL_OPOR")
        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            _Debug.Write(_dt_DocEntry.Rows(i)(Purchase.Fld_DocEntry), "DocEntry")
            Try
                oDoc = Nothing
                _GRPO = Nothing
                _msgHeader = Nothing
                _TargetEntry = ""
                _alDocumentLineBatch.Clear()
                _alDocumentLine.Clear()
                _GRPO = New GRPO.PurchaseDeliveryNotesService
                _msgHeader = New GRPO.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = B1WebService.GRPO.MsgHeaderServiceName.PurchaseDeliveryNotesService
                _msgHeader.ServiceNameSpecified = True
                _GRPO.MsgHeaderValue = _msgHeader
                oDoc = New GRPO.Document

                oDoc.DocDate = Today.Date
                oDoc.DocDueDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
                oDoc.CardCode = _dt_DocEntry.Rows(i).Item(Purchase.Fld_CardCode)

                _dt_DocLine = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, sum({3}) as 'Quantity' " & _
                                                                   "From [dbo].[CPS_TBL_OPOR] " & _
                                                                   "Where {4} = {5} and isNull({9},'') = ''  Group by {6},{7},{8}", _
                                                                   Purchase.Fld_DocEntry, _
                                                                   Purchase.Fld_LineNum, _
                                                                   Purchase.Fld_ItemCode, _
                                                                   Purchase.Fld_Quantity, _
                                                                   Purchase.Fld_DocEntry, _
                                                                   _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry),
                                                                   Purchase.Fld_DocEntry, _
                                                                   Purchase.Fld_LineNum,
                                                                   Purchase.Fld_ItemCode, Purchase.Fld_TrxStatus))
                _Debug.WriteTable(_dt_DocLine, "Line Table")
                For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
                    _alDocumentLineBatch.Clear()
                    docLine = Nothing
                    docLine = New GRPO.DocumentDocumentLine
                    docLine.BaseTypeSpecified = True
                    docLine.BaseType = "22"
                    docLine.BaseEntrySpecified = True
                    docLine.BaseEntry = _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry)
                    docLine.BaseLineSpecified = True
                    docLine.BaseLine = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)
                    docLine.ItemCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_ItemCode)
                    docLine.QuantitySpecified = True
                    docLine.Quantity = _dt_DocLine.Rows(j).Item(Purchase.Fld_Quantity)
                    docLine.WarehouseCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_WhsCode)
                    'Karrson: Add WMS Number to UDF

                    _Debug.Write(docLine.Quantity, "Item Quantity")
                    _dt_BatchNum = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, {3} " & _
                                                                   "From [dbo].[CPS_TBL_OPOR] " & _
                                                                   "Where {4} = {5} and {6} = {7} and isNull({8},'') = ''", _
                                                                   Purchase.Fld_BatchNumber, _
                                                                   Purchase.Fld_Quantity, _
                                                                   Purchase.Fld_ExpireDate, _
                                                                   Purchase.Fld_MfrDate, _
                                                                   Purchase.Fld_DocEntry, _
                                                                   _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                                   Purchase.Fld_LineNum, _
                                                                   _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum), Purchase.Fld_TrxStatus))


                    _Debug.WriteTable(_dt_BatchNum, "Batch Table")
                    For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
                        If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)) Then

                            mSql = String.Format(sqlGetNumPerMsg, _
                                                 _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                 _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum))
                            mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

                            docLineBatch = Nothing
                            docLineBatch = New GRPO.DocumentDocumentLineBatchNumber
                            'docLineBatch.BaseLineNumberSpecified = True
                            'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)

                            docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)

                            docLineBatch.ManufacturingDateSpecified = True
                            docLineBatch.ManufacturingDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_MfrDate)
                            docLineBatch.ExpiryDateSpecified = True
                            docLineBatch.ExpiryDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_ExpireDate)

                            docLineBatch.QuantitySpecified = True
                            docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(Purchase.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
                            _Debug.Write(docLine.ItemCode, "Item Code")
                            _Debug.Write(docLineBatch.BatchNumber, "Batch Number")
                            _Debug.Write(docLineBatch.Quantity, "Batch Quantity")
                            _alDocumentLineBatch.Add(docLineBatch)
                        End If
                    Next
                    If _alDocumentLineBatch.Count > 0 Then
                        _Debug.Write("Batches")
                        For Each o As GRPO.DocumentDocumentLineBatchNumber In _alDocumentLineBatch.ToArray
                            _Debug.Write(o.BaseLineNumber, "Base Line Number")
                            _Debug.Write(o.BatchNumber, "Batch Number")
                            _Debug.Write(o.Quantity, "Quantity")
                        Next
                        docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
                    End If

                    _alDocumentLine.Add(docLine)
                Next
                If _alDocumentLine.Count > 0 Then
                    _Debug.Write("Document Lines")
                    For Each o As GRPO.DocumentDocumentLine In _alDocumentLine.ToArray
                        _Debug.Write(o.BaseEntry, "BaseEntry")
                        _Debug.Write(o.BaseLine, "BaseLine")
                        _Debug.Write(o.ItemCode, "ItemCode")
                        _Debug.Write(o.Quantity, "Quantity")
                    Next
                End If
                oDoc.DocumentLines = MyBase.GRPOtoDocumentLineArray(_alDocumentLine)
                _Debug.Write("Create GRPO")


                DocParams = _GRPO.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Debug.Write("Update Success Status")
                    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), DocParams.DocEntry, String.Empty)
                    _TargetEntry = DocParams.DocEntry.ToString
                End If


                'If _PurConfig.isDraft Then
                '    _Debug.Write("create to draft document")

                '    _GRPO.SaveDraftToDocument()
                '    _Debug.Write("Update Success Status")
                '    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), -1, String.Empty)
                'Else
                '    _Debug.Write("create to actrual document")


                'End If
            Catch ex As Exception
                _ret = False
                _Debug.Write("Update Error Status")
                _Purchase.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), "-1", ex.Message.Replace("'", "''"))
                _Message = ex.Message
            End Try



        Next

        Return _ret
    End Function

    '--------------MK Development-------------------------
    Public Function Create_GRPO() As Boolean
        Dim _ret As Boolean = True
        _Debug.Write("Createing GRPO")
        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _msgHeader As GRPO.MsgHeader
        Dim oDoc As GRPO.Document

        Dim _TargetEntry As String
        Dim docLine As GRPO.DocumentDocumentLine
        Dim docLineBatch As GRPO.DocumentDocumentLineBatchNumber
        Dim DocParams As GRPO.DocumentParams
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList
        _alDocumentLine = New ArrayList
        _alDocumentLineBatch = New ArrayList
        Dim SAPConnection As SAPSQLConnections

        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[POR1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        _dt_DocEntry = _Purchase.ExecuteDatatable(String.Format("Select distinct {0}, {1}, {2},{3} " & _
                                                               "From [dbo].[CPS_TBL_OPOR] " & _
                                                               "Where ISNULL(TRXSTATUS,'') = '' ", _
                                                               Purchase.Fld_DocEntry, _
                                                               Purchase.Fld_DocDueDate, _
                                                               Purchase.Fld_CardCode,
                                                               Purchase.Fld_WhsCode))

        _Debug.WriteTable(_dt_DocEntry, "Distinct CPS_TBL_OPOR")
        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            _Debug.Write(_dt_DocEntry.Rows(i)(Purchase.Fld_DocEntry), "DocEntry")
            Try
                oDoc = Nothing
                _GRPO = Nothing
                _msgHeader = Nothing
                _TargetEntry = ""
                _alDocumentLineBatch.Clear()
                _alDocumentLine.Clear()
                _GRPO = New GRPO.PurchaseDeliveryNotesService
                _msgHeader = New GRPO.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = B1WebService.GRPO.MsgHeaderServiceName.PurchaseDeliveryNotesService
                _msgHeader.ServiceNameSpecified = True
                _GRPO.MsgHeaderValue = _msgHeader
                oDoc = New GRPO.Document

                oDoc.DocDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
                'oDoc.DocDate = DateTime.Now
                oDoc.DocDueDate = _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocDueDate)
                'oDoc.DocDueDate = DateTime.Now
                oDoc.CardCode = _dt_DocEntry.Rows(i).Item(Purchase.Fld_CardCode)

                _dt_DocLine = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2},{10}, sum({3}) as 'Quantity' " & _
                                                                   "From [dbo].[CPS_TBL_OPOR] " & _
                                                                   "Where {4} = {5} and isNull({9},'') = ''  Group by {6},{7},{8},{10}", _
                                                                   Purchase.Fld_DocEntry, _
                                                                   Purchase.Fld_LineNum, _
                                                                   Purchase.Fld_ItemCode, _
                                                                   Purchase.Fld_Quantity, _
                                                                   Purchase.Fld_DocEntry, _
                                                                   _dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry),
                                                                   Purchase.Fld_DocEntry, _
                                                                   Purchase.Fld_LineNum,
                                                                   Purchase.Fld_ItemCode, Purchase.Fld_TrxStatus, Purchase.Fld_WhsCode))
                _Debug.WriteTable(_dt_DocLine, "Line Table")
                For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
                    _alDocumentLineBatch.Clear()
                    docLine = Nothing
                    docLine = New GRPO.DocumentDocumentLine
                    docLine.BaseTypeSpecified = True
                    docLine.BaseType = "22"
                    docLine.BaseEntrySpecified = True
                    docLine.BaseEntry = _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry)
                    docLine.BaseLineSpecified = True
                    docLine.BaseLine = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)
                    docLine.ItemCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_ItemCode)
                    docLine.QuantitySpecified = True
                    docLine.Quantity = _dt_DocLine.Rows(j).Item(Purchase.Fld_Quantity)
                    docLine.WarehouseCode = _dt_DocLine.Rows(j).Item(Purchase.Fld_WhsCode)
                    'Karrson: Add WMS Number to UDF

                    _Debug.Write(docLine.Quantity, "Item Quantity")
                    _dt_BatchNum = _Purchase.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, {3} " & _
                                                                   "From [dbo].[CPS_TBL_OPOR] " & _
                                                                   "Where {4} = {5} and {6} = {7} and isNull({8},'') = ''", _
                                                                   Purchase.Fld_BatchNumber, _
                                                                   Purchase.Fld_Quantity, _
                                                                   Purchase.Fld_ExpireDate, _
                                                                   Purchase.Fld_MfrDate, _
                                                                   Purchase.Fld_DocEntry, _
                                                                   _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                                   Purchase.Fld_LineNum, _
                                                                   _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum), Purchase.Fld_TrxStatus))


                    _Debug.WriteTable(_dt_BatchNum, "Batch Table")
                    For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
                        If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)) Then

                            mSql = String.Format(sqlGetNumPerMsg, _
                                                 _dt_DocLine.Rows(j).Item(Purchase.Fld_DocEntry), _
                                                 _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum))
                            mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

                            docLineBatch = Nothing
                            docLineBatch = New GRPO.DocumentDocumentLineBatchNumber
                            'docLineBatch.BaseLineNumberSpecified = True
                            'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(Purchase.Fld_LineNum)

                            docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Purchase.Fld_BatchNumber)

                            docLineBatch.ManufacturingDateSpecified = True
                            docLineBatch.ManufacturingDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_MfrDate)
                            docLineBatch.ExpiryDateSpecified = True
                            docLineBatch.ExpiryDate = _dt_BatchNum.Rows(k).Item(Purchase.Fld_ExpireDate)

                            docLineBatch.QuantitySpecified = True
                            docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(Purchase.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
                            _Debug.Write(docLine.ItemCode, "Item Code")
                            _Debug.Write(docLineBatch.BatchNumber, "Batch Number")
                            _Debug.Write(docLineBatch.Quantity, "Batch Quantity")
                            _alDocumentLineBatch.Add(docLineBatch)
                        End If
                    Next
                    If _alDocumentLineBatch.Count > 0 Then
                        _Debug.Write("Batches")
                        For Each o As GRPO.DocumentDocumentLineBatchNumber In _alDocumentLineBatch.ToArray
                            _Debug.Write(o.BaseLineNumber, "Base Line Number")
                            _Debug.Write(o.BatchNumber, "Batch Number")
                            _Debug.Write(o.Quantity, "Quantity")
                        Next
                        docLine.BatchNumbers = MyBase.GRPOtoDocumentLineBatchArray(_alDocumentLineBatch)
                    End If

                    _alDocumentLine.Add(docLine)
                Next
                If _alDocumentLine.Count > 0 Then
                    _Debug.Write("Document Lines")
                    For Each o As GRPO.DocumentDocumentLine In _alDocumentLine.ToArray
                        _Debug.Write(o.BaseEntry, "BaseEntry")
                        _Debug.Write(o.BaseLine, "BaseLine")
                        _Debug.Write(o.ItemCode, "ItemCode")
                        _Debug.Write(o.Quantity, "Quantity")
                    Next
                End If
                oDoc.DocumentLines = MyBase.GRPOtoDocumentLineArray(_alDocumentLine)
                _Debug.Write("Create GRPO")


                DocParams = _GRPO.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Debug.Write("Update Success Status")
                    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), DocParams.DocEntry, String.Empty)
                    _TargetEntry = DocParams.DocEntry.ToString
                End If


                'If _PurConfig.isDraft Then
                '    _Debug.Write("create to draft document")

                '    _GRPO.SaveDraftToDocument()
                '    _Debug.Write("Update Success Status")
                '    _Purchase.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), -1, String.Empty)
                'Else
                '    _Debug.Write("create to actrual document")


                'End If
            Catch ex As Exception
                _ret = False
                _Debug.Write("Update Error Status")
                _Purchase.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Purchase.Fld_DocEntry), "-1", ex.Message.Replace("'", "''"))
                _Message = ex.Message
            End Try



        Next

        Return _ret

    End Function

End Class
