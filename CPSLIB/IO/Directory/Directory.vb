Imports System.IO
Namespace IO.Directory
    Public Class Directory
        Private _Path As String
        Private _DirInfo As DirectoryInfo
        Public Sub New(ByVal _Path As String)
            Me.New(_Path, False)
        End Sub
        Public Sub New(ByVal _Path As String, ByVal _Create As Boolean)
            Me._Path = _Path
            Try
                _DirInfo = New DirectoryInfo(_Path)
                If _DirInfo.Exists = False Then
                    If _Create Then
                        _DirInfo.Create()
                    End If
                End If
            Catch ex As Exception

            End Try


        End Sub

#Region "Property"
        Public ReadOnly Property Information() As DirectoryInfo
            Get
                Return _DirInfo

            End Get
        End Property
        Public ReadOnly Property FileInfo() As FileInfo()
            Get
                Return _DirInfo.GetFiles

            End Get
        End Property
        Public ReadOnly Property FileInfo(ByVal _patten As String) As FileInfo()
            Get
                Return _DirInfo.GetFiles(_patten)
            End Get
        End Property
        Public ReadOnly Property FileCount() As Integer
            Get
                If _DirInfo Is Nothing Then
                    Return 0
                Else
                    _DirInfo.GetFiles.Count()
                End If
            End Get
        End Property

        Public ReadOnly Property CurrentDirLastFile() As String
            Get
                Return FileInfoByDate(SearchOption.TopDirectoryOnly)(FileInfoByDate(SearchOption.TopDirectoryOnly).Count - 1).FullName
                ' Return FileInfoByDate(SearchOption.TopDirectoryOnly)(0).FullName
            End Get
        End Property

        Public ReadOnly Property CurrentDirFirstFile() As String
            Get
                Return FileInfoByDate(SearchOption.TopDirectoryOnly)(0).FullName
                'Return FileInfoByDate(SearchOption.TopDirectoryOnly)(FileInfoByDate(SearchOption.TopDirectoryOnly).Count - 1).FullName
            End Get
        End Property

#End Region
        Public Function FileInfoByDate(ByVal _SrhOption As SearchOption) As FileInfo()
            Dim _FileInfo() As System.IO.FileInfo
            _FileInfo = _DirInfo.GetFiles("*.*", _SrhOption)
            Array.Sort(_FileInfo, New clsCompareFileInfo)
            Return _FileInfo

        End Function
    End Class

    Public Class clsCompareFileInfo
        Implements IComparer
        Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements IComparer.Compare
            Dim File1 As FileInfo
            Dim File2 As FileInfo

            File1 = DirectCast(x, FileInfo)
            File2 = DirectCast(y, FileInfo)

            Compare = DateTime.Compare(File1.LastWriteTime, File2.LastWriteTime)
        End Function
    End Class
End Namespace
