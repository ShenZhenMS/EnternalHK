Imports SAPbobsCOM
Imports CPSLIB.Data
Imports CPSLIB.DataInterface.Company

Namespace DataInterface.Finiancial.BP
    Public Class BP
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _BP As SAPbobsCOM.BusinessPartners
        Private _diCompany As DICompany
        Private _diCompanys As DICompanies
        Private _Status As Document.Document.DocumentStatus
        ' User Define Field
        Private _htUserField As Hashtable

        Public Enum BPType
            None = 0
            Vendor = 1
            Customer = 2
            Lead = 3
        End Enum

        Private _CardCode As String
        Private _CardName As String
        Private _CardType As BPType

        Public Property CardType() As BPType
            Get
                Return _CardType
            End Get
            Set(ByVal value As BPType)
                _CardType = value
                
            End Set
        End Property

        Public Property CardName() As String
            Get
                Return _CardName
            End Get
            Set(ByVal value As String)
                _CardName = value
            End Set
        End Property

        Public Property CardCode() As String
            Get
                Return _CardCode
            End Get
            Set(ByVal value As String)
                _CardCode = value
            End Set
        End Property


        Public Sub New(ByVal _diCompany As DICompany)
            Me.New(_diCompany, BPType.None, String.Empty)
        End Sub

        Public Sub New(ByVal _diCompany As DICompany, ByVal _BPType As BPType)
            Me.New(_diCompany, _BPType, String.Empty)
        End Sub

        Public Sub New(ByVal _diCompany As DICompany, ByVal _BPType As BPType, ByVal _CardCode As String, Optional ByVal _CardName As String = "")
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _Status = Document.Document.DocumentStatus.Add
            Me._diCompany = _diCompany
            Me._CardType = _BPType
            Me._CardCode = _CardCode
            Me._CardName = _CardName
            Init()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub
#Region "Init"
        Public Function SearchBP() As Boolean

            If Validation.IsNull(_CardCode) <> String.Empty Then
                If _BP.GetByKey(_CardCode) Then
                    _Status = Document.Document.DocumentStatus.Update
                Else
                    _Status = Document.Document.DocumentStatus.Add
                End If

            End If
        End Function

        Private Sub Init()
            Try
                If _diCompany.Connected = False Then
                    _diCompany.Connect()
                End If
                _BP = _diCompany.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners)
                SearchBP()
                Select Case _CardType
                    Case BPType.Customer
                        _BP.CardType = BoCardTypes.cCustomer
                    Case BPType.Lead
                        _BP.CardType = BoCardTypes.cLid
                    Case BPType.Vendor
                        _BP.CardType = BoCardTypes.cSupplier
                End Select

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

#End Region
#Region "Property"
        Public ReadOnly Property BusinessParnter() As BusinessPartners
            Get
                Return _BP
            End Get

        End Property
#End Region
#Region "User Field"
        Public Sub SetUDF(ByVal Field As String, ByVal value As Object)
            If _htUserField Is Nothing Then
                _htUserField = New Hashtable
            End If
            _htUserField(Field) = value
        End Sub
        Public Sub SetUDF(ByVal _UserField As SAPbobsCOM.UserFields)
            _htUserField.Clear()
            For i As Integer = 0 To _UserField.Fields.Count - 1
                _htUserField.Add(_UserField.Fields.Item(i).Name, _UserField.Fields.Item(i).Value)
            Next
        End Sub
#End Region
#Region "Execute"

#End Region
    End Class
End Namespace
