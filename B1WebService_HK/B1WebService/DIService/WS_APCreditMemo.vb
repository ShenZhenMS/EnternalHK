Public Class WS_APCreditMemo : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable

    Dim _APCreditMemoSrv As APCreditMemo.PurchaseCreditNotesService
    Dim _APCreditMemoConfig As PurchaseCreditMemoConfig

    Dim _APCreditMemo As PurchaseCreditMemoForWMS
    Dim _SAPAPCreditMemo As PurchaseCreditMemoForSAP
    Dim _htKeyValue As Hashtable
    Dim _htDocStatus As Hashtable
    Public Shared ObjType As String = "19"
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
        _SAPAPCreditMemo = New PurchaseCreditMemoForSAP(_Setting, Nothing)
        _APCreditMemoConfig = New PurchaseCreditMemoConfig(_Setting)
        _APCreditMemo = New PurchaseCreditMemoForWMS(_Setting, Nothing, PurchaseCreditMemoForWMS._DocumentType.PR)
        _htDocStatus = New Hashtable
    End Sub

    Public Function Start(ByVal _o As String) As Boolean
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty

        Dim _msgHeader As APCreditMemo.MsgHeader
        Dim _APCreditMemoSrv As APCreditMemo.PurchaseCreditNotesService

        Dim oDoc As APCreditMemo.Document
        Dim docLine As APCreditMemo.DocumentDocumentLine
        Dim docLineBatch As APCreditMemo.DocumentDocumentLineBatchNumber
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLIneBatch As ArrayList
        Dim _LineQuantity As Decimal
        Dim DocParams As APCreditMemo.DocumentParams
        _alDocumentLine = New ArrayList
        _alDocumentLIneBatch = New ArrayList

        _alDocumentLIneBatch.Clear()
        _alDocumentLine.Clear()




        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _APCreditMemoConfig.KeyField, _o.ToString), String.Format("{0} asc", _APCreditMemoConfig.KeyField))
            If _drRow.Length > 0 Then
                _APCreditMemoSrv = New APCreditMemo.PurchaseCreditNotesService
                oDoc = New APCreditMemo.Document

                _msgHeader = New APCreditMemo.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = APCreditMemo.MsgHeaderServiceName.PurchaseCreditNotesService
                _msgHeader.ServiceNameSpecified = True
                _APCreditMemoSrv.MsgHeaderValue = _msgHeader

                oDoc.DocDate = Convert.ToDateTime(_drRow(0)(PurchaseCreditMemo.Fld_DocDate))
                If Settings.DBNull(_drRow(0)(PurchaseCreditMemo.Fld_ReasonCode)) <> String.Empty Then
                    oDoc.Series = _SAPAPCreditMemo.DocSeries(WS_APCreditMemo.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(PurchaseCreditMemo.Fld_ReasonCode))
                    oDoc.SeriesSpecified = True
                End If
                If Settings.DBNull(_drRow(0)(PurchaseCreditMemo.Fld_CardCode)) <> String.Empty Then
                    oDoc.CardCode = Settings.DBNull(_drRow(0)(PurchaseCreditMemo.Fld_CardCode))
                End If
                oDoc.U_WMSEntry = _drRow(0)(PurchaseCreditMemo.Fld_ReceiveEntry)
                oDoc.U_WMSUser = _drRow(0)(PurchaseCreditMemo.Fld_WMSUser)
                oDoc.DocDateSpecified = True

                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(_APCreditMemoConfig.KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line
                            docLine.Quantity = _LineQuantity
                            docLine.QuantitySpecified = True
                            docLine.ItemCode = _PrevItemCode
                            docLine.BatchNumbers = APCreditMemotoDocumentLineBatchArray(_alDocumentLIneBatch)
                            _alDocumentLine.Add(docLine)
                            _alDocumentLIneBatch.Clear()
                            _LineQuantity = 0

                        End If
                        docLine = New APCreditMemo.DocumentDocumentLine

                    End If
                    docLineBatch = Nothing
                    docLineBatch = New APCreditMemo.DocumentDocumentLineBatchNumber
                    If IsDBNull(dr(PurchaseCreditMemoForWMS.Fld_BatchNum)) = False Then
                        docLineBatch.BatchNumber = dr(PurchaseCreditMemoForWMS.Fld_BatchNum)
                        docLineBatch.QuantitySpecified = True
                        docLineBatch.Quantity = dr(PurchaseCreditMemoForWMS.Fld_Quantity) * _SAPAPCreditMemo.GetPurchaseItemPerBaseUnit(dr(PurchaseCreditMemo.Fld_ItemCode))
                        _alDocumentLIneBatch.Add(docLineBatch)
                    End If
                    
                    docLine.U_WMSEntry = dr(PurchaseCreditMemoForWMS.Fld_ReceiveEntry)

                    docLine.U_WMSLineNum = dr(PurchaseCreditMemoForWMS.Fld_ReceiveLineNum)
                    docLine.U_WMSLineNumSpecified = True


                    If dr(PurchaseCreditMemoForWMS.Fld_DocEntry) > 0 Then
                        docLine.BaseTypeSpecified = True
                        docLine.BaseType = "18"
                        docLine.BaseEntry = dr(PurchaseCreditMemoForWMS.Fld_DocEntry)
                        docLine.BaseEntrySpecified = True
                        docLine.BaseLine = dr(PurchaseCreditMemoForWMS.Fld_LineNum)
                        docLine.BaseLineSpecified = True
                    Else
                        docLine.WarehouseCode = dr(PurchaseCreditMemoForWMS.Fld_WhsCode)
                        docLine.PriceSpecified = True

                        docLine.Price = _SAPAPCreditMemo.ItemCost(dr(PurchaseCreditMemoForWMS.Fld_ItemCode))

                    End If
                    _LineQuantity = _LineQuantity + dr(PurchaseCreditMemoForWMS.Fld_Quantity)

                    _PrevLineNum = dr(_APCreditMemoConfig.KeyLineField)
                    _PrevItemCode = dr(PurchaseCreditMemoForWMS.Fld_ItemCode)
                Next

                docLine.Quantity = _LineQuantity
                docLine.QuantitySpecified = True
                docLine.ItemCode = _PrevItemCode

                If _alDocumentLIneBatch.Count > 0 Then
                    docLine.BatchNumbers = APCreditMemotoDocumentLineBatchArray(_alDocumentLIneBatch)
                End If

                _alDocumentLine.Add(docLine)

                oDoc.DocumentLines = APCreditMemotoDocumentLineArray(_alDocumentLine)

                _Debug.Write("create to actrual document")
                DocParams = _APCreditMemoSrv.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Debug.Write("Update Success Status")

                    _APCreditMemo.UpdateSuccessStatus(_APCreditMemoConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
                    _ret = True


                End If

            End If
        Catch ex As Exception
            _APCreditMemo.UpdateErrorStatus(_APCreditMemoConfig.KeyField, _o, "-1", ex.Message)
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
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _APCreditMemoConfig.KeyField, _o.ToString), String.Format("{0} asc", _APCreditMemoConfig.KeyField))
            If _drRow.Length > 0 Then
                _StockIn = New DocDraft.DraftsService
                oDoc = New DocDraft.Document
                oDoc.DocObjectCode = WS_APCreditMemo.ObjType
                _msgHeader = New DocDraft.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
                _msgHeader.ServiceNameSpecified = True
                _StockIn.MsgHeaderValue = _msgHeader

                oDoc.DocDate = Convert.ToDateTime(_drRow(0)(Inventory_Inout.Fld_DocDate))
                'If Settings.DBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_ReasonCode)) <> String.Empty Then
                '    oDoc.Series = _SAPAPCreditMemo.DocSeries(WS_APCreditMemo.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(PurchaseCreditMemoForWMS.Fld_ReasonCode))
                '    oDoc.SeriesSpecified = True
                'End If
                
                oDoc.U_WMSEntry = _drRow(0)(PurchaseCreditMemoForWMS.Fld_ReceiveEntry)
                oDoc.U_WMSUser = _drRow(0)(PurchaseCreditMemoForWMS.Fld_WMSUser)
                oDoc.DocDateSpecified = True
                If Settings.DBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_CardCode)) <> String.Empty Then
                    oDoc.CardCode = _drRow(0)(PurchaseCreditMemo.Fld_CardCode)
                End If
                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(_APCreditMemoConfig.KeyLineField)) <> _PrevLineNum Then
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
                    If IsDBNull(dr(PurchaseCreditMemoForWMS.Fld_BatchNum)) = False Then
                        docLineBatch.BatchNumber = dr(PurchaseCreditMemoForWMS.Fld_BatchNum)
                        docLineBatch.QuantitySpecified = True
                        docLineBatch.Quantity = dr(PurchaseCreditMemoForWMS.Fld_Quantity)
                        _alDocumentLIneBatch.Add(docLineBatch)
                    End If

                    If Settings.DBNull(dr(PurchaseCreditMemoForWMS.Fld_DocEntry), "-1") > 0 Then
                        docLine.BaseTypeSpecified = True
                        docLine.BaseType = "18"
                        docLine.BaseEntry = dr(PurchaseCreditMemoForWMS.Fld_DocEntry)
                        docLine.BaseEntrySpecified = True
                        docLine.BaseLine = dr(PurchaseCreditMemoForWMS.Fld_LineNum)
                        docLine.BaseLineSpecified = True
                    Else
                        docLine.WarehouseCode = dr(PurchaseCreditMemoForWMS.Fld_WhsCode)
                        docLine.PriceSpecified = True


                        docLine.Price = _SAPAPCreditMemo.ItemCost(dr(PurchaseCreditMemoForWMS.Fld_ItemCode))
                    End If
                    docLine.U_WMSEntry = dr(PurchaseCreditMemoForWMS.Fld_ReceiveEntry)

                    docLine.U_WMSLineNum = dr(PurchaseCreditMemoForWMS.Fld_ReceiveLineNum)
                    docLine.U_WMSLineNumSpecified = True
                    _LineQuantity = _LineQuantity + dr(PurchaseCreditMemoForWMS.Fld_Quantity)

                    _PrevLineNum = dr(_APCreditMemoConfig.KeyLineField)
                    _PrevItemCode = dr(PurchaseCreditMemoForWMS.Fld_ItemCode)
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

                    _APCreditMemo.UpdateSuccessStatus(_APCreditMemoConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
                    _ret = True


                End If

            End If
        Catch ex As Exception
            _APCreditMemo.UpdateErrorStatus(_APCreditMemoConfig.KeyField, _o, "-1", ex.Message)
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
                        _htKeyValue(dr(_APCreditMemoConfig.KeyField)) = "Y"
                    Else
                        _htKeyValue(dr(_APCreditMemoConfig.KeyField)) = "N"
                    End If

                Next

            Else
                _Message = "No data found."
                Return False
            End If
            If _htKeyValue.Count > 0 Then
                For Each o As Object In _htKeyValue.Keys
                    If _htKeyValue(o) = "Y" Then
                        _ret = StartDraft(o)
                    Else
                        _ret = StartDraft(o)
                    End If

                Next

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret



    End Function
End Class


