
Imports Microsoft.Office.Interop.Excel
Namespace IO.Excel
    Public Class ExcelTable : Inherits ExcelApplication
        Private _CPSException As CPSException
        Private _Debug As CPSLIB.Debug
        Private _dtExcel As System.Data.DataTable
        Private _FullPath As String

        Private _TitleRow As Integer = Consts.Default_Title_Row
        Private _FieldRow As Integer = Consts.Default_Field_Row
        Private _DataRow As Integer = Consts.Default_Data_Row ' Default Value
        Private _DataColumn As Integer = Consts.Default_Data_Column
        Private _MaxDataRow As Integer = Consts.Default_Maximum_Data_Row
        Private _MaxColumn As Integer = Consts.Default_Maximum_Column
        Private _AllowSkipColumn As Boolean = Consts.Default_Allow_SkipColumn
        Private _AllowSkipRow As Boolean = Consts.Default_Allow_SkipRow

        Private _htTable As Hashtable
        Private _htColumn As Hashtable


        Public Sub New(ByVal FullPath As String)

            Me.New(FullPath, Consts.Default_Title_Row, Consts.Default_Field_Row, Consts.Default_Data_Row, Consts.Default_Data_Column)

        End Sub
        Public Sub New(ByVal FullPath As String, ByVal _TitleRow As Integer, ByVal _FieldRow As Integer, ByVal _DataRow As Integer, ByVal _DataColumn As Integer)
            MyBase.New(FullPath)
            _FullPath = FullPath
            TitleRow = _TitleRow
            FieldRow = _FieldRow
            DataRow = _DataRow
            DataColumn = _DataColumn
            _htTable = New Hashtable
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
        End Sub

#Region "Read"
        Public Function Read(ByVal index As Object) As DataTable
            Dim _BlankRow As Boolean
            Dim _dr As DataRow
            CreateStructure(index)
            ' Start Read
            _dr = _dtExcel.NewRow
            For i As Integer = _DataRow To _MaxDataRow
                _BlankRow = False
                For Each o As Object In _htColumn.Keys
                    If CType(MyBase.WorkSheet.Cells(i, o), Range).Value = String.Empty Then
                        _BlankRow = True
                    Else
                        _BlankRow = False
                    End If
                    _dr(_htColumn(o)) = CType(MyBase.WorkSheet.Cells(i, o), Range).Value
                Next
                If _BlankRow Then
                    If Not AllowSkipRow Then
                        Exit For
                    End If
                Else
                    _dtExcel.Rows.Add(_dr)
                End If
            Next

            _htTable(index) = _dtExcel
            Return _dtExcel
        End Function

        Public Function Read() As Hashtable

            If MyBase.IOInfo.Exists Then
                Try
                    For i As Integer = 0 To MyBase.WorkSheetCount - 1
                        SetWorkSheet(i)
                        Read(MyBase.WorkSheet.Name)
                    Next
                Catch ex As Exception

                End Try
            End If
            Return _htTable
        End Function

#End Region

#Region "Write"

#End Region

#Region "Property"


        Public Property AllowSkipRow() As Boolean
            Get
                Return _AllowSkipRow
            End Get
            Set(ByVal value As Boolean)
                _AllowSkipRow = value
            End Set
        End Property


        Public Property AllowSkipColumn() As Boolean
            Get
                Return _AllowSkipColumn
            End Get
            Set(ByVal value As Boolean)
                _AllowSkipColumn = value
            End Set
        End Property

        Public Property MaxDataRow() As Integer
            Get
                Return _MaxDataRow
            End Get
            Set(ByVal value As Integer)
                _MaxDataRow = value
            End Set
        End Property


        Public Property DataColumn() As Integer
            Get
                Return _DataColumn
            End Get
            Set(ByVal value As Integer)
                _DataColumn = value
            End Set
        End Property

        Public Property DataRow() As Integer
            Get
                Return _DataRow
            End Get
            Set(ByVal value As Integer)
                _DataRow = value
            End Set
        End Property

        Public Property FieldRow() As Integer
            Get
                Return _FieldRow
            End Get
            Set(ByVal value As Integer)
                _FieldRow = value
            End Set
        End Property

        Public Property TitleRow() As Integer
            Get
                Return _TitleRow
            End Get
            Set(ByVal value As Integer)
                _TitleRow = value
            End Set
        End Property
#End Region

#Region "Data Table"
        Private Sub CreateStructure(ByVal index As Object)
            _htColumn = New Hashtable
            SetWorkSheet(index)
            _dtExcel = New System.Data.DataTable
            For i As Integer = Me._DataColumn To Me._MaxColumn
                If CType(Worksheet.Cells(Me._FieldRow, i), Range).Value <> String.Empty Then
                    _dtExcel.Columns.Add(CType(Worksheet.Cells(Me._FieldRow, i), Range).Value)
                    _htColumn.Add(i, CType(Worksheet.Cells(Me._FieldRow, i), Range).Value)
                Else
                    If Not _AllowSkipColumn Then
                        Exit For
                    End If
                End If
            Next

        End Sub
#End Region
    End Class
End Namespace
