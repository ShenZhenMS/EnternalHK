Imports CPSLIB
Namespace DataInterface.Finiancial.ExchangeRates
    Public Class JEExAdjustment : Inherits ExchangeRatesDifferent


        Private _ReverceNetgative As Boolean


        Public Enum PostConsolidDocumentType
            JournalVoucher = 1
            JournalEntry = 2
        End Enum

        ' Consolid into new Journal Entry or Voucher
        Public Enum DocumentConsolidType
            Month = 1
            Day = 2
            None = 3

        End Enum
        ' Consolid From Source Journal Entry
        Public Enum SourceTransactionConsolidType
            Month = 1
            Day = 2
            MonthAndRate = 3
        End Enum


        Public Enum CalculateMethod
            FCtoLoc = 1
            FCtoSys = 2
            LoctoSys = 3
        End Enum

        ''' <summary>
        ''' 
        ''' Month: Sperate By Month when date range more then 1 month
        ''' Transaction: Adjustment By Transaction, just compare with cutoff date
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum AdjustmentType
            Month = 1
            Transaction = 2
        End Enum


#Region "Data Table Field"


        Public Const FieldDate As String = "RefDate"
        Public Const FieldAjustDate As String = "AdjDate"
        Public Const FieldYear As String = "YearVal"
        Public Const FieldMonth As String = "MonthVal"
        Public Const FieldAccount As String = "Account"
        Public Const FieldDebit As String = "Debit"
        Public Const FieldCredit As String = "Credit"
        Public Const FieldFCDebit As String = "FCDebit"
        Public Const FieldFCCredit As String = "FCCredit"
        Public Const FieldCurrency As String = "Currency"
        Public Const FieldRates As String = "Rates"
        Public Const FieldPostDate As String = "PostDate"
        Public Const FieldTransid As String = "Transid"
        Public Const FieldDebitCredit As String = "D_C"

#End Region

        Private _CPSException As CPSException
        Private _Debug As CPSLIB.Debug
        Private _SourceTransactionConsolidType As SourceTransactionConsolidType
        Private _PostConsolidDocumentType As PostConsolidDocumentType
        Private _AdjustmentType As AdjustmentType
        Private _DocConsolidType As DocumentConsolidType

        Private _dtSummary As DataTable

        ' Posting Class
        Private _DOCJV As DataInterface.Finiancial.JournalVoucher
        Private _DocH As DataInterface.Finiancial.JournalEntries
        Private _DocLines As DataInterface.Finiancial.journalEntryLines

        Private _diCompany As DataInterface.Company.DICompany

        ' User Define Field Assign into Adjustment Entry by Batch
        Private _UDFH As Hashtable
        Private _UDFL As Hashtable

        ' Account Mapping with Adjust Method
        Private _htAcctAdjustMethod As Hashtable

        Private _StartDate As DateTime
        Private _CutOffDate As DateTime


        Private _DFT_JEHeader As DataInterface.Finiancial.JournalEntries

        Private _DFT_JELines As DataInterface.Finiancial.journalEntryLines

        Private _dtAdjutment As DataTable

        Private _AdjustmentCOA As String


        Private _JVRemarks As String

        Private _ProfitCode As String


        Private _Reference3 As String
        

        Private _Rounding As Integer = 2

        Public Sub New(ByVal StartDate As DateTime, ByVal CutOffDate As DateTime, ByVal _diCompany As DataInterface.Company.DICompany)
            MyBase.New(_diCompany.ServerName, _diCompany.DBUserName, _diCompany.DBPassword, _diCompany.CompanyDB, StartDate, CutOffDate)
            Me.ReverseNetgative = False
            Me._StartDate = StartDate
            Me._CutOffDate = CutOffDate
            Me._diCompany = _diCompany
            If _diCompany.Connected = False Then
                _diCompany.Connect()
            End If
            ' Init Adjustment Property

            _SourceTransactionConsolidType = SourceTransactionConsolidType.MonthAndRate
            _AdjustmentType = AdjustmentType.Month
            _PostConsolidDocumentType = PostConsolidDocumentType.JournalVoucher
            _DocConsolidType = DocumentConsolidType.None

            _CPSException = New CPSException

            _Debug = New CPSLIB.Debug(Me.GetType.ToString())

            CreateStructure()
            ' Active Document Header and Line for another information in the document
            _DFT_JEHeader = New DataInterface.Finiancial.JournalEntries
            _DFT_JELines = New DataInterface.Finiancial.journalEntryLines


        End Sub


#Region "Property"
        Public Property ReverseNetgative() As Boolean
            Get
                Return _ReverceNetgative
            End Get
            Set(ByVal value As Boolean)
                _ReverceNetgative = value
            End Set
        End Property

        Public Property Rounding() As Integer
            Get
                Return _Rounding
            End Get
            Set(ByVal value As Integer)
                _Rounding = value
            End Set
        End Property
        
        Public Property ProfitCode() As String
            Get
                Return _ProfitCode
            End Get
            Set(ByVal value As String)
                _ProfitCode = value
            End Set
        End Property

        Public Property JVRemarks() As String
            Get
                Return _JVRemarks
            End Get
            Set(ByVal value As String)
                _JVRemarks = value
            End Set
        End Property

        Public Property Reference3() As String
            Get
                Return _Reference3
            End Get
            Set(ByVal value As String)
                _Reference3 = value
            End Set
        End Property



        Public Property JEAdjustmentCOA() As String
            Get
                Return _AdjustmentCOA
            End Get
            Set(ByVal value As String)
                _AdjustmentCOA = value
            End Set
        End Property
        Public Property JEAdjustmentHistory() As DataTable
            Get
                Return _dtAdjutment
            End Get
            Set(ByVal value As DataTable)
                _dtAdjutment = value
            End Set
        End Property

        Public Property TransactionDataTable() As DataTable
            Get
                Return _dtSummary
            End Get
            Set(ByVal value As DataTable)
                _dtSummary = value
            End Set
        End Property

        Public Property DFT_JELines() As DataInterface.Finiancial.journalEntryLines
            Get
                Return _DFT_JELines
            End Get
            Set(ByVal value As DataInterface.Finiancial.journalEntryLines)
                _DFT_JELines = value
            End Set
        End Property

        Public Property DFT_JEHeader() As DataInterface.Finiancial.JournalEntries
            Get
                Return _DFT_JEHeader
            End Get
            Set(ByVal value As DataInterface.Finiancial.JournalEntries)
                _DFT_JEHeader = value
            End Set
        End Property


        Public Property AdjustmentMethod() As AdjustmentType
            Get
                Return _AdjustmentType
            End Get
            Set(ByVal value As AdjustmentType)
                _AdjustmentType = value
            End Set
        End Property

        Public Property PostConsolidDocument() As PostConsolidDocumentType
            Get
                Return _PostConsolidDocumentType
            End Get

            Set(ByVal value As PostConsolidDocumentType)
                _PostConsolidDocumentType = value
            End Set

        End Property

        Public Property AdjustmentConsolidType() As SourceTransactionConsolidType
            Get
                Return _SourceTransactionConsolidType
            End Get
            Set(ByVal value As SourceTransactionConsolidType)
                _SourceTransactionConsolidType = value
            End Set
        End Property
        Public Property DocConsolidType() As DocumentConsolidType
            Get
                Return _DocConsolidType
            End Get
            Set(ByVal value As DocumentConsolidType)
                _DocConsolidType = value
            End Set
        End Property


#End Region
#Region "Get"
        Public Function getAccountMethod(ByVal acctcode As String) As ExchangeRatesDifferent.Method
            _Debug.Write(_htAcctAdjustMethod.Count, "getAccountMethod", CPSLIB.Debug.LineType.Information)
            If _htAcctAdjustMethod.ContainsKey(acctcode) Then
                _Debug.Write(_htAcctAdjustMethod(acctcode), "AccountMethod", CPSLIB.Debug.LineType.Information)
                Return CType(_htAcctAdjustMethod(acctcode), ExchangeRatesDifferent.Method)
            Else
                _Debug.Write(acctcode, "Cannot Found", CPSLIB.Debug.LineType.Information)
                Return Nothing
            End If
        End Function
#End Region


#Region "Set"
        Public Sub SetAccountMethod(ByVal _htAcctMth As Hashtable)
            _htAcctAdjustMethod = _htAcctMth
        End Sub
        Public Sub SetAccountMethod(ByVal acctcode As String, ByVal methodtype As ExchangeRatesDifferent.Method)
            If _htAcctAdjustMethod Is Nothing Then
                _htAcctAdjustMethod(acctcode) = methodtype
            End If
        End Sub

        Public Sub setHeaderUDF(ByVal strField As String, ByVal strValue As Object)
            If _UDFH Is Nothing Then
                _UDFH = New Hashtable
            End If
            _UDFH(strField) = strValue
        End Sub
        Public Sub setLinesUDF(ByVal strField As String, ByVal strValue As Object)
            If _UDFL Is Nothing Then
                _UDFL = New Hashtable
            End If
            _UDFL(strField) = strValue
        End Sub





        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="_Date"></param>
        ''' <param name="_Currency"></param>
        ''' <param name="_Account"></param>
        ''' <param name="_Debit"></param>
        ''' <param name="_Credit"></param>
        ''' <param name="_FCDebit"></param>
        ''' <param name="_FCCredit"></param>
        ''' <param name="_SysDebit"></param>
        ''' <param name="_SysCredit"></param>
        ''' <remarks></remarks>
        Public Sub SetTransaction(ByVal _Date As DateTime, ByVal _Currency As String, ByVal _Rates As Decimal, ByVal _Account As String, _
                            ByVal _Debit As Decimal, ByVal _Credit As Decimal, ByVal _FCDebit As Decimal, ByVal _FCCredit As Decimal, ByVal _SysDebit As Decimal, ByVal _SysCredit As Decimal, ByVal _CalMth As CalculateMethod)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)

            Dim _dr() As DataRow
            Dim dr As DataRow
            Dim dr_Diff As DataRow
            Dim dr_Adjustment As DataRow
            Dim _AcctMethod As ExchangeRatesDifferent.Method
            ' CalDate: Exchange Rates Compare Date
            Dim CalDate As DateTime
            ' PostDate: Day: Transaction Date
            '           Month: End of the month

            Dim PostDate As DateTime
            ' Temp Date for Calculate current calculating transaction date
            Dim CurrDate As DateTime
            ' Condition : Date, Currency and Account
            ' {6} - {11} DOES NOT SUPPORT STRING VALUE
            ' Debit Transation or Credit Transaction

            Dim isDebitTransactionCondition As String
            If _Debit <> 0 Then
                isDebitTransactionCondition = "D"
            Else
                isDebitTransactionCondition = "C"
            End If

            Dim _strCondition As String = " {0} = '{1}' AND {2} = '{3}' AND {4} = '{5}' AND {6} = {7} AND {8} = {9} AND {10} = '{11}' AND {12} = '{13}' "
            Dim _Year As Integer
            Dim _Month As Integer

            Try
                _AcctMethod = Me.getAccountMethod(_Account)
                If _AcctMethod = Nothing Then
                    _AcctMethod = ExchangeRatesDifferent._DFT_EXCHANGERATESDIFFMETHOD
                End If

                _Year = _Date.Year
                _Month = _Date.Month
                If _dtSummary Is Nothing Then
                    CreateStructure()
                End If

                Select Case _AdjustmentType
                    Case AdjustmentType.Transaction
                        Select Case _SourceTransactionConsolidType

                            Case SourceTransactionConsolidType.Day
                                PostDate = _Date
                                '_dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd")), FieldAccount & " asc, " & FieldDate & " asc ")
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            Case SourceTransactionConsolidType.Month
                                PostDate = New DateTime(_Date.Year, _Date.Month, 1).AddMonths(1).AddDays(-1)
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            Case SourceTransactionConsolidType.MonthAndRate
                                PostDate = New DateTime(_Date.Year, _Date.Month, 1).AddMonths(1).AddDays(-1)
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, FieldRates, _Rates, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")

                        End Select
                        _Debug.Write(_Currency, "Source DB Compare Currency", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_diCompany.CompInfomation.LocalCurrency, "Local Currency", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_Date, "Reference Date", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_Account, "Account", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_SysCredit, "SysCredit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_SysDebit, "SysDebit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_FCCredit, "FCCredit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_FCDebit, "FCDebit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_Credit, "Credit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_Debit, "Debit", CPSLIB.Debug.LineType.Information)

                        If _dr.Length > 0 Then
                            _Debug.Write(_CalMth, "Calculate Method", CPSLIB.Debug.LineType.Information)
                            dr = _dr(0)
                            Select Case _CalMth
                                Case CalculateMethod.FCtoLoc
                                    dr(FieldCredit) = dr(FieldCredit) + (_FCCredit * dr(FieldRates) - _Credit)
                                    dr(FieldDebit) = dr(FieldDebit) + (_FCDebit * dr(FieldRates) - _Debit)
                                Case CalculateMethod.FCtoSys
                                    dr(FieldCredit) = dr(FieldCredit) + (_FCCredit * dr(FieldRates) - _SysCredit)
                                    dr(FieldDebit) = dr(FieldDebit) + (_FCDebit * dr(FieldRates) - _SysDebit)
                                Case CalculateMethod.LoctoSys
                                    dr(FieldCredit) = dr(FieldCredit) + (_SysCredit * dr(FieldRates) - _Credit)
                                    dr(FieldDebit) = dr(FieldDebit) + (_FCDebit * dr(FieldRates) - _Debit)
                                Case Else

                            End Select

                        Else

                            dr_Diff = DifferentRates(_AcctMethod, _Currency, _Date, _CutOffDate)

                            ' Find Adjustment History Record
                            dr_Adjustment = AdjustmentHistory(_Account)
                            dr = _dtSummary.NewRow
                            dr(FieldDate) = _Date
                            dr(FieldYear) = _Year
                            dr(FieldMonth) = _Month
                            dr(FieldAccount) = _Account
                            dr(FieldCurrency) = _Currency
                            dr(FieldAjustDate) = _CutOffDate
                            dr(FieldPostDate) = _CutOffDate
                            dr(FieldDebitCredit) = isDebitTransactionCondition

                            If Not dr_Diff Is Nothing Then
                                dr(FieldRates) = dr_Diff(Field_CurrRates)
                                _Debug.Write(dr_Diff(Field_CurrRates), "CurrRates", CPSLIB.Debug.LineType.Information)
                                _Debug.Write(_CalMth, "Calculate Method", CPSLIB.Debug.LineType.Information)
                                Select Case _CalMth
                                    Case CalculateMethod.FCtoLoc
                                        dr(FieldCredit) = _FCCredit * dr(FieldRates) - _Credit
                                        dr(FieldDebit) = _FCDebit * dr(FieldRates) - _Debit
                                    Case CalculateMethod.FCtoSys
                                        dr(FieldCredit) = _FCCredit * dr(FieldRates) - _SysCredit
                                        dr(FieldDebit) = _FCDebit * dr(FieldRates) - _SysDebit
                                    Case CalculateMethod.LoctoSys
                                        dr(FieldCredit) = _SysCredit * dr(FieldRates) - _Credit
                                        dr(FieldDebit) = _FCDebit * dr(FieldRates) - _Debit
                                    Case Else

                                End Select

                                _Debug.Write(dr(FieldDebit), "Debit Calculated", CPSLIB.Debug.LineType.Information)
                                _Debug.Write(dr(FieldCredit), "Credit Calculated", CPSLIB.Debug.LineType.Information)

                                'dr(FieldFCCredit) = _FCCredit * dr_Diff(FIeld_CurrRates) - _FCCredit
                                'dr(FieldFCDebit) = _FCDebit * dr_Diff(FIeld_CurrRates) - _FCDebit

                            Else
                                _Debug.Write("No Rates found", "CurrRates", CPSLIB.Debug.LineType.Information)
                                dr(FieldDebit) = 0
                                dr(FieldCredit) = 0
                                _Debug.Write(dr(FieldDebit), "Debit Calculated", CPSLIB.Debug.LineType.Information)
                                _Debug.Write(dr(FieldCredit), "Credit Calculated", CPSLIB.Debug.LineType.Information)
                                'dr(FieldFCCredit) = 0
                                'dr(FieldFCDebit) = 0
                                dr(FieldRates) = Decimal.Zero
                            End If

                            _dtSummary.Rows.Add(dr)

                        End If
                    Case AdjustmentType.Month
                        CalDate = _Date

                        For i As Integer = 1 To DateDiff(DateInterval.Month, _Date, _CutOffDate)

                            'For i As Integer = _Date.Month To _CutOffDate.Month

                            CurrDate = CalDate
                            If CalDate.AddMonths(1) > _CutOffDate Then
                                CalDate = _CutOffDate
                            Else
                                CalDate = CalDate.AddMonths(1)
                            End If

                            ' Declare Calulate Date as the end of the month
                            'CalDate = New DateTime(CalDate.Year, CalDate.Month, 1).AddMonths(1).AddDays(-1)

                            Select Case _SourceTransactionConsolidType

                                Case SourceTransactionConsolidType.Day
                                    PostDate = CurrDate
                                    '_dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd")), FieldAccount & " asc, " & FieldDate & " asc ")
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, CurrDate, FieldCurrency, _Currency, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                                Case SourceTransactionConsolidType.Month
                                    PostDate = New DateTime(CurrDate.Year, CurrDate.Month, 1).AddMonths(1).AddDays(-1)
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                                Case SourceTransactionConsolidType.MonthAndRate
                                    'PostDate = New DateTime(CalDate.Year, CalDate.Month, 1).AddMonths(1).AddDays(-1)
                                    PostDate = New DateTime(CurrDate.Year, CurrDate.Month, 1).AddMonths(1).AddDays(-1)
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, FieldRates, _Rates, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            End Select

                            If Not _dr Is Nothing And _dr.Length > 0 Then
                                dr = _dr(0)
                                dr(FieldCredit) = dr(FieldCredit) + _Credit
                                dr(FieldDebit) = dr(FieldDebit) + _Debit
                                dr(FieldFCCredit) = dr(FieldFCCredit) + _FCCredit
                                dr(FieldFCDebit) = dr(FieldFCDebit) + _FCDebit
                            Else
                                dr_Diff = DifferentRates(_AcctMethod, _Currency, _Date, CalDate)
                                dr = _dtSummary.NewRow
                                dr(FieldDate) = _Date
                                dr(FieldYear) = _Year
                                dr(FieldMonth) = _Month
                                dr(FieldAccount) = _Account
                                dr(FieldCurrency) = _Currency
                                dr(FieldDebit) = _Debit
                                dr(FieldCredit) = _Credit
                                dr(FieldFCCredit) = _FCCredit
                                dr(FieldFCDebit) = _FCDebit
                                dr(FieldAjustDate) = CalDate
                                dr(FieldPostDate) = PostDate
                                If Not dr_Diff Is Nothing Then
                                    dr(FieldRates) = dr_Diff(Field_DiffRates)
                                Else
                                    dr(FieldRates) = Decimal.Zero
                                End If
                                dr(FieldDebitCredit) = isDebitTransactionCondition
                                _dtSummary.Rows.Add(dr)
                            End If

                        Next

                End Select

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        ''' <summary>
        ''' Backup Sub-Procedure
        ''' </summary>
        ''' <param name="_Date"></param>
        ''' <param name="_Currency"></param>
        ''' <param name="_Rates"></param>
        ''' <param name="_Account"></param>
        ''' <param name="_Debit"></param>
        ''' <param name="_Credit"></param>
        ''' <param name="_FCDebit"></param>
        ''' <param name="_FCCredit"></param>
        ''' <remarks></remarks>
        Public Sub SetTransaction_Backup(ByVal _Date As DateTime, ByVal _Currency As String, ByVal _Rates As Decimal, ByVal _Account As String, _
                            ByVal _Debit As Decimal, ByVal _Credit As Decimal, ByVal _FCDebit As Decimal, ByVal _FCCredit As Decimal)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _dr() As DataRow
            Dim dr As DataRow
            Dim dr_Diff As DataRow
            Dim dr_Adjustment As DataRow
            Dim _AcctMethod As ExchangeRatesDifferent.Method
            ' CalDate: Exchange Rates Compare Date
            Dim CalDate As DateTime
            ' PostDate: Day: Transaction Date
            '           Month: End of the month

            Dim PostDate As DateTime
            ' Temp Date for Calculate current calculating transaction date
            Dim CurrDate As DateTime
            ' Condition : Date, Currency and Account
            ' {6} - {11} DOES NOT SUPPORT STRING VALUE
            ' Debit Transation or Credit Transaction

            Dim isDebitTransactionCondition As String
            If _Debit <> 0 Then
                isDebitTransactionCondition = "D"
            Else
                isDebitTransactionCondition = "C"
            End If

            Dim _strCondition As String = " {0} = '{1}' AND {2} = '{3}' AND {4} = '{5}' AND {6} = {7} AND {8} = {9} AND {10} = '{11}' AND {12} = '{13}' "
            Dim _Year As Integer
            Dim _Month As Integer

            Try
                _AcctMethod = Me.getAccountMethod(_Account)
                If _AcctMethod = Nothing Then
                    _AcctMethod = ExchangeRatesDifferent._DFT_EXCHANGERATESDIFFMETHOD
                End If

                _Year = _Date.Year
                _Month = _Date.Month
                If _dtSummary Is Nothing Then
                    CreateStructure()
                End If

                Select Case _AdjustmentType
                    Case AdjustmentType.Transaction
                        Select Case _SourceTransactionConsolidType

                            Case SourceTransactionConsolidType.Day
                                PostDate = _Date
                                '_dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd")), FieldAccount & " asc, " & FieldDate & " asc ")
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            Case SourceTransactionConsolidType.Month
                                PostDate = New DateTime(_Date.Year, _Date.Month, 1).AddMonths(1).AddDays(-1)
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, 1, 1, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            Case SourceTransactionConsolidType.MonthAndRate
                                PostDate = New DateTime(_Date.Year, _Date.Month, 1).AddMonths(1).AddDays(-1)
                                _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, FieldRates, _Rates, FieldAjustDate, _CutOffDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")

                        End Select
                        If _dr.Length > 0 Then
                            dr = _dr(0)
                            dr(FieldCredit) = dr(FieldCredit) + _Credit
                            dr(FieldDebit) = dr(FieldDebit) + _Debit
                            dr(FieldFCCredit) = dr(FieldFCCredit) + _FCCredit
                            dr(FieldFCDebit) = dr(FieldFCDebit) + _FCDebit
                        Else
                            dr_Diff = DifferentRates(_AcctMethod, _Currency, _Date, _CutOffDate)
                            ' Find Adjustment History Record
                            dr_Adjustment = AdjustmentHistory(_Account)
                            dr = _dtSummary.NewRow
                            dr(FieldDate) = _Date
                            dr(FieldYear) = _Year
                            dr(FieldMonth) = _Month
                            dr(FieldAccount) = _Account
                            dr(FieldCurrency) = _Currency
                            If Not dr_Adjustment Is Nothing Then
                                dr(FieldDebit) = dr_Adjustment(FieldDebit) * -1
                                dr(FieldCredit) = dr_Adjustment(FieldCredit) * -1
                                dr(FieldFCCredit) = dr_Adjustment(FieldFCCredit) * -1
                                dr(FieldFCDebit) = dr_Adjustment(FieldFCDebit) * -1
                            Else
                                dr(FieldDebit) = 0
                                dr(FieldCredit) = 0
                                dr(FieldFCCredit) = 0
                                dr(FieldFCDebit) = 0
                            End If

                            dr(FieldDebit) = dr(FieldDebit) + _Debit
                            dr(FieldCredit) = dr(FieldCredit) + _Credit
                            dr(FieldFCCredit) = dr(FieldFCCredit) + _FCCredit
                            dr(FieldFCDebit) = dr(FieldFCDebit) + _FCDebit
                            dr(FieldAjustDate) = _CutOffDate
                            dr(FieldPostDate) = PostDate
                            dr(FieldDebitCredit) = isDebitTransactionCondition

                            If Not dr_Diff Is Nothing Then
                                dr(FieldRates) = dr_Diff(Field_DiffRates)
                            Else
                                dr(FieldRates) = Decimal.Zero
                            End If

                            _dtSummary.Rows.Add(dr)

                        End If
                    Case AdjustmentType.Month
                        CalDate = _Date

                        For i As Integer = 1 To DateDiff(DateInterval.Month, _Date, _CutOffDate)

                            'For i As Integer = _Date.Month To _CutOffDate.Month

                            CurrDate = CalDate
                            If CalDate.AddMonths(1) > _CutOffDate Then
                                CalDate = _CutOffDate
                            Else
                                CalDate = CalDate.AddMonths(1)
                            End If

                            ' Declare Calulate Date as the end of the month
                            'CalDate = New DateTime(CalDate.Year, CalDate.Month, 1).AddMonths(1).AddDays(-1)

                            Select Case _SourceTransactionConsolidType

                                Case SourceTransactionConsolidType.Day
                                    PostDate = CurrDate
                                    '_dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, _Date, FieldCurrency, _Currency, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd")), FieldAccount & " asc, " & FieldDate & " asc ")
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldDate, CurrDate, FieldCurrency, _Currency, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                                Case SourceTransactionConsolidType.Month
                                    PostDate = New DateTime(CurrDate.Year, CurrDate.Month, 1).AddMonths(1).AddDays(-1)
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, 1, 1, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                                Case SourceTransactionConsolidType.MonthAndRate
                                    'PostDate = New DateTime(CalDate.Year, CalDate.Month, 1).AddMonths(1).AddDays(-1)
                                    PostDate = New DateTime(CurrDate.Year, CurrDate.Month, 1).AddMonths(1).AddDays(-1)
                                    _dr = _dtSummary.Select(String.Format(_strCondition, FieldAccount, _Account, FieldMonth, _Month, FieldCurrency, _Currency, FieldYear, _Year, FieldRates, _Rates, FieldAjustDate, CalDate.ToString("yyyy-MM-dd"), FieldDebitCredit, isDebitTransactionCondition), FieldAccount & " asc, " & FieldDate & " asc ")
                            End Select

                            If Not _dr Is Nothing And _dr.Length > 0 Then
                                dr = _dr(0)
                                dr(FieldCredit) = dr(FieldCredit) + _Credit
                                dr(FieldDebit) = dr(FieldDebit) + _Debit
                                dr(FieldFCCredit) = dr(FieldFCCredit) + _FCCredit
                                dr(FieldFCDebit) = dr(FieldFCDebit) + _FCDebit
                            Else
                                dr_Diff = DifferentRates(_AcctMethod, _Currency, _Date, CalDate)
                                dr = _dtSummary.NewRow
                                dr(FieldDate) = _Date
                                dr(FieldYear) = _Year
                                dr(FieldMonth) = _Month
                                dr(FieldAccount) = _Account
                                dr(FieldCurrency) = _Currency
                                dr(FieldDebit) = _Debit
                                dr(FieldCredit) = _Credit
                                dr(FieldFCCredit) = _FCCredit
                                dr(FieldFCDebit) = _FCDebit
                                dr(FieldAjustDate) = CalDate
                                dr(FieldPostDate) = PostDate
                                If Not dr_Diff Is Nothing Then
                                    dr(FieldRates) = dr_Diff(Field_DiffRates)
                                Else
                                    dr(FieldRates) = Decimal.Zero
                                End If
                                dr(FieldDebitCredit) = isDebitTransactionCondition
                                _dtSummary.Rows.Add(dr)
                            End If

                        Next

                End Select

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub
#End Region
#Region "Post"
        Public Function Post(ByVal _PostConsolidDocumentType As PostConsolidDocumentType) As Boolean
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim ret As Boolean = True
            Dim _rowCnt As Integer = 0
            Dim _PrevDate As DateTime
            Dim _PrevMonth As Integer
            Dim dr() As DataRow
            Dim _isNewDoc As Boolean
            Dim _isCreateDoc As Boolean
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            'Debug: _dtSummary                                    ''''
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            Dim _debugrowcnt As Integer = 0
            _Debug.Write("Exchange Rate Adjustment exchange rates")
            _Debug.WriteTable(_dtSummary, "Summary")
            
            _Debug.Write("Start Looping Rows in Post Function")
            Try

                ' Sorting By Date

                dr = _dtSummary.Select(" 1 = 1 ", FieldDate & " asc ")
                _DocH = New DataInterface.Finiancial.JournalEntries(_diCompany)

                If _dtSummary.Rows.Count > 0 Then
                    For Each _dr As DataRow In dr

                        _isNewDoc = False
                        _isCreateDoc = False
                        Select Case _DocConsolidType
                            Case DocumentConsolidType.Day
                                If _PrevDate <> Convert.ToDateTime(_dr.Item(FieldDate)) Then
                                    If _DocH.LineCount > 0 Then
                                        _isCreateDoc = True
                                    End If
                                    _isNewDoc = True

                                End If

                            Case DocumentConsolidType.Month
                                If _PrevMonth <> Convert.ToDateTime(_dr.Item(FieldDate)).Month Then
                                    If _DocH.LineCount > 0 Then
                                        _isCreateDoc = True
                                    End If
                                    _isNewDoc = True
                                End If
                            Case DocumentConsolidType.None
                                If _rowCnt = 0 Then
                                    _isNewDoc = True
                                End If
                        End Select

                        If _isCreateDoc Then
                            ' Calculate Balance and add adjustment COA when not balance

                            _DocH = setAdjustmentBalance(_DocH)

                            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Create Document", TimeSet.Status.Start)
                            ret = ExecuteDocument(_DocH)
                            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Create Document", TimeSet.Status.Finish)
                            If ret = False Then

                                Exit For
                            End If

                        End If
                        _PrevDate = Convert.ToDateTime(_dr(FieldDate))
                        _PrevMonth = _PrevDate.Month

                        If _isNewDoc Then
                            ' Assign User Defined Field
                            '_DocH = New CPSLIB.DataInterface.Finiancial.JournalEntries
                            _DFT_JEHeader.Lines.Clear()
                            _DocH = _DFT_JEHeader

                            _DocH.ReferenceDate = _dr(FieldPostDate)

                            If _UDFH Is Nothing = False Then
                                For Each o As Object In _UDFH.Keys
                                    _DocH.SetUserField(o.ToString, _UDFH(o))
                                Next
                            End If

                        End If

                        _DocLines = New DataInterface.Finiancial.journalEntryLines

                        '_DocLines = _DFT_JELines
                        _DocLines.AccountCode = _dr(FieldAccount)
                        '_DocLines.ReferenceDate1 = _dr(FieldPostDate)

                        _DocLines.TaxDate = _dr(FieldAjustDate)

                        _Debug.Write(_dr(FieldAccount), "Account", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_dr(FieldCredit), "Credit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_dr(FieldDebit), "Debit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(Data.Validation.DBNull(_dr(FieldRates)), "Rates", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_ReverceNetgative, "Reverse Netgative", CPSLIB.Debug.LineType.Information)
                        '_DocLines.Credit = Decimal.Round(CPSLIB.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency) * CPSLIB.Validation.DBNull(_dr(FieldRates), System.Data.DbType.Currency), _Rounding)
                        '_DocLines.Debit = Decimal.Round(CPSLIB.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency) * CPSLIB.Validation.DBNull(_dr(FieldRates), System.Data.DbType.Currency), _Rounding)


                        '_DocLines.Credit = Decimal.Round(CPSLIB.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency), _Rounding)
                        '_DocLines.Debit = Decimal.Round(CPSLIB.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency), _Rounding)

                        If Data.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency) <> 0 Then
                            If _ReverceNetgative Then
                                If Decimal.Round(Data.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency), _Rounding) < 0 Then
                                    _Debug.Write("Credit", "Reverse Method, Netgative Found", CPSLIB.Debug.LineType.Information)
                                    _DocLines.Debit = Decimal.Round(Data.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency), _Rounding) * -1
                                Else
                                    _Debug.Write("Credit", "Reverse Method, Netgative Not Found", CPSLIB.Debug.LineType.Information)
                                    _DocLines.Credit = Decimal.Round(Data.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency), _Rounding)
                                End If
                            Else
                                _Debug.Write("Credit", "Standard Method", CPSLIB.Debug.LineType.Information)
                                _DocLines.Credit = Decimal.Round(Data.Validation.DBNull(_dr(FieldCredit), System.Data.DbType.Currency), _Rounding)

                            End If
                        End If

                        If Data.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency) <> 0 Then
                            If _ReverceNetgative Then

                                If Decimal.Round(Data.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency), _Rounding) < 0 Then
                                    _Debug.Write("Debit", "Reverse Method, Netgative Found", CPSLIB.Debug.LineType.Information)
                                    _DocLines.Credit = Decimal.Round(Data.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency), _Rounding) * -1

                                Else
                                    _Debug.Write("Debit", "Reverse Method, Netgative Not Found", CPSLIB.Debug.LineType.Information)
                                    _DocLines.Debit = Decimal.Round(Data.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency), _Rounding)
                                End If
                            Else
                                _Debug.Write("Debit", "Standard Method", CPSLIB.Debug.LineType.Information)
                                _DocLines.Debit = Decimal.Round(Data.Validation.DBNull(_dr(FieldDebit), System.Data.DbType.Currency), _Rounding)
                            End If
                        End If
                        If _Reference3 <> String.Empty Then
                            _DocLines.Reference3 = _Reference3
                        End If
                        _Debug.Write(_DocLines.Credit, "DocLines.Credit", CPSLIB.Debug.LineType.Information)
                        _Debug.Write(_DocLines.Debit, "DocLines.Debit", CPSLIB.Debug.LineType.Information)

                        ' Karrson: Remark Foreign Logic
                        'If Not (_diCompany.CompInfomation.LocalCurrency = _dr(FieldCurrency) Or _diCompany.CompInfomation.SystemCurrency = _dr(FieldCurrency)) Then
                        '    _DocLines.FCCurrency = _dr(FieldCurrency)
                        '    _DocLines.FCCredit = CPSLIB.Validation.DBNull(_dr(FieldFCCredit), System.Data.DbType.Currency) * CPSLIB.Validation.DBNull(_dr(FieldRates), System.Data.DbType.Currency)
                        '    _DocLines.FCDebit = CPSLIB.Validation.DBNull(_dr(FieldFCDebit), System.Data.DbType.Currency) * CPSLIB.Validation.DBNull(_dr(FieldRates), System.Data.DbType.Currency)
                        'End If

                        ' User Definded Field
                        _Debug.Write("Setting User Defined Field")
                        If Not _UDFL Is Nothing Then
                            For Each o As Object In _UDFL.Keys
                                _DocLines.SetUserField(o.ToString, _UDFL(o))
                            Next
                        End If
                        TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Debit: " & _DocLines.Debit.ToString() & ", Credit: " & _DocLines.Credit.ToString(), TimeSet.Status.Start)
                        If Not (_DocLines.Credit = 0 And _DocLines.Debit = 0 And _DocLines.FCCredit = 0 And _DocLines.FCCredit = 0) Then
                            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Add Line", TimeSet.Status.Start)
                            _DocH.addLine(_DocLines)
                            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Add Line", TimeSet.Status.Finish)
                        End If

                        TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ", Debit: " & _DocLines.Debit.ToString() & ", Credit: " & _DocLines.Credit.ToString(), TimeSet.Status.Finish)
                        If Not _ProfitCode = Nothing Then
                            _DocLines.ProfitCode = _ProfitCode
                        End If

                        _rowCnt = _rowCnt + 1

                    Next
                    _Debug.Write(_DocH.LineCount, "Line Count", CPSLIB.Debug.LineType.Information)

                    If _DocH.LineCount > 0 And IsError() = False Then
                        _Debug.Write("Post Journal Entry/Voucher")
                        _DocH = setAdjustmentBalance(_DocH)

                        ret = ExecuteDocument(_DocH)
                    End If
                Else

                End If

                If ret = Nothing Then
                    ret = True
                End If
            Catch ex As Exception
                ret = False
                _CPSException.ExecuteHandle(ex)
                _Debug.Write(ex.Message)
                _Debug.WriteException(ex, "Exception with Post Adjustment")


            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return ret
        End Function
#End Region

        Private Sub CreateStructure()
            _dtSummary = New DataTable
            _dtSummary.Columns.Add(JEExAdjustment.FieldAccount)
            _dtSummary.Columns.Add(JEExAdjustment.FieldDebit, System.Type.GetType("System.Double"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldCredit, System.Type.GetType("System.Double"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldFCDebit, System.Type.GetType("System.Double"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldFCCredit, System.Type.GetType("System.Double"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldRates)
            _dtSummary.Columns.Add(JEExAdjustment.FieldCurrency)
            _dtSummary.Columns.Add(JEExAdjustment.FieldDate, System.Type.GetType("System.DateTime"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldMonth, System.Type.GetType("System.Int32"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldYear, System.Type.GetType("System.Int32"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldAjustDate, System.Type.GetType("System.DateTime"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldPostDate, System.Type.GetType("System.DateTime"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldTransid, System.Type.GetType("System.Int32"))
            _dtSummary.Columns.Add(JEExAdjustment.FieldDebitCredit)

        End Sub

        Private Function setAdjustmentBalance(ByVal _DocH As DataInterface.Finiancial.JournalEntries) As JournalEntries
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim DebitAmt As Decimal = 0
            Dim CreditAmt As Decimal = 0
            Dim _oJEL As journalEntryLines
            _Debug.Write("setAdjustmentBalance")
            For Each key As Object In _DocH.Lines.Keys
                _oJEL = CType(_DocH.Lines(key), journalEntryLines)
                _Debug.Write(_oJEL.AccountCode, "Account", CPSLIB.Debug.LineType.Information)
                _Debug.Write(_oJEL.Debit, "Debit", CPSLIB.Debug.LineType.Information)
                _Debug.Write(_oJEL.Credit, "Credit", CPSLIB.Debug.LineType.Information)

                DebitAmt = DebitAmt + Decimal.Round(_oJEL.Debit, 2)
                CreditAmt = CreditAmt + Decimal.Round(_oJEL.Credit, 2)
                _Debug.Write(DebitAmt, "Total of Debit", CPSLIB.Debug.LineType.Information)
                _Debug.Write(CreditAmt, "Total of Credit", CPSLIB.Debug.LineType.Information)

            Next

            If _AdjustmentCOA <> String.Empty Then
                If DebitAmt <> CreditAmt Then
                    _oJEL = New journalEntryLines
                    _oJEL.AccountCode = _AdjustmentCOA

                    If DebitAmt > CreditAmt Then
                        _oJEL.Credit = DebitAmt - CreditAmt
                        _oJEL.Debit = 0
                    Else
                        _oJEL.Credit = 0
                        _oJEL.Debit = CreditAmt - DebitAmt
                    End If
                    If Not _ProfitCode Is Nothing Then
                        _oJEL.ProfitCode = _ProfitCode
                    End If
                    ' User Definded Field
                    _Debug.Write("User Define Field in AdjustmentBalance")
                    If Not _UDFL Is Nothing Then
                        For Each o As Object In _UDFL.Keys
                            _DocLines.SetUserField(o.ToString, _UDFL(o))
                        Next
                    End If
                    _Debug.Write("User Define Field in AdjustmentBalance End")
                    _DocH.addLine(_oJEL)

                End If
            End If
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _DocH
        End Function

        Private Function ExecuteDocument(ByVal _Doc As DataInterface.Finiancial.JournalEntries) As Boolean
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            _Debug.Write("Execute Document")
            Dim ret As Boolean = True
            Dim _result As Integer
            Dim _ObjectKey As String
            Try
                Select Case _PostConsolidDocumentType
                    Case PostConsolidDocumentType.JournalEntry
                        _result = _DocH.Execute(_diCompany)
                    Case PostConsolidDocumentType.JournalVoucher
                        _DOCJV = New DataInterface.Finiancial.JournalVoucher(_diCompany)
                        _DOCJV.AddJournalEntry(_Doc)
                        _result = _DOCJV.Execute
                End Select
                _Debug.Write(_result, "Result of Execute Document", CPSLIB.Debug.LineType.Information)
                If _result <> 0 Then
                    ret = False
                    Select Case _PostConsolidDocumentType
                        Case PostConsolidDocumentType.JournalEntry
                            Message = "Error on Execute Journal Entry: " & _diCompany.Message
                        Case PostConsolidDocumentType.JournalVoucher
                            Message = "Error on Execute Journal Voucher: " & _DOCJV.Message
                    End Select

                Else

                    ' Update Set Remark Field
                    _ObjectKey = _DOCJV.OBjectKey
                    If _PostConsolidDocumentType = PostConsolidDocumentType.JournalVoucher Then
                        If _JVRemarks = Nothing Then

                            _JVRemarks = String.Empty
                        End If
                        _diCompany.RecordSet.DoQuery(String.Format("UPDATE OBTD SET REMARKS = '{0}' WHERE BATCHNUM = {1}", "Exchange Rates Adjustment: " & _JVRemarks & " " & _DocH.ReferenceDate.ToString("yyyy-MM-dd"), _ObjectKey.Split(vbTab)(0).ToString))
                    End If
                    ret = True
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
                Message = "Exception: " & ex.Message
                _Debug.Write(ex.Message, "Exception", CPSLIB.Debug.LineType.Error)
                ret = False
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return ret
        End Function


    End Class
End Namespace
