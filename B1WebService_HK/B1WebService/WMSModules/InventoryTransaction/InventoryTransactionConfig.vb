Public Class InventoryTransactionConfig : Inherits SAPSQLConnections

#Region "ModuleFunction"
    Public Shared FUNC_KEYCOLUMN As String = "WMSKEYCOLUMN"
    Public Shared FUNC_KEYLINECOLUMN As String = "WMSKEYLINECOLUMN"
    Public Shared FUNC_GENERATE_DRAFT As String = "ISDRAFT"
    Dim _dtConfig As DataTable
    Dim _dtModule As DataTable
    Dim _dr As DataRow()
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
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
    Private _isActive As Boolean
    Public Property isActive() As Boolean
        Get
            Return _isActive
        End Get
        Set(ByVal value As Boolean)
            _isActive = value
        End Set
    End Property

    Private _isDraft As Boolean
    Public Property isDraft() As Boolean
        Get
            Return _isDraft
        End Get
        Set(ByVal value As Boolean)
            _isDraft = value
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
        MyBase.New(_Setting)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _KeyField = _Setting.getValue(Settings.WMSModule.STOCKIO.ToString & FUNC_KEYCOLUMN)
        _KeyLineField = _Setting.getValue(Settings.WMSModule.STOCKIO.ToString & FUNC_KEYLINECOLUMN)
        'setConfig()
    End Sub

    Private Sub setConfig()
        _dtConfig = MyBase.WMSConfig(Settings.WMSModule.STOCKTRN.ToString)
        _dtModule = MyBase.WMSModule(Settings.WMSModule.STOCKTRN.ToString)
        _isError = False
        _Message = ""

        Try
            If _dtConfig.Rows.Count > 0 Then
                If _dtConfig.Rows(0)(SAPSQLConnections.Fld_Config_Active) = "N" Then
                    _isActive = False
                    _Message = "Module is in-active."
                    _CPSException.ExecuteHandle(New Exception("Module is in-active."), System.Reflection.MethodInfo.GetCurrentMethod.Name)
                Else
                    _isActive = True
                End If
            Else
                _Message = "Module is in-active."
                _CPSException.ExecuteHandle(New Exception("Module is in-active."), System.Reflection.MethodInfo.GetCurrentMethod.Name)
            End If

            If _dtModule.Rows.Count > 0 Then
                _dr = _dtModule.Select(String.Format("{0} = '{1}'", Fld_Module_FlowCode, FUNC_GENERATE_DRAFT))
                If _dr.Length > 0 Then
                    _isDraft = (_dr(0)(Fld_Module_Value) = "Y")
                Else
                    _isError = True
                    _Message = "Config error. Please contact administrator"
                    _CPSException.ExecuteHandle(New Exception("Config error. Please contact administrator"), System.Reflection.MethodInfo.GetCurrentMethod.Name)
                    _isDraft = True
                End If
            Else
                _isError = True
                _Message = "Config error. Please contact administrator"
                _CPSException.ExecuteHandle(New Exception("Config error. Please contact administrator"), System.Reflection.MethodInfo.GetCurrentMethod.Name)
            End If
        Catch ex As Exception
            MyBase.isError = True
            MyBase.Message = ex.Message
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod.Name)
        End Try

    End Sub

End Class
