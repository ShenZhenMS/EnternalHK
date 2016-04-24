Imports SBODI_Server
Imports CPSLIB.XML.XMLNodeList

Namespace DIServer
    Public Class DIServerConnection : Inherits DIServer.Core
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _connected As Boolean
        Private _Server As String
        Private _LicenseServer As String
        Private _CompanyDB As String
        Private _DBUserName As String
        Private _DBPassword As String
        Private _UserName As String
        Private _Password As String
        Private _Language As String
        Private _DBServerType As SAPbobsCOM.BoDataServerTypes
        Private _SessionID As String
        Public Sub New(ByVal _Server As String, ByVal _LicenseServer As String, ByVal _CompanyDB As String, ByVal _DBUsername As String, ByVal _DBPassword As String, _
                       ByVal _Username As String, ByVal _Password As String, ByVal _DBServerType As DataInterface.Company.DICompany.DataBaseType)
            MyBase.New("Connection")
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            Me._Server = _Server
            Me._LicenseServer = _LicenseServer
            Me._CompanyDB = _CompanyDB
            Me._DBUserName = _DBUsername
            Me._DBPassword = _DBPassword
            Me._UserName = _Username
            Me._Password = _Password
            Me._DBServerType = _DBServerType

            If String.IsNullOrEmpty(SessionID) Then
                Login()
            End If
        End Sub
        Public Function Login() As CommandStatus
            Dim _Lang As String
            If Data.Validation.IsNull(_Language) = String.Empty Then
                _Lang = "ln_English"
            Else
                _Lang = _Language
            End If
            SetCommandString(String.Format(RequestLoginXML, xmlns, _Server, _CompanyDB, [Enum].GetName(_DBServerType.GetType, _DBServerType), _DBUserName, _DBPassword, _UserName, _Password, _Lang, _LicenseServer))
            _Debug.Write(String.Format(RequestLoginXML, xmlns, _Server, _CompanyDB, [Enum].GetName(_DBServerType.GetType, _DBServerType), _DBUserName, _DBPassword, _UserName, _Password, _Lang, _LicenseServer))
            MyBase.Execute()
            If MyBase.CmdStatus = CommandStatus.Fail Then
                _Debug.Write("DI Server Connection Fail")
                _Debug.Write(CmdMessage)
            Else
                SessionID = ResponseElement.InnerText
            End If
            Return MyBase.CmdStatus
        End Function

        Public Function Logout() As Boolean
            SetCommandString(String.Format(RequestLogoutXML, xmlns))
            Return MyBase.Execute
        End Function

        Private Function GetSessionID() As String
            Dim _ret As String = String.Empty

            Return _ret
        End Function

#Region "Property"
        Public Property DBServerType() As SAPbobsCOM.BoDataServerTypes
            Get
                Return _DBServerType
            End Get
            Set(ByVal value As SAPbobsCOM.BoDataServerTypes)
                _DBServerType = value
            End Set
        End Property

        Public Property Language() As String
            Get
                Return _Language
            End Get
            Set(ByVal value As String)
                _Language = value
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

        Public Property UserName() As String
            Get
                Return _UserName
            End Get
            Set(ByVal value As String)
                _UserName = value
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

        Public Property CompanyDB() As String
            Get
                Return _CompanyDB
            End Get
            Set(ByVal value As String)
                _CompanyDB = value
            End Set
        End Property

        Public Property LicenseServer() As String
            Get
                Return _LicenseServer
            End Get
            Set(ByVal value As String)
                _LicenseServer = value
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

#End Region
    End Class
End Namespace
