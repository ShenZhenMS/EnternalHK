Public Class DIServer_SalesDelivery : Inherits CPSLIB.DIServer.Document

    Dim _WSInvoice As DIServer_SalesInvoice
    Dim Fld_HDR_DocDate As String = "DocDate"
    Dim Fld_HDR_DocDueDate As String = "DocDueDate"
    Dim Fld_HDR_TaxDate As String = "TaxDate"
    Dim Fld_HDR_CardCode As String = "CardCode"
    Dim Fld_HDR_PickNum As String = "U_PickNum"
    Dim Fld_HDR_NumatWMS As String = "U_NumAtWMS"
    Dim Fld_HDR_AllowBackDate As String = "U_AllowBackDate"
    Dim Fld_LINE_Quantity As String = "Quantity"
    Dim Fld_LINE_BaseEntry As String = "BaseEntry"
    Dim Fld_LINE_BaseLine As String = "BaseLine"
    Dim Fld_LINE_ItemCode As String = "ItemCode"
    Dim Fld_LINE_BaseType As String = "BaseType"
    Dim Fld_HDR_COUNTER As String = "U_COUNTER"
    Dim Fld_HDR_KEEPER As String = "U_Keeper"

    Dim _CPSBatch As CPSLIB.DIServer.BatchNumbers
    Dim _diConnection As CPSLIB.DIServer.DIServerConnection
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    'Dim _SalesDelivery As SalesDelivery.DeliveryNotesService
    Dim _SalesConfig As SalesConfig
    Dim _Sales As SalesForWMS
    Dim _Setting As Settings
    'Dim _DN As SalesDelivery.DeliveryNotesService
    Dim _htDocStatus As Hashtable
    Private _isError As Boolean
    Private _Message As String
    Public Property Msg() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property


    Public Sub New(ByVal _Setting As Settings, ByVal _diConnection As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_diConnection, SAPbobsCOM.BoObjectTypes.oDeliveryNotes)
        Me._Setting = _Setting
        Me._diConnection = _diConnection
        'If MyBase.isConnected = False Then
        '    IsError = True
        '    Message = MyBase.Message
        'End If
        _SalesConfig = New SalesConfig(_Setting)
        _Sales = New SalesForWMS(_Setting, Nothing)
        _htDocStatus = New Hashtable
        _CPSException = New CPSLIB.CPSException
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
    End Sub

    Public Function Create() As Boolean
        Dim _dt_DocEntry As DataTable
        Dim _dt_DocLine As DataTable
        Dim _dt_BatchNum As DataTable
        Dim _ret As Boolean = True
        Dim _TargetEntry As String
        Dim mNumPerMsg As DataTable
        Dim mSql As String
        Dim _alDocumentLine As ArrayList
        Dim _alDocumentLineBatch As ArrayList
        _alDocumentLine = New ArrayList
        _alDocumentLineBatch = New ArrayList
        Dim SAPConnection As SAPSQLConnections
        Dim _allowbackdate As Boolean
        Dim whsCode As String = String.Empty
        SAPConnection = New SAPSQLConnections(New B1WebService.Settings)

        Dim sqlGetNumPerMsg As String = "select NumPerMsr " & _
                                        "From [dbo].[RDR1] " & _
                                        "Where DocEntry = {0} and LineNum = {1}"

        mSql = String.Format("Select distinct {0}, {1}, {2} ,{3},{4} " & _
                                                               "From [dbo].[CPS_TBL_ORDR] " & _
                                                               "Where ISNULL(TRXSTATUS,'') ='' ", _
                                                               Sales.Fld_DocEntry, _
                                                               Sales.Fld_DocDueDate, _
                                                               Sales.Fld_CardCode,
                                                               Sales.Fld_WhsCode,
                                                               Sales.Fld_LocCode)
        _dt_DocEntry = _Sales.ExecuteDatatable(mSql)

        For i As Integer = 0 To _dt_DocEntry.Rows.Count - 1
            Try
                NewDocument()
                _TargetEntry = ""
                _alDocumentLineBatch.Clear()
                _alDocumentLine.Clear()
                MyBase.setUDF("ORDR", _dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry))

                'add counter & keeper logic
                whsCode = IIf(_dt_DocEntry.Rows(i).Item(Sales.Fld_WhsCode) = Nothing, "", _dt_DocEntry.Rows(i).Item(Sales.Fld_WhsCode))
                If whsCode = "C-00-001" Then
                    SetValue(Fld_HDR_COUNTER, IIf(_dt_DocEntry.Rows(i).Item(Sales.Fld_LocCode) = Nothing, "",
                             _dt_DocEntry.Rows(i).Item(Sales.Fld_LocCode)))

                ElseIf whsCode = "K-OP-001" Then
                    SetValue(Fld_HDR_KEEPER, IIf(_dt_DocEntry.Rows(i).Item(Sales.Fld_LocCode) = Nothing, "",
                           _dt_DocEntry.Rows(i).Item(Sales.Fld_LocCode)))
                End If

                If SAPConnection.AllowBackDate(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry)) Then
                    _allowbackdate = True
                    SetValue(Fld_HDR_DocDate, SAPConnection.GetSODocDate(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry)).ToString("yyyyMMdd"))
                    SetValue(Fld_HDR_DocDueDate, SAPConnection.GetSODueDate(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry)).ToString("yyyyMMdd"))
                    SetValue(Fld_HDR_TaxDate, SAPConnection.GetSOTaxDate(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry)).ToString("yyyyMMdd"))
                    SetValue(Fld_HDR_AllowBackDate, "Y")
                Else
                    _allowbackdate = False
                    SetValue(Fld_HDR_DocDate, DateTime.Now.ToString("yyyyMMdd"))
                    SetValue(Fld_HDR_DocDueDate, DateTime.Now.ToString("yyyyMMdd"))
                End If
                SetValue(Fld_HDR_CardCode, _dt_DocEntry.Rows(i).Item(Sales.Fld_CardCode))
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
                    setUDF("RDR1", _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry), _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum))
                    setRowsValue(Fld_LINE_BaseType, "17")
                    setRowsValue(Fld_LINE_BaseEntry, _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry))
                    setRowsValue(Fld_LINE_BaseLine, _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum))
                    setRowsValue(Fld_LINE_ItemCode, _dt_DocLine.Rows(j).Item(Sales.Fld_ItemCode))
                    setRowsValue(Fld_LINE_Quantity, _dt_DocLine.Rows(j).Item(Sales.Fld_Quantity))

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
                            _CPSBatch = New CPSLIB.DIServer.BatchNumbers

                            mSql = String.Format(sqlGetNumPerMsg, _
                                                 _dt_DocLine.Rows(j).Item(Sales.Fld_DocEntry), _
                                                 _dt_DocLine.Rows(j).Item(Sales.Fld_LineNum))
                            mNumPerMsg = SAPConnection.ExecuteDatatable(mSql)


                            _CPSBatch.BatchNumber = _dt_BatchNum.Rows(k).Item(Sales.Fld_BatchNum)
                            _CPSBatch.Quantity = _dt_BatchNum.Rows(k).Item(Sales.Fld_Quantity) * CDbl(mNumPerMsg.Rows(0).Item(0))
                            setBatchNumberRow(_CPSBatch)

                        End If
                    Next
                    AddRow()
                Next

                If MyBase.Post(Command.AddObject) = CommandStatus.Fail Then
                    _Message = MyBase.CmdMessage
                    _Sales.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), "-1", MyBase.CmdMessage)
                    _ret = False
                Else
                    _WSInvoice = New DIServer_SalesInvoice(_Setting, _diConnection)
                    If _WSInvoice.Create(MyBase.NewEntry, _dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), _dt_DocEntry.Rows(i).Item(Sales.Fld_CardCode), _allowbackdate) Then
                        _ret = True
                        _Sales.UpdateSuccessStatus(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), NewEntry, String.Empty)
                        _TargetEntry = NewEntry
                        _Message = String.Empty
                    Else
                        _ret = True
                        _Message = _WSInvoice.CmdMessage
                    End If
                End If
            Catch ex As Exception
                _Sales.UpdateErrorStatus(_dt_DocEntry.Rows(i).Item(Sales.Fld_DocEntry), "-1", ex.Message)
                _ret = False
                _CPSException.ExecuteHandle(ex)
                _ret = False
                _Message = ex.Message
            End Try
        Next
        Return _ret
    End Function
End Class

