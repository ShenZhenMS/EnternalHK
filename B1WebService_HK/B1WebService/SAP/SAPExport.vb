Public Class SAPMasterExport : Inherits SAPSQLConnections

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _sqlBatchInfo As String = "SELECT * FROM CPS_VIEW_BATCHINFORMATION WHERE 1 = 1"
    Dim _sqlBarCode As String = "SELECT * FROM CPS_VIEW_BARCODE"
    Dim _sqlBarCodeByDateRng As String = "SELECT * FROM CPS_FUNC_MASTER_EXPORT_BARCODE_RANGE('{0}','{1}')"
    Dim _sqlBarCodeByDateCode As String = "SELECT * FROM CPS_FUNC_MASTER_EXPORT_BARCODE_CODE('{0}','{1}')"
    Dim _sqlItem As String = "SELECT * FROM CPS_VIEW_OITM"
    Dim _sqlItemByDateRng As String = "SELECT * FROM CPS_FUNC_MASTER_EXPORT_ITEM('{0}','{1}')"
    Dim _sqlWarehosue As String = "SELECT * FROM CPS_VIEW_WAREHOUSE"
    Dim _sqlBPList As String = "SELECT * FROM CPS_VIEW_BP"
    Dim _sqlReason As String = "SELECT ReasonCode FROM CPS_VIEW_REASON Where 1 = 1 and DocType = '{0}'"
    Public Shared Fld_ItemCode As String = "ItemCode"
    Public Shared Fld_ItemName As String = "ItemName"
    Public Shared Fld_LocCode As String = "LocCode"
    Public Shared Fld_BatchNum As String = "BatchNum"
    Public Shared Fld_Quantity As String = "Quantity"
    Public Shared Fld_WhsCode As String = "WhsCode"



    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
    End Sub

    Public Function BarCodeInfo_Code(ByVal pItemCode As String, ByVal pBarCode As String) As DataTable
        Dim _dt As DataTable
        _dt = MyBase.ExecuteDatatable(String.Format(_sqlBarCodeByDateCode, IIf(String.IsNullOrEmpty(pItemCode), "", pItemCode), IIf(String.IsNullOrEmpty(pBarCode), "", pBarCode)))
        Return _dt
    End Function

    Public Function BarCodeInfo_Range(ByVal pFromDate As String, ByVal pToDate As String) As DataTable
        Dim _dt As DataTable
        _dt = MyBase.ExecuteDatatable(String.Format(_sqlBarCodeByDateRng, pFromDate, pToDate))
        Return _dt
    End Function

    Public Function BarCodeInfo() As DataTable
        Dim _dt As DataTable
        _dt = MyBase.ExecuteDatatable(_sqlBarCode)
        Return _dt
    End Function

    Public Function ItemInfo_Range(ByVal pFromDate As String, ByVal pToDate As String) As DataTable
        Dim _dt As DataTable
        _dt = MyBase.ExecuteDatatable(String.Format(_sqlItemByDateRng, pFromDate, pToDate))
        Return _dt
    End Function

    Public Function BPList() As DataTable
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(_sqlBPList)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function ItemInfo() As DataTable
        Dim _dt As DataTable
        _dt = MyBase.ExecuteDatatable(_sqlItem)
        Return _dt
    End Function

    Public Function BatchInformation(ByVal _ItemCode As String, ByVal WhsCode As String) As DataTable
        Dim _sql As String = _sqlBatchInfo

        _sql = String.Format(_sql & " AND {0} = '{1}' ", Fld_ItemCode, _ItemCode.Replace("'", "''"))
        If WhsCode <> String.Empty Then
            _sql = String.Format(_sql & " AND {0} = '{1}'", Fld_WhsCode, WhsCode.Replace("'", "''"))
        End If

        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function


    Public Function BatchInformation(ByVal _ItemCode As String, ByVal WhsCode As String, ByVal _LocCode As String) As DataTable
        Dim _sql As String = _sqlBatchInfo
        _sql = String.Format(_sql & " AND {0} = '{1}' ", Fld_ItemCode, _ItemCode.Replace("'", "''"))
        If WhsCode <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} = '{1}'", Fld_WhsCode, WhsCode.Replace("'", "''"))
        End If
        If _LocCode <> String.Empty Then
            _sql = _sql & String.Format(" AND {0} = '{1}'", Fld_LocCode, _LocCode.Replace("'", "''"))
        End If

        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function WarehouseInfo() As DataTable
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(_sqlWarehosue)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function ReasonCode(ByVal _ObjType As String) As DataTable
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlReason, _ObjType))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function
End Class
