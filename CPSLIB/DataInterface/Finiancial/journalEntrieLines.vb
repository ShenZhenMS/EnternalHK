Imports SAPbobsCOM
Namespace DataInterface.Finiancial
    Public Class journalEntryLines
        Private _Debug As CPSLIB.Debug
        Private _JournalEntryLines As JournalEntries_Lines

        Private _htUserField As Hashtable
        Private _ControlAccount As String
        Private _DueDate As DateTime
        Private _ReferenceDate1 As DateTime
        Private _ReferenceDate2 As DateTime
        Private _ShortName As String
        Private _FCCredit As Decimal
        Private _FCDebit As Decimal
        Private _FCCurrency As String
        Private _DebitSy As Decimal
        Private _CreditSy As Decimal
        Private _Debit As Decimal
        Private _Credit As Decimal
        Private _Reference1 As String
        Private _Reference2 As String
        Private _Reference3 As String
        Private _AccountCode As String
        Private _Line_ID As Integer
        Private _ProjectCode As String
        Private _GrossValueFC As Decimal
        Private _GrossValue As Decimal
        Private _VatDate As DateTime
        Private _ProfitCode As String


        Private _ContraAccount As String
        Public Property ContraAccount() As String
            Get
                Return _ContraAccount
            End Get
            Set(ByVal value As String)
                _ContraAccount = value
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

        Public Property Reference3() As String
            Get
                Return _Reference3
            End Get
            Set(ByVal value As String)
                _Reference3 = value
            End Set
        End Property

        Public Property GrossValueFC() As Decimal
            Get
                Return _GrossValueFC
            End Get
            Set(ByVal value As Decimal)
                _GrossValueFC = value
            End Set
        End Property

        Public Property GrossValue() As Decimal
            Get
                Return _GrossValue
            End Get
            Set(ByVal value As Decimal)
                _GrossValue = value
            End Set
        End Property

        Public Property VatDate() As DateTime
            Get
                Return _VatDate
            End Get
            Set(ByVal value As DateTime)
                _VatDate = value
            End Set
        End Property

        Private _VatLine As SAPbobsCOM.BoYesNoEnum
        Public Property VatLine() As String
            Get
                If _VatLine = BoYesNoEnum.tYES Then
                    Return "Y"
                Else
                    Return "N"
                End If

            End Get
            Set(ByVal value As String)
                Select Case value

                    Case "Y"
                        _VatLine = BoYesNoEnum.tYES
                    Case "N"
                        _VatLine = BoYesNoEnum.tNO
                    Case "1"
                        _VatLine = BoYesNoEnum.tYES
                    Case "0"
                        _VatLine = BoYesNoEnum.tNO
                End Select

            End Set
        End Property

        Private _VatAmount As Decimal
        Public Property VatAmount() As Decimal
            Get
                Return _VatAmount
            End Get
            Set(ByVal value As Decimal)
                _VatAmount = value
            End Set
        End Property

        Private _VatGroup As String
        Public Property VatGroup() As String
            Get
                Return _VatGroup
            End Get
            Set(ByVal value As String)
                _VatGroup = value
            End Set
        End Property

        Private _LineMemo As String

        Private _CostCode5 As String
        Public Property CostCode5() As String
            Get
                Return _CostCode5
            End Get
            Set(ByVal value As String)
                _CostCode5 = value
            End Set
        End Property

        Private _CostCode4 As String
        Public Property CostCode4() As String
            Get
                Return _CostCode4
            End Get
            Set(ByVal value As String)
                _CostCode4 = value
            End Set
        End Property

        Private _CostCode3 As String
        Public Property CostCode3() As String
            Get
                Return _CostCode3
            End Get
            Set(ByVal value As String)
                _CostCode3 = value
            End Set
        End Property

        Private _CostCode2 As String
        Public Property CostCode2() As String
            Get
                Return _CostCode2
            End Get
            Set(ByVal value As String)
                _CostCode2 = value
            End Set
        End Property

        Private _CostCode1 As String
        Public Property CostCode() As String
            Get
                Return _CostCode1
            End Get
            Set(ByVal value As String)
                _CostCode1 = value
            End Set
        End Property


        Private _TaxCode As String
        Public Property TaxCode() As String
            Get
                Return _TaxCode
            End Get
            Set(ByVal value As String)
                _TaxCode = value
            End Set
        End Property

        Public Property LineMemo() As String
            Get
                Return _LineMemo
            End Get
            Set(ByVal value As String)
                _LineMemo = value
            End Set
        End Property

        Private _TaxDate As DateTime

#Region "Property"


        Public Property ControlAccount() As String
            Get
                Return _ControlAccount
            End Get
            Set(ByVal value As String)
                _ControlAccount = value
            End Set
        End Property
        Public Property DueDate() As DateTime
            Get
                Return _DueDate
            End Get
            Set(ByVal value As DateTime)
                _DueDate = value
            End Set
        End Property
        Public Property ReferenceDate1() As DateTime
            Get
                Return _ReferenceDate1
            End Get
            Set(ByVal value As DateTime)
                _ReferenceDate1 = value
            End Set
        End Property
        Public Property ReferenceDate2() As DateTime
            Get
                Return _ReferenceDate2
            End Get
            Set(ByVal value As DateTime)
                _ReferenceDate2 = value
            End Set
        End Property
        Public Property ShortName() As String
            Get
                Return _ShortName
            End Get
            Set(ByVal value As String)
                _ShortName = value
            End Set
        End Property
        Public Property FCCredit() As Decimal
            Get
                Return _FCCredit
            End Get
            Set(ByVal value As Decimal)
                _FCCredit = value
            End Set
        End Property
        Public Property FCDebit() As Decimal
            Get
                Return _FCDebit
            End Get
            Set(ByVal value As Decimal)
                _FCDebit = value
            End Set
        End Property
        Public Property FCCurrency() As String
            Get
                Return _FCCurrency
            End Get
            Set(ByVal value As String)
                _FCCurrency = value
            End Set
        End Property
        Public Property DebitSy() As Decimal
            Get
                Return _DebitSy
            End Get
            Set(ByVal value As Decimal)
                _DebitSy = value
            End Set
        End Property
        Public Property CreditSy() As Decimal
            Get
                Return _CreditSy
            End Get
            Set(ByVal value As Decimal)
                _CreditSy = value
            End Set
        End Property
        Public Property Debit() As Decimal
            Get
                Return _Debit
            End Get
            Set(ByVal value As Decimal)
                _Debit = value
            End Set
        End Property
        Public Property Credit() As Decimal
            Get
                Return _Credit
            End Get
            Set(ByVal value As Decimal)
                _Credit = value
            End Set
        End Property
        Public Property Reference1() As String
            Get
                Return _Reference1
            End Get
            Set(ByVal value As String)
                _Reference1 = value
            End Set
        End Property
        Public Property Reference2() As String
            Get
                Return _Reference2
            End Get
            Set(ByVal value As String)
                _Reference2 = value
            End Set
        End Property
        Public Property AccountCode() As String
            Get
                Return _AccountCode
            End Get
            Set(ByVal value As String)
                _AccountCode = value
            End Set
        End Property
        Public Property Line_ID() As Integer
            Get
                Return _Line_ID
            End Get
            Set(ByVal value As Integer)
                _Line_ID = value
            End Set
        End Property
        Public Property ProjectCode() As String
            Get
                Return _ProjectCode
            End Get
            Set(ByVal value As String)
                _ProjectCode = value
            End Set
        End Property
        Public Property TaxDate() As DateTime
            Get
                Return _TaxDate
            End Get
            Set(ByVal value As DateTime)
                _TaxDate = value
            End Set
        End Property




#End Region

        Public Sub New()
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _htUserField = New Hashtable

        End Sub

#Region "User Defined Field"
        Public Sub SetUserField(ByVal FieldName As String, ByVal Value As Object)
            If _htUserField.ContainsKey(FieldName) Then
                _htUserField(FieldName) = Value
            Else
                _htUserField.Add(FieldName, Value)
            End If

        End Sub

        Public Sub SetUserField(ByVal _UserField As SAPbobsCOM.UserFields)
            _htUserField.Clear()
            For i As Integer = 0 To _UserField.Fields.Count - 1
                _htUserField.Add(_UserField.Fields.Item(i).Name, _UserField.Fields.Item(i).Value)
            Next
        End Sub


        Public ReadOnly Property UserField() As Hashtable
            Get
                Return _htUserField
            End Get

        End Property
#End Region
#Region "Process"

#End Region

    End Class
End Namespace
