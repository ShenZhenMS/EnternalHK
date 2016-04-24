Public Class InventoryTransaction : Inherits WMSSQLConnections

    Public Shared Fld_DocEntry As String = "DocEntry"
    Public Shared Fld_LineNum As String = "LineNum"
    Public Shared Fld_DocNum As String = "DocNum"
    Public Shared Fld_DocDate As String = "DocDate"
    Public Shared Fld_ItemCode As String = "ItemCode"
    Public Shared Fld_ItemName As String = "ItemName"
    Public Shared Fld_Quantity As String = "Quantity"
    Public Shared Fld_UOM As String = "UOM"
    Public Shared Fld_FrmWhsCode As String = "FrmWhsCode"
    Public Shared Fld_WhsName As String = "WhsName"
    Public Shared Fld_ToWhsCode As String = "ToWhsCode"
    Public Shared Fld_ToWhsName As String = "ToWhsName"
    Public Shared Fld_LineQuantity As String = "LineQuantity"
    Public Shared Fld_OldItemCode As String = "OldItemCode"
    Public Shared Fld_BatchNum As String = "BatchNum"
    Public Shared Fld_TransferType As String = "TransferType"
    Public Shared Fld_LocCode As String = "LocCode"
    Public Shared Fld_Remark As String = "Remark"
    Public Shared Fld_TgtEntry As String = "TgtEntry"
    Public Shared Fld_TgtNum As String = "TgtNum"
    Public Shared Fld_ErrCode As String = "ErrCode"
    Public Shared Fld_Errdscr As String = "Errdscr"
    Public Shared Fld_TrtCreateDate As String = "TrtCreateDate"
    Public Shared Fld_ReceiveEntry As String = "ReceiveEntry"
    Public Shared Fld_CreateDate As String = "CreateDate"
    Public Shared Fld_LastRunDate As String = "LastRunDate"
    Public Shared Fld_TrxStatus As String = "TrxStatus"
    Public Shared Fld_BatchQuantity As String = "BatchQuantity"
    Public Shared Fld_ItemPerUnit As String = "ItemPerUnit"
    Public Shared Fld_ReceiveLineNum As String = "ReceiveLineNum"
    Public Shared Fld_Counter As String = "Counter"
    Public Shared Fld_FrmLocCode As String = "FrmLocCode"
    Public Shared Fld_ToLocCode As String = "ToLocCode"
    Public Shared Fld_AllowBTChange As String = "AllowBTChange"

    Public Shared TABLENAME As String = "CPS_TBL_OWTR"
    Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S', LastRunDate = getDate(), TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where {4} = '{3}' "
    Dim _sqlUpdate_Err As String = "UPDATE {0} SET TrxStatus = 'F', LastRunDate = getDate(), TrtCreateDate = null, ErrCode = '{1}',ErrDscr = '{2}', TgtEntry = null,TgtNum = null  Where {4} = '{3}' "
    Dim _sqlOpenITR As String = "SELECT {0} {1} FROM CPS_TBL_OWTR WHERE ISNULL(TRXSTATUS,'') IN ('F','') ORDER BY DOCENTRY ASC"

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dtOpenITR As DataTable
    Dim _dtDistinctOpenITR As DataTable
    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _SAPSQLConn As SAPSQLConnections
    Dim _InventoryTransactionConfig As InventoryInoutConfig
    Private _ErrorMsg As String

    Public Property ErrorMessage() As String
        Get
            Return _ErrorMsg
        End Get
        Set(ByVal value As String)
            _ErrorMsg = value
        End Set
    End Property

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn

        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _InventoryTransactionConfig = New InventoryInoutConfig(_Setting)
        If Not _InventoryTransactionConfig.isActive Then
            MyBase.isError = True
            _ErrorMsg = _InventoryTransactionConfig.Message
        End If
        'OpenPurchaseOrder()

    End Sub

    Public Function OpenInventoryTransferRequest() As Boolean
        _dtDistinctOpenITR = MyBase.ExecuteDatatable(String.Format(_sqlOpenITR, "Distinct", Fld_DocEntry))
        _dtOpenITR = MyBase.ExecuteDatatable(String.Format(_sqlOpenITR, "", "*"))
        If MyBase.isError Then
            _ErrorMsg = MyBase.Message
        End If
        Return Not MyBase.isError
    End Function


    Public Function UpdateSuccessStatus(ByVal _KeyField As String, ByVal _DocEntry As String, ByVal _TgtDocEntry As String, ByVal _TgtDocNum As String) As Boolean
        UpdateSuccessStatus = True
        Try
            MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Suc, InventoryTransaction.TABLENAME, _TgtDocEntry, _TgtDocNum, _DocEntry, _KeyField))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateSuccessStatus = False
        End Try

    End Function

    Public Function UpdateErrorStatus(ByVal _KeyField As String, ByVal _DocEntry As String, ByVal _ErrCode As String, ByVal _ErrorDesc As String) As Boolean
        UpdateErrorStatus = True
        Try
            MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Err, InventoryTransaction.TABLENAME, _ErrCode, _ErrorDesc.Replace("'", "''"), _DocEntry, _KeyField))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateErrorStatus = False
        End Try

    End Function

End Class
