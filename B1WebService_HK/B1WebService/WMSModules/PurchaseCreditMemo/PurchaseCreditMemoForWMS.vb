Public Class PurchaseCreditMemoForWMS : Inherits PurchaseCreditMemo

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections


    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogReturn '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}'"


    Public Enum _DocumentType
        SR = 1
        PR = 2
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
       
        Try


            _sql = String.Format(_sqlCreateHist,
                                Settings.DBNull(_dr(Fld_DocType)), _
                                Settings.DBNull(_dr(Fld_CardCode), ""), _
                                Settings.DBNull(_dr(Fld_DocEntry), "-1"), _
                                Settings.DBNull(_dr(Fld_LineNum), "-1"), _
                    Settings.DBNull(_dr(Fld_ItemCode)), _
                                Convert.ToDateTime(Settings.DBNull(_dr(Fld_DocDate), "1970-01-01")).ToString("yyyyMMdd"), _
                                Settings.DBNull(_dr(Fld_Quantity), "0"), _
                                Settings.DBNull(_dr(Fld_UOM)), _
                                Settings.DBNull(_dr(Fld_WhsCode)), _
                                Settings.DBNull(_dr(Fld_BatchNum)), _
                                Settings.DBNull(_dr(Fld_WMSUser)), _
                                Settings.DBNull(_dr(Fld_ReceiveEntry)), _
                                Settings.DBNull(_dr(Fld_ReceiveLineNum), "-1"), _
                                Settings.DBNull(_dr(Fld_ReasonCode)), Settings.DBNull(_dr(Fld_isDamage), "N"), Settings.DBNull(_dr(Fld_isProblem), "N"), Settings.DBNull(_dr(Fld_BaseEntry), "-1"), Settings.DBNull(_dr(Fld_BaseLine), "-1"))

           



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

   
End Class
