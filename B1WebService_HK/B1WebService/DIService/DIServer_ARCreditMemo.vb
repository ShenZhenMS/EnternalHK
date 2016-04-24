Public Class DIServer_ARCreditMemo : Inherits CPSLIB.DIServer.Document

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _Config As SalesCreditMemoConfig
    Dim _WMSSalesCR As SalesCreditMemoForWMS
    Dim _htDocStatus As Hashtable
    Dim _isDraft As Boolean
    Private _isError As Boolean
    Dim _Setting As Settings
    Dim _dt As DataTable
    Private _Message As String
    Dim _SAPARCreditMemo As SalesCreditMemoForSAP
    Dim _ARCreditMemoConfig As SalesCreditMemoConfig
    Dim _ARCreditMemo As SalesCreditMemoForWMS
    Public Shared ObjType As String = "14"
    Dim _diServerConn As CPSLIB.DIServer.DIServerConnection

    Dim _htKeyValue As Hashtable
    Dim _BatchRow As CPSLIB.DIServer.BatchNumbers

    Public Shared Fld_HDR_ObjectCode As String = "DocObjectCode"
    Public Shared Fld_HDR_DocDate As String = "DocDate"
    Public Shared Fld_HDR_DocDueDate As String = "DocDueDate"
    Public Shared Fld_HDR_CardCode As String = "CardCode"
    Public Shared Fld_HDR_WMSEntry As String = "U_WMSEntry"
    Public Shared Fld_HDR_WMSUser As String = "U_WMSUser"
    Public Shared Fld_HDR_ASNNum As String = "U_ASNNum"
    Public Shared Fld_HDR_SalesPerson As String = "SalesPersonCode"
    Public Shared FLD_DTL_Quantity As String = "Quantity"
    Public Shared FLD_DTL_ItemCode As String = "ItemCode"
    Public Shared FLD_DTL_BaseType As String = "BaseType"
    Public Shared FLD_DTL_BaseEntry As String = "BaseEntry"
    Public Shared FLD_DTL_BaseLine As String = "BaseLine"
    Public Shared FLD_DTL_WhsCode As String = "WarehouseCode"
    'Public Shared FLD_DTL_UNITPrice As String = "Price"
    Public Shared FLD_DTL_WMSEntry As String = "U_WMSEntry"
    Public Shared FLD_DTL_WMSLine As String = "U_WMSLine"
    Public Shared FLD_DTL_WMSDraftEntry As String = "U_WMSDraftEntry"
    Public Shared FlD_DTL_WMSDraftLine As String = "U_WMSDraftLine"

    Public Sub New(ByVal _Setting As Settings, ByVal _DiConn As CPSLIB.DIServer.DIServerConnection)
        MyBase.New(_DiConn, SAPbobsCOM.BoObjectTypes.oCreditNotes)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._diServerConn = _DiConn
        Me._Setting = _Setting
        _isDraft = False
        
        _SAPARCreditMemo = New SalesCreditMemoForSAP(_Setting, Nothing)
        _ARCreditMemoConfig = New SalesCreditMemoConfig(_Setting)
        _ARCreditMemo = New SalesCreditMemoForWMS(_Setting, Nothing, SalesCreditMemoForWMS._DocumentType.PR)
    End Sub

    Public Sub New(ByVal _Setting As Settings, ByVal _DiConn As CPSLIB.DIServer.DIServerConnection, ByVal _isDraft As Boolean)
        MyBase.New(_DiConn, SAPbobsCOM.BoObjectTypes.oDrafts)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._diServerConn = _DiConn
        Me._Setting = _Setting
        Me._isDraft = _isDraft
        _SAPARCreditMemo = New SalesCreditMemoForSAP(_Setting, Nothing)
        _ARCreditMemoConfig = New SalesCreditMemoConfig(_Setting)
        _ARCreditMemo = New SalesCreditMemoForWMS(_Setting, Nothing, SalesCreditMemoForWMS._DocumentType.PR)
    End Sub

    Public Function Start(ByVal _o As String, ByVal KeyLineField As String, ByVal _isProblem As Boolean, ByVal _isDamaage As Boolean) As Boolean
        _Debug.Write(String.Format("Creating Document: Key Value : {0}", _o))
        _Debug.Write("isDraft Document: " & _isDraft)
        _Debug.Write("isProblem: " & _isProblem)
        _Debug.Write("isDamage: " & _isDamaage)
        Dim _ret As Boolean = True
        Dim _drRow As DataRow()
        Dim _PrevLineNum As String = String.Empty
        Dim _PrevItemCode As String = String.Empty


        Dim _LineQuantity As Decimal
        Dim _DocSeries As String = String.Empty
        Dim DocParams As DocDraft.DocumentParams



        _Debug.Write(_o, "Key Value")
        _Debug.Write(_ARCreditMemoConfig.KeyField, "Key Column")
        Try
            If Not _isDamaage And Not _isProblem Then
                _Debug.Write(String.Format("{0} = '{1}' and isNull({2},'N') = 'N' and isNull({3},'N') = 'N' ", _ARCreditMemoConfig.KeyField, _o.ToString, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem), "Condition")
                _drRow = _dt.Select(String.Format("{0} = '{1}' and isNull({2},'N') = 'N' and isNull({3},'N') = 'N' ", _ARCreditMemoConfig.KeyField, _o.ToString, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem), String.Format("{0} asc", _ARCreditMemoConfig.KeyField))
            Else
                _Debug.Write(String.Format("{0} = '{1}' and isNull({2},'N') = 'Y'", _ARCreditMemoConfig.KeyField, _o.ToString, _
                                                  IIf(_isDamaage, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem)), "Condition")
                _drRow = _dt.Select(String.Format("{0} = '{1}' and isNull({2},'N') = 'Y'", _ARCreditMemoConfig.KeyField, _o.ToString, _
                                                  IIf(_isDamaage, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem)), String.Format("{0} asc", _ARCreditMemoConfig.KeyField))
            End If
            _Debug.Write(_drRow.Length, "Row Count")
            If _drRow.Length > 0 Then



                NewDocument()

                If IsDBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry)) = False Then
                    If Convert.ToInt32(_drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry)) > 0 Then

                        setUDF("ODRF", _drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry))

                        ' Standard Field in draft header
                        SetAllHeaderStandardField("ODRF", _drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry))
                        SetHeaderStandardField("ODRF", _drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry), "DocCur")
                        'SetDoctotalByDraft_ARCMTemp("ODRF", _drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry))
                    End If
                End If
                If _isDraft Then

                    setValue(Fld_HDR_ObjectCode, DIServer_ARCreditMemo.ObjType)
                End If

                'SetValue(Fld_HDR_DocDate, DateTime.Now.ToString("yyyyMMdd"))
                SetValue(Fld_HDR_DocDate, Convert.ToDateTime(_drRow(0)(SalesCreditMemo.Fld_DocDate)).ToString("yyyyMMdd"))

                SetValue(Fld_HDR_ASNNum, _drRow(0)(SalesCreditMemoForWMS.Fld_DocNum))
                'SetValue(Fld_HDR_DocDueDate, _
                '         MyBase.GetDueDate("ODRF", _drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry), _
                '                           DateTime.Now).ToString("yyyyMMdd"))
                If Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_CardCode)) <> String.Empty Then


                    setValue(Fld_HDR_CardCode, Settings.DBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_CardCode)))

                End If
                setValue(Fld_HDR_WMSEntry, _drRow(0)(SalesCreditMemoForWMS.Fld_ReceiveEntry))
                SetValue(Fld_HDR_WMSUser, _drRow(0)(SalesCreditMemoForWMS.Fld_WMSUser))



                ' UDF for Receive Entry and WMS User
                _LineQuantity = 0
                For Each dr In _drRow

                    If Settings.DBNull(dr(KeyLineField)) <> _PrevLineNum Then
                        If _PrevLineNum <> String.Empty Then
                            ' Add new Line

                            setRowsValue(FLD_DTL_Quantity, _LineQuantity)
                            setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)

                            AddRow()

                            _LineQuantity = 0

                        End If
                        If IsDBNull(_drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry)) = False Then
                            If Convert.ToInt32(_drRow(0)(SalesCreditMemoForWMS.Fld_DocEntry)) > 0 Then
                                setUDF("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum))

                            End If
                        End If

                    End If

                    'SetALLLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum))
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "FreeTxt")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "CogsOcrCod")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "CogsOcrCo2")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "CogsOcrCo3")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "CogsOcrCo4")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "CogsOcrCo5")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "WhsCode")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "OcrCode")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "OcrCode2")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "OcrCode3")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "OcrCode4")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "OcrCode5")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "AcctCode")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "DiscPrcnt")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "Currency")
                    SetLineStandardField("DRF1", dr(SalesCreditMemoForWMS.Fld_DocEntry), dr(SalesCreditMemoForWMS.Fld_LineNum), "PriceBefDi")
                   
                    If IsDBNull(dr(SalesCreditMemoForWMS.Fld_BatchNum)) = False Then
                        _BatchRow = New CPSLIB.DIServer.BatchNumbers
                        _BatchRow.BatchNumber = dr(SalesCreditMemoForWMS.Fld_BatchNum)
                        _BatchRow.Quantity = dr(SalesCreditMemoForWMS.Fld_Quantity)
                        setBatchNumberRow(_BatchRow)


                    End If

                    If Settings.DBNull(dr(SalesCreditMemoForWMS.Fld_BaseEntry), "-1") > 0 Then
                        setRowsValue(FLD_DTL_BaseType, "13")
                        setRowsValue(FLD_DTL_BaseEntry, dr(SalesCreditMemoForWMS.Fld_BaseEntry))
                        setRowsValue(FLD_DTL_BaseLine, dr(SalesCreditMemoForWMS.Fld_BaseLine))

                    Else
                        setRowsValue(FLD_DTL_WhsCode, dr(SalesCreditMemoForWMS.Fld_WhsCode))
                        'setRowsValue(FLD_DTL_UNITPrice, _SAPARCreditMemo.ItemCost(dr(Inventory_Inout.Fld_ItemCode)))



                    End If

                    If _isProblem Then
                        setRowsValue(FLD_DTL_WhsCode, _ARCreditMemoConfig.getFlowValue(SalesCreditMemoConfig.FUNC_PROBLEMWHSE))
                    End If
                    If _isDamaage Then
                        setRowsValue(FLD_DTL_WhsCode, _ARCreditMemoConfig.getFlowValue(SalesCreditMemoConfig.FUNC_DAMAGEWHSE))
                    End If
                    setRowsValue(FLD_DTL_WMSDraftEntry, dr(SalesCreditMemoForWMS.Fld_DocEntry))
                    setRowsValue(FlD_DTL_WMSDraftLine, dr(SalesCreditMemoForWMS.Fld_LineNum))

                    setRowsValue(FLD_DTL_WMSEntry, dr(SalesCreditMemoForWMS.Fld_ReceiveEntry))
                    setRowsValue(FLD_DTL_WMSLine, dr(SalesCreditMemoForWMS.Fld_ReceiveLineNum))

                    _LineQuantity = _LineQuantity + dr(SalesCreditMemoForWMS.Fld_Quantity)

                    _PrevLineNum = dr(KeyLineField)
                    _PrevItemCode = dr(SalesCreditMemoForWMS.Fld_ItemCode)
                Next
                setRowsValue(FLD_DTL_Quantity, _LineQuantity)
                setRowsValue(FLD_DTL_ItemCode, _PrevItemCode)
                AddRow()


                If MyBase.Post(CPSLIB.DIServer.DI_Object.Command.AddObject) = CPSLIB.DIServer.DI_Object.CommandStatus.Fail Then
                    _ret = False
                    _Message = MyBase.CmdMessage
                    _ARCreditMemo.UpdateErrorStatus(_ARCreditMemoConfig.KeyField, _o, "-1", _Message.Replace("'", "''"), _isDamaage, _isProblem)
                    _CPSException.ExecuteHandle(New Exception(_Message))
                Else
                    _Debug.Write("Update Success Status")

                    _ARCreditMemo.UpdateSuccessStatus(_ARCreditMemoConfig.KeyField, _o, NewEntry, String.Empty, _isDamaage, _isProblem)
                    _ret = True


                End If
            Else
                _ret = True
            End If
        Catch ex As Exception
            _ret = False
            _Message = ex.Message
            _ARCreditMemo.UpdateErrorStatus(_ARCreditMemoConfig.KeyField, _o, "-1", ex.Message.Replace("'", "''"), _isDamaage, _isProblem)
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function Start(ByVal _dt As DataTable) As Boolean
        Me._dt = _dt

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

                        _ret = Start(o, _ARCreditMemoConfig.KeyLineField, False, False)
                        If _ret Then
                            _ret = Start(o, _ARCreditMemoConfig.KeyLineField, True, False)
                            If _ret Then
                                _ret = Start(o, _ARCreditMemoConfig.KeyLineField, False, True)
                            End If
                        End If


                    Else

                        _ret = Start(o, SalesCreditMemo.Fld_ItemCode, False, False)
                        If _ret Then
                            _ret = Start(o, SalesCreditMemo.Fld_ItemCode, True, False)
                            If _ret Then
                                _ret = Start(o, SalesCreditMemo.Fld_ItemCode, False, True)
                            End If
                        End If
                    End If

                Next

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

        Return _ret



    End Function

   

End Class
