Namespace DataInterface.UDF
    Public Class UDF
        Private _UDFName As String
        Private _UDFDesc As String
        Private _UDFSize As Integer
        Private _UDFType As SAPbobsCOM.BoFieldTypes
        Private _UDFValidValue As Hashtable
        Private _UDFTBLName As String
        Private _UDFDFTValue As Object



        Public Sub New()
            _UDFValidValue = New Hashtable
        End Sub

        Public Function Execute(ByVal DICompany As DataInterface.Company.DICompany) As Boolean
            Dim _ret As Boolean = True
            Dim lRetCode As Integer
            Dim oUserFieldsMD As SAPbobsCOM.UserFieldsMD


            oUserFieldsMD = DICompany.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)

            Try
                oUserFieldsMD.TableName = _UDFTable
                oUserFieldsMD.Name = _UDFName
                oUserFieldsMD.Description = _UDFDesc
                oUserFieldsMD.Type = _UDFType
                oUserFieldsMD.Size = _UDFSize


                '// Adding the Field to the Table
                lRetCode = oUserFieldsMD.Add

                '// Check for errors
                If lRetCode <> 0 Then
                    _ret = False
                Else
                    _ret = True
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD)
                oUserFieldsMD = Nothing
                GC.Collect()
            Catch ex As Exception
                _ret = False
            End Try
            Return _ret
        End Function

        Public Property UDFDFTValue() As Object
            Get
                Return _UDFDFTValue
            End Get
            Set(ByVal value As Object)
                _UDFDFTValue = value
            End Set
        End Property


        Private _UDFTable As String
        Public Property UDFTable() As String
            Get
                Return _UDFTable
            End Get
            Set(ByVal value As String)
                _UDFTable = value
            End Set
        End Property

        Public Property UDFName() As String
            Get
                Return _UDFName
            End Get
            Set(ByVal value As String)
                _UDFName = value
            End Set
        End Property
        Public Property UDFDesc() As String
            Get
                Return _UDFDesc
            End Get
            Set(ByVal value As String)
                _UDFDesc = value
            End Set
        End Property

        Public Property UDFSize() As Integer
            Get
                Return _UDFSize
            End Get
            Set(ByVal value As Integer)
                _UDFSize = value
            End Set
        End Property
        Public Property UDFType() As SAPbobsCOM.BoFieldTypes
            Get
                Return _UDFType
            End Get
            Set(ByVal value As SAPbobsCOM.BoFieldTypes)
                _UDFType = value
            End Set
        End Property
        Public Sub ValidValue(ByVal value As Object, ByVal Desc As String)
            If _UDFValidValue Is Nothing Then
                _UDFValidValue = New Hashtable
            End If
            _UDFValidValue(value) = Desc
        End Sub
        Public ReadOnly Property Value() As Hashtable
            Get
                Return _UDFValidValue
            End Get
        End Property


    End Class
End Namespace
