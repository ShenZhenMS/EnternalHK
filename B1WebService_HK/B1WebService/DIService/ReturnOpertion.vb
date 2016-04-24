Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CompilerServices

Public Class ReturnOperation

    'jerry  Add Fields DocDueDate CardCode


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _WMSConn As WMSSQLConnections
    Dim _SRConfig As SalesCreditMemoConfig
    Dim _SRS As SalesReturnService.ReturnsService

    Dim _WMSSR As SalesCreditMemoForWMS
    Dim _Setting As Settings
   
    Dim _WSSR As DIServer_ARCreditMemo
    Dim _WSPR As DIServer_APCreditMemo

    Dim oDoc As Object
    Dim _DiConn As CPSLIB.DIServer.DIServerConnection


    Private _Message As String
    Public Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Private _isError As Boolean
    Public Property isError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property

    Public Sub New(ByVal _Setting As Settings, ByVal _DocType As SalesCreditMemoForWMS._DocumentType)
        Me._Setting = _Setting

        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _WMSConn = New WMSSQLConnections(_Setting)
        _SRConfig = New SalesCreditMemoConfig(_Setting)
        _WMSSR = New SalesCreditMemoForWMS(_Setting, Nothing, _DocType)



    End Sub


    Public Function Start(ByVal _dt As DataTable) As Boolean

        _DiConn = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
        If _DiConn.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Fail Then
            _isError = True
            _Message = _DiConn.CmdMessage
        Else
            Select Case _WMSSR.DocumentType
                Case SalesCreditMemoForWMS._DocumentType.SR
                    _WSSR = New DIServer_ARCreditMemo(_Setting, _DiConn)
                    If _WSSR.Start(_dt) = False Then
                        _isError = True
                        _Message = _WSSR.CmdMessage
                    Else
                        _isError = False
                        _Message = String.Empty
                    End If
                Case SalesCreditMemoForWMS._DocumentType.PR
                    _WSPR = New DIServer_APCreditMemo(_Setting, _DiConn)
                    If _WSPR.Start(_dt) = False Then
                        _isError = True
                        _Message = _WSPR.CmdMessage
                    Else
                        _isError = False
                        _Message = String.Empty
                    End If
            End Select
            _DiConn.Logout()
        End If
        
        Return Not _isError
    End Function










End Class




