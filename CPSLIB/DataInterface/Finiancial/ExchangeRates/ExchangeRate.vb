Namespace DataInterface.Finiancial.ExchangeRates

    Public Class ExchangeRate : Inherits Data.Connection.MSSQLClient
        Public Const RateQuery As String = "SELECT {0} FROM {1} WHERE {2} = '{3}' AND {4} = '{5}'"
        Public Const TableQuery As String = "SELECT * FROM {0} "
        Public Const Field_Rates As String = "Rate"
        Public Const Field_Currency As String = "Currency"
        Public Const Field_RateDate As String = "RateDate"
        Public Const TableName As String = "ORTT"

        Private _diCompany As DataInterface.Company.DICompany
        Private _CPSException As CPSException
        Private _Debug As CPSLIB.Debug
        Private _dtExchangeRate As DataTable
        Private _isError As Boolean
        Private _hasException As Boolean

        Public Sub New(ByVal _diCompany As DataInterface.Company.DICompany)
            MyBase.New(_diCompany.ServerName, _diCompany.CompanyDB, _diCompany.DBUserName, _diCompany.DBPassword)
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _isError = False
            _hasException = False
            ReadExchangeRates()
        End Sub

        Public Function GetRates(ByVal _Currency As String, ByVal _Date As DateTime) As Decimal
            Dim ret As Decimal = -1
            Dim dr() As DataRow
            Dim _dr As DataRow
            Try
                dr = _dtExchangeRate.Select(String.Format(" {0} = '{1}' AND {2} = '{3}' ", ExchangeRate.Field_Currency, _Currency, ExchangeRate.Field_RateDate, _Date.ToString("yyyy-MM-dd")))
                If dr.Length > 0 Then
                    _dr = dr(0)
                    ret = Convert.ToDecimal(_dr(ExchangeRate.Field_Rates))
                Else
                    _Debug.Write("There is no exchange rates information in the table: " & _Currency, _Date.ToString("yyyy-MM-dd"), CPSLIB.Debug.LineType.Information)
                    ret = -1
                End If
                hasException = False

            Catch ex As Exception
                hasException = True
                Message = "Exception: " & ex.Message
                IsError = True
                _CPSException.ExecuteHandle(ex)
            End Try
            'If isConnected = False Then
            '    Connect()
            'End If
            'Try
            '    ret = Convert.ToDecimal(ExecuteValue(String.Format(RateQuery, ExchangeRate.Field_Rates, ExchangeRate.TableName, ExchangeRate.Field_Currency, _Currency, ExchangeRate.Field_RateDate, _Date.ToString("yyyy-MM-dd"))))
            'Catch ex As Exception
            '    _CPSException.ExecuteHandle(ex)
            'End Try
            'Close()
            Return ret
        End Function

        Private Sub ReadExchangeRates()
            If isConnected = False Then
                Connect()
            End If
            Try
                _dtExchangeRate = ExecuteDatatable(String.Format(TableQuery, ExchangeRate.TableName))
                hasException = False
                IsError = False
            Catch ex As Exception
                hasException = True
                IsError = True
                Message = "Exception: " & ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try

            Close()

        End Sub
    End Class
End Namespace
