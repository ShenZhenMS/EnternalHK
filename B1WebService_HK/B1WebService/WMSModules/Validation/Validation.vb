Public Class Validation : Inherits SAPSQLConnections
    Dim _Setting As Settings
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _Module As Settings.WMSModule
    Public Shared PFX_IMPORTFIELDS As String = "IMPORTFIELDS"
    Public Shared PFX_REQUIREDFIELDS As String = "REQUIREDFIELDS"
    Public Shared PFX_NUMERICFIELDS As String = "NUMERICFIELDS"
    Public Shared PFX_DATEFIELDS As String = "DATEFIELDS"

    Private _TableFields As ArrayList
    Public ReadOnly Property TableFields() As ArrayList
        Get
            Return _TableFields
        End Get
    End Property


    Private _RequiredFields As ArrayList
    Public ReadOnly Property RequiredFields() As ArrayList
        Get

            Return _RequiredFields
        End Get
    End Property

    Private _NumericFields As ArrayList
    Public ReadOnly Property NumericFields() As ArrayList
        Get
            Return _NumericFields
        End Get
    End Property

    Private _DateFields As ArrayList
    Public ReadOnly Property DateField() As ArrayList
        Get
            Return _DateFields
        End Get

    End Property


    Public Sub New(ByVal _Setting As Settings, ByVal _Module As Settings.WMSModule)
        MyBase.New(_Setting)
        Me._Module = _Module
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        init()
    End Sub

    Public Sub init()
        Dim _importfld As String()
        Dim _requiredfld As String()
        Dim _numericfld As String()
        Dim _datefld As String()

        Try
            _importfld = System.Configuration.ConfigurationManager.AppSettings(_Module.ToString & Validation.PFX_IMPORTFIELDS).Split(",")
            _TableFields = New ArrayList(_importfld)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

        Try
            _requiredfld = System.Configuration.ConfigurationManager.AppSettings(_Module.ToString & Validation.PFX_REQUIREDFIELDS).Split(",")
            _RequiredFields = New ArrayList(_requiredfld)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

        Try
            _numericfld = System.Configuration.ConfigurationManager.AppSettings(_Module.ToString & Validation.PFX_NUMERICFIELDS).Split(",")
            _NumericFields = New ArrayList(_numericfld)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

        Try
            _datefld = System.Configuration.ConfigurationManager.AppSettings(_Module.ToString & Validation.PFX_DATEFIELDS).Split(",")
            _DateFields = New ArrayList(_datefld)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Sub

    Public Function ValidateTableStructure(ByRef dt As DataTable) As Boolean
        For Each o As Object In _TableFields
            If Not dt.Columns.Contains(o.ToString) Then
                dt.Columns.Add(o.ToString)
            End If
        Next
    End Function
End Class
