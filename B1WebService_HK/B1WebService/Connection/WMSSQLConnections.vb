Public Class WMSSQLConnections : Inherits CPSLIB.Data.Connection.SQLServerInfo

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _Setting As Settings
    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting.WMSServer, _Setting.WMSDBUserName, _Setting.WMSDBPassword, _Setting.WMSDatabase)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._Setting = _Setting
    End Sub

End Class
