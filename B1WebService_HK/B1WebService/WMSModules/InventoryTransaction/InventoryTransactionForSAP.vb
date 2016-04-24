Public Class InventoryTransactionForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _sqlOpenInventoryTransferRequest As String = "SELECT * FROM CPS_VIEW_OWTR WHERE 1 = 1 "

    Public Shared EXP_FLD_DOCENTRY As String = "DocEntry"
    Public Shared EXP_FLD_LINENUM As String = "LineNum"
    Public Shared EXP_FLD_DOCNUM As String = "DocNum"
    Public Shared EXP_FLD_DOCDUEDATE As String = "DocDueDate"
    Public Shared EXP_FLD_CARDCODE As String = "ItemCode"
    Public Shared EXP_FLD_CARDNAME As String = "ItemName"
    Public Shared EXP_FLD_QUANTITY As String = "Quantity"
    Public Shared EXP_FLD_UOM As String = "UOM"
    Public Shared EXP_FLD_FRMWHSCODE As String = "FrmWhsCode"
    Public Shared EXP_FLD_WHSNAME As String = "WhsName"
    Public Shared EXP_FLD_TOWHSCODE As String = "ToWhsCode"
    Public Shared EXP_FLD_TOWHSNAME As String = "ToWhsName"
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

    Public Function OpenInventoryTransferList() As DataTable
        Return OpenInventoryTransferList(String.Empty, String.Empty, String.Empty)
    End Function

    Public Function OpenInventoryTransferList(ByVal _FromDocDate As String, ByVal _ToDocDate As String, ByVal _DocNum As String) As DataTable
        Dim _sql As String
        Try
            _sql = _sqlOpenInventoryTransferRequest
            'If _CardCode <> String.Empty Then
            '    _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_CARDCODE, _CardCode.Replace("'", "''"))
            'End If
            If _FromDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} >= '{1}'", EXP_FLD_DOCDUEDATE, _FromDocDate)
            End If
            If _ToDocDate <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} <= '{1}'", EXP_FLD_DOCDUEDATE, _ToDocDate)
            End If
            If _DocNum <> String.Empty Then
                _sql = _sql & String.Format(" AND {0} = '{1}'", EXP_FLD_DOCNUM, _DocNum)
            End If
            OpenInventoryTransferList = MyBase.ExecuteDatatable(_sql)
        Catch ex As Exception
            OpenInventoryTransferList = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
    End Function

End Class
