Imports System.IO
Namespace CPSLIB.Settings
    Public Class MainSetting
        'Setting Type
        Public Enum _Method
            None = 0
            File = 1
            SQLServer = 2
            XML = 3
            Other = 4

        End Enum
        'Library Operation
#Region "Ascii File"
        Private _CreateFileWhenNotExists As Boolean = False
#End Region
#Region "Message"
        Private _MessageReadFromDatabase As Boolean = False
        Private _MessageReadFromXML As Boolean = False
        'Message Database
        Private _MessageServerName As String = ""
        Private _MessageDatabase As String = ""
        Private _MessageLogin As String = ""
        Private _MessagePassword As String = ""
        'Message XML/Ascii File
        Private _MessageFilePath As String = ""

#End Region
#Region "Logging"

#End Region
#Region "SQL Connection"

        Private _SQLConnectionDatabase As String = ""
        Private _SQLConnectionServer As String = ""
        Private _SQLConnectionLoginID As String = ""
        Private _SQLConnectionPassword As String = ""
        Private _SQLConnectionPooling As Boolean = CPSLIB.Data.Connection.Consts.DEFAULT_Pooling
        Private _SQLConnectionPresistentSecurityInfo As Boolean = CPSLIB.Data.Connection.Consts.DEFAULT_PersisSecurityInfo

#End Region
#Region "Company"
        Private _DICompany As ArrayList

#End Region
#Region "Constructor"
        Public Sub New()

        End Sub
        ''' <summary>
        ''' Files, XML AND Database From File
        ''' </summary>
        ''' <param name="SettingMethod"></param>
        ''' <param name="strSettingFile"></param>
        ''' <remarks></remarks>
        ''' 
        Public Sub New(ByVal SettingMethod As MainSetting._Method, ByVal strSettingFile As String)


        End Sub
        Public Sub New(ByVal SettingMethod As MainSetting._Method, ByVal DataBaseInfo As Object)
            Select Case SettingMethod
                Case _Method.SQLServer
                    'DataBaseInfo = CType(DataBaseInfo, CPSLIB.Data.Connection.SQLServerInfo)

                Case _Method.Other
            End Select
        End Sub
#End Region
#Region "Property"
        Public WriteOnly Property setDICompany() As CPSLIB.DataInterface.Company.DICompany
            Set(ByVal value As CPSLIB.DataInterface.Company.DICompany)
                If _DICompany Is Nothing Then
                    _DICompany = New ArrayList
                End If
                _DICompany.Add(value)
            End Set
        End Property

        Public ReadOnly Property DICompanys() As ArrayList
            Get
                Return _DICompany
            End Get

        End Property
        Public Property MessageReadFromDatabase() As Boolean
            Get
                Return _MessageReadFromDatabase
            End Get
            Set(ByVal value As Boolean)
                _MessageReadFromDatabase = value
            End Set
        End Property
        Public Property MessageReadFromXML() As Boolean
            Get
                Return _MessageReadFromXML
            End Get
            Set(ByVal value As Boolean)
                _MessageReadFromXML = value
            End Set
        End Property
        Public Property MessageSQLServer() As String
            Get
                Return _MessageServerName
            End Get
            Set(ByVal value As String)
                _MessageServerName = value
            End Set
        End Property
        Public Property MessageSQLDatabase() As String
            Get
                Return _MessageDatabase
            End Get
            Set(ByVal value As String)
                _MessageDatabase = value
            End Set
        End Property
        Public Property MessageSQLLogin() As String
            Get
                Return _MessageLogin
            End Get
            Set(ByVal value As String)
                _MessageLogin = value
            End Set
        End Property
        Public Property MessageSQLPassword() As String
            Get
                Return _MessagePassword
            End Get
            Set(ByVal value As String)
                _MessagePassword = value
            End Set
        End Property
        Public Property MessageFilePath() As String
            Get
                Return _MessageFilePath
            End Get
            Set(ByVal value As String)
                _MessageFilePath = value
            End Set
        End Property


#Region "SQL Connection"
        Public Property SQLConnectionServer() As String
            Get
                Return _SQLConnectionServer
            End Get
            Set(ByVal value As String)
                _SQLConnectionServer = value
            End Set
        End Property
        Public Property SQLConnectionDatabase() As String
            Get
                Return _SQLConnectionDatabase
            End Get
            Set(ByVal value As String)
                _SQLConnectionDatabase = value
            End Set
        End Property
        Public Property SQLConnectionLoginID() As String
            Get
                Return _SQLConnectionLoginID
            End Get
            Set(ByVal value As String)
                _SQLConnectionLoginID = value
            End Set
        End Property

#End Region
#Region "Ascii File"
        Public Property CreateFileWhenNotExist() As Boolean
            Get
                Return _CreateFileWhenNotExists
            End Get
            Set(ByVal value As Boolean)
                _CreateFileWhenNotExists = value
            End Set
        End Property
#End Region
#End Region

    End Class
End Namespace
