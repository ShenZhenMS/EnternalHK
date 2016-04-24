Public Class MasterForWMS : Inherits WMSSQLConnections


    Dim _Setting As Settings
 

    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlBatchLog As String = "Insert into CPS_TBL_BatchInfo select getdate(),'{0}','{1}','{2}','{3}','{4}'"
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogPurchase '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}',{15},{16},'{17}','{18}','{19}','{20}','{21}'"
    Dim _sqlOSLOGTABLE As String = "SELECT * FROM CPS_TBL_OPOR WHERE isNull(TrxStatus,'') not in ('E','F') and 1 = 1 "
    Dim _sqlPurchaseResult As String = "SELECT * FROM CPS_FUNC_PURCHASERESULT('{0}')"



    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting)
        Me._Setting = _Setting

        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToBatchTable(ByVal _ItemCode As String, ByVal _BatchNum As String, ByVal _WhsCode As String, ByVal _MfrDate As String, ByVal _ExpDate As String) As Boolean
        Try
            MyBase.ExecuteUpdate(String.Format(_sqlBatchLog, _ItemCode.Replace("'", "''"), _BatchNum.Replace("'", "''"), _WhsCode.Replace("'", "''"), _MfrDate.Replace("'", "''"), _ExpDate.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return True
    End Function

   

    'Private Function ToPurchaseTable(ByVal _dr As DataRow) As Boolean

    '    Dim _ExpireDate As String
    '    Dim _MfrDate As String
    '    Dim _sql As String
    '    Try
    '        _ExpireDate = "'" & Convert.ToDateTime(_dr("ExpireDate")).ToString("yyyyMMdd") & "'"

    '    Catch ex As Exception
    '        _ExpireDate = "NULL"
    '    End Try

    '    Try
    '        _MfrDate = "'" & Convert.ToDateTime(_dr("MfrDate")).ToString("yyyyMMdd") & "'"

    '    Catch ex As Exception
    '        _MfrDate = "NULL"
    '    End Try



    '    Try
    '        _sql = String.Format("INSERT INTO [dbo].[CPS_TBL_OPOR] " & _
    '                             "([DocEntry] " & _
    '                             ",[LineNum] " & _
    '                             ",[DocNum]" & _
    '                             ",[DocDueDate]" & _
    '                             ",[CardCode]" & _
    '                             ",[CardName]" & _
    '                             ",[ItemCode]" & _
    '                             ",[OldItemCode] " & _
    '                             ",[ItemName]" & _
    '                             ",[Quantity]" & _
    '                             ",[UOM]" & _
    '                             ",[UnitPrice]" & _
    '                             ",[WhsCode]" & _
    '                             ",[WhsName]" & _
    '                             ",[BatchNumber]" & _
    '                             ",[ExpireDate]" & _
    '                             ",[MfrDate]" & _
    '                             ",[Barcode]" & _
    '                             ",[ReceiveEntry]" & _
    '                             ",[ReceiveLine]" & _
    '                             ",[TgtEntry]" & _
    '                             ",[TgtNum]" & _
    '                             ",[ErrCode]" & _
    '                             ",[ErrDscr]" & _
    '                             ",[TrtCreateDate]" & _
    '                             ",[CreateDate]" & _
    '                             ",[LastRunDate]" & _
    '                             ",[TrxStatus]) " & _
    '                             "VALUES " & _
    '                            "('{0}'" & _
    '                            ",'{1}' " & _
    '                            ",'{2}'" & _
    '                            ",'{3}'" & _
    '                            ",'{4}'" & _
    '                            ",'{5}'" & _
    '                            ",'{6}'" & _
    '                            ",'{7}'" & _
    '                            ",'{8}'" & _
    '                            ",'{9}'" & _
    '                            ",'{10}'" & _
    '                            ",'{11}'" & _
    '                            ",'{12}'" & _
    '                            ",'{13}'" & _
    '                            ",'{14}'" & _
    '                            ",{15}" & _
    '                            ",{16}" & _
    '                            ",'{17}'" & _
    '                            ",'{18}'" & _
    '                            ",'{19}'" & _
    '                            ",null" & _
    '                            ",null" & _
    '                            ",null" & _
    '                            ",null" & _
    '                            ",getdate()" & _
    '                            ",getdate()" & _
    '                            ",getdate()" & _
    '                            ",'F')",
    '                            _dr(Fld_DocEntry), _
    '                            _dr(Fld_LineNum), _
    '                            _dr(Fld_DocNum), _
    '                            Convert.ToDateTime(_dr(Fld_DocDueDate)).ToString("yyyyMMdd"), _
    '                            _dr(Fld_CardCode), _
    '                            _dr(Fld_CardName), _
    '                            _dr(Fld_ItemCode), _
    '                            _dr(Fld_OldItemCode),
    '                            _dr(Fld_ItemName), _
    '                            _dr(Fld_Quantity), _
    '                            _dr(Fld_UOM), _
    '                            _dr(Fld_UnitPrice), _
    '                            _dr(Fld_WhsCode), _
    '                            _dr(Fld_WhsName), _
    '                            _dr(Fld_BatchNumber), _
    '                            _ExpireDate, _
    '                            _MfrDate, _
    '                            _dr(Fld_BarCode), _
    '                            _dr(Fld_ReceiveEntry),
    '                            _dr(Fld_ReceiveLine)
    '                            )
    '        MyBase.ExecuteUpdate(_sql)

    '        If MyBase.isError Then

    '            Throw New Exception(MyBase.Message)
    '        End If

    '    Catch ex As Exception

    '        _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
    '        Return False
    '    End Try
    '    Return True
    'End Function

    
  
   


End Class
