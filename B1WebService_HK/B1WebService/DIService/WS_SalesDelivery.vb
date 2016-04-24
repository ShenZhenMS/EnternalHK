Public Class WS_SalesDelivery : Inherits DIServer

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _SalesDelivery As SalesDelivery.DeliveryNotesService
    Dim _SalesConfig As SalesConfig
    Dim _Sales As SalesForWMS

    Dim _DN As SalesDelivery.DeliveryNotesService

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

        If MyBase.isConnected = False Then
            isError = True
            Message = MyBase.Message
        End If
        _SalesConfig = New SalesConfig(_Setting)
        _Sales = New SalesForWMS(_Setting, Nothing)
        _htDocStatus = New Hashtable
    End Sub

    Public Function Create_DN() As String

        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _msgHeader As SalesDelivery.MsgHeader
        Dim oDoc As SalesDelivery.Document
        Dim _ret As String
        Dim _TargetEntry As String
        Dim docLine As SalesDelivery.DocumentDocumentLine
        Dim docLineBatch As SalesDelivery.DocumentDocumentLineBatchNumber
        Dim DocParams As SalesDelivery.DocumentParams
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList
        _alDocumentLine = New ArrayList
        _alDocumentLineBatch = New ArrayList
        Dim SAPConnection As SAPSQLConnections

        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)


        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[RDR1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        mSql = String.Format("Select distinct {0}, {1}, {2} " & _
                                                               "From [dbo].[CPS_TBL_ORDR] " & _
                                                               "Where ISNULL(TRXSTATUS,'') ='' ", _
                                                               Sales.Fld_DocEntry, _
                                                               Sales.Fld_DocDueDate, _
                                                               Sales.Fld_CardCode)

        _dt_DocEntry = _Sales.ExecuteDatatable(mSql)

        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            Try
                oDoc = Nothing
                _DN = Nothing
                _msgHeader = Nothing
                _TargetEntry = ""
                _alDocumentLineBatch.Clear()
                _alDocumentLine.Clear()
                _DN = New SalesDelivery.DeliveryNotesService
                _msgHeader = New SalesDelivery.MsgHeader
                _msgHeader.SessionID = MyBase.SessionID
                _msgHeader.ServiceName = B1WebService.SalesDelivery.MsgHeaderServiceName.DeliveryNotesService
                _msgHeader.ServiceNameSpecified = True
                _DN.MsgHeaderValue = _msgHeader
                oDoc = New SalesDelivery.Document

                oDoc.DocDate = Today.Date
                oDoc.DocDueDate = _dt_DocEntry.Rows(i).Item(Sales.Fld_DocDueDate)
                oDoc.CardCode = _dt_DocEntry.Rows(i).Item(Sales.Fld_CardCode)

                _dt_DocLine = _Sales.ExecuteDatatable(String.Format("Select {0}, {1}, {2}, sum({3}) as 'Quantity' " & _
                                                                   "From [dbo].[CPS_TBL_ORDR] " & _
                                                                   "Where {4} = {5} and isNull({9},'') = ''  Group by {6},{7},{8}", _
                                                                   Sales.Fld_DocEntry, _
                                                                   Sales.Fld_LineNum, _
                                                                   Sales.Fld_ItemCode, _
                                                                   Sales.Fld_Quantity, _
                                                                   Sales.Fld_DocEntry, _
                                                                   _dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry),
                                                                   Sales.Fld_DocEntry, _
                                                                   Sales.Fld_LineNum,
                                                                   Sales.Fld_ItemCode, Sales.Fld_TrxStatus))
                For j As Integer = 0 To _dt_DocLine.Rows.Count - 1
                    _alDocumentLineBatch.Clear()
                    docLine = Nothing
                    docLine = New SalesDelivery.DocumentDocumentLine
                    docLine.BaseTypeSpecified = True
                    docLine.BaseType = "17"
                    docLine.BaseEntrySpecified = True
                    docLine.BaseEntry = _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry)
                    docLine.BaseLineSpecified = True
                    docLine.BaseLine = _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum)
                    docLine.ItemCode = _dt_DocLine.Rows(j).Item(Sales.Fld_ItemCode)
                    docLine.QuantitySpecified = True
                    docLine.Quantity = _dt_DocLine.Rows(j).Item(Sales.Fld_Quantity)

                    _dt_BatchNum = _Sales.ExecuteDatatable(String.Format("Select {0}, {1} " & _
                                                                   "From [dbo].[CPS_TBL_ORDR] " & _
                                                                   "Where {2} = {3} and {4} = {5} and isNull({6},'') = '' ", _
                                                                   Sales.Fld_BatchNum, _
                                                                   Sales.Fld_Quantity, _
                                                                   Sales.Fld_DocEntry, _
                                                                   _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry), _
                                                                   Sales.Fld_LineNum, _
                                                                   _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum), Sales.Fld_TrxStatus))



                    For k As Integer = 0 To _dt_BatchNum.Rows.Count - 1
                        If Not String.IsNullOrEmpty(_dt_BatchNum.Rows(k).Item(Sales.Fld_BatchNum)) Then

                            mSql = String.Format(sqlGetNumPerMsg, _
                                                 _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry), _
                                                 _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum))
                            mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)

                            docLineBatch = Nothing
                            docLineBatch = New SalesDelivery.DocumentDocumentLineBatchNumber
                            'docLineBatch.BaseLineNumberSpecified = True
                            'docLineBatch.BaseLineNumber = _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum)
                            docLineBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Sales.Fld_BatchNum)
                            docLineBatch.QuantitySpecified = True
                            docLineBatch.Quantity = _dt_BatchNum.Rows(k).Item(Sales.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))

                            _alDocumentLineBatch.Add(docLineBatch)
                        End If
                    Next
                    If _alDocumentLineBatch.Count > 0 Then
                        docLine.BatchNumbers = MyBase.SalesDeliverytoDocumentLineBatchArray(_alDocumentLineBatch)
                    End If

                    _alDocumentLine.Add(docLine)
                Next
                oDoc.DocumentLines = MyBase.SalesDeliverytoDocumentLineArray(_alDocumentLine)



                DocParams = _DN.Add(oDoc)
                If DocParams.DocEntry > 0 Then
                    ' Update Success Status
                    _Sales.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), DocParams.DocEntry, String.Empty)
                    _TargetEntry = DocParams.DocEntry.ToString
                End If


            Catch ex As Exception
                _Sales.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), "-1", ex.Message)
                _ret = ex.Message
            End Try

            _ret = _ret + _TargetEntry + "/"

        Next

        _TargetEntry = Left(_TargetEntry, _TargetEntry.Length - 1)
        MyBase.Logout(String.Empty)
        Return _TargetEntry

    End Function

    'Karrson: Create DN By DocEntry
    Public Function CreateDeliveryNote(ByVal _DocEntry As String) As Boolean


    End Function

End Class

