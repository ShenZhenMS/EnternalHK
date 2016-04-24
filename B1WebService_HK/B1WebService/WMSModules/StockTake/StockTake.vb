Public Class StockTake : Inherits WMSSQLConnections

    
    Public Shared TableName As String = "CPS_TBL_STOCKTAKE"
    Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S', LastRunDate = getDate(), TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where DocEntry = {3} and isNull(TrxStatus,'') = ''"
    Dim _sqlUpdate_Err As String = "UPDATE {0} SET TrxStatus = 'F', LastRunDate = getDate(), TrtCreateDate = null, ErrCode = '{1}',ErrDscr = '{2}', TgtEntry = null,TgtNum = null  Where DocEntry = {3} and isNull(TrxStatus,'') = ''"
    
    Public Shared Fld_WMSENtry As String = "WMSEntry"
    Public Shared Fld_AdjType As String = "AdjType"
    Public Shared Fld_RefNum As String = "RefNum"
    Public Shared Fld_WMSUser As String = "WMSUser"
    Public Shared Fld_CreateDate As String = "CreateDate"
    Public Shared Fld_DraftEntry As String = "DraftEntry"
    Public Shared Fld_Approval As String = "Approval"
    Public Shared Fld_TrxStatus As String = "TrxStatus"
    Public Shared Fld_ErrMsg As String = "ErrMsg"
    Public Shared Fld_lastRunDate As String = "LastRunDate"


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable
    Dim _dtDistinctOpenPO As DataTable
    Dim _Setting As Settings
    Dim _SAPSQLConn As SAPSQLConnections
    Dim _StockTakeConfig As StockTakeConfig
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
        Me._SAPSQLConn = _SAPSQLConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

        If Not _StockTakeConfig.isActive Then
            MyBase.isError = True
            _ErrorMsg = _StockTakeConfig.Message
        End If
        'OpenPurchaseOrder()



    End Sub

    
    Public Function UpdateSuccessStatus(ByVal _DocEntry As String, ByVal _TgtDocEntry As String, ByVal _TgtDocNum As String) As Boolean
        UpdateSuccessStatus = True
        Dim mSql As String
        Try
            mSql = String.Format(_sqlUpdate_Suc, StockTake.TableName, _TgtDocEntry, _TgtDocNum, _DocEntry)
            MyBase.ExecuteUpdate(mSql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateSuccessStatus = False
        End Try

    End Function

    Public Function UpdateErrorStatus(ByVal _DocEntry As String, ByVal _ErrCode As String, ByVal _ErrorDesc As String) As Boolean
        UpdateErrorStatus = True
        Dim mSql As String
        Try
            mSql = String.Format(_sqlUpdate_Err, StockTake.TableName, _ErrCode, _ErrorDesc, _DocEntry)
            MyBase.ExecuteUpdate(mSql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateErrorStatus = False
        End Try

    End Function

End Class
