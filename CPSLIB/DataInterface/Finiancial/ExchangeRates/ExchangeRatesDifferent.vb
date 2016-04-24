Imports CPSLIB

Imports CPSLIB.Data.Connection
Namespace DataInterface.Finiancial.ExchangeRates

    Public Class ExchangeRatesDifferent : Inherits Data.Connection.SQLServerInfo
        Public Shared SettingPath As String = System.Environment.CurrentDirectory & "\SystemConfig.ini"
        Public Shared ExchangeRatesSection As String = "ExchangeRate"

        Public AverageTableName As String = "CPS_Func_AverageRates"
        Public MonthEndTableName As String = "CPS_Func_MonthEndRates"
        Public JEHistoryTableName As String = "CPS_Func_JEHistory"
        Public ExAdjustHistTableName As String = "CPS_Func_ExHist"

        Private _StartDate As DateTime
        Private _CutOffDate As DateTime

        Private _CPSException As CPSException
        Private _Debug As CPSLib.Debug
        Private _dtAverage As DataTable
        Private _dtMonthEnd As DataTable
        Private _dtJEHistory As DataTable
        Private _dtExAdjustHistory As DataTable
        Private _calAverageRates As Boolean = True
        Private _calMonthEndRates As Boolean = True
        Private _iniSetting As Programming.Business_Config
        ' JEHistory and ExAdjustment Table Field


        ' Field Defination
        Public Const Field_Currency As String = "CURRENCY"
        Public Const Field_RateDate As String = "RateDate"
        Public Const Field_RateMonth As String = "RateMonth"
        Public Const Field_RateYear As String = "RateYear"
        Public Const Field_OrgRates As String = "OrgRates"
        Public Const Field_CurrRates As String = "CurrRates"
        Public Const Field_DiffRates As String = "DiffRate"
        Public Const Field_CalDate As String = "CalDate"

        Public Const _DFT_EXCHANGERATESDIFFMETHOD As Method = Method.None
        Public Enum Method
            Book_Value = 1
            Month_End = 2
            Year_Average = 3
            None = 4
        End Enum



        Public Sub New(ByVal strServerName As String, ByVal strUserName As String, ByVal strPassword As String, ByVal strDatabase As String, ByVal StartDate As DateTime, ByVal CutOffDate As DateTime)

            MyBase.New(strServerName, strUserName, strPassword, strDatabase)
            _StartDate = StartDate
            _CutOffDate = CutOffDate

            _CPSException = New CPSException
            _Debug = New CPSLib.Debug(Me.GetType.ToString())
            _iniSetting = New Programming.Business_Config
            If _iniSetting.hasFile Then
                If _iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_AVERAGEVIEW) <> String.Empty Then
                    AverageTableName = _iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_AVERAGEVIEW)
                End If

                If _iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_MONTHENDVIEW) <> String.Empty Then
                    MonthEndTableName = _iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_MONTHENDVIEW)
                End If


                _calAverageRates = (_iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_AVERAGEMETHOD) = "Y")
                _calMonthEndRates = (_iniSetting.getValue(Programming.Business_Config._SECTION_EXCHANGERATES, Programming.Business_Config._KEY_ER_MONTHENDMETHOD) = "Y")
            End If
            Execute()
            MyBase.Close()
        End Sub

        Private Sub Execute()
            If _calMonthEndRates Then
                ExecuteMonthEnd()
            End If

            If _calAverageRates Then
                ExecuteAverage()
            End If

        End Sub

        Public Function ExecuteJETransaction() As DataTable
            Return ExecuteJETransaction("*", String.Empty)
        End Function

        Public Function ExecuteJETransaction(ByVal strField As String) As DataTable
            Return ExecuteJETransaction(strField, String.Empty)
        End Function

        Public Function ExecuteJETransaction(ByVal strField As String, ByVal strCondition As String) As DataTable
            If MyBase.isObjectExists(SysObjectType.SQL_TABLE_VALUED_FUNCTION, JEHistoryTableName) Then
                If strCondition = String.Empty Then
                    _dtJEHistory = MyBase.ExecuteDatatable(String.Format("SELECT {3} FROM {0}('{1}','{2}')", JEHistoryTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd"), strField))
                Else
                    _dtJEHistory = MyBase.ExecuteDatatable(String.Format("SELECT {3} FROM {0}('{1}','{2}') WHERE {4}", JEHistoryTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd"), strField, strCondition))
                End If

            Else
                IsError = True
                Message = String.Format(New Logging.MessageCode().Read(Logging.MessageCode.MessageCode.OBJECT_NOT_EXISTS), MonthEndTableName)

            End If
            Return _dtJEHistory
        End Function

        Public Function ExecuteJEExAdjustment() As DataTable
            Return ExecuteJEExAdjustment("*", String.Empty)
        End Function

        Public Function ExecuteJEExAdjustment(ByVal strField As String) As DataTable
            Return ExecuteJEExAdjustment(strField, String.Empty)
        End Function

        Public Function ExecuteJEExAdjustment(ByVal strField As String, ByVal strCondition As String) As DataTable
            If MyBase.isObjectExists(SysObjectType.SQL_TABLE_VALUED_FUNCTION, ExAdjustHistTableName) Then
                If strCondition = String.Empty Then
                    _dtJEHistory = MyBase.ExecuteDatatable(String.Format("SELECT {3} FROM {0}('{1}','{2}')", ExAdjustHistTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd"), strField))
                Else
                    _dtJEHistory = MyBase.ExecuteDatatable(String.Format("SELECT {3} FROM {0}('{1}','{2}') WHERE {4}", ExAdjustHistTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd"), strField, strCondition))
                End If

            Else
                IsError = True
                Message = String.Format(New Logging.MessageCode().Read(Logging.MessageCode.MessageCode.OBJECT_NOT_EXISTS), MonthEndTableName)

            End If
            Return _dtJEHistory
        End Function


        Private Sub ExecuteMonthEnd()
            If MyBase.isObjectExists(SysObjectType.SQL_TABLE_VALUED_FUNCTION, MonthEndTableName) Then

                _dtMonthEnd = MyBase.ExecuteDatatable(String.Format("SELECT * FROM {0}('{1}','{2}')", MonthEndTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd")))
            Else
                IsError = True
                Message = String.Format(New Logging.MessageCode().Read(Logging.MessageCode.MessageCode.OBJECT_NOT_EXISTS), MonthEndTableName)

            End If
        End Sub

        Private Sub ExecuteAverage()
            If MyBase.isObjectExists(SysObjectType.SQL_TABLE_VALUED_FUNCTION, AverageTableName) Then
                _dtAverage = MyBase.ExecuteDatatable(String.Format("SELECT * FROM {0}('{1}','{2}')", AverageTableName, _StartDate.ToString("yyyy-MM-dd"), _CutOffDate.ToString("yyyy-MM-dd")))
            Else
                IsError = True
                Message = String.Format(New Logging.MessageCode().Read(Logging.MessageCode.MessageCode.OBJECT_NOT_EXISTS), AverageTableName)
            End If
        End Sub

        Public Function AdjustmentHistory(ByVal strAcctCode As String) As DataRow
            Dim ret As DataRow = Nothing
            Dim _dr() As DataRow
            Dim dr As DataRow
            Dim strCondition As String = " {0} = '{1}'"

            'strCondition = String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd"))
            Try
                If strAcctCode <> String.Empty Then
                    If Not _dtExAdjustHistory Is Nothing Then
                        _dr = _dtExAdjustHistory.Select(strCondition, DataInterface.Finiancial.ExchangeRates.JEExAdjustment.FieldAccount, strAcctCode)
                        If _dr.Length > 0 Then
                            dr = _dr(0)
                            ret = dr
                        End If
                    End If
                End If

            Catch ex As Exception
                ret = Nothing
                _CPSException.ExecuteHandle(ex)
            End Try
            Return ret
        End Function

        Public Function DifferentRates(ByVal _DiffMethod As ExchangeRatesDifferent.Method, ByVal Currency As String, ByVal TransactionDate As Date, ByVal CutOffDate As Date) As DataRow
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & _DiffMethod.ToString & "," & Currency & "," & TransactionDate.ToString("yyyy-MM-dd") & "," & CutOffDate.ToString("yyyy-MM-dd"), TimeSet.Status.Start)
            _Debug.Write("Different Rates")
            Dim ret As DataRow = Nothing
            Dim _dr() As DataRow
            Dim dr As DataRow
            Dim strCondition As String = " {0} = '{1}' AND {2} = '{3}' AND {4} = '{5}' "
            ' Debug Write Columns'
            Try
                For Each _c As DataColumn In _dtAverage.Columns
                    _Debug.Write(_c.ColumnName, "Column Name", CPSLIB.Debug.LineType.Information)
                Next
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            Try
                For Each _c As DataColumn In _dtMonthEnd.Columns
                    _Debug.Write(_c.ColumnName, "Column Name", CPSLIB.Debug.LineType.Information)
                Next
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            'strCondition = String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd"))
            Try
                If _dtAverage Is Nothing Then
                    _Debug.Write("Average Datatable is nothing")
                Else
                    _Debug.Write(_dtAverage.Rows.Count, "Average Datatable Count", CPSLIB.Debug.LineType.Information)
                End If
                If _dtMonthEnd Is Nothing Then
                    _Debug.Write("Month End Datatable is nothing")
                Else
                    _Debug.Write(_dtMonthEnd.Rows.Count, "Month End Datatable Count", CPSLIB.Debug.LineType.Information)
                End If
                _Debug.Write(_DiffMethod, "Account Type", CPSLIB.Debug.LineType.Information)
                Select Case _DiffMethod

                    Case Method.Book_Value
                        ret = Nothing
                    Case Method.Month_End
                        _dr = _dtMonthEnd.Select(String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd")))
                        _Debug.Write(String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd")), "Condition", CPSLIB.Debug.LineType.Information)
                    Case Method.Year_Average
                        _dr = _dtAverage.Select(String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd")))
                        _Debug.Write(String.Format(strCondition, Field_Currency, Currency, Field_RateDate, TransactionDate.ToString("yyyy-MM-dd"), Field_CalDate, CutOffDate.ToString("yyyy-MM-dd")), "Condition", CPSLIB.Debug.LineType.Information)
                End Select
                _Debug.Write("DifferentRates1")
                If Not _dr Is Nothing Then
                    _Debug.Write("DifferentRates2")
                    If _dr.Length > 0 Then
                        _Debug.Write("DifferentRates3")

                        dr = _dr(0)
                        _Debug.Write(dr(Field_DiffRates).ToString(), "Rates", CPSLIB.Debug.LineType.Information)
                        ret = dr
                    End If
                Else
                    ret = Nothing
                End If

            Catch ex As Exception
                ret = Nothing
                _CPSException.ExecuteHandle(ex)
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & _DiffMethod.ToString & "," & Currency & "," & TransactionDate.ToString("yyyy-MM-dd") & "," & CutOffDate.ToString("yyyy-MM-dd"), TimeSet.Status.Finish)
            Return ret
        End Function

#Region "Property"

        Public ReadOnly Property MonthEndTable() As DataTable
            Get
                Return _dtMonthEnd
            End Get
        End Property

        Public ReadOnly Property AverageTable() As DataTable
            Get
                Return _dtAverage
            End Get
        End Property

#End Region
    End Class
End Namespace
