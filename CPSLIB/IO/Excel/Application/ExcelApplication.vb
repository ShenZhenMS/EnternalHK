Imports Microsoft.Office.Interop.Excel

Namespace IO.Excel
    ''' <summary>
    ''' Excel Version: 2007
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ExcelApplication
        Private _fi As System.IO.FileInfo
        Private _FullName As String
        Private _App As Microsoft.Office.Interop.Excel.Application
        Private _xlBook As Microsoft.Office.Interop.Excel.Workbook
        Private _WorkSheet As Microsoft.Office.Interop.Excel.Worksheet
        Private _CPSException As CPSException
        Private _Debug As CPSLIB.Debug
        Private _hasException As Boolean
        Private _isError As Boolean
        Private _isMultiSheets As Boolean

        Public Sub New(ByVal FullName As String)
            _fi = New System.IO.FileInfo(FullName)
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)

            _App = New Microsoft.Office.Interop.Excel.Application
            If _fi.Exists Then
                init()
            Else

            End If


        End Sub

#Region "Property"
        Public ReadOnly Property IOInfo() As System.IO.FileInfo
            Get
                Return _fi
            End Get
        End Property

        Public ReadOnly Property WorkSheetCount() As Integer
            Get
                Return _xlBook.Worksheets.Count
            End Get
        End Property

        Public ReadOnly Property isMultiSheet() As Boolean
            Get
                Return _xlBook.Worksheets.Count > 1
            End Get
        End Property

        Public ReadOnly Property WorkBook() As Microsoft.Office.Interop.Excel.Workbook
            Get
                Return _xlBook
            End Get
        End Property
#End Region
        Private Sub init()
            Try
                _xlBook = GetObject(_FullName)
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)

            End Try
        End Sub

        Public Function SetWorkSheet(ByVal index As Object) As Worksheet
            Try
                _WorkSheet = _xlBook.Worksheets(index)
                _hasException = False
                Return _WorkSheet
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
                _hasException = True
                Return Nothing
            End Try

        End Function

        Public ReadOnly Property WorkSheet() As Worksheet
            Get
                Return _WorkSheet
            End Get
        End Property

#Region "Process"
        Public Sub Save()

        End Sub
        Public Sub SaveAs()

        End Sub
        Public Sub Quit()
            Try
                _App.Quit()
                _hasException = False
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
                _hasException = True
            End Try
        End Sub
#End Region
    End Class
End Namespace
