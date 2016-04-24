Public Class PurchaseForWMS : Inherits Purchase

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections

   
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlCreateHist As String = "Exec CPS_Proc_LogPurchase '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}',{15},{16},'{17}','{18}','{19}','{20}','{21}'"
    Dim _sqlOSLOGTABLE As String = "SELECT * FROM CPS_TBL_OPOR WHERE isNull(TrxStatus,'') not in ('E','F') and 1 = 1 "
    Dim _sqlPurchaseResult As String = "SELECT * FROM CPS_FUNC_PURCHASERESULT('{0}')"



    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting, _SAPDIConn)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function ToPurchaseTable(ByVal _dt As DataTable) As Boolean
        _Debug.WriteTable(_dt, "Purchase Table From XML")
        Dim _ret As Boolean = True
        Try
            For Each _dr As DataRow In _dt.Rows
                If ToPurchaseTable(_dr) = False Then

                End If
            Next
        Catch ex As Exception
            _ret = False
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _ret
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

    Public Function OSPurchaseOrder() As DataTable
        Dim _dt As DataTable = Nothing

        Try

            _dt = MyBase.ExecuteDatatable(_sqlOSLOGTABLE)

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _dt
    End Function

    Public Function PurchaseResult(ByVal _ReceiveEntry As String) As DataTable
        Dim _dt As DataTable = Nothing

        Try

            _dt = MyBase.ExecuteDatatable(String.Format(_sqlPurchaseResult, _ReceiveEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
        End Try
        Return _dt
    End Function

    '--------------MK Development-------------------------
    Private Function ToPurchaseTable(ByVal _dr As DataRow) As Boolean

        Dim _ExpireDate As String
        Dim _MfrDate As String
        Dim _sql As String
        Try
            _ExpireDate = "'" & Convert.ToDateTime(_dr("ExpireDate")).ToString("yyyyMMdd") & "'"

        Catch ex As Exception
            _ExpireDate = "NULL"
        End Try

        Try
            _MfrDate = "'" & Convert.ToDateTime(_dr("MfrDate")).ToString("yyyyMMdd") & "'"

        Catch ex As Exception
            _MfrDate = "NULL"
        End Try



        Try
            _sql = String.Format("INSERT INTO [dbo].[CPS_TBL_OPOR] " & _
                                 "([DocEntry] " & _
                                 ",[LineNum] " & _
                                 ",[DocNum]" & _
                                 ",[DocDueDate]" & _
                                 ",[CardCode]" & _
                                 ",[CardName]" & _
                                 ",[ItemCode]" & _
                                 ",[OldItemCode] " & _
                                 ",[ItemName]" & _
                                 ",[Quantity]" & _
                                 ",[UOM]" & _
                                 ",[UnitPrice]" & _
                                 ",[WhsCode]" & _
                                 ",[WhsName]" & _
                                 ",[BatchNumber]" & _
                                 ",[ExpireDate]" & _
                                 ",[MfrDate]" & _
                                 ",[Barcode]" & _
                                 ",[ReceiveEntry]" & _
                                 ",[ReceiveLine]" & _
                                 ",[TgtEntry]" & _
                                 ",[TgtNum]" & _
                                 ",[ErrCode]" & _
                                 ",[ErrDscr]" & _
                                 ",[TrtCreateDate]" & _
                                 ",[CreateDate]" & _
                                 ",[LastRunDate]" & _
                                 ",[TrxStatus] " & _
                                 ",[DraftEntry] " & _
                                 ",[DraftLIne] " & _
                                 ",[ASNNum] " & _
                                 ",[WMSUser], [Remark]) " & _
                                 "VALUES " & _
                                "('{0}'" & _
                                ",'{1}' " & _
                                ",'{2}'" & _
                                ",'{3}'" & _
                                ",'{4}'" & _
                                ",'{5}'" & _
                                ",'{6}'" & _
                                ",'{7}'" & _
                                ",'{8}'" & _
                                ",'{9}'" & _
                                ",'{10}'" & _
                                ",'{11}'" & _
                                ",'{12}'" & _
                                ",'{13}'" & _
                                ",'{14}'" & _
                                ",{15}" & _
                                ",{16}" & _
                                ",'{17}'" & _
                                ",'{18}'" & _
                                ",'{19}'" & _
                                ",null" & _
                                ",null" & _
                                ",null" & _
                                ",null" & _
                                ",getdate()" & _
                                ",getdate()" & _
                                ",getdate()" & _
                                ",'', '{20}','{21}','{22}','{23}','{24}')",
                                _dr(Fld_DocEntry), _
                                _dr(Fld_LineNum), _
                                _dr(Fld_DocNum), _
                                Convert.ToDateTime(_dr(Fld_DocDueDate)).ToString("yyyyMMdd"), _
                                _dr(Fld_CardCode), _
                                _dr(Fld_CardName), _
                                _dr(Fld_ItemCode), _
                                _dr(Fld_OldItemCode),
                                _dr(Fld_ItemName), _
                                _dr(Fld_BatchQuantity), _
                                _dr(Fld_UOM), _
                                _dr(Fld_UnitPrice), _
                                _dr(Fld_WhsCode), _
                                _dr(Fld_WhsName), _
                                _dr(Fld_BatchNumber), _
                                _ExpireDate, _
                                _MfrDate, _
                                _dr(Fld_BarCode), _
                                _dr(Fld_ReceiveEntry),
            _dr(Fld_ReceiveLine), _dr(Fld_DraftEntry), _dr(Fld_DraftLine), _dr(Fld_ASNNum), _dr(Fld_WMSUser), _dr(Fld_Remark).ToString.Replace("'", "''")
                                )
            _Debug.Write(_sql)
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
