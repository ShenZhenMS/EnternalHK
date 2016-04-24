Public Class SalesForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _sqlOpenSales As String = "SELECT * FROM CPS_VIEW_ORDR WHERE 1 = 1 "
    Dim _sqlPickList As String = "select distinct PickNum From CPS_View_ORDR"
    Dim _sqlAddress As String = "SELECT * FROM CPS_View_DeliveryAddress WHERE 1 = 1"

    Dim _sqlPosStockOut As String = "SELECT * FROM CPS_VIEW_POSSTOCKOUT WHERE 1=1"
    Dim _sqlPosSalesReturn As String = "SELECT * FROM CPS_VIEW_POSSALESRETURN WHERE 1=1"
    Dim _sqlPosShopToShop As String = "SELECT * FROM CPS_VIEW_POSSHOPTOSHOP WHERE 1=1"

    Public Shared EXP_FLD_PICKNUM As String = "PickNum"
    Public Shared EXP_FLD_DOCENTRY As String = "DocEntry"
    Public Shared EXP_FLD_LINENUM As String = "LineNum"
    Public Shared EXP_FLD_DOCNUM As String = "DocNum"
    Public Shared EXP_FLD_DOCDUEDATE As String = "DocDueDate"
    Public Shared EXP_FLD_CARDCODE As String = "CardCode"
    Public Shared EXP_FLD_CARDNAME As String = "CardName"
    Public Shared EXP_FLD_ITEMCODE As String = "ItemCode"
    Public Shared EXP_FLD_ITEMNAME As String = "ItemName"
    Public Shared EXP_FLD_QUANTITY As String = "Quantity"
    Public Shared EXP_FLD_UOM As String = "UOM"
    Public Shared EXP_FLD_WHSCODE As String = "WhsCode"
    Public Shared EXP_FLD_WHSNAME As String = "WhsName"
    Public Shared EXP_FLD_ADDRESS As String = "Address"


    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function PickListReport() As DataTable
        Dim _sql As String
        Try
            _sql = _sqlPickList
            PickListReport = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            PickListReport = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function OpenSalesList() As DataTable
        Return OpenSalesList(String.Empty, String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenSalesList(ByVal _CardCode As String, ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenSales
            If _CardCode <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_CARDCODE, _CardCode.Replace("'", "''"))
            End If
            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDUEDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDUEDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_PICKNUM, _DocNum)
            End If
            OpenSalesList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenSalesList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function AddressList(ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlAddress

            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            AddressList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            AddressList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function SalesList(ByVal _CardCode As String, ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenSales
            If _CardCode <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_CARDCODE, _CardCode.Replace("'", "''"))
            End If
            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDUEDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDUEDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            SalesList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            SalesList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function ValidateBatchNumber(ByVal _CardCode As String, ByVal _ItemCode As String, ByVal _BatchNum As String) As Boolean
        Dim _sql As String = "SELECT 1 FROM CPS_FUNC_VALIDBATCH('{0}','{1}','{2}')"
        Dim _ret As Boolean
        Try
            _ret = MyBase.Exists(String.Format(_sql, _CardCode.Replace("'", "''"), _ItemCode.Replace("'", "''"), _BatchNum.Replace("'", "''")))
        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function PosStockOut(ByVal docDate As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlPosStockOut
            If docDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", Inventory_Inout.Fld_DocDate, docDate.Replace("'", "''"))
            End If
            PosStockOut = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            PosStockOut = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Function
    Public Function PosSalesReturn(ByVal docDate As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlPosSalesReturn
            If docDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", Inventory_Inout.Fld_DocDate, docDate.Replace("'", "''"))
            End If
            PosSalesReturn = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            PosSalesReturn = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function PosShopToShop(ByVal docDate As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlPosShopToShop
            If docDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", Inventory_Inout.Fld_DocDate, docDate.Replace("'", "''"))
            End If
            PosShopToShop = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            PosShopToShop = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

End Class
