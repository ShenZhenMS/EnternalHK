Namespace DataInterface.Company
    Public Class DICompanies
        Private _Message As String
        Private _htDICompanies As Hashtable
        Private _msgcode As Logging.MessageCode
        Private _CPSException As CPSException

        Public Sub New()
            _CPSException = New CPSException
            _msgcode = New Logging.MessageCode()
            _htDICompanies = New Hashtable
        End Sub
        Public Sub Add(ByVal _oCompany As DICompany)

            If _htDICompanies.ContainsKey(_oCompany.CompanyDB) = False Then
                _htDICompanies.Add(_oCompany.CompanyDB, _oCompany)
            Else
                _htDICompanies(_oCompany.CompanyDB) = _oCompany
            End If

        End Sub
#Region "Process"
        Public Function Disconnect() As String
            Dim _dicompany As DataInterface.Company.DICompany
            If _htDICompanies.Count > 0 Then
                For i As Integer = 0 To _htDICompanies.Count - 1
                    Try

                        _dicompany = Me.Company(_htDICompanies.Keys(i))
                        If _dicompany.Connected Then
                            _dicompany.Disconnect()
                        End If

                        _htDICompanies(_dicompany.CompanyDB) = _dicompany

                        _Message = _Message & _htDICompanies.Keys(_dicompany.CompanyDB).ToString() & ":" & _dicompany.Message & vbCrLf


                    Catch ex As Exception
                        _Message = _Message & ex.Message & vbCrLf
                        _CPSException.ExecuteHandle(ex)
                        'Karrson: Remark: throw new CPSException(ex)
                    End Try
                Next


            Else
                _msgcode.Read(Logging.MessageCode.MessageCode.BLANK_COMPANY, _Message, vbCrLf)
            End If
            Return _Message
        End Function

        Public Sub Clear()
            _htDICompanies.Clear()
        End Sub
        Public Function Connect() As String
            Dim _dicompany As DICompany
            If _htDICompanies.Count > 0 Then
                For i As Integer = 0 To _htDICompanies.Count - 1
                    Try
                        _dicompany = Me.Company(_htDICompanies.Keys(i))
                        _dicompany.Connect()

                        If _dicompany.Connected Then
                            _htDICompanies(i) = _dicompany
                        Else
                            _Message = _Message & _htDICompanies.Keys(i).ToString() & ":" & _dicompany.Message & vbCrLf
                        End If

                    Catch ex As Exception
                        _Message = _Message & ex.Message & vbCrLf
                        'Karrson: Remark: throw new CPSException(ex)
                        _CPSException.ExecuteHandle(ex)
                    End Try
                Next
            Else
                _msgcode.Read(Logging.MessageCode.MessageCode.BLANK_COMPANY, _Message, vbCrLf)
            End If
            Return _Message
        End Function
#End Region
#Region "Property"
        Public ReadOnly Property Count() As Integer
            Get
                Return _htDICompanies.Count
            End Get
        End Property

        Public ReadOnly Property Companies() As Hashtable
            Get
                Return _htDICompanies
            End Get

        End Property
        Public Function toArray() As Object()

            Dim _alDICompany As ArrayList = New ArrayList

            If _htDICompanies.Count > 0 Then
                For Each _o As String In _htDICompanies.Keys
                    Try

                        _alDICompany.Add(CType(_htDICompanies(_o), DICompany))
                    Catch ex As Exception
                        _Message = _Message & ex.Message & vbCrLf
                    End Try
                Next
            End If
            Return _alDICompany.ToArray

        End Function

        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property

#End Region
        Public Function Company(ByVal strCompanyDB As String) As DICompany
            Return _htDICompanies(strCompanyDB)
        End Function
        Public Function Company(ByVal index As Integer) As DICompany
            Return CType(_htDICompanies(index), DICompany)
        End Function
    End Class
End Namespace
