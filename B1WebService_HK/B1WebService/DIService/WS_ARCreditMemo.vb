Public Class WS_ARCreditMemo : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable

    Dim _ARCreditMemoSrv As ARCreditMemo.CreditNotesService
    Dim _ARCreditMemoConfig As SalesCreditMemoConfig

    Dim _ARCreditMemo As SalesCreditMemoForWMS
    Dim _SAPARCreditMemo As SalesCreditMemoForSAP
    Dim _htKeyValue As Hashtable
    Dim _htDocStatus As Hashtable
    Public Shared ObjType As String = "14"
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
        _SAPARCreditMemo = New SalesCreditMemoForSAP(_Setting, Nothing)
        _ARCreditMemoConfig = New SalesCreditMemoConfig(_Setting)
        _ARCreditMemo = New SalesCreditMemoForWMS(_Setting, Nothing, SalesCreditMemoForWMS._DocumentType.PR)
        _htDocStatus = New Hashtable
    End Sub

    'Public Function Start(ByVal _o As String) As Boolean
    '    Dim _ret As Boolean = True
    '    Dim _drRow As DataRow()
    '    Dim _PrevLineNum As String = String.Empty
    '    Dim _PrevItemCode As String = String.Empty

    '    Dim _msgHeader As ARCreditMemo.MsgHeader
    '    Dim _ARCreditMemoSrv As ARCreditMemo.CreditNotesService

    '    Dim oDoc As ARCreditMemo.Document
    '    Dim docLine As ARCreditMemo.DocumentDocumentLine
    '    Dim docLineBatch As ARCreditMemo.DocumentDocumentLineBatchNumber
    '    Dim _alDocumentLine As ArrayList
    '    Dim _alDocumentLIneBatch As ArrayList
    '    Dim _LineQuantity As Decimal
    '    Dim DocParams As ARCreditMemo.DocumentParams
    '    _alDocumentLine = New ArrayList
    '    _alDocumentLIneBatch = New ArrayList

    '    _alDocumentLIneBatch.Clear()
    '    _alDocumentLine.Clear()




    '    Try
    '        _drRow = _dt.Select(String.Format("{0} = '{1}'", _ARCreditMemoConfig.KeyField, _o.ToString), String.Format("{0} asc", _ARCreditMemoConfig.KeyField))
    '        If _drRow.Length > 0 Then
    '            _ARCreditMemoSrv = New ARCreditMemo.CreditNotesService
    '            oDoc = New ARCreditMemo.Document

    '            _msgHeader = New ARCreditMemo.MsgHeader
    '            _msgHeader.SessionID = MyBase.SessionID
    '            _msgHeader.ServiceName = ARCreditMemo.MsgHeaderServiceName.CreditNotesService
    '            _msgHeader.ServiceNameSpecified = True
    '            _ARCreditMemoSrv.MsgHeaderValue = _msgHeader

    '            oDoc.DocDate = Convert.ToDateTime(_drRow(0)(SalesCreditMemoForWMS.Fld_DocDate))
    '            oDoc.Series = _SAPARCreditMemo.DocSeries(WS_ARCreditMemo.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), _drRow(0)(SalesCreditMemo.Fld_ReasonCode))
    '            oDoc.SeriesSpecified = True
    '            oDoc.U_WMSEntry = _drRow(0)(SalesCreditMemo.Fld_ReceiveEntry)
    '            oDoc.U_WMSUser = _drRow(0)(SalesCreditMemo.Fld_WMSUser)
    '            oDoc.DocDateSpecified = True
    '            ' UDF for Receive Entry and WMS User
    '            _LineQuantity = 0
    '            For Each dr In _drRow

    '                If Settings.DBNull(dr(_ARCreditMemoConfig.KeyLineField)) <> _PrevLineNum Then
    '                    If _PrevLineNum <> String.Empty Then
    '                        ' Add new Line
    '                        docLine.Quantity = _LineQuantity
    '                        docLine.QuantitySpecified = True
    '                        docLine.ItemCode = _PrevItemCode
    '                        docLine.BatchNumbers = ARCreditMemotoDocumentLineBatchArray(_alDocumentLIneBatch)
    '                        _alDocumentLine.Add(docLine)
    '                        _alDocumentLIneBatch.Clear()
    '                        _LineQuantity = 0

    '                    End If
    '                    docLine = New ARCreditMemo.DocumentDocumentLine

    '                End If
    '                docLineBatch = Nothing
    '                docLineBatch = New ARCreditMemo.DocumentDocumentLineBatchNumber
    '                If IsDBNull(dr(SalesCreditMemoForWMS.Fld_BatchNum)) = False Then
    '                    docLineBatch.BatchNumber = dr(SalesCreditMemoForWMS.Fld_BatchNum)
    '                    docLineBatch.QuantitySpecified = True
    '                    docLineBatch.Quantity = dr(SalesCreditMemoForWMS.Fld_Quantity)
    '                    _alDocumentLIneBatch.Add(docLineBatch)
    '                End If
    '                docLine.WarehouseCode = dr(SalesCreditMemoForWMS.Fld_WhsCode)
    '                docLine.PriceSpecified = True

    '                docLine.Price = _SAPARCreditMemo.ItemCost(dr(SalesCreditMemoForWMS.Fld_ItemCode))

    '                If dr(SalesCreditMemoForWMS.Fld_DocEntry) > 0 Then
    '                    docLine.BaseTypeSpecified = True
    '                    docLine.BaseType = "13"
    '                    docLine.BaseEntry = dr(SalesCreditMemoForWMS.Fld_DocEntry)
    '                    docLine.BaseEntrySpecified = True
    '                    docLine.BaseLine = dr(SalesCreditMemoForWMS.Fld_LineNum)
    '                    docLine.BaseLineSpecified = True
    '                End If

    '                docLine.U_WMSEntry = dr(SalesCreditMemoForWMS.Fld_ReceiveEntry)

    '                docLine.U_WMSLineNum = dr(SalesCreditMemoForWMS.Fld_ReceiveLineNum)
    '                docLine.U_WMSLineNumSpecified = True
    '                _LineQuantity = _LineQuantity + dr(SalesCreditMemoForWMS.Fld_Quantity)

    '                _PrevLineNum = dr(_ARCreditMemoConfig.KeyLineField)
    '                _PrevItemCode = dr(SalesCreditMemoForWMS.Fld_ItemCode)
    '            Next

    '            docLine.Quantity = _LineQuantity
    '            docLine.QuantitySpecified = True
    '            docLine.ItemCode = _PrevItemCode

    '            If _alDocumentLIneBatch.Count > 0 Then
    '                docLine.BatchNumbers = ARCreditMemotoDocumentLineBatchArray(_alDocumentLIneBatch)
    '            End If

    '            _alDocumentLine.Add(docLine)

    '            oDoc.DocumentLines = ARCreditMemotoDocumentLineArray(_alDocumentLine)

    '            _Debug.Write("create to actrual document")
    '            DocParams = _ARCreditMemoSrv.Add(oDoc)
    '            If DocParams.DocEntry > 0 Then
    '                ' Update Success Status
    '                _Debug.Write("Update Success Status")

    '                _ARCreditMemo.UpdateSuccessStatus(_o, DocParams.DocEntry, String.Empty)
    '                _ret = True


    '            End If

    '        End If
    '    Catch ex As Exception
    '        _ARCreditMemo.UpdateErrorStatus(_o, "-1", ex.Message)
    '        _CPSException.ExecuteHandle(ex)
    '    End Try
    '    Return _ret
    'End Function



    'Public Function StartDraft(ByVal _o As String, ByVal KeyLineField As String) As Boolean
    '    _Debug.Write(String.Format("Creating Draft Document: Key Value : {0}", _o))
    '    Dim _ret As Boolean = True
    '    Dim _drRow As DataRow()
    '    Dim _PrevLineNum As String = String.Empty
    '    Dim _PrevItemCode As String = String.Empty

    '    Dim _msgHeader As DocDraft.MsgHeader
    '    Dim _oDraft As DocDraft.DraftsService

    '    Dim oDoc As DocDraft.Document
    '    Dim docLine As DocDraft.DocumentDocumentLine
    '    Dim docLineBatch As DocDraft.DocumentDocumentLineBatchNumber
    '    Dim _alDocumentLine As ArrayList
    '    Dim _alDocumentLIneBatch As ArrayList
    '    Dim _LineQuantity As Decimal
    '    Dim _DocSeries As String = String.Empty
    '    Dim DocParams As DocDraft.DocumentParams
    '    _alDocumentLine = New ArrayList
    '    _alDocumentLIneBatch = New ArrayList

    '    _alDocumentLIneBatch.Clear()
    '    _alDocumentLine.Clear()




    '    Try
    '        _drRow = _dt.Select(String.Format("{0} = '{1}'", _ARCreditMemoConfig.KeyField, _o.ToString), String.Format("{0} asc", _ARCreditMemoConfig.KeyField))
    '        If _drRow.Length > 0 Then
    '            _oDraft = New DocDraft.DraftsService
    '            oDoc = New DocDraft.Document
    '            oDoc.DocObjectCode = WS_ARCreditMemo.ObjType
    '            _msgHeader = New DocDraft.MsgHeader
    '            _msgHeader.SessionID = MyBase.SessionID
    '            _msgHeader.ServiceName = DocDraft.MsgHeaderServiceName.DraftsService
    '            _msgHeader.ServiceNameSpecified = True
    '            _oDraft.MsgHeaderValue = _msgHeader
    '            oDoc.DocDate = Convert.ToDateTime(_drRow(0)(SalesCreditMemoForWMS.Fld_DocDate))
    '            'If Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_ReasonCode)) <> String.Empty Then

    '            '    _DocSeries = Settings.DBNull(_SAPARCreditMemo.DocSeries(WS_ARCreditMemo.ObjType, oDoc.DocDate.ToString("yyyyMMdd"), Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_ReasonCode))))
    '            '    If _DocSeries <> String.Empty Then
    '            '        oDoc.Series = _DocSeries
    '            '        oDoc.SeriesSpecified = True
    '            '    End If
    '            'End If
    '            If Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_CardCode)) <> String.Empty Then
    '                oDoc.CardCode = Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_CardCode))
    '            End If
    '            oDoc.U_WMSEntry = _drRow(0)(SalesCreditMemoForWMS.Fld_ReceiveEntry)
    '            oDoc.U_WMSUser = _drRow(0)(SalesCreditMemoForWMS.Fld_WMSUser)
    '            oDoc.DocDateSpecified = True
    '            ' UDF for Receive Entry and WMS User
    '            _LineQuantity = 0
    '            For Each dr In _drRow

    '                If Settings.DBNull(dr(KeyLineField)) <> _PrevLineNum Then
    '                    If _PrevLineNum <> String.Empty Then
    '                        ' Add new Line
    '                        docLine.Quantity = _LineQuantity
    '                        docLine.QuantitySpecified = True


    '                        If docLine.BaseEntry > 0 = False Then
    '                            docLine.ItemCode = _PrevItemCode
    '                        End If
    '                        docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
    '                        _alDocumentLine.Add(docLine)
    '                        _alDocumentLIneBatch.Clear()
    '                        _LineQuantity = 0

    '                    End If
    '                    docLine = New DocDraft.DocumentDocumentLine

    '                End If
    '                docLineBatch = Nothing
    '                docLineBatch = New DocDraft.DocumentDocumentLineBatchNumber
    '                If IsDBNull(dr(SalesCreditMemoForWMS.Fld_BatchNum)) = False Then
    '                    docLineBatch.BatchNumber = dr(SalesCreditMemoForWMS.Fld_BatchNum)
    '                    docLineBatch.QuantitySpecified = True
    '                    docLineBatch.Quantity = dr(SalesCreditMemoForWMS.Fld_Quantity)
    '                    _alDocumentLIneBatch.Add(docLineBatch)
    '                End If


    '                If Settings.DBNull(dr(SalesCreditMemoForWMS.Fld_DocEntry), "-1") > 0 Then
    '                    docLine.BaseTypeSpecified = True
    '                    docLine.BaseType = "13"
    '                    docLine.BaseEntry = dr(SalesCreditMemoForWMS.Fld_DocEntry)
    '                    docLine.BaseEntrySpecified = True
    '                    docLine.BaseLine = dr(SalesCreditMemoForWMS.Fld_LineNum)
    '                    docLine.BaseLineSpecified = True
    '                Else
    '                    docLine.WarehouseCode = dr(SalesCreditMemoForWMS.Fld_WhsCode)
    '                    docLine.PriceSpecified = True
    '                    docLine.Price = _SAPARCreditMemo.ItemCost(dr(Inventory_Inout.Fld_ItemCode))


    '                End If



    '                docLine.U_WMSEntry = dr(SalesCreditMemoForWMS.Fld_ReceiveEntry)
    '                docLine.U_WMSLineNum = dr(SalesCreditMemoForWMS.Fld_ReceiveLineNum)
    '                docLine.U_WMSLineNumSpecified = True
    '                _LineQuantity = _LineQuantity + dr(SalesCreditMemoForWMS.Fld_Quantity)

    '                _PrevLineNum = dr(KeyLineField)
    '                _PrevItemCode = dr(SalesCreditMemoForWMS.Fld_ItemCode)
    '            Next

    '            docLine.Quantity = _LineQuantity

    '            docLine.QuantitySpecified = True
    '            If docLine.BaseEntry > 0 = False Then
    '                docLine.ItemCode = _PrevItemCode

    '            End If

    '            If _alDocumentLIneBatch.Count > 0 Then
    '                docLine.BatchNumbers = DraftDocumentLineBatchArray(_alDocumentLIneBatch)
    '            End If

    '            _alDocumentLine.Add(docLine)

    '            oDoc.DocumentLines = DraftDocumentLineArray(_alDocumentLine)
    '            '' For Debug


    '            DocParams = _oDraft.Add(oDoc)
    '            If DocParams.DocEntry > 0 Then
    '                ' Update Success Status
    '                _Debug.Write("Update Success Status")

    '                _ARCreditMemo.UpdateSuccessStatus(_ARCreditMemoConfig.KeyField, _o, DocParams.DocEntry, String.Empty)
    '                _ret = True


    '            End If

    '        End If
    '    Catch ex As Exception
    '        _ret = False
    '        _Message = ex.Message
    '        _ARCreditMemo.UpdateErrorStatus(_ARCreditMemoConfig.KeyField, _o, "-1", ex.Message)
    '        _CPSException.ExecuteHandle(ex)
    '    End Try
    '    Return _ret
    'End Function

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
                        _htKeyValue(dr(_ARCreditMemoConfig.KeyField)) = "Y"
                    Else
                        _htKeyValue(dr(_ARCreditMemoConfig.KeyField)) = "N"
                    End If

                Next

            Else
                _Message = "No data found."
                Return False
            End If
            If _htKeyValue.Count > 0 Then
                For Each o As Object In _htKeyValue.Keys
                    If _htKeyValue(o) = "Y" Then

                        _ret = StartDraft(o, _ARCreditMemoConfig.KeyLineField)



                    Else
                    _ret = StartDraft(o, SalesCreditMemo.Fld_ItemCode)
                    End If

                Next

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret



    End Function
End Class


