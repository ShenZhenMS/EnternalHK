Public Class SalesConfig : Inherits SAPSQLConnections

#Region "ModuleFunction"

    Public Shared FUNC_TARGET_DOCTYPE As String = "TARGETDOCTYPE"
    Public Shared FUNC_ADDITIONAL_TARGET_DOCTYPE As String = "ADTRGTDOCTYPE"
    Public Shared FUNC_ADDITIONAL_TARGET_CONDITION As String = "ADTRGTCONDITION"
    Dim _dtConfig As DataTable
    Dim _dtModule As DataTable
    Dim _dr As DataRow()
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException

    Private _additDocType As String
    Public Property AdditionDoctype() As String
        Get
            Return _additDocType
        End Get
        Set(ByVal value As String)
            _additDocType = value
        End Set
    End Property


    Private _additCondition As String
    Public Property AdditionCondition() As String
        Get
            Return _additCondition
        End Get
        Set(ByVal value As String)
            _additCondition = value
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

    Private _trtDocType As String
    Public Property trtDocType() As String
        Get
            Return _trtDocType
        End Get
        Set(ByVal value As String)
            _trtDocType = value
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
        'setConfig()
    End Sub

    Private Sub setConfig()
        _dtConfig = MyBase.WMSConfig(Settings.WMSModule.SALES.ToString)
        _dtModule = MyBase.WMSModule(Settings.WMSModule.SALES.ToString)
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
                _dr = _dtModule.Select(String.Format("{0} = '{1}'", Fld_Module_FlowCode, FUNC_TARGET_DOCTYPE))
                If _dr.Length > 0 And (_dr(0)(Fld_Module_Value) = 13 Or _dr(0)(Fld_Module_Value) = 15) Then

                    _trtDocType = _dr(0)(Fld_Module_Value)

                Else
                    _isError = True
                    _Message = "Config error. Please contact administrator"
                    _CPSException.ExecuteHandle(New Exception("Config error. Please contact administrator"), System.Reflection.MethodInfo.GetCurrentMethod.Name)
                End If

                ' Addition Document Type
                _dr = _dtModule.Select(String.Format("{0} = '{1}'", Fld_Module_FlowCode, FUNC_ADDITIONAL_TARGET_DOCTYPE))
                If _dr.Length > 0 And (_dr(0)(Fld_Module_Value) = 13 Or _dr(0)(Fld_Module_Value) = 15) Then

                    _additDocType = _dr(0)(Fld_Module_Value)
                Else
                    _additDocType = 0
                End If
                ' Addition Document Condition
                _dr = _dtModule.Select(String.Format("{0} = '{1}'", Fld_Module_FlowCode, FUNC_ADDITIONAL_TARGET_CONDITION))

                If _dr.Length > 0 Then
                    _additCondition = _dr(0)(Fld_Module_Value)
                Else
                    _additCondition = "1 = 1"
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
