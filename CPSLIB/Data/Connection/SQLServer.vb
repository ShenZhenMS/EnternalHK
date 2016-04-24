Namespace Data.Connection
    Public Class SQLServerInfo : Inherits Data.Connection.MSSQLClient

        Private _Debug As CPSLib.Debug
        Private _ServerName As String = ""
        Private _LoginID As String = ""
        Private _Password As String = ""
        Private _Database As String = ""

#Region "Consturctor"
        Public Sub New(ByVal ServerName As String, ByVal LoginID As String, ByVal Password As String, ByVal Database As String)
            MyBase.New(ServerName, Database, LoginID, Password)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _Debug.Write("Check Point A")
            _ServerName = ServerName
            _LoginID = LoginID
            _Password = Password
            _Database = Database
            _Debug.Write("Check Point B")
            MyBase.Connect()
            _Debug.Write("Check Point C")
            _Debug.Write(_isConnected, "Connected", CPSLIB.Debug.LineType.Information)
        End Sub
#End Region
    End Class
End Namespace
