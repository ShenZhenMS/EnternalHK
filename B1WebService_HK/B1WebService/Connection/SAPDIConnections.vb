Public Class SAPDIConnections : Inherits CPSLIB.DataInterface.Company.DICompany

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _Setting As Settings
    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting.ServerName, _Setting.Database, _Setting.LicServer, _Setting.Username, _Setting.Password, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.DBServerType)
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

End Class
