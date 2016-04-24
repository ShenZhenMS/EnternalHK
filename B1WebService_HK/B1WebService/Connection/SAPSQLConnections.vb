Public Class SAPSQLConnections : Inherits CPSLIB.Data.Connection.SQLServerInfo
    
    Dim _Setting As Settings
    Dim _SQLConfig As String = "SELECT * FROM [@WMSCONFIG] where U_FUNCCODE = '{0}'"
    Dim _SQLModule As String = "SELECT * FROM [@WMSMODULE] WHERE CODE = '{0}'"
    Dim _sqlAcctDR As String = "SELECT * FROM OACT WHERE ACCTCODE = '{0}'"
    Dim _SQLBatchItem As String = "SELECT 1 FROM OITM WHERE ManBtchNum = 'Y' AND ITEMCODE = '{0}'"

    Public Shared Fld_Config_Code As String = "Code"
    Public Shared Fld_Config_Active As String = "U_Active"
    Public Shared Fld_Module_Code As String = "U_FuncCode"
    Public Shared Fld_Module_FlowCode As String = "U_FlowCode"
    Public Shared Fld_Module_FlowDesc As String = "U_FlowDesc"
    Public Shared Fld_Module_Value As String = "U_Value"

    Public Shared Fld_Dim1Relvnt As String = "Dim1Relvnt"
    Public Shared Fld_Dim2Relvnt As String = "Dim2Relvnt"
    Public Shared Fld_Dim3Relvnt As String = "Dim3Relvnt"
    Public Shared Fld_Dim4Relvnt As String = "Dim4Relvnt"
    Public Shared Fld_Dim5Relvnt As String = "Dim5Relvnt"


    Public Shared Fld_OverCode1 As String = "OverCode"
    Public Shared Fld_OverCode2 As String = "OverCode2"
    Public Shared Fld_OverCode3 As String = "OverCode3"
    Public Shared Fld_OverCode4 As String = "OverCode4"
    Public Shared Fld_OverCode5 As String = "OverCode5"
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException


    Public Sub New(ByVal _Setting As Settings)
        MyBase.New(_Setting.ServerName, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Database)
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub

    Public Function isBatchItem(ByVal _ItemCode As String) As Boolean
        isBatchItem = MyBase.Exists(String.Format(_SQLBatchItem, _ItemCode.Replace("'", "''")))
    End Function

    Public Function WMSConfig(ByVal ModuleCode As String) As DataTable
        Try
            WMSConfig = MyBase.ExecuteDatatable(String.Format(_SQLModule, ModuleCode))
        Catch ex As Exception
            WMSConfig = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod.Name)
        End Try

    End Function

    Public Function WMSModule(ByVal ModuleCode As String) As DataTable
        Try
            WMSModule = MyBase.ExecuteDatatable(String.Format(_SQLConfig, ModuleCode))
        Catch ex As Exception
            WMSModule = Nothing
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod.Name)
        End Try

    End Function

    Public Function GetPurchaseItemPerBaseUnit(ByVal _ItemCode As String) As Double
        Dim _ret As Double = 1
        Dim _sql As String = "SELECT isNull(NumInBuy,1) From OITM where ItemCode = '{0}'"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GetSalesItemPerBaseUnit(ByVal _ItemCode As String) As Double
        Dim _ret As Double = 1
        Dim _sql As String = "SELECT isNull(NumInSale,1) From OITM where ItemCode = '{0}'"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GetSODeliveryDate(ByVal _DocEntry As String) As Date
        Dim _ret As Date
        Dim _sql As String = "SELECT DOCDUEDATE FROM ORDR WHERE DOCENTRY = {0}"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _DocEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function AllowBackDate(ByVal _DocEntry As String) As Boolean
        Dim _ret As Boolean = False
        Dim _sql As String = "Select isNull(U_AllowBackDate,'N') From ORDR where DocEntry = '{0}' and isNull(U_AllowBackDate,'N') = 'Y'"
        Try
            _ret = MyBase.Exists(String.Format(_sql, _DocEntry))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GetSODocDate(ByVal _DocEntry As String) As Date
        Dim _ret As Date
        Dim _sql As String = "SELECT DocDate FROM ORDR WHERE DOCENTRY = {0}"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _DocEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function GetSODueDate(ByVal _DocEntry As String) As Date
        Dim _ret As Date
        Dim _sql As String = "SELECT DocDueDate FROM ORDR WHERE DOCENTRY = {0}"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _DocEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function GetSOTaxDate(ByVal _DocEntry As String) As Date
        Dim _ret As Date
        Dim _sql As String = "SELECT TaxDate FROM ORDR WHERE DOCENTRY = {0}"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _DocEntry))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function GetUDF(ByVal _tblName As String) As Hashtable
        Dim _ht As New Hashtable
        Dim _sql As String = "select 'U_' + AliasID as AliasID,TypeID from CUFD where TableID = '{0}'"
        Try
            _ht = MyBase.ExecuteHashTable("AliasID", String.Format(_sql, _tblName))

        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ht
    End Function

    Public Function GetSeries(ByVal _DocEntry As String, ByVal _TBLName As String) As String
        Dim _ret As String
        Dim _tgtSeries As String
        Dim _sql As String = "SELECT Series FROM {1} WHERE DOCENTRY = {0}"
        Dim _sqlTgt As String = "SELECT SERIES FROM CPS_FUNC_GETITSERIES('{0}','{1}')"
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _DocEntry, _TBLName))
            If _ret <> "" Then
                _tgtSeries = MyBase.ExecuteValue(String.Format(_sqlTgt, _ret, DateTime.Now.ToString("yyyyMMdd")))
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _tgtSeries
    End Function




    Public Function GetCOADefaultDR(ByVal _AcctCode As String) As Hashtable
        Dim _ht As Hashtable = New Hashtable
        Dim _dt As DataTable
        Try
            _Debug.Write(String.Format(_sqlAcctDR, _AcctCode.Replace("'", "''")))
            _dt = MyBase.ExecuteDatatable(String.Format(_sqlAcctDR, _AcctCode.Replace("'", "''")))
            If _dt.Rows.Count > 0 Then
                _Debug.WriteTable(_dt, "OACT")
                _ht.Add(Fld_Dim1Relvnt, IIf(_dt.Rows(0)(Fld_Dim1Relvnt) = "Y", _dt.Rows(0)(Fld_OverCode1), ""))
                _ht.Add(Fld_Dim2Relvnt, IIf(_dt.Rows(0)(Fld_Dim2Relvnt) = "Y", _dt.Rows(0)(Fld_OverCode2), ""))
                _ht.Add(Fld_Dim3Relvnt, IIf(_dt.Rows(0)(Fld_Dim3Relvnt) = "Y", _dt.Rows(0)(Fld_OverCode3), ""))
                _ht.Add(Fld_Dim4Relvnt, IIf(_dt.Rows(0)(Fld_Dim4Relvnt) = "Y", _dt.Rows(0)(Fld_OverCode4), ""))
                _ht.Add(Fld_Dim5Relvnt, IIf(_dt.Rows(0)(Fld_Dim5Relvnt) = "Y", _dt.Rows(0)(Fld_OverCode5), ""))
            Else
                _ht.Add(Fld_Dim1Relvnt, "")
                _ht.Add(Fld_Dim2Relvnt, "")
                _ht.Add(Fld_Dim3Relvnt, "")
                _ht.Add(Fld_Dim4Relvnt, "")
                _ht.Add(Fld_Dim5Relvnt, "")
            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ht
    End Function
End Class

