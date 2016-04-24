
Namespace DataInterface.Company
    Public Class CompanyInfo
        Private _DICompany As DICompany
        Private _CPSException As CPSException
        Private _Debug As CPSLib.Debug
        Private _rs As SAPbobsCOM.Recordset

        Private _SystemCurrency As String
        Private _LocalCurrency As String
        Private _CompanyName As String
        Private _Country As String

        Private _Information As SAPbobsCOM.Fields
        Public ReadOnly Property Information() As SAPbobsCOM.Fields
            Get
                Return _Information
            End Get

        End Property

        Public ReadOnly Property Country() As String
            Get
                Return _Country
            End Get

        End Property

        Public ReadOnly Property CompanyName() As String
            Get
                Return _CompanyName
            End Get

        End Property

        Public ReadOnly Property LocalCurrency() As String
            Get
                Return _LocalCurrency
            End Get

        End Property

        Public ReadOnly Property SystemCurrency() As String
            Get
                Return _SystemCurrency
            End Get

        End Property

        Public Sub New(ByVal DICompany As DICompany)
            _CPSException = New CPSException
            _Debug = New CPSLib.Debug(Me.GetType().ToString)

            _DICompany = DICompany
            If _DICompany.Connected = False Then
                _DICompany.Connect()
            End If
            Read()

        End Sub
        Private Sub Read()
            _rs = _DICompany.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            Try
                _rs.DoQuery(DataInterface.Consts.CompanyInformationQuery)
                If Not _rs.EoF Then
                    _Information = _rs.Fields
                    _CompanyName = _rs.Fields.Item(DataInterface.Consts.Field_Company_Name).Value
                    _Country = _rs.Fields.Item(DataInterface.Consts.Field_Company_Country).Value
                    _SystemCurrency = _rs.Fields.Item(DataInterface.Consts.Field_System_Currency).Value
                    _LocalCurrency = _rs.Fields.Item(DataInterface.Consts.Field_Local_Currency).Value
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)

            End Try
        End Sub
    End Class
End Namespace
