Imports SBODI_Server
Imports CPSLIB

Public Class DIServer : Inherits CPSLIB.DIServer.Core

    Dim _Setting As Settings
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _SessionID As String


    Private _Message As String
    Public Overridable Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Private _isError As Boolean
    Public Overridable Property isError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property


    Private _connected As Boolean


    Private _Server As String

    Private _Company As String

    Private _LicServer As String

    Private _DBUserName As String

    Private _DBPassword As String

    Private _Username As String

    Private _Password As String

    Private _DBServerType As B1WebService.LoginService.LoginDatabaseType

    Dim _LoginService As LoginService.LoginService

    Dim _DIServerNode As CPSLIB.DIServer.DI_Node

    Public ReadOnly Property SessionID As String
        Get
            Return _SessionID
        End Get
    End Property

    Public Property DBServerType() As B1WebService.LoginService.LoginDatabaseType
        Get
            Return _DBServerType
        End Get
        Set(ByVal value As B1WebService.LoginService.LoginDatabaseType)
            _DBServerType = value
        End Set
    End Property

    Public Property Password() As String
        Get
            Return _Password
        End Get
        Set(ByVal value As String)
            _Password = value
        End Set
    End Property

    Public Property Username() As String
        Get
            Return _Username
        End Get
        Set(ByVal value As String)
            _Username = value
        End Set
    End Property

    Public Property DBPassword() As String
        Get
            Return _DBPassword
        End Get
        Set(ByVal value As String)
            _DBPassword = value
        End Set
    End Property

    Public Property DBUserName() As String
        Get
            Return _DBUserName
        End Get
        Set(ByVal value As String)
            _DBUserName = value
        End Set
    End Property

    Public Property LicServer() As String
        Get
            Return _LicServer
        End Get
        Set(ByVal value As String)
            _LicServer = value
        End Set
    End Property

    Public Property Company() As String
        Get
            Return _Company
        End Get
        Set(ByVal value As String)
            _Company = value
        End Set
    End Property

    Public Property Server() As String
        Get
            Return _Server
        End Get
        Set(ByVal value As String)
            _Server = value
        End Set
    End Property


    Public ReadOnly Property isConnected() As Boolean
        Get
            Return _connected
        End Get

    End Property

    Public Sub New(ByVal _Setting As Settings)
        MyBase.New("Connection")
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _Server = _Setting.ServerName
        _Company = _Setting.Database
        _LicServer = _Setting.LicServer
        _DBUserName = _Setting.SQLUserName
        _DBPassword = _Setting.SQLPasswd
        _Username = _Setting.Username
        _Password = _Setting.Password

        Select Case _Setting.DBServerType


            Case "1"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_DB_2
            Case "2"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_SYBASE
            Case "0"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_MSSQL
            Case "3"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_MSSQL2005
            Case "4"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_MSSQL
            Case "5"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_MAXDB
            Case "6"
                _DBServerType = B1WebService.LoginService.LoginDatabaseType.dst_MSSQL

        End Select


        ' DI Server 
        _LoginService = New LoginService.LoginService
        _DIServerNode = New CPSLIB.DIServer.DI_Node

        If (Not Me.isConnected) Then
            Login()
        End If

    End Sub


    Public Function Login() As Boolean

        Try
            _SessionID = _LoginService.Login(_Server, _Company, _DBServerType, True, _Username, _Password, B1WebService.LoginService.LoginLanguage.ln_English, True, _LicServer)
            If _SessionID.LastIndexOf("Error") < 0 Then

                _connected = True
            Else

                _connected = False
            End If
        Catch ex As Exception
            _connected = False
            _Message = ex.Message
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

        Return _connected

    End Function

    Public Function Logout(ByVal _SessionID As String) As Boolean
        Try
            _LoginService.Logout()
            _connected = False
        Catch ex As Exception
            _connected = True
        End Try
    End Function

    Public Function Logout() As Boolean


        'pDISnode = New SBODI_Server.Node
        Try
            If _DIServerNode.Execute(LogoutCommand) = CPSLIB.DIServer.DI_Node.CommandStatus.Fail Then
                _Message = _DIServerNode.CmdMessage
                Return False
            Else
                Return True
            End If
        Catch ex As Exception

            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            Return False
        End Try


        ''build the soap string
        'sCmdXml = "<?xml version=""1.0"" encoding=""UTF-16""?>" & _
        '"<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" & _
        '"<env:Header>" & _
        '"<SessionID>" & sSessionID & "</SessionID>" & _
        '"</env:Header>" & _
        '"<env:Body>" & _
        '"<dis:Logout xmlns:dis=""http://www.sap.com/SBO/DIS"">" & _
        '"</dis:Logout>" & _
        '"</env:Body>" & _
        '"</env:Envelope>"


        ''execute interact and return the result
        'sSOAPans = pDISnode.Interact(sCmdXml)

        'Return Status_Msg

    End Function


    Public Function LogoutCommand() As String
        Dim _strCmd As String = String.Empty

        _strCmd = String.Format(CPSLIB.DIServer.Core.RequestXML, String.Format(CPSLIB.DIServer.Core.RequestHeaderXML, _SessionID), String.Format(CPSLIB.DIServer.Core.RequestLogoutXML, "http://www.sap.com/SBO/DIS"))
        Return _strCmd
    End Function

    Public Function DraftDocumentLineArray(ByVal _al As ArrayList) As DocDraft.DocumentDocumentLine()
        Dim l(_al.Count - 1) As DocDraft.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), DocDraft.DocumentDocumentLine)
        Next
        Return l
    End Function

    Public Function DraftDocumentLineBatchArray(ByVal _al As ArrayList) As DocDraft.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As DocDraft.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), DocDraft.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function InventoryReceiptDocumentLineArray(ByVal _al As ArrayList) As InventoryReceive.DocumentDocumentLine()
        Dim l(_al.Count - 1) As InventoryReceive.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), InventoryReceive.DocumentDocumentLine)
        Next
        Return l
    End Function

    Public Function InventoryReceipttoDocumentLineBatchArray(ByVal _al As ArrayList) As InventoryReceive.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As InventoryReceive.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), InventoryReceive.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function InventoryIssueDocumentLineArray(ByVal _al As ArrayList) As InventoryIssue.DocumentDocumentLine()
        Dim l(_al.Count - 1) As InventoryIssue.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), InventoryIssue.DocumentDocumentLine)
        Next
        Return l
    End Function

    Public Function InventoryIssuetoDocumentLineBatchArray(ByVal _al As ArrayList) As InventoryIssue.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As InventoryIssue.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), InventoryIssue.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function



    Public Function GRPOtoDocumentLineArray(ByVal _al As ArrayList) As GRPO.DocumentDocumentLine()
        Dim l(_al.Count - 1) As GRPO.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), GRPO.DocumentDocumentLine)
        Next
        Return l
    End Function



    Public Function GRPOtoDocumentLineBatchArray(ByVal _al As ArrayList) As GRPO.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As GRPO.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), GRPO.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function SalesInvoicetoDocumentLineArray(ByVal _al As ArrayList) As SalesInvoice.DocumentDocumentLine()
        Dim l(_al.Count - 1) As SalesInvoice.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesInvoice.DocumentDocumentLine)
        Next
        Return l
    End Function

    Public Function SalesDeliverytoDocumentLineArray(ByVal _al As ArrayList) As SalesDelivery.DocumentDocumentLine()
        Dim l(_al.Count - 1) As SalesDelivery.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesDelivery.DocumentDocumentLine)
        Next
        Return l
    End Function
    Public Function SalesDeliverytoDocumentLineBatchArray(ByVal _al As ArrayList) As SalesDelivery.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As SalesDelivery.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesDelivery.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function APCreditMemotoDocumentLineArray(ByVal _al As ArrayList) As APCreditMemo.DocumentDocumentLine()
        Dim l(_al.Count - 1) As APCreditMemo.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), APCreditMemo.DocumentDocumentLine)
        Next
        Return l
    End Function
    Public Function APCreditMemotoDocumentLineBatchArray(ByVal _al As ArrayList) As APCreditMemo.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As APCreditMemo.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), APCreditMemo.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function ARCreditMemotoDocumentLineArray(ByVal _al As ArrayList) As ARCreditMemo.DocumentDocumentLine()
        Dim l(_al.Count - 1) As ARCreditMemo.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), ARCreditMemo.DocumentDocumentLine)
        Next
        Return l
    End Function
    Public Function ARCreditMemotoDocumentLineBatchArray(ByVal _al As ArrayList) As ARCreditMemo.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As ARCreditMemo.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), ARCreditMemo.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function SalesReturntoDocumentLineArray(ByVal _al As ArrayList) As SalesReturnService.DocumentDocumentLine()
        Dim l(_al.Count - 1) As SalesReturnService.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesReturnService.DocumentDocumentLine)
        Next
        Return l
    End Function
    Public Function SalesReturntoDocumentLineBatchArray(ByVal _al As ArrayList) As SalesReturnService.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As SalesReturnService.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesReturnService.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function PurchaseReturntoDocumentLineArray(ByVal _al As ArrayList) As PurchaseReturnService.DocumentDocumentLine()
        Dim l(_al.Count - 1) As PurchaseReturnService.DocumentDocumentLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), PurchaseReturnService.DocumentDocumentLine)
        Next
        Return l
    End Function
    Public Function PurchaseReturntoDocumentLineBatchArray(ByVal _al As ArrayList) As PurchaseReturnService.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As PurchaseReturnService.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), PurchaseReturnService.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function

    Public Function StockTransfertoDocumentLineArray(ByVal _al As ArrayList) As StockTransfer.StockTransferStockTransferLine()
        Dim l(_al.Count - 1) As StockTransfer.StockTransferStockTransferLine
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), StockTransfer.StockTransferStockTransferLine)
        Next
        Return l
    End Function
    Public Function StockTransfertoDocumentLineBatchArray(ByVal _al As ArrayList) As StockTransfer.StockTransferStockTransferLineBatchNumber()
        Dim l(_al.Count - 1) As StockTransfer.StockTransferStockTransferLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), StockTransfer.StockTransferStockTransferLineBatchNumber)
        Next
        Return l
    End Function
    Public Function SalesInvoicetoDocumentLineBatchArray(ByVal _al As ArrayList) As SalesInvoice.DocumentDocumentLineBatchNumber()
        Dim l(_al.Count - 1) As SalesInvoice.DocumentDocumentLineBatchNumber
        For i As Integer = 0 To _al.Count - 1
            l(i) = CType(_al(i), SalesInvoice.DocumentDocumentLineBatchNumber)
        Next
        Return l
    End Function


End Class
