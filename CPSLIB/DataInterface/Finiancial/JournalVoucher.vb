Imports SAPbobsCOM
Imports CPSLIB.DataInterface.Company
Imports CPSLIB.Data
Imports CPSLIB.Logging

Namespace DataInterface.Finiancial
    Public Class JournalVoucher
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _htJournalEntry As Hashtable
        Private _htJournalLines As Hashtable
        Private _htUserField As Hashtable
        Private _Message As String = ""
        Private _DICompany As DICompany
        Private _Doc As SAPbobsCOM.JournalVouchers
        Private _DocStatus As Document.Document.DocumentStatus
        Private _Status As Document.Document.PostStatus
        Private _MsgCode As MessageCode
        Private _ObjectKey As String
        Private Const _ObjectType As Integer = 28
        Private _Remarks As String

        ''' <summary>
        ''' Batch Execute Version
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _MsgCode = New MessageCode
            _htJournalEntry = New Hashtable
            _Status = Document.Document.PostStatus.Ready
            _DocStatus = Document.Document.DocumentStatus.Add
            _htUserField = New Hashtable

        End Sub

        Public Sub New(ByVal _DICompany As DICompany)
            Me.New(_DICompany, Nothing)
        End Sub

        Public Sub New(ByVal _DICompany As DICompany, ByVal transid As Integer)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _MsgCode = New MessageCode
            _Status = Document.Document.PostStatus.Ready
            _htUserField = New Hashtable
            '_htJournalLines = New Hashtable
            _htJournalEntry = New Hashtable
            Me._DICompany = _DICompany
            _Doc = Me._DICompany.Company.GetBusinessObject(BoObjectTypes.oJournalVouchers)

            If Not transid = Nothing Then
                If _Doc.JournalEntries.GetByKey(transid) = False Then
                    _DocStatus = Document.Document.DocumentStatus.Add
                Else
                    _DocStatus = Document.Document.DocumentStatus.Update

                End If
            Else
                _DocStatus = Document.Document.DocumentStatus.Add
            End If
        End Sub


#Region "Function"
        Public Sub SetUserField(ByVal _FieldName As String, ByVal _Value As Object)
            _htUserField(_FieldName) = _Value
        End Sub
        Public Sub ReadLines()

        End Sub

        Public Sub AddJournalEntry(ByVal _oJE As JournalEntries)

            '_htJournalLines.Add(_htJournalLines.Count + 1, _oJE)
            _htJournalEntry.Add(_htJournalEntry.Count + 1, _oJE)
            _Debug.Write(_htJournalEntry.Count, "Journal Entry Count", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
        End Sub
#End Region
#Region "Property"
        Public Property Remarks() As String
            Get
                Return _Remarks
            End Get
            Set(ByVal value As String)
                _Remarks = value
            End Set
        End Property
        Public ReadOnly Property JournalEntries() As Hashtable
            Get
                Return _htJournalEntry
            End Get
        End Property

        Public Function LineCount() As Integer
            Return _Doc.JournalEntries.Lines.Count
        End Function

        Public Property PostStatus() As Document.Document.PostStatus

            Get
                Return _Status
            End Get
            Set(ByVal value As Document.Document.PostStatus)

            End Set
        End Property
        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property
        Public ReadOnly Property OBjectKey() As String
            Get
                Return _ObjectKey
            End Get
        End Property
#End Region
#Region "Execute"
        Public Function Execute() As Integer
            _Debug.Write("Execute Journal Voucher")
            Return Execute(_DICompany)
        End Function

        Public Function Execute(ByVal _diCompany As DICompany) As Integer
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As Integer
            Dim _JECount As Integer = 0
            Dim _JELineCount As Integer = 0
            Dim _oJE As JournalEntries
            Dim _htJournalLines As Hashtable
            Dim _JournalEntryLines As journalEntryLines
            If _Doc Is Nothing Then
                _Doc = _diCompany.Company.GetBusinessObject(BoObjectTypes.oJournalVouchers)

            End If

            WriteDataTable()

            If _htJournalEntry.Count > 0 Then

                For Each o As Object In _htJournalEntry.Keys
                    _JELineCount = 0
                    _oJE = CType(_htJournalEntry(o), JournalEntries)
                    _htJournalLines = _oJE.Lines()
                    _JECount = _JECount + 1
                    If _JECount > 1 Then
                        _Doc.JournalEntries.Add()
                    End If
                    ' JournalEntry Header
                    With _Doc.JournalEntries
                        '.AutoVAT = ""
                        If Validation.IsNull(_oJE.DueDate, DbType.DateTime) <> Nothing Then
                            .DueDate = _oJE.DueDate
                        End If
                        If Validation.IsNull(_oJE.Indicator) <> String.Empty Then
                            .Indicator = _oJE.Indicator
                        End If
                        If Validation.IsNull(_oJE.Memo) <> String.Empty Then
                            .Memo = _oJE.Memo
                        End If
                        If Validation.IsNull(_oJE.ProjectCode) <> String.Empty Then
                            .ProjectCode = _oJE.ProjectCode
                        End If
                        If Validation.IsNull(_oJE.Reference) <> String.Empty Then
                            .Reference = _oJE.Reference
                        End If
                        If Validation.IsNull(_oJE.Reference2) <> String.Empty Then
                            .Reference2 = _oJE.Reference2
                        End If
                        If Validation.IsNull(_oJE.ReferenceDate) <> Nothing Then
                            .ReferenceDate = _oJE.ReferenceDate

                        End If
                        If Validation.IsNull(_oJE.Report347) <> Nothing Then
                            .Report347 = _oJE.Report347
                        End If
                        If Validation.IsNull(_oJE.ReportEU) <> Nothing Then
                            .ReportEU = _oJE.ReportEU
                        End If
                        If Validation.IsNull(_oJE.Series) <> String.Empty Then
                            .Series = _oJE.Series
                        End If
                        If Validation.IsNull(_oJE.StampTax) <> Nothing Then
                            .StampTax = _oJE.StampTax
                        End If
                        If Validation.IsNull(_oJE.StorenoDate) <> Nothing Then
                            .StornoDate = _oJE.StorenoDate

                        End If
                        If Validation.IsNull(_oJE.TaxDate) <> Nothing Then
                            .TaxDate = _oJE.TaxDate
                        End If
                        If Validation.IsNull(_oJE.TransactionCode) <> String.Empty Then
                            .TransactionCode = _oJE.TransactionCode
                        End If
                        If Validation.IsNull(_oJE.UseAutoStorno) <> Nothing Then
                            .UseAutoStorno = _oJE.UseAutoStorno
                        End If
                        If Validation.IsNull(_oJE.VatDate) <> Nothing Then
                            .VatDate = _oJE.VatDate
                        End If

                        For Each _UDFField As Object In _oJE.UserField.Keys
                            .UserFields.Fields.Item(_UDFField).Value = _oJE.UserField(_UDFField)
                        Next

                    End With
                    For Each _o As Object In _htJournalLines.Keys
                        _JELineCount = _JELineCount + 1
                        _JournalEntryLines = CType(_htJournalLines(_o), journalEntryLines)
                        If _JELineCount > 1 Then
                            _Doc.JournalEntries.Lines.Add()
                        End If
                        With _Doc.JournalEntries.Lines
                            .AccountCode = _JournalEntryLines.AccountCode
                            If Data.Validation.IsNull(_JournalEntryLines.CostCode, DbType.AnsiString) <> String.Empty Then
                                .CostingCode = _JournalEntryLines.CostCode
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.CostCode2, DbType.AnsiString) <> String.Empty Then
                                .CostingCode2 = _JournalEntryLines.CostCode2
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.CostCode3) <> String.Empty Then
                                .CostingCode3 = _JournalEntryLines.CostCode3
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.CostCode4) <> String.Empty Then
                                .CostingCode4 = _JournalEntryLines.CostCode4
                            End If

                            If Data.Validation.IsNull(_JournalEntryLines.CostCode5) <> String.Empty Then
                                .CostingCode5 = _JournalEntryLines.CostCode5
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.DueDate) <> Nothing Then
                                .DueDate = _JournalEntryLines.DueDate
                            End If

                            If Data.Validation.IsNull(_JournalEntryLines.GrossValue) <> 0 Then
                                .GrossValue = _JournalEntryLines.GrossValue
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.LineMemo) <> String.Empty Then
                                .LineMemo = _JournalEntryLines.LineMemo
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.ProjectCode) <> String.Empty Then
                                .ProjectCode = _JournalEntryLines.ProjectCode
                            End If
                            'Posting Date
                            'If Validation.IsNull(_JournalEntryLines.Reference1) <> String.Empty Then
                            '    .Reference1 = _JournalEntryLines.Reference1
                            'End If
                            If Data.Validation.IsNull(_JournalEntryLines.Reference2) <> String.Empty Then
                                .Reference2 = _JournalEntryLines.Reference2
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.ReferenceDate1) <> Nothing Then
                                .ReferenceDate1 = _JournalEntryLines.ReferenceDate1
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.ReferenceDate2) <> Nothing Then
                                .ReferenceDate2 = _JournalEntryLines.ReferenceDate2
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.ShortName) <> String.Empty Then
                                .ShortName = _JournalEntryLines.ShortName
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.TaxCode) <> String.Empty Then
                                .TaxCode = _JournalEntryLines.TaxCode
                            End If
                            'If Validation.IsNull(_jvl.TaxDate) <> Nothing Then
                            '    .TaxDate = _jvl.TaxDate
                            'End If
                            If Data.Validation.IsNull(_JournalEntryLines.VatAmount) <> 0 Then
                                .VatAmount = _JournalEntryLines.VatAmount
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.VatDate) <> Nothing Then
                                .VatDate = _JournalEntryLines.VatDate
                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.VatLine) <> String.Empty Then
                                Select Case _JournalEntryLines.VatLine
                                    Case "Y"
                                        .VatLine = BoYesNoEnum.tYES
                                    Case "N"
                                        .VatLine = BoYesNoEnum.tNO
                                End Select

                            End If
                            If Data.Validation.IsNull(_JournalEntryLines.ProfitCode) <> String.Empty Then
                                .CostingCode = Data.Validation.IsNull(_JournalEntryLines.ProfitCode)
                            End If

                            _Debug.Write(_JournalEntryLines.AccountCode, "Account", CPSLib.Debug.LineType.Information)
                            _Debug.Write(_JournalEntryLines.FCCurrency, "FCCurrency in Line", CPSLib.Debug.LineType.Information)
                            _Debug.Write(_JournalEntryLines.FCCredit, "FCCredit in Line", CPSLib.Debug.LineType.Information)
                            _Debug.Write(_JournalEntryLines.FCDebit, "FCDebit in Line", CPSLib.Debug.LineType.Information)
                            _Debug.Write(_JournalEntryLines.Debit, "Debit in Line", CPSLib.Debug.LineType.Information)
                            _Debug.Write(_JournalEntryLines.Credit, "Credit in Line", CPSLib.Debug.LineType.Information)
                            If Data.Validation.IsNull(_JournalEntryLines.FCCurrency) <> String.Empty Then
                                .FCCurrency = Data.Validation.IsNull(_JournalEntryLines.FCCurrency)
                                .FCCredit = Data.Validation.IsNull(_JournalEntryLines.FCCredit)
                                .FCDebit = Data.Validation.IsNull(_JournalEntryLines.FCDebit)
                            Else
                                '.Credit = Validation.IsNull(_JournalEntryLines.Credit)
                                '.Debit = Validation.IsNull(_JournalEntryLines.Debit)
                            End If
                            .Credit = Data.Validation.IsNull(_JournalEntryLines.Credit)
                            .Debit = Data.Validation.IsNull(_JournalEntryLines.Debit)
                            For Each _LUDFField As Object In _JournalEntryLines.UserField.Keys

                                .UserFields.Fields.Item(_LUDFField).Value = Convert.ToString(_JournalEntryLines.UserField(_LUDFField))
                            Next
                        End With

                    Next

                Next
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
                        _Debug.Write(_Message, "Fail Message", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Error)
                        _Status = Document.Document.PostStatus.Fail
                    Else

                        _ObjectKey = _diCompany.Company.GetNewObjectKey.Split(vbTab)(0)
                        _Debug.Write(_ObjectKey, "JV Object Key", CPSLib.Debug.LineType.Information)
                        ' Update Remark Field
                        If Data.Validation.IsNull(_Remarks) <> String.Empty Then
                            Dim rs As Recordset = _diCompany.Company.GetBusinessObject(BoObjectTypes.BoRecordset)
                            Dim sql As String = "UPDATE OBTD SET REMARKS = '{0}' WHERE BATCHNUM = {1}"
                            _Debug.Write(String.Format(sql, _Remarks, _ObjectKey), "Update Remarks Query", CPSLib.Debug.LineType.Information)
                            rs.DoQuery(String.Format(sql, _Remarks, _ObjectKey))
                        End If
                        _Debug.Write(_diCompany.Company.GetNewObjectKey, "Success", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLib.Debug.LineType.Information)
                        _Status = Document.Document.PostStatus.Success
                    End If
                Catch ex As Exception
                    _Message = "Exception (Execute): " & ex.Message & vbCrLf
                    _CPSException.ExecuteHandle(ex)
                End Try

            Else
                _Message = _Message & _MsgCode.Read(MessageCode.MessageCode.NO_RECORD_FOUND)

            End If
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Return _ret
        End Function
#End Region
        Public Sub WriteDataTable()

            Dim _jvl As journalEntryLines
            Dim _dr As DataRow
            Dim _dtJELine As DataTable
            Try


                If _htJournalLines.Count > 0 Then
                    For Each o As Object In _htJournalLines.Keys
                        _jvl = CType(_htJournalLines(o), journalEntryLines)
                        Exit For
                    Next


                    _dtJELine = New DataTable
                    _dtJELine.Columns.Add("AccountCode")
                    _dtJELine.Columns.Add("ShortName")
                    _dtJELine.Columns.Add("Debit")
                    _dtJELine.Columns.Add("Credit")
                    _dtJELine.Columns.Add("ProfitCode")
                    _dtJELine.Columns.Add("CostCode2")
                    _dtJELine.Columns.Add("CostCode3")
                    _dtJELine.Columns.Add("CostCode4")
                    _dtJELine.Columns.Add("CostCode5")
                    _dtJELine.Columns.Add("Project")
                    _dtJELine.Columns.Add("RefDate")
                    _dtJELine.Columns.Add("TaxDate")
                    For Each _UDF As Object In _jvl.UserField.Keys
                        _dtJELine.Columns.Add(_UDF)
                    Next

                    ' Assign Data 
                    For Each o As Object In _htJournalLines.Keys
                        _dr = _dtJELine.NewRow
                        _jvl = _htJournalLines(o)
                        _dr.Item("AccountCode") = _jvl.AccountCode
                        _dr.Item("ShortName") = _jvl.ShortName
                        _dr.Item("Debit") = _jvl.Debit
                        _dr.Item("Credit") = _jvl.Credit
                        _dr.Item("ProfitCode") = _jvl.ProfitCode
                        _dr.Item("CostCode2") = _jvl.CostCode2
                        _dr.Item("CostCode3") = _jvl.CostCode3
                        _dr.Item("CostCode4") = _jvl.CostCode4
                        _dr.Item("CostCode5") = _jvl.CostCode5
                        _dr.Item("Project") = _jvl.ProjectCode
                        _dr.Item("RefDate") = _jvl.ReferenceDate1
                        _dr.Item("TaxDate") = _jvl.TaxDate

                        For Each _UDF As Object In _jvl.UserField.Keys
                            _dr.Item(_UDF) = Convert.ToString(_jvl.UserField(_UDF))
                        Next

                        _dtJELine.Rows.Add(_dr)
                    Next

                    _Debug.WriteTable(_dtJELine, "Generated JE Lines")

                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub
    End Class
End Namespace
