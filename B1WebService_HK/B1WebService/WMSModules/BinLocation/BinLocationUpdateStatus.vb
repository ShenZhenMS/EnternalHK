Public Class BinLocationUpdateStatus : Inherits WMSSQLConnections
    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections

    Public Shared TBL_BINLOCATION As String = "CPS_TBL_BINLOCATION"
    Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S' WHERE ItemCode = '{1}' AND WhsCode = '{2}' AND ToLocCode = '{3}' AND BatchNum = '{4}'"
    Dim _sqlUpdate_Err As String = "UPDATE {0} SET TrxStatus = 'F' WHERE ItemCode = '{1}' AND WhsCode = '{2}' AND ToLocCode = '{3}' AND BatchNum = '{4}'"


    Public Sub New(ByVal _Setting As Settings)

        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn

    End Sub

    Public Function UpdateSuccessStatus(ByVal _ItemCode As String, ByVal _WhsCode As String, ByVal _ToLocCode As String, ByVal _BatchNum As String) As Boolean
        UpdateSuccessStatus = True
        Try
            MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Suc, TBL_BINLOCATION, _ItemCode, _WhsCode, _ToLocCode, _BatchNum))
        Catch ex As Exception

            UpdateSuccessStatus = False
        End Try

    End Function

    Public Function UpdateErrorStatus(ByVal _ItemCode As String, ByVal _WhsCode As String, ByVal _ToLocCode As String, ByVal _BatchNum As String) As Boolean
        UpdateErrorStatus = True
        Try
            MyBase.ExecuteUpdate(String.Format(_sqlUpdate_Err, TBL_BINLOCATION, _ItemCode, _WhsCode, _ToLocCode, _BatchNum))
        Catch ex As Exception

            UpdateErrorStatus = False

        End Try

    End Function

End Class
