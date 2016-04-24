Namespace DataInterface.Document
    Public Class Document_Line
        ' Standard Fields

        Private _ItemCode As String
        Private _Quantity As Decimal
        Private _UnitPrice As Decimal
        Private _Discount As Decimal
        Private _Warehouse As String
        Private _Account As String
        Private _Project As String
        Private _ProfitCode1 As String
        Private _ProfitCode2 As String
        Private _ProfitCode3 As String
        Private _ProfitCode4 As String
        Private _ProfitCode5 As String

        ' User Defined Field
        Private _htUDF As Hashtable

        Public Sub New()
            _htUDF = New Hashtable

        End Sub

        Public Property ProfitCode5() As String
            Get
                Return _ProfitCode5
            End Get
            Set(ByVal value As String)
                _ProfitCode5 = value
            End Set
        End Property

        Public Property ProfitCode4() As String
            Get
                Return _ProfitCode4
            End Get
            Set(ByVal value As String)
                _ProfitCode4 = value
            End Set
        End Property

        Public Property ProfitCode3() As String
            Get
                Return _ProfitCode3
            End Get
            Set(ByVal value As String)
                _ProfitCode3 = value
            End Set
        End Property

        Public Property ProfitCode2() As String
            Get
                Return _ProfitCode2
            End Get
            Set(ByVal value As String)
                _ProfitCode2 = value
            End Set
        End Property

        Public Property ProfitCode1() As String
            Get
                Return _ProfitCode1
            End Get
            Set(ByVal value As String)
                _ProfitCode1 = value
            End Set
        End Property

        Public Property Project() As String
            Get
                Return _Project
            End Get
            Set(ByVal value As String)
                _Project = value
            End Set
        End Property


        Public Property Account() As String
            Get
                Return _Account
            End Get
            Set(ByVal value As String)
                _Account = value
            End Set
        End Property


        Public Property Warehouse() As String
            Get
                Return _Warehouse
            End Get
            Set(ByVal value As String)
                _Warehouse = value
            End Set
        End Property


        Public Property Discount() As Decimal
            Get
                Return _Discount
            End Get
            Set(ByVal value As Decimal)
                _Discount = value
            End Set
        End Property

        Public Property UnitPrice() As Decimal
            Get
                Return _UnitPrice
            End Get
            Set(ByVal value As Decimal)
                _UnitPrice = value
            End Set
        End Property

        Public Property Quantity() As Decimal
            Get
                Return _Quantity
            End Get
            Set(ByVal value As Decimal)
                _Quantity = value
            End Set
        End Property

        Public Property ItemCode() As String
            Get
                Return _ItemCode
            End Get
            Set(ByVal value As String)
                _ItemCode = value
            End Set
        End Property


        ' Set UDF
        Public Sub setUDF(ByVal name As String, ByVal value As Object)
            If _htUDF Is Nothing Then
                _htUDF = New Hashtable
            End If
            _htUDF(name) = value
        End Sub

        Public ReadOnly Property UserFields() As Hashtable
            Get
                Return _htUDF
            End Get
        End Property

    End Class
End Namespace
