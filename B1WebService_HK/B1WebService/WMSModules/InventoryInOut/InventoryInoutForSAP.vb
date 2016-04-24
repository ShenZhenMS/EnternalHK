Public Class InventoryInoutForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException



    Dim _sqlOpenDraftInOut As String = "SELECT * FROM CPS_VIEW_INVTRAN WHERE 1 = 1 "
    Dim _sqlItemCost As String = "SELECT AvgPrice FROM OITW Where ItemCode = '{0}' and WhsCode = '{1}'"
    Dim _sqlGLAccount As String = "SELECT * FROM CPS_Func_DocAccountWithBrand('{0}','{1}','{2}','{3}')"
    Dim _sqlDocSeries As String = "SELECT * FROM CPS_FUNC_DOCSERIES('{0}','{1}','{2}')"
    Dim _sqlDocStatus As String = "SELECT * FROM CPS_FUNC_WMSSTATUS('{0}')"

    Public Shared EXP_FLD_DOCENTRY As String = "DocEntry"
    Public Shared EXP_FLD_LINENUM As String = "LineNum"
    Public Shared EXP_FLD_DOCNUM As String = "DocNum"
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

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)

        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function OpenInventoryTranList() As DataTable
        Return OpenInventoryTranList(String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenInventoryTranList(ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenDraftInOut
            'If _CardCode <> String.Empty Then
            '    _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_CARDCODE, _CardCode.Replace("'", "''"))
            'End If
            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            OpenInventoryTranList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenInventoryTranList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function ItemCost(ByVal ItemCode As String) As Decimal
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlItemCost, ItemCode))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function ItemCost(ByVal ItemCode As String, ByVal WhsCode As String) As Decimal
        Dim _ret As Decimal = 0
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sqlItemCost, ItemCode, WhsCode))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GLAccount(ByVal _DocType As String, ByVal DocDate As String, ByVal _ReasonCode As String, ByVal _ItemCode As String) As String
        Dim _ret As Decimal = 0
        Try
            _Debug.Write(String.Format(_sqlGLAccount, _DocType, _ReasonCode, DocDate, _ItemCode))
            _ret = MyBase.ExecuteValue(String.Format(_sqlGLAccount, _DocType, _ReasonCode, DocDate, _ItemCode))
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

    Public Function DocumentStatus(ByVal _ReceiveEntry As String) As String
        Dim _ret As String = String.Empty
        Dim _dt As DataTable = Nothing
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlDocStatus, _ReceiveEntry))
            If _dt.Rows.Count > 0 Then
                Select Case _dt.Rows(0)("Stx")
                    Case "S"
                        _ret = _dt.Rows(0)("DocNum")
                    Case "P"
                        _ret = "Waiting"
                    Case "F"
                        _ret = "Reject"
                End Select
            End If
            

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            _ret = "Fail"
        End Try
        Return _ret
    End Function
End Class
