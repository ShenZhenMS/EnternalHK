Public Class SalesCreditMemo : Inherits WMSSQLConnections


    Public Shared Fld_DocType As String = "DocType"
    Public Shared Fld_CardCode As String = "CardCode"

    Public Shared Fld_DocEntry As String = "DocEntry"
    Public Shared Fld_LineNum As String = "LineNum"
    Public Shared Fld_DocDate As String = "DocDate"
    Public Shared Fld_ItemCode As String = "ItemCode"
    Public Shared Fld_Quantity As String = "Quantity"
    Public Shared Fld_UOM As String = "UOM"
    Public Shared Fld_WhsCode As String = "WhsCode"
    Public Shared Fld_BatchNum As String = "BatchNum"
    Public Shared Fld_DocNum As String = "DocNum"
    Public Shared Fld_ReasonCode As String = "ReasonCode"
    Public Shared Fld_WMSUser As String = "WMSUser"
    Public Shared Fld_TgtEntry As String = "TgtEntry"
    Public Shared Fld_TgtNum As String = "TgtNum"
    Public Shared Fld_ErrCode As String = "ErrCode"
    Public Shared Fld_Errdscr As String = "Errdscr"
    Public Shared Fld_TrtCreateDate As String = "TrtCreateDate"
    Public Shared Fld_ReceiveEntry As String = "ReceiveEntry"
    Public Shared Fld_CreateDate As String = "CreateDate"
    Public Shared Fld_LastRunDate As String = "LastRunDate"
    Public Shared Fld_TrxStatus As String = "TrxStatus"
    Public Shared Fld_ReceiveLineNum As String = "ReceiveLineNum"
    Public Shared Fld_isDamage As String = "isDamage"
    Public Shared Fld_isProblem As String = "isProblem"
    Public Shared Fld_BaseEntry As String = "BaseEntry"
    Public Shared Fld_BaseLine As String = "BaseLine"

    Public Shared TABLENAME As String = "CPS_TBL_RETURN"

    Dim _sqlUpdate_Err_General As String = "UPDATE {0} SET TrxStatus = 'F', TrtCreateDate = null, ErrCode = '{1}',ErrDscr = '{2}', TgtEntry = null,TgtNum = null  Where {4} = '{3}'  and isDamage = 'N' and isProblem = 'N'"
    Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S', TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where {4} = '{3}' and isNull({5},'N') = 'N' and isNull({6},'N') = 'N'"
    Dim _sqlUpdate_Suc_General As String = "UPDATE {0} SET TrxStatus = 'S', TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where {4} = '{3}' and isDamage = 'N' and isProblem = 'N'"
    Dim _sqlUpdate_Err As String = "UPDATE {0} SET TrxStatus = 'F', TrtCreateDate = null, ErrCode = '{1}',ErrDscr = '{2}', TgtEntry = null,TgtNum = null  Where {4} = '{3}' and isNull({5},'N') = 'Y' "
    Dim _sqlOpenITR As String = "SELECT {0} {1} FROM CPS_TBL_RETURN WHERE ISNULL(TRXSTATUS,'') IN ('F','') ORDER BY DOCENTRY ASC"

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _SAPSQLConn As SAPSQLConnections
    Dim _SalesCreditMemoConfig As SalesCreditMemoConfig
    Private _ErrorMsg As String


    Private _DocType As String
    Public Property DocumentType() As String
        Get
            Return _DocType
        End Get
        Set(ByVal value As String)
            _DocType = value
        End Set
    End Property


    Public Property ErrorMessage() As String
        Get
            Return _ErrorMsg
        End Get
        Set(ByVal value As String)
            _ErrorMsg = value
        End Set
    End Property

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections, ByVal _DocType As String)
        MyBase.New(_Setting)
        Me._DocType = _DocType
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn

        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _SalesCreditMemoConfig = New SalesCreditMemoConfig(_Setting)

        If Not _SalesCreditMemoConfig.isActive Then
            MyBase.isError = True
            _ErrorMsg = _SalesCreditMemoConfig.Message
        End If
        'OpenPurchaseOrder()

    End Sub




    Public Function UpdateSuccessStatus(ByVal _KeyField As String, ByVal _DocEntry As String, ByVal _TgtDocEntry As String, ByVal _TgtDocNum As String, ByVal _isDamage As Boolean, ByVal _isProblem As Boolean) As Boolean
        UpdateSuccessStatus = True
        '  Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S', TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where {4} = '{3}' and isNull({5},'N') = 'N' and isNull({6},'N') = 'N'"
        ' Dim _sqlUpdate_Suc_General As String = "UPDATE {0} SET TrxStatus = 'S', TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where {4} = '{3}' and isDamage = 'N' and isProblem = 'N'"

        Try
            If _isProblem Or _isDamage Then
                MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Suc, SalesCreditMemo.TABLENAME, _TgtDocEntry, _TgtDocNum, _DocEntry, _KeyField, IIf(_isDamage, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem), IIf(_isDamage, _isDamage, _isProblem)))
            Else
                MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Suc_General, SalesCreditMemo.TABLENAME, _TgtDocEntry, _TgtDocNum, _DocEntry, _KeyField, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem))
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateSuccessStatus = False
        End Try

    End Function

    Public Function UpdateErrorStatus(ByVal _KeyField As String, ByVal _DocEntry As String, ByVal _ErrCode As String, ByVal _ErrorDesc As String, ByVal _isDamage As Boolean, ByVal _isProblem As Boolean) As Boolean
        UpdateErrorStatus = True
        _Debug.Write(_isProblem, "isProblem")
        _Debug.Write(_isDamage, "isDamage")
        Try
            If _isProblem Or _isDamage Then
                MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Err, SalesCreditMemo.TABLENAME, _ErrCode, _ErrorDesc, _DocEntry, _KeyField, IIf(_isDamage, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem)))
            Else
                MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Err_General, SalesCreditMemo.TABLENAME, _ErrCode, _ErrorDesc, _DocEntry, _KeyField, SalesCreditMemo.Fld_isDamage, SalesCreditMemo.Fld_isProblem))
            End If

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateErrorStatus = False
        End Try

    End Function

End Class
