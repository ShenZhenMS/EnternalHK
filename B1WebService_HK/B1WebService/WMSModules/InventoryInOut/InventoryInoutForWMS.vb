Public Class InventoryInoutForWMS : Inherits Inventory_Inout

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogInventoryInout '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}'"
    Dim _sqlInventoryTransResult As String = "SELECT * FROM CPS_FUNC_INVENTORYInOutRESULT('{0}')"

    Public Enum _DocumentType
        GI = 1
        GR = 2
    End Enum

    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections, ByVal _DocType As _DocumentType)

        MyBase.New(_Setting, _SAPDIConn, _DocType)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToWMSTable(ByVal _dt As DataTable) As Boolean

        Try
            For Each _dr As DataRow In _dt.Rows
                ToWMSTable(_dr)
            Next
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try

    End Function

    Private Function ToWMSTable(ByVal _dr As DataRow) As Boolean

        Dim _sql As String
        Dim _docentry As String
        Dim _linenum As String
        If IsDBNull(_dr(Fld_DocEntry)) Or _dr(Fld_DocEntry).ToString = String.Empty Then
            _docentry = "-1"
        Else
            _docentry = _dr(Fld_DocEntry)
        End If
        If IsDBNull(_dr(Fld_LineNum)) Or _dr(Fld_LineNum).ToString = String.Empty Then
            _linenum = "-1"
        Else
            _linenum = _dr(Fld_LineNum)
        End If
        Try
            _sql = String.Format(_sqlCreateHist,
                                        Settings.DBNull(_docentry, "-1"), _
                                       Settings.DBNull(_linenum, "-1"), _
                                       Settings.DBNull(_dr(Fld_DocType)), _
                                       Settings.DBNull(_dr(Fld_ItemCode)), _
                                       Convert.ToDateTime(Settings.DBNull(_dr(Fld_DocDate), "1970-01-01")).ToString("yyyyMMdd"), _
                                       Settings.DBNull(_dr(Fld_Quantity), "-1"), _
                                       Settings.DBNull(_dr(Fld_UOM)), _
                                       Settings.DBNull(_dr(Fld_WhsCode)), _
                                       Settings.DBNull(_dr(Fld_BatchNum)), _
                                       Settings.DBNull(_dr(Fld_WMSUser)), _
                                    Settings.DBNull(_dr(Fld_ReceiveEntry)), _
                                       Settings.DBNull(_dr(Fld_ReceiveLineNum), "-1") _
                                       )


            MyBase.ExecuteUpdate(_sql)
            If MyBase.isError Then

                Throw New Exception(MyBase.Message)

            End If
        Catch ex As Exception

            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            Return False
        End Try
        Return True
    End Function

    Public Function InventoryInOutResult(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlInventoryTransResult, _ReceiveEntry))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _dt
    End Function
End Class
