Imports SAPbobsCOM
Imports CPSLIB.DataInterface.Company

Namespace DataInterface.UDO
    Public Class UDODocuments
        Private _CPSExeption As CPSException
        Private _Debug As CPSLIB.Debug
        Private _isError As Boolean
        Private _hasException As Boolean
        Private _Message As String

        Private _UDOHeaderName As String
        Private _UDOChildName As String

        Private _oDocGeneralService As GeneralService
        Private _oDocGeneralData As GeneralData
        Private _oDocGeneralLineData As GeneralData
        Private _oDocLineGeneralData As GeneralData
        Private _oDocLineCollection As GeneralDataCollection

        Private _DICompany As DataInterface.Company.DICompany
        Private _CompService As SAPbobsCOM.CompanyService

        Private _DocStatus As DataInterface.Document.Document.DocumentStatus


        Public Sub New(ByVal _DICompany As DataInterface.Company.DICompany, ByVal _UDOHeader As String, ByVal _UDOChild As String)

            _DocStatus = Document.Document.DocumentStatus.Add
            _isError = False
            _hasException = False
            _Message = ""
            _CPSExeption = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _UDOHeaderName = _UDOHeader
            _UDOChildName = _UDOChild
            Me._DICompany = _DICompany
            ' Get Service
            Try
                _CompService = Me._DICompany.Company.GetCompanyService
                _hasException = False
                _isError = False
            Catch ex As Exception
                _hasException = True
                _isError = True
            End Try

            RetriveUDOObject()
        End Sub

#Region "Property"

        Public ReadOnly Property isError() As Boolean
            Get
                Return _isError
            End Get
        End Property
        Public ReadOnly Property hasException() As Boolean
            Get
                Return _hasException
            End Get
        End Property
        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property
        Public ReadOnly Property LineCount() As Integer
            Get
                Return _oDocLineCollection.Count
            End Get
        End Property
        Public ReadOnly Property Rows() As GeneralDataCollection
            Get
                Return _oDocLineCollection
            End Get
        End Property
        Public ReadOnly Property DocMode() As DataInterface.Document.Document.DocumentStatus
            Get
                Return _DocStatus
            End Get
        End Property
#End Region
#Region "Proces"
        Public Function Delete(ByVal _strFieldName As String, ByVal _val As Object) As Boolean
            Dim _ParamResult As SAPbobsCOM.GeneralDataParams = _oDocGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams)
            _ParamResult.SetProperty(_strFieldName, _val)
            Dim ret As Boolean = False
            Try
                _oDocGeneralService.Delete(_ParamResult)


                _isError = False
                _hasException = False
                ret = True
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
            Return ret

        End Function
        Public Function Execute() As Boolean
            Dim _ParamResult As SAPbobsCOM.GeneralDataParams
            Dim ret As Boolean = False
            Try


                Select Case _DocStatus
                    Case Document.Document.DocumentStatus.Add

                        _ParamResult = _oDocGeneralService.Add(_oDocGeneralData)

                    Case Document.Document.DocumentStatus.Update
                        _oDocGeneralService.Update(_oDocGeneralData)
                    Case Else
                        'nothing to do
                End Select

                _isError = False
                _hasException = False
                ret = True
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
            Return ret

        End Function
#End Region
#Region "Header"
        Public Sub SetData(ByVal strField As String, ByVal val As Object)
            Try
                _oDocGeneralData.SetProperty(strField, val)
                _hasException = False
                _isError = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
        End Sub
        Public Sub SetData(ByVal _htData As Hashtable)
            Try
                For Each key As Object In _htData.Keys
                    _oDocGeneralData.SetProperty(key.ToString, _htData(key.ToString))
                Next
                _hasException = False
                _isError = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try

        End Sub
#End Region
#Region "Row"
        Public Sub gotoLine(ByVal i As Integer)
            Try
                _oDocGeneralLineData = _oDocLineCollection.Item(i)
                _isError = False
                _hasException = False

            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message

            End Try

        End Sub
        Public Sub AddLine()
            Try

                _oDocGeneralLineData = _oDocLineCollection.Add()
                _hasException = False
                _isError = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
        End Sub
        Public Sub RemoveLine(ByVal i As Integer)
            Try
                _oDocLineCollection.Remove(i)
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
        End Sub
        Public Sub AddLine(ByVal _htData As Hashtable)
            Try
                AddLine()
                For Each key As Object In _htData.Keys
                    _oDocGeneralLineData.SetProperty(key.ToString, _htData(key.ToString))
                Next
            Catch ex As Exception

            End Try
        End Sub


        Public Sub SetLineData(ByVal strField As String, ByVal val As Object)
            Try
                _oDocGeneralLineData.SetProperty(strField, val)
                _isError = False
                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
        End Sub
        
        Public Sub SetLine(ByVal linenum As Integer)
            Try
                _oDocGeneralLineData = _oDocLineCollection.Item(linenum)
                _isError = False
                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
        End Sub

        Public Function SearchLineData(ByVal _htCondition As Hashtable, ByVal setLine As Boolean) As Integer
            Dim ret As Integer = -1
            Dim _found As Boolean = False
            For i As Integer = 0 To _oDocLineCollection.Count - 1

                For Each key As Object In _htCondition.Keys
                    If _oDocLineCollection.Item(i).GetProperty(key.ToString) = _htCondition(key.ToString) Then
                        _found = True
                    Else
                        _found = False
                    End If
                Next
                If _found Then
                    ret = i
                    Exit For
                End If
            Next
            If ret > -1 And setLine Then
                _oDocGeneralLineData = _oDocLineCollection.Item(ret)
            End If
            Return ret

        End Function
        Public Function SearchLineData(ByVal strField As String, ByVal val As Object, ByVal setLine As Boolean) As Integer
            Dim ret As Integer = -1
            For i As Integer = 0 To _oDocLineCollection.Count - 1
                If _oDocLineCollection.Item(i).GetProperty(strField) = val Then
                    ret = i
                    Exit For
                End If
            Next
            If ret > -1 And setLine Then
                _oDocGeneralLineData = _oDocLineCollection.Item(ret)
            End If
            Return ret

        End Function
        Public Function GetLineData(ByVal linenum As Integer, ByVal strField As String) As Object
            Dim ret As Object
            Try
                ret = _oDocLineCollection.Item(linenum).GetProperty(strField)
                _isError = False
                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
            End Try
            Return ret
        End Function
#End Region
#Region "Define Status"
        Public Sub SetAsNew()
            Me._DocStatus = Document.Document.DocumentStatus.Add
            RetriveUDOObject()
        End Sub


        Public Function FindByKey(ByVal _FieldName As String, ByVal val As Object) As Boolean
            Dim ret As Boolean
            Dim _oGeneralParameter As GeneralDataParams
            Dim _tDocGeneralData As GeneralData

            Try

                _oGeneralParameter = CType(_oDocGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralDataParams), GeneralDataParams)
                _oGeneralParameter.SetProperty(_FieldName, val)
                _tDocGeneralData = _oDocGeneralService.GetByParams(_oGeneralParameter)

                If Not _tDocGeneralData Is Nothing Then
                    _oDocGeneralData = _tDocGeneralData
                    _DocStatus = Document.Document.DocumentStatus.Update
                    _oDocLineCollection = CType(_oDocGeneralData.Child(_UDOChildName), GeneralDataCollection)
                Else
                    _DocStatus = Document.Document.DocumentStatus.Add
                End If
                ret = True
                _isError = False
                _hasException = False
            Catch ex As Exception
                _hasException = True
                _isError = True
                _CPSExeption.ExecuteHandle(ex)
                _Message = _Message & "Exception: " & ex.Message
                ret = False
            End Try
            Return ret
        End Function

#End Region
        Private Sub RetriveUDOObject()
            ' Get Header UDO
            Try

                _oDocGeneralService = _CompService.GetGeneralService(_UDOHeaderName)
                _oDocGeneralData = _oDocGeneralService.GetDataInterface(GeneralServiceDataInterfaces.gsGeneralData)
                'Child
                _oDocLineCollection = CType(_oDocGeneralData.Child(_UDOChildName), GeneralDataCollection)

                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _Message = _Message & "Exception: " & ex.Message
                _CPSExeption.ExecuteHandle(ex)
            End Try
        End Sub
    End Class

End Namespace
