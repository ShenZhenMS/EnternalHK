Imports CPSLIB.Data
Imports CPSLIB.DataInterface.Company

Imports SAPbobsCOM

Namespace DataInterface.Finiancial
    Public Class JournalVoucherEntries
        Private _Debug As CPSLIB.Debug
        Private _Doc As SAPbobsCOM.JournalEntries
        Private _htJournalLines As Hashtable
        Private _htUserField As Hashtable
        Private _DocStatus As Document.Document.DocumentStatus
        Private _Status As Document.Document.PostStatus
        Private _Message As String

        ''' <summary>
        ''' Batch Post Version. It don't carry di company object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            _Status = Document.Document.PostStatus.Ready
            _htUserField = New Hashtable
            _htJournalLines = New Hashtable
            _DocStatus = Document.Document.DocumentStatus.Add
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
        End Sub


#Region "Execute"
        Public Sub ReadLines()
            Dim _jvLines As journalEntryLines
            Dim _jeLines As JournalEntries_Lines = _Doc.JournalEntries.Lines
            For i As Integer = 0 To _jeLines.Count - 1
                _jvLines = New journalEntryLines
                _jeLines.SetCurrentLine(i)
                _jvLines.AccountCode = _jeLines.AccountCode
                _jvLines.Credit = _jeLines.Credit
                _jvLines.CreditSy = _jeLines.CreditSys
                _jvLines.Debit = _jeLines.Debit
                _jvLines.DebitSy = _jeLines.DebitSys
                _jvLines.DueDate = _jeLines.DueDate
                _jvLines.FCCredit = _jeLines.FCCredit
                _jvLines.FCCurrency = _jeLines.FCCurrency
                _jvLines.FCDebit = _jeLines.FCDebit
                _jvLines.Line_ID = _jeLines.Line_ID
                _jvLines.ProjectCode = _jeLines.ProjectCode
                _jvLines.Reference1 = _jeLines.Reference1
                _jvLines.Reference2 = _jeLines.Reference2
                _jvLines.ReferenceDate1 = _jeLines.ReferenceDate1
                _jvLines.ReferenceDate2 = _jeLines.ReferenceDate2
                _jvLines.ShortName = _jeLines.ShortName
                _jvLines.TaxDate = _jeLines.TaxDate
                _htJournalLines.Add(_htJournalLines.Count + 1, _jvLines)

            Next
            _Debug.Write(_htJournalLines.Count, "JE Line Count", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)

        End Sub
#Region "User Field"

        Public Sub SetUserField(ByVal _FieldName As String, ByVal _Value As Object)
            _htUserField(_FieldName) = _Value
        End Sub
#End Region

        Public Sub FindRecord(ByVal transid As Integer)
            If _Doc.GetByKey(transid) = True Then
                _DocStatus = Document.Document.DocumentStatus.Update
            Else
                _DocStatus = Document.Document.DocumentStatus.Add
            End If

        End Sub

        Public Sub addLine(ByVal _jeline As journalEntryLines)
            _Debug.Write(_jeline.Line_ID, "Add Line", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            _htJournalLines.Add(_htJournalLines.Count + 1, _jeline)
        End Sub


        Public Function Execute(ByVal _diCompany As DICompany) As Integer
            Dim _jvl As journalEntryLines
            Dim _ret As Integer
            Dim _RowCount As Integer
            If _Doc Is Nothing Then
                _Doc = _diCompany.Company.GetBusinessObject(BoObjectTypes.oJournalEntries)
            End If
            ' JournalEntry Header
            'Declare Header Variable
            With _Doc.JournalEntries
                '.AutoVAT = ""
                If Validation.IsNull(DueDate, DbType.DateTime) <> Nothing Then
                    .DueDate = DueDate
                End If
                If Validation.IsNull(Indicator) <> String.Empty Then
                    .Indicator = Indicator
                End If
                If Validation.IsNull(Memo) <> String.Empty Then
                    .Memo = Memo
                End If
                If Validation.IsNull(ProjectCode) <> String.Empty Then
                    .ProjectCode = ProjectCode
                End If
                If Validation.IsNull(Reference) <> String.Empty Then
                    .Reference = Reference
                End If
                If Validation.IsNull(Reference2) <> String.Empty Then
                    .Reference2 = Reference2
                End If
                If Validation.IsNull(ReferenceDate) <> Nothing Then
                    .ReferenceDate = Reference

                End If
                If Validation.IsNull(Report347) <> Nothing Then
                    .Report347 = Report347
                End If
                If Validation.IsNull(ReportEU) <> Nothing Then
                    .ReportEU = ReportEU
                End If
                If Validation.IsNull(Series) <> String.Empty Then
                    .Series = Series
                End If
                If Validation.IsNull(StampTax) <> Nothing Then
                    .StampTax = StampTax
                End If
                If Validation.IsNull(StorenoDate) <> Nothing Then
                    .StornoDate = StorenoDate

                End If
                If Validation.IsNull(TaxDate) <> Nothing Then
                    .TaxDate = TaxDate
                End If
                If Validation.IsNull(TransactionCode) <> String.Empty Then
                    .TransactionCode = TransactionCode
                End If
                If Validation.IsNull(UseAutoStorno) <> Nothing Then
                    .UseAutoStorno = UseAutoStorno
                End If
                If Validation.IsNull(VatDate) <> Nothing Then
                    .VatDate = VatDate
                End If

                For Each o As Object In _htUserField.Keys
                    .UserFields.Fields.Item(o).Value = _htUserField(o)
                Next

            End With


            ' Generate Document Strcture
            _RowCount = 0

            For Each o As Object In _htJournalLines.Keys
                _RowCount = _RowCount + 1
                _jvl = _htJournalLines(o)
                If _DocStatus = Document.Document.DocumentStatus.Add Then
                    If _RowCount > 1 Then
                        _Doc.JournalEntries.Lines.Add()
                    End If
                Else
                    _Doc.JournalEntries.Lines.Add()
                End If

                With _Doc.JournalEntries.Lines

                    .AccountCode = _jvl.AccountCode
                    If Validation.IsNull(_jvl.CostCode, DbType.AnsiString) <> String.Empty Then
                        .CostingCode = _jvl.CostCode
                    End If
                    If Validation.IsNull(_jvl.CostCode2, DbType.AnsiString) <> String.Empty Then
                        .CostingCode2 = _jvl.CostCode2
                    End If
                    If Validation.IsNull(_jvl.CostCode3) <> String.Empty Then
                        .CostingCode3 = _jvl.CostCode3
                    End If
                    If Validation.IsNull(_jvl.CostCode4) <> String.Empty Then
                        .CostingCode4 = _jvl.CostCode4
                    End If

                    If Validation.IsNull(_jvl.CostCode5) <> String.Empty Then
                        .CostingCode5 = _jvl.CostCode5
                    End If

                    .DueDate = _jvl.DueDate
                    If Validation.IsNull(_jvl.GrossValue) <> 0 Then
                        .GrossValue = _jvl.GrossValue
                    End If
                    If Validation.IsNull(_jvl.LineMemo) <> String.Empty Then
                        .LineMemo = _jvl.LineMemo
                    End If
                    If Validation.IsNull(_jvl.ProjectCode) <> String.Empty Then
                        .ProjectCode = _jvl.ProjectCode
                    End If
                    If Validation.IsNull(_jvl.Reference1) <> String.Empty Then
                        .Reference1 = _jvl.Reference1
                    End If
                    If Validation.IsNull(_jvl.Reference2) <> String.Empty Then
                        .Reference2 = _jvl.Reference2
                    End If
                    If Validation.IsNull(_jvl.ReferenceDate1) <> Nothing Then
                        .ReferenceDate1 = _jvl.ReferenceDate1
                    End If
                    If Validation.IsNull(_jvl.ReferenceDate2) <> Nothing Then
                        .ReferenceDate2 = _jvl.ReferenceDate2
                    End If
                    If Validation.IsNull(_jvl.ShortName) <> String.Empty Then
                        .ShortName = _jvl.ShortName
                    End If
                    If Validation.IsNull(_jvl.TaxCode) <> String.Empty Then
                        .TaxCode = _jvl.TaxCode
                    End If
                    'If Validation.IsNull(_jvl.TaxDate) <> Nothing Then
                    '    .TaxDate = _jvl.TaxDate
                    'End If
                    If Validation.IsNull(_jvl.VatAmount) <> 0 Then
                        .VatAmount = _jvl.VatAmount
                    End If
                    If Validation.IsNull(_jvl.VatDate) <> Nothing Then
                        .VatDate = _jvl.VatDate
                    End If
                    If Validation.IsNull(_jvl.VatLine) <> String.Empty Then
                        Select Case _jvl.VatLine
                            Case "Y"
                                .VatLine = BoYesNoEnum.tYES
                            Case "N"
                                .VatLine = BoYesNoEnum.tNO

                        End Select

                    End If

                    If Validation.IsNull(_jvl.FCCurrency) <> String.Empty Then
                        .FCCurrency = Validation.IsNull(_jvl.FCCurrency)
                        .FCCredit = Validation.IsNull(_jvl.FCCredit)
                        .FCDebit = Validation.IsNull(_jvl.FCDebit)
                    Else
                        .Credit = Validation.IsNull(_jvl.Credit)
                        .Debit = Validation.IsNull(_jvl.Debit)
                    End If

                    If Validation.IsNull(_jvl.ProfitCode) <> String.Empty Then
                        .CostingCode = _jvl.ProfitCode
                    End If
                End With

                For Each _UDF As Object In _jvl.UserField.Keys
                    _Doc.Lines.UserFields.Fields.Item(_UDF).Value = Convert.ToString(_jvl.UserField(_UDF))
                Next
            Next
            _Debug.Write(_Doc.Lines.Count, "Journal Entry Line Count", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            Try

                Select Case _DocStatus
                    Case Document.Document.DocumentStatus.Add
                        _ret = _Doc.JournalEntries.Add
                        If _ret = 0 Then
                            _ret = _Doc.Add
                        Else

                        End If
                    Case Document.Document.DocumentStatus.Update
                        _ret = _Doc.JournalEntries.Update


                End Select
                If _ret <> 0 Then
                    _Message = _diCompany.Company.GetLastErrorCode & ":" & _diCompany.Company.GetLastErrorDescription & vbCrLf
                    _Status = Document.Document.PostStatus.Fail
                Else
                    _Status = Document.Document.PostStatus.Success
                End If
            Catch ex As Exception
                _Message = "Exception (Execute): " & ex.Message & vbCrLf
            End Try
        End Function
#End Region

#Region "Property"
        Public ReadOnly Property UserField() As Hashtable
            Get
                If _htUserField Is Nothing Then
                    Return New Hashtable
                Else
                    Return _htUserField
                End If
            End Get
        End Property
        Public ReadOnly Property LineCount() As Integer
            Get
                If _htJournalLines Is Nothing Then
                    Return 0
                Else
                    Return _htJournalLines.Count
                End If
            End Get
        End Property
        Public ReadOnly Property Lines() As Hashtable
            Get
                If _htJournalLines Is Nothing Then
                    Return New Hashtable
                Else
                    Return _htJournalLines
                End If
            End Get
        End Property

        Private _DueDate As DateTime
        Public Property DueDate() As DateTime
            Get
                Return _DueDate
            End Get
            Set(ByVal value As DateTime)
                _DueDate = value
            End Set
        End Property


        Private _Indicator As String
        Public Property Indicator() As String
            Get
                Return _Indicator
            End Get
            Set(ByVal value As String)
                _Indicator = value
            End Set
        End Property


        Private _Memo As String
        Public Property Memo() As String
            Get
                Return _Memo
            End Get
            Set(ByVal value As String)
                _Memo = value
            End Set
        End Property


        Private _ProjectCode As String
        Public Property ProjectCode() As String
            Get
                Return _ProjectCode
            End Get
            Set(ByVal value As String)
                _ProjectCode = value
            End Set
        End Property


        Private _Reference As String
        Public Property Reference() As String
            Get
                Return _Reference
            End Get
            Set(ByVal value As String)
                _Reference = value
            End Set
        End Property


        Private _Reference2 As String
        Public Property Reference2() As String
            Get
                Return _Reference2
            End Get
            Set(ByVal value As String)
                _Reference2 = value
            End Set
        End Property


        Private _ReferenceDate As DateTime
        Public Property ReferenceDate() As DateTime
            Get
                Return _ReferenceDate
            End Get
            Set(ByVal value As DateTime)
                _ReferenceDate = value
            End Set
        End Property


        Private _Report347 As SAPbobsCOM.BoYesNoEnum
        Public Property Report347() As SAPbobsCOM.BoYesNoEnum
            Get
                Return _Report347
            End Get
            Set(ByVal value As SAPbobsCOM.BoYesNoEnum)
                _Report347 = value
            End Set
        End Property


        Private _ReportEU As SAPbobsCOM.BoYesNoEnum
        Public Property ReportEU() As SAPbobsCOM.BoYesNoEnum
            Get
                Return _ReportEU
            End Get
            Set(ByVal value As SAPbobsCOM.BoYesNoEnum)
                _ReportEU = value
            End Set
        End Property


        Private _Series As String
        Public Property Series() As String
            Get
                Return _Series
            End Get
            Set(ByVal value As String)
                _Series = value
            End Set
        End Property


        Private _StampTax As SAPbobsCOM.BoYesNoEnum
        Public Property StampTax() As SAPbobsCOM.BoYesNoEnum
            Get
                Return _StampTax
            End Get
            Set(ByVal value As SAPbobsCOM.BoYesNoEnum)
                _StampTax = value
            End Set
        End Property


        Private _StorenoDate As DateTime
        Public Property StorenoDate() As DateTime
            Get
                Return _StorenoDate
            End Get
            Set(ByVal value As DateTime)
                _StorenoDate = value
            End Set
        End Property


        Private _TaxDate As DateTime
        Public Property TaxDate() As DateTime
            Get
                Return _TaxDate
            End Get
            Set(ByVal value As DateTime)
                _TaxDate = value
            End Set
        End Property


        Private _TransactionCode As String
        Public Property TransactionCode() As String
            Get
                Return _TransactionCode
            End Get
            Set(ByVal value As String)
                _TransactionCode = value
            End Set
        End Property


        Private _UseAutoStorno As SAPbobsCOM.BoYesNoEnum
        Public Property UseAutoStorno() As SAPbobsCOM.BoYesNoEnum
            Get
                Return _UseAutoStorno
            End Get
            Set(ByVal value As SAPbobsCOM.BoYesNoEnum)
                _UseAutoStorno = value
            End Set
        End Property


        Private _VatDate As DateTime
        Public Property VatDate() As DateTime
            Get
                Return _VatDate
            End Get
            Set(ByVal value As DateTime)
                _VatDate = value
            End Set
        End Property
#End Region

    End Class
End Namespace
