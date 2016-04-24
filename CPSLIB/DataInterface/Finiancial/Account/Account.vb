Imports CPSLIB.DataInterface.Company
Namespace DataInterface.Finiancial.Account
    Public Class SAPAccount : Inherits Data.Connection.MSSQLClient
        Private _CPSException As CPSException
        Private _Debug As CPSLIB.Debug
        Private _DICompany As DICompany
        Private _ExchangeRateAdjustmentField As String = "ExportCode"

        Public Shared ConditionQuery As String = "SELECT {0} FROM OACT WHERE {1} = '{2}'"
        Public Shared AccountQuery As String = "SELECT {0} FROM OACT WHERE 1 = 1 {1} {2}"
        Public Shared ExchangeRatesAdjustmentQuery As String = "SELECT {0},{1} FROM OACT"
        Public Shared ExternalCode As String = "ACCNTNTCOD"
        Public Shared AccountCode As String = "ACCTCODE"
        Public Shared AcctName As String = "AcctName"
        Public Shared ActiveCondition As String = ""

        Private _htAcctCodeList As Hashtable



        Public Sub New(ByVal _ServerName As String, ByVal _LoginID As String, ByVal _Password As String, ByVal _Database As String)
            MyBase.New(_ServerName, _Database, _LoginID, _Password)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _htAcctCodeList = New Hashtable

        End Sub

        Public Function ExternalAccountMap() As Hashtable
            Dim _htExternalCode As New Hashtable
            Dim _dtExternalCode As DataTable
            MyBase.Connect()

            _Debug.Write(String.Format(AccountQuery, ExternalCode & "," & AccountCode, String.Format(" And isNull({0},'') <> '' ", ExternalCode), String.Empty), "Query", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            _dtExternalCode = MyBase.ExecuteDatatable(String.Format(AccountQuery, ExternalCode & "," & AccountCode, String.Format(" And isNull({0},'') <> '' ", ExternalCode), String.Empty))
            If Not _dtExternalCode Is Nothing Then
                For Each dr As DataRow In _dtExternalCode.Rows
                    If _htExternalCode.ContainsKey(dr(0)) Then
                        _htExternalCode(dr(0)) = (dr(1))
                    Else
                        _htExternalCode.Add(dr(0), dr(1))
                    End If

                Next
            End If
            MyBase.Close()
            Return _htExternalCode
        End Function

        Public Function ExchangeRatesAdjustmentMap() As Hashtable
            Dim ret As Hashtable
            Dim val As Object
            Dim _ht As Hashtable
            ret = New Hashtable
            If _ExchangeRateAdjustmentField <> String.Empty Then
                Try
                    MyBase.Connect()
                    _ht = MyBase.ExecuteHashTable(AccountCode, String.Format(ExchangeRatesAdjustmentQuery, AccountCode, _ExchangeRateAdjustmentField))
                    For Each o As Object In _ht.Keys
                        val = CType(_ht(o), DataRow)(_ExchangeRateAdjustmentField)
                        Select Case Data.Validation.DBNull(val)
                            Case "1"
                                ret.Add(o, DataInterface.Finiancial.ExchangeRates.ExchangeRatesDifferent.Method.Book_Value)
                            Case "2"
                                ret.Add(o, DataInterface.Finiancial.ExchangeRates.ExchangeRatesDifferent.Method.Month_End)
                            Case "3"
                                ret.Add(o, DataInterface.Finiancial.ExchangeRates.ExchangeRatesDifferent.Method.Year_Average)
                            Case "4"
                                ret.Add(o, DataInterface.Finiancial.ExchangeRates.ExchangeRatesDifferent.Method.None)
                            Case Else
                                ret.Add(o, DataInterface.Finiancial.ExchangeRates.ExchangeRatesDifferent.Method.None)
                        End Select


                    Next
                    MyBase.Close()
                Catch ex As Exception
                    _CPSException.ExecuteHandle(ex)
                End Try
            End If
            Return ret
        End Function
#Region "Property"
        Public Property ExchangeRatesAdjustmentField() As String
            Get
                Return _ExchangeRateAdjustmentField
            End Get
            Set(ByVal value As String)
                _ExchangeRateAdjustmentField = value
            End Set
        End Property
#End Region
    End Class
End Namespace