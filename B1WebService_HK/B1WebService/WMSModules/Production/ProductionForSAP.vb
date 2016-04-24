Public Class ProductionForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _sqlOpenPurchase_FG As String = "SELECT * FROM CPS_VIEW_OWOR_FG WHERE 1 = 1 "
    Dim _sqlOpenPurchase_Child As String = "SELECT * FROM CPS_VIEW_OWOR_Child WHERE 1 = 1 "

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

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub


    Public Function OpenListFG() As DataTable
        Return OpenListFG(String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenListFG(ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenPurchase_FG

            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDUEDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDUEDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            OpenListFG = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenListFG = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function


    Public Function OpenListChild() As DataTable
        Return OpenListChild(String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenListChild(ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenPurchase_Child

            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDUEDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDUEDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            OpenListChild = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenListChild = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function
   
End Class
