Public Class DIServer_APCreditMemo : Inherits CPSLIB.DIServer.Document

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _Config As PurchaseCreditMemoConfig
    Dim _WMSPurchaseCR As PurchaseCreditMemoForWMS
    Dim _htDocStatus As Hashtable
    Dim _isDraft As Boolean
    Private _isError As Boolean
    Dim _Setting As Settings
    Dim _dt As DataTable
    Private _Message As String
    Dim _SAPAPCreditMemo As PurchaseCreditMemoForSAP
    Dim _APCreditMemoConfig As PurchaseCreditMemoConfig
    Dim _APCreditMemo As PurchaseCreditMemoForWMS

    Dim _diServerConn As CPSLIB.DIServer.DIServerConnection

    Dim _htKeyValue As Hashtable
    Dim _BatchRow As CPSLIB.DIServer.BatchNumbers
    Public Shared ObjType As String = "19"

    Public Shared Fld_HDR_ObjectCode As String = "DocObjectCode"
    Public Shared Fld_HDR_DocDate As String = "DocDate"
    Public Shared Fld_HDR_DocDueDate As String = "DocDueDate"
    Public Shared Fld_HDR_CardCode As String = "CardCode"
    Public Shared Fld_HDR_WMSEntry As String = "U_WMSEntry"
    Public Shared Fld_HDR_WMSUser As String = "U_WMSUser"
    Public Shared FLD_DTL_Quantity As String = "Quantity"
    Public Shared FLD_DTL_ItemCode As String = "ItemCode"
    Public Shared FLD_DTL_BaseType As String = "BaseType"
    Public Shared FLD_DTL_BaseEntry As String = "BaseEntry"
    Public Shared FLD_DTL_BaseLine As String = "BaseLine"
    Public Shared FLD_DTL_WhsCode As String = "WarehouseCode"
    Public Shared FLD_DTL_UNITPrice As String = "Price"
    Public Shared FLD_DTL_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_DTL_WMSLine As String = "U_WMSLine"
    Public Shared FLD_DTL_WMSDraftEntry As String = "U_WMSDraftEntry"
    Public Shared Fld_DTL_WMSDraftLine As String = "U_WMSDraftLine"


    Public Sub New(ByVal _Setting As Settings, ByVal _DiConn As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_DiConn, SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._diServerConn = _DiConn
        Me._Setting = _Setting
        _isDraft = False

        _SAPAPCreditMemo = New PurchaseCreditMemoForSAP(_Setting, Nothing)
        _APCreditMemoConfig = New PurchaseCreditMemoConfig(_Setting)
        _APCreditMemo = New PurchaseCreditMemoForWMS(_Setting, Nothing, PurchaseCreditMemoForWMS._DocumentType.PR)
    End Sub

    Public Sub New(ByVal _Setting As Settings, ByVal _DiConn As CPSLIB.DIServer.DIServerConnection, ByVal _isDraft As Boolean)
        MyBase.New(_DiConn, SAPbobsCOM.BoObjectTypes.oDrafts)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._diServerConn = _DiConn
        Me._Setting = _Setting
        Me._isDraft = _isDraft
        _SAPAPCreditMemo = New PurchaseCreditMemoForSAP(_Setting, Nothing)
        _APCreditMemoConfig = New PurchaseCreditMemoConfig(_Setting)
        _APCreditMemo = New PurchaseCreditMemoForWMS(_Setting, Nothing, PurchaseCreditMemoForWMS._DocumentType.PR)
    End Sub

    Public Function Start(ByVal _o As String, ByVal KeyLineField As String) As Boolean
        _Debug.Write(String.Format("Creating Document: Key Value : {0}", _o))
        _Debug.Write("isDraft Document: " & _isDraft)
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty
        Dim _LineQuantity As Decimal
        Dim _DocSeries As String = String.Empty
        Dim DocParams As DocDraft.DocumentParams

        Try
            _drRow = _dt.Select(String.Format("{0} = '{1}'", _APCreditMemoConfig.KeyField, _o.ToString), String.Format("{0} asc", _APCreditMemoConfig.KeyField))
            If _drRow.Length > 0 Then
                NewDocument()

                If IsDBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)) = False Then
                    If Convert.ToInt32(_drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)) > 0 Then
                        setUDF("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry))
                    End If
                End If

                If _isDraft Then
                    setValue(Fld_HDR_ObjectCode, DIServer_ARCreditMemo.ObjType)
                End If

                SetValue(Fld_HDR_DocDate, GetDocDate("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)).ToString("yyyyMMdd"))
                SetValue(Fld_HDR_DocDueDate, GetDueDate("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)).ToString("yyyyMMdd"))
                
                If Settings.DBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_CardCode)) <> String.Empty Then
                    setValue(Fld_HDR_CardCode, Settings.DBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_CardCode)))
                End If

                SetHeaderStandardField("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), "DocCur")
                SetHeaderStandardField("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), "Comments")
                SetHeaderStandardField("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), "OwnerCode")
                SetHeaderStandardField("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), "SlpCode")
                SetHeaderStandardField("ODRF", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), "NumAtCard")

                setValue(Fld_HDR_WMSEntry, _drRow(0)(PurchaseCreditMemoForWMS.Fld_ReceiveEntry))
                SetValue(Fld_HDR_WMSUser, _drRow(0)(PurchaseCreditMemoForWMS.Fld_WMSUser))

                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line
                            setRowsValue(Fld_DTL_Quantity, _LineQuantity)
                            setRowsValue(Fld_DTL_ItemCode, _PrevItemCode)
                            AddRow()
                            _LineQuantity = 0
                        End If
                        If IsDBNull(_drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)) = False Then
                            If Convert.ToInt32(_drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry)) > 0 Then
                                setUDF("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum))
                            End If
                        End If

                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "CogsOcrCod")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "CogsOcrCo2")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "CogsOcrCo3")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "CogsOcrCo4")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "CogsOcrCo5")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "OcrCode")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "OcrCode2")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "OcrCode3")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "OcrCode4")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "OcrCode5")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "PriceBefDi")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "DiscPrcnt")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "Currency")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "FreeTxt")
                        SetLineStandardField("DRF1", dr(PurchaseCreditMemoForWMS.Fld_DocEntry), dr(PurchaseCreditMemoForWMS.Fld_LineNum), "Price")
                    End If

                    If IsDBNull(dr(PurchaseCreditMemoForWMS.Fld_BatchNum)) = False Then
                        _BatchRow = New CPSLIB.DIServer.BatchNumbers
                        _BatchRow.BatchNumber = dr(PurchaseCreditMemoForWMS.Fld_BatchNum)
                        _BatchRow.Quantity = dr(PurchaseCreditMemoForWMS.Fld_Quantity)
                        setBatchNumberRow(_BatchRow)
                    End If

                    If Settings.DBNull(dr(PurchaseCreditMemoForWMS.Fld_BaseEntry), "-1") > 0 Then
                        setRowsValue(FLD_DTL_BaseType, "18")
                        setRowsValue(FLD_DTL_BaseEntry, dr(PurchaseCreditMemoForWMS.Fld_BaseEntry))
                        setRowsValue(FLD_DTL_BaseLine, dr(PurchaseCreditMemoForWMS.Fld_BaseLine))
                    Else
                        setRowsValue(FLD_DTL_WhsCode, dr(PurchaseCreditMemoForWMS.Fld_WhsCode))
                        'setRowsValue(FLD_DTL_UNITPrice, _SAPAPCreditMemo.ItemCost(dr(PurchaseCreditMemoForWMS.Fld_ItemCode),
                        '                                                          dr(PurchaseCreditMemoForWMS.Fld_CardCode),
                        '                                                          dr(Inventory_Inout.Fld_DocEntry),
                        '                                                          dr(Inventory_Inout.Fld_LineNum)))
                    End If

                    setRowsValue(FLD_DTL_WMSDraftEntry, dr(PurchaseCreditMemoForWMS.Fld_DocEntry))
                    setRowsValue(Fld_DTL_WMSDraftLine, dr(PurchaseCreditMemoForWMS.Fld_LineNum))
                    setRowsValue(Fld_DTL_WMSEntry, dr(PurchaseCreditMemoForWMS.Fld_ReceiveEntry))
                    setRowsValue(Fld_DTL_WMSLine, dr(PurchaseCreditMemoForWMS.Fld_ReceiveLineNum))
                    _LineQuantity = _LineQuantity + dr(PurchaseCreditMemoForWMS.Fld_Quantity)
                    _PrevLineNum = dr(KeyLineField)
                    _PrevItemCode = dr(PurchaseCreditMemoForWMS.Fld_ItemCode)
                Next

                setRowsValue(Fld_DTL_Quantity, _LineQuantity)
                setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "CogsOcrCod")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "CogsOcrCo2")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "CogsOcrCo3")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "CogsOcrCo4")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "CogsOcrCo5")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "OcrCode")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "OcrCode2")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "OcrCode3")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "OcrCode4")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "OcrCode5")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "PriceBefDi")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "DiscPrcnt")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "Currency")
                SetLineStandardField("DRF1", _drRow(0)(PurchaseCreditMemoForWMS.Fld_DocEntry), _PrevLineNum, "FreeTxt")
                AddRow()

                If MyBase.Post(CPSLIB.DIServer.DI_Object.Command.AddObject) = CPSLIB.DIServer.DI_Object.CommandStatus.Fail Then
                    _ret = False
                    _Message = MyBase.CmdMessage
                    _APCreditMemo.UpdateErrorStatus(_APCreditMemoConfig.KeyField, _o, "-1", _Message.Replace("'", "''"))
                    _CPSException.ExecuteHandle(New Exception(_Message))
                Else
                    _Debug.Write("Update Success Status")

                    _APCreditMemo.UpdateSuccessStatus(_APCreditMemoConfig.KeyField, _o, NewEntry, String.Empty)
                    _ret = True
                End If

            End If
        Catch ex As Exception
            _ret = False
            _Message = ex.Message
            _APCreditMemo.UpdateErrorStatus(_APCreditMemoConfig.KeyField, _o, "-1", ex.Message.Replace("'", "''"))
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function Start(ByVal _dt As DataTable) As Boolean
        Me._dt = _dt
        _Debug.WriteTable(_dt, "dtTable")
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
                        _ret = Start(o, _APCreditMemoConfig.KeyLineField)
                    Else
                        _ret = Start(o, PurchaseCreditMemo.Fld_ItemCode)
                    End If
                Next
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret
    End Function
End Class

