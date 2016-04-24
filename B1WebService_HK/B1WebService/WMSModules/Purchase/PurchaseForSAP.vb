Public Class PurchaseForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _sqlOpenPurchase As String = "SELECT * FROM CPS_VIEW_OPOR WHERE 1 = 1 "

    Public Shared EXP_FLD_DOCENTRY As String = "DocEntry"
    Public Shared EXP_FLD_LINENUM As String = "LineNum"
    Public Shared EXP_FLD_DOCNUM As String = "DocNum"
    Public Shared EXP_FLD_ASNNUM As String = "ASNNum"
    Public Shared EXP_FLD_DOCDUEDATE As String = "DocDueDate"
    Public Shared EXP_FLD_CARDCODE As String = "CardCode"
    Public Shared EXP_FLD_CARDNAME As String = "CardName"
    Public Shared EXP_FLD_ITEMCODE As String = "ItemCode"
    Public Shared EXP_FLD_ITEMNAME As String = "ItemName"
    Public Shared EXP_FLD_QUANTITY As String = "Quantity"
    Public Shared EXP_FLD_UOM As String = "UOM"
    Public Shared EXP_FLD_WHSCODE As String = "WhsCode"
    Public Shared EXP_FLD_WHSNAME As String = "WhsName"

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub


    Public Function OpenPurchaseList() As DataTable
        Return OpenPurchaseList(String.Empty, String.Empty, String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenPurchaseList(ByVal _CardCode As String, ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String, ByVal _ASNNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenPurchase
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
            If _ASNNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_ASNNUM, _ASNNum)
            End If
            OpenPurchaseList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenPurchaseList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

    Public Function isSameWarehouse(ByVal _DocEntry As String, ByVal _LineNum As String, ByVal _TgtWhse As String) As Boolean
        Dim _sql As String = "SELECT 1 FROM POR1 WHERE DOCENTRY = '{0}' AND LINENUM = '{1}' AND WHSCODE = '{2}'"
        Try
            isSameWarehouse = Exists(String.Format(_sql, _DocEntry, _LineNum, _TgtWhse.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            isSameWarehouse = False
        End Try


    End Function


    Public Function GetDraftHeader(ByVal _DocEntry As String) As DataTable
        Dim _sql As String = "SELECT * FROM ODRF WHERE DOCENTRY = '{0}' "
        Dim _ret As DataTable = Nothing
        Try
            _ret = MyBase.ExecuteDatatable(String.Format(_sql, _DocEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GetDraftLine(ByVal _DocEntry As String, ByVal _LineNum As String) As DataTable
        Dim _sql As String = "SELECT * FROM DRF1 WHERE DOCENTRY = '{0}' AND LINENUM = '{1}'"
        Dim _ret As DataTable = Nothing
        Try
            _ret = MyBase.ExecuteDatatable(String.Format(_sql, _DocEntry, _LineNum))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

End Class
