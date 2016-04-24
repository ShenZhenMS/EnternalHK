Public Class InventoryInoutConfig : Inherits SAPConfig

#Region "ModuleFunction"


    Public Shared FUNC_KEYCOLUMN As String = "WMSKEYCOLUMN"
    Public Shared FUNC_KEYLINECOLUMN As String = "WMSKEYLINECOLUMN"
    Public Shared FUNC_RECEVIEACCOUNT As String = "RECEIVEACCOUNT"

    Dim _dtConfig As DataTable
    Dim _dtModule As DataTable
    Dim _dr As DataRow()
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _Setting As Settings


    Private _KeyLineField As String
    Public Property KeyLineField() As String
        Get
            Return _KeyLineField
        End Get
        Set(ByVal value As String)
            _KeyLineField = value
        End Set
    End Property

    Private _KeyField As String
    Public Property KeyField() As String
        Get
            Return _KeyField
        End Get
        Set(ByVal value As String)
            _KeyField = value
        End Set
    End Property


    Private _ReceiveAcctCode As String
    Public Property ReceiveAcctCode() As String
        Get
            Return _ReceiveAcctCode
        End Get
        Set(ByVal value As String)
            _ReceiveAcctCode = value
        End Set
    End Property


   

   

    Private _isError As Boolean

    Private _Message As String
    Public Property ErrorMessage() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Public Property hasError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property

#End Region

    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting, Settings.WMSModule.STOCKIO)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        Me._Setting = _Setting
        Me.setConfig()
    End Sub

    Private Sub setConfig()
        _isError = False
        _Message = ""

        Try
            _KeyField = _Setting.getValue(Settings.WMSModule.STOCKIO.ToString & FUNC_KEYCOLUMN)
            _KeyLineField = _Setting.getValue(Settings.WMSModule.STOCKIO.ToString & FUNC_KEYLINECOLUMN)
            _ReceiveAcctCode = MyBase.getFlowValue(FUNC_RECEVIEACCOUNT)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

    End Sub

End Class
