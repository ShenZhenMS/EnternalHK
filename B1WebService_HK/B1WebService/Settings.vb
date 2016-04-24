
Public Class Settings

    Public Shared SettingFiles As String = "Settings.ini"

    Public Enum WMSModule
        PURCHASE = 1
        SALES = 2
        STOCKTRN = 3
        STOCKTAKE = 4
        EXPITEM = 5
        EXPBARCODE = 6
        EXPWAREHOUSE = 7
        BINLOCATION = 8
        BP = 9
        STOCKIO = 10
        APCREDITMEMO = 11
        ARCREDITMEMO = 12
        REASON = 13
        PRODUCTION = 14
        POSTRAN = 15
    End Enum
    Private _ServerName As String
    Private _LicServer As String
    Private _SQLUserName As String
    Private _SQLPasswd As String
    Private _Database As String
    Private _DftSection As String = "Setting"
    Private _WMSSection As String = "WMS"
    Private _htDataBase As Hashtable
    Private _isError As Boolean
    Private _ErrMsg As String
    Private _LogDatabase As String
    Private _Logtable As String
    Private _Debug As Boolean
    Private _WMSDatabase As String
    Private _WMSServer As String
    Private _WMSDBUsername As String
    Private _WMSDBPassword As String

    Private _DBServerType As String
    Public Property DBServerType() As String
        Get
            Return _DBServerType
        End Get
        Set(ByVal value As String)
            _DBServerType = value
        End Set
    End Property

    Public Property WMSDBPassword() As String
        Get
            Return _WMSDBPassword
        End Get
        Set(ByVal value As String)
            _WMSDBPassword = value
        End Set
    End Property

    Public Property WMSDBUserName() As String
        Get
            Return _WMSDBUsername
        End Get
        Set(ByVal value As String)
            _WMSDBUsername = value
        End Set
    End Property

    Public Property WMSServer() As String
        Get
            Return _WMSServer
        End Get
        Set(ByVal value As String)
            _WMSServer = value
        End Set
    End Property

    Public Property WMSDatabase() As String
        Get
            Return _WMSDatabase
        End Get
        Set(ByVal value As String)
            _WMSDatabase = value
        End Set
    End Property


    Private _LabelReport As String

    Private _Username As String

    Private _Password As String


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

    Public Property Debug() As Boolean
        Get
            Return _Debug
        End Get
        Set(ByVal value As Boolean)
            _Debug = value
        End Set
    End Property

    Public Property LogDatabase() As String
        Get
            Return _LogDatabase
        End Get
        Set(ByVal value As String)
            _LogDatabase = value
        End Set
    End Property

    Public Property ErrMsg() As String
        Get
            Return _ErrMsg
        End Get
        Set(ByVal value As String)
            _ErrMsg = value
        End Set
    End Property

    Public Property isError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property

    Public Sub New()

        _isError = False
        Settings()

    End Sub

#Region "Property"
    Public ReadOnly Property Company() As Hashtable
        Get
            Return _htDataBase
        End Get

    End Property

    Public Property Database() As String
        Get
            Return _Database
        End Get
        Set(ByVal value As String)
            _Database = value
        End Set
    End Property

    Public Property SQLPasswd() As String
        Get
            Return _SQLPasswd
        End Get
        Set(ByVal value As String)
            _SQLPasswd = value
        End Set
    End Property

    Public Property SQLUserName() As String
        Get
            Return _SQLUserName
        End Get
        Set(ByVal value As String)
            _SQLUserName = value
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

    Public Property ServerName() As String
        Get
            Return _ServerName
        End Get
        Set(ByVal value As String)
            _ServerName = value
        End Set
    End Property

    Public ReadOnly Property SCLDBCompany() As Hashtable
        Get
            Return _htDataBase
        End Get
    End Property

#End Region

    Public Function getCompanyList() As ArrayList
        Dim _alCompList As ArrayList
        Dim _oCompany As SAPbobsCOM.Company
        _oCompany = New SAPbobsCOM.Company
        _oCompany.Server = _ServerName
        _oCompany.LicenseServer = _LicServer
        _oCompany.DbUserName = _SQLUserName
        _oCompany.DbPassword = _SQLPasswd
        _oCompany.DbServerType = _DBServerType

        Dim rs As SAPbobsCOM.Recordset = _oCompany.GetCompanyList
        _alCompList = New ArrayList

        If Not rs Is Nothing Then

            While rs.EoF = False
                _alCompList.Add(rs.Fields.Item(0).Value)
                rs.MoveNext()
            End While
        End If

        Return _alCompList
    End Function
    Public Function getValue(ByVal _Key As String) As String
        Return System.Configuration.ConfigurationManager.AppSettings(_Key).ToString
    End Function
    Private Function Settings() As Boolean

        Dim _Settings As CPSLIB.Settings.File.Files
        Try
            _Settings = New CPSLIB.Settings.File.Files(System.Environment.CurrentDirectory & "\" & SettingFiles)



            _ServerName = System.Configuration.ConfigurationManager.AppSettings("DB_Server").ToString
            _LicServer = System.Configuration.ConfigurationManager.AppSettings("B1_LicenseServer").ToString
            _SQLUserName = System.Configuration.ConfigurationManager.AppSettings("DB_UserName").ToString
            _SQLPasswd = System.Configuration.ConfigurationManager.AppSettings("DB_UserPassword").ToString
            _Database = System.Configuration.ConfigurationManager.AppSettings("DB_Name").ToString

            _Debug = (System.Configuration.ConfigurationManager.AppSettings("DEBUG").ToString = "Y")
            _Logtable = System.Configuration.ConfigurationManager.AppSettings("LOGTABLE").ToString
            _DBServerType = System.Configuration.ConfigurationManager.AppSettings("DBSERVERTYPE").ToString
            _Username = System.Configuration.ConfigurationManager.AppSettings("B1_UserName").ToString
            _Password = System.Configuration.ConfigurationManager.AppSettings("B1_UserPassword").ToString
            _WMSServer = System.Configuration.ConfigurationManager.AppSettings("WMS_Server").ToString

            _WMSDBUsername = System.Configuration.ConfigurationManager.AppSettings("WMS_DBUsername").ToString
            _WMSDBPassword = System.Configuration.ConfigurationManager.AppSettings("WMS_DBPassword").ToString
            _WMSDatabase = System.Configuration.ConfigurationManager.AppSettings("Midd_DB_Name").ToString


        Catch ex As Exception
            isError = True
            _ErrMsg = "Exception on reading setting file: " & ex.Message
        End Try
    End Function

    Public Shared Function DBNull(ByVal o As Object, Optional ByVal _dftValue As String = "") As String
        If IsDBNull(o) Then
            Return _dftValue
        Else
            If o.ToString = String.Empty Then
                Return _dftValue
            Else
                Return o.ToString
            End If

        End If

    End Function
End Class
