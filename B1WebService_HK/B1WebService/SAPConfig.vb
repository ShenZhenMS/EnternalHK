Public Class SAPConfig : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _ModuleCode As Settings.WMSModule

    Private _active As Boolean
    Public Shared FUNC_GENERATE_DRAFT As String = "ISDRAFT"
    Public Shared FUNC_GENERATEAPPROVED_DRAFT As String = "APPROVED_DRAFT"
    Private _isError As Boolean
    Dim _dtConfig As DataTable
    Dim _dtModule As DataTable
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
    Public Property isActive() As Boolean
        Get
            Return _active
        End Get
        Set(ByVal value As Boolean)
            _active = value
        End Set
    End Property

    Public Sub New(ByVal _Setting As Settings, ByVal _ModuleCode As Settings.WMSModule)
        MyBase.New(_Setting)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._Setting = _Setting
        Me._ModuleCode = _ModuleCode
        setConfig()
    End Sub

    Public Sub setConfig()
        _dtConfig = MyBase.WMSConfig(Me._ModuleCode.ToString)
        _dtModule = MyBase.WMSModule(Me._ModuleCode.ToString)
        _isError = False
        _Message = ""

        Try

            If _dtConfig.Rows.Count > 0 Then
                If _dtConfig.Rows(0)(SAPSQLConnections.Fld_Config_Active) = "N" Then
                    _active = False
                    _Message = "Module is in-active."
                    _CPSException.ExecuteHandle(New Exception("Module is in-active."), System.Reflection.MethodInfo.GetCurrentMethod.Name)
                Else
                    _active = True
                End If
            Else
                _active = False
                _Message = "Module is in-active."
                _CPSException.ExecuteHandle(New Exception("Module is in-active."), System.Reflection.MethodInfo.GetCurrentMethod.Name)
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try

    End Sub

    Public Function PostToDraft() As Boolean

        Return (getFlowValue(FUNC_GENERATE_DRAFT) = "Y")
        
    End Function

    Public Function PostApprovedToDraft() As Boolean
        Return (getFlowValue(FUNC_GENERATEAPPROVED_DRAFT) = "Y")
    End Function


    Public Function getFlowValue(ByVal FlowCode As String) As String
        Dim _dr As DataRow()
        _dr = _dtModule.Select(String.Format("{0} = '{1}'", Fld_Module_FlowCode, FlowCode))
        If _dr.Length > 0 Then
            Return _dr(0)(Fld_Module_Value).ToString
        Else
            _isError = True
            _Message = String.Format("Module Code: {0} Flow Code: {1} Config error. Please contact administrator", _ModuleCode.ToString, FlowCode)
            _CPSException.ExecuteHandle(New Exception("Config error. Please contact administrator"), System.Reflection.MethodInfo.GetCurrentMethod.Name)
            Return String.Empty
        End If
    End Function
End Class
