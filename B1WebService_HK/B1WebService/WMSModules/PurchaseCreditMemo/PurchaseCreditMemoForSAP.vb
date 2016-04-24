Public Class PurchaseCreditMemoForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException


    Dim _sqlOpenList As String = "SELECT * FROM CPS_VIEW_RETURN WHERE 1 = 1 "

    Dim _sqlItemCost As String = "SELECT * FROM CPS_Func_ItemCost('{0}')"
    Dim _sqlItemPriceDraft As String = "SELECT * FROM CPS_Func_ItemCost('{0}','{1}','{2}','{3}')"
    Dim _sqlGLAccount As String = "SELECT * FROM CPS_Func_DocAccount('{0}','{1}','{2}')"
    Dim _sqlDocSeries As String = "SELECT * FROM CPS_FUNC_DOCSERIES('{0}','{1}','{2}')"
    Dim _sqlDocStatus As String = "SELECT * FROM CPS_FUNC_DOCSTATUS('{0}','{1}')"

    Public Shared EXP_FLD_CARDCODE As String = "CardCode"
    Public Shared EXP_FLD_DOCENTRY As String = "DocEntry"
    Public Shared EXP_FLD_LINENUM As String = "LineNum"
    Public Shared EXP_FLD_DOCNUM As String = "DocNum"
    Public Shared EXP_FLD_ASNNUM As String = "ASNNum"
    Public Shared EXP_FLD_DOCDATE As String = "DocDate"
    Public Shared EXP_FLD_ITEMCODE As String = "ItemCode"
    Public Shared EXP_FLD_ITEMNAME As String = "ItemName"
    Public Shared EXP_FLD_QUANTITY As String = "Quantity"
    Public Shared EXP_FLD_UOM As String = "UOM"
    Public Shared EXP_FLD_WHSNAME As String = "WhsName"
    Public Shared EXP_FLD_WHSCODE As String = "WhsCode"
    Public Shared EXP_FLD_BATCHNUM As String = "BatchNum"
    Public Shared EXP_FLD_LOCCODE As String = "LocCode"
    Public Shared EXP_FLD_REMARK As String = "Remark"

    Public Shared EXP_FLD_DOCTYPE As String = "DocType"

    Public Shared VALUE_DocType As String = "PR"
    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)

        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

   
    Public Function ItemCost(ByVal ItemCode As String) As Decimal
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlItemCost, ItemCode))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function


    Public Function ItemCost(ByVal itemCode As String, ByVal cardCode As String,
                             ByVal firstDraftEntry As String, ByVal firstDraftLineNumber As String) As Decimal
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlItemPriceDraft, itemCode, cardCode,
                                                           firstDraftEntry, firstDraftLineNumber))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function



    Public Function GLAccount(ByVal _DocType As String, ByVal DocDate As String, ByVal _ReasonCode As String) As String
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlGLAccount, _DocType, _ReasonCode, DocDate))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function DocSeries(ByVal _DocType As String, ByVal DocDate As String, ByVal _ReasonCode As String) As String
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlDocSeries, _DocType, _ReasonCode, DocDate))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function OpenList() As DataTable
        Dim _dt As DataTable
        Dim _sql = _sqlOpenList & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCTYPE, VALUE_DocType)
        Try
            _dt = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            isError = True
            Message = ex.Message
            _dt = Nothing
        End Try
        Return _dt
    End Function

    Public Function OpenList(ByVal _DocNum As String, ByVal _CardCode As String, ByVal _DateFrom As String, ByVal _DateTo As String) As DataTable
        Dim _dt As DataTable
        Dim _sql = _sqlOpenList & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCTYPE, VALUE_DocType)
        If _DocNum <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum.Replace("'", "''"))
        End If
        If _CardCode <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_CARDCODE, _CardCode.Replace("'", "''"))
        End If
        If _DateFrom <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDATE, Convert.ToDateTime(_DateFrom.Replace("'", "''")).ToString("yyyyMMdd"))
        End If
        If _DateTo <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDATE, Convert.ToDateTime(_DateTo.Replace("'", "''")).ToString("yyyyMMdd"))
        End If

        Try
            _dt = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            isError = True
            Message = ex.Message
            _dt = Nothing
        End Try
        Return _dt
    End Function

    Public Function DocumentStatus(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable = Nothing
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlDocStatus, _ReceiveEntry, WS_APCreditMemo.ObjType))

        Catch ex As Exception

        End Try
        Return _dt
    End Function
End Class
