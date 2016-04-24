Public Class BinLocationForWMS : Inherits WMSSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogBinLocationTransfer '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}'"

    Dim _sqlReadHist As String = "SELECT * FROM {0} WHERE {1} = '{2}'"

    Public Shared Fld_ItemCode As String = "ItemCode"
    Public Shared Fld_WhsCode As String = "WhsCode"
    Public Shared Fld_FromLocCode As String = "FromLocCode"
    Public Shared Fld_ToLocCode As String = "ToLocCode"
    Public Shared Fld_BatchNum As String = "BatchNum"
    Public Shared Fld_Quantity As String = "Quantity"
    Public Shared Fld_User As String = "User"
    Public Shared Fld_ReceiveEntry As String = "ReceiveEntry"
    Public Shared Fld_ReceiveLineNum As String = "ReceiveLineNum"
    Public Shared TBL_NAME As String = "CPS_TBL_BINLOCATION"

    Public Sub New(ByVal _Setting As Settings)

        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function BinLocationTransferList(ByVal _ReceiveEntry As String) As DataTable
        Dim _sql As String = _sqlReadHist
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sql, _ReceiveEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function ToBinLocationTransferTable(ByVal _dt As DataTable) As Boolean
        Try
            For Each _dr As DataRow In _dt.Rows
                ToBinLocationTransferTable(_dr)
            Next
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Function

    Private Function ToBinLocationTransferTable(ByVal _dr As DataRow) As Boolean

        'Dim _ExpireDate As String
        'Dim _MfrDate As String
        Dim _sql As String

        'Try
        '    _ExpireDate = Convert.ToDateTime(_dr("ExpireDate")).ToString("yyyyMMdd")
        'Catch ex As Exception
        '    _ExpireDate = "NULL"
        'End Try

        'Try
        '    _MfrDate = Convert.ToDateTime(_dr("MfrDate")).ToString("yyyyMMdd")

        'Catch ex As Exception
        '    _MfrDate = "NULL"
        'End Try
        '@ItemCode nvarchar(20),
        '@WhsCode nvarchar(8),
        '@FromLocCode nvarchar(100),
        '@ToLocCode nvarchar(100),
        '@BatchNum nvarchar(50),
        '@Quantity numeric(19,6),
        '@ReceiveEntry nvarchar(100),
        '@ReceiveLineNum int,
        '@User		nvarchar(50)
        Try
            _sql = String.Format(_sqlCreateHist,
                                        _dr(Fld_ItemCode), _
                                       _dr(Fld_WhsCode), _
                                       _dr(Fld_FromLocCode), _
                                       _dr(Fld_ToLocCode), _
                                       _dr(Fld_BatchNum), _
                                       _dr(Fld_Quantity), _
                                       _dr(Fld_ReceiveEntry), _
                                       _dr(Fld_ReceiveLineNum), _
                                       _dr(Fld_User).Replace("'", "''")
                                       )
            MyBase.ExecuteUpdate(_sql)
            If MyBase.isError Then
                Throw New Exception(MyBase.Message)
            End If
            'SQL_SP(_Server, _Database, _DBUserName, _DBPassword, _
            '                String.Format(_sql, _
            'dr("DocEntry"), _
            'dr("LineNum"), _
            'dr("DocNum"), _
            'Convert.ToDateTime(dr("DocDueDate")).ToString("yyyyMMdd"), _
            'Convert.ToDateTime(dr("DocDueDate")).ToString("yyyyMMdd"), _
            'dr("CardCode"), _
            'dr("CardName"), _
            'dr("NumAtCard"), _
            'dr("ItemCode"), _
            'dr("ItemName"), _
            'dr("Quantity"), _
            'dr("UOM"), _
            'dr("WhsCode"), _
            'dr("WhsName"), _
            'dr("BatchNumber"), _
            '_ExpireDate, _
            '_MfrDate, _
            'dr("BottleBarCode"), _
            'dr("CartonBarCode"), _
            'dr("AlcoholLv"), _
            'dr("LocCode"), _
            'dr("ReceiveEntry")))

        Catch ex As Exception

            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)

            Return False
        End Try
        Return True
    End Function

End Class
