Public Class DeliveryOperation : Inherits DIServer

    'jerry  Add Fields DocDueDate CardCode
    Public Shared SQL_SALES_TABLE_BY_DOCENTRY As String = "SELECT DISTINCT DOCENTRY,DocDueDate,CardCode,NewDocDueDate FROM CPS_TBL_ORDR WHERE ISNULL(TRXSTATUS,'') = '' ORDER BY DOCENTRY ASC"
    Public Shared SQL_SALES_TABLE_BY_LINE As String = "SELECT DISTINCT LINENUM FROM CPS_TBL_ORDR WHERE ISNULL(TRXSTATUS,'') = '' AND DOCENTRY = '{0}' ORDER BY LINENUM ASC"
    Public Shared SQL_SALES_TABLE_BY_BATCH As String = "SELECT * FROM CPS_TBL_ORDR WHERE ISNULL(TRXSTATUS,'') = '' AND DOCENTRY = '{0}' AND LINENUM = '{1}'"

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _WMSConn As WMSSQLConnections
    Dim _SalsConfig As SalesConfig
    Dim _SalesDelivery As SalesDelivery.DeliveryNotesService
    Dim _SalesInvoice As SalesInvoice.InvoicesService
    Dim _Sales As SalesForWMS
    Private _Message As String
    Public Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Dim _Setting As Settings
    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _WMSConn = New WMSSQLConnections(_Setting)
        _SalsConfig = New SalesConfig(_Setting)
        _Sales = New SalesForWMS(_Setting, Nothing)
        Me._Setting = _Setting
    End Sub

    Public Function Start() As Boolean
        Dim _ret As Boolean = True
        Dim _DiServerConnection As CPSLIB.DIServer.DIServerConnection
        Dim _WSDN As DIServer_SalesDelivery
        Try
            _DiServerConnection = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
            If _DiServerConnection.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Success Then
                _WSDN = New DIServer_SalesDelivery(_Setting, _DiServerConnection)
                _ret = _WSDN.Create
                If _ret = False Then
                    _Message = _WSDN.Msg
                End If
                _DiServerConnection.Logout()
            Else
                _ret = False
                Throw New Exception(_DiServerConnection.CmdMessage)
            End If

        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function Start(ByVal sapConnection As CPSLIB.DIServer.DIServerConnection) As Boolean
        Dim _ret As Boolean = True
        Dim _WSDN As DIServer_SalesDelivery
        Try
            _WSDN = New DIServer_SalesDelivery(_Setting, sapConnection)
            _ret = _WSDN.Create
            If _ret = False Then
                _Message = _WSDN.Msg
            End If
        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

End Class
