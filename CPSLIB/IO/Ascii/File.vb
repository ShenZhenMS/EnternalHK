Imports System.IO
Imports CPSLIB.Settings

Namespace IO.Ascii
    Public Class AsciiFile

        Dim _FullFileName As String
        Dim _FilePath As String
        Dim _FileName As String
        Dim _Fi As FileInfo
        Dim _FileExists As Boolean
        Dim _sr As StreamReader
        Dim _sw As StreamWriter
        Dim _ReadOnly As Boolean

        
        Dim _lineContent As ArrayList

        Public Shared Error_Msg_EMPTYFILE As String = "Preivous function is providing EMPTY file."
        Public Shared Error_Msg_EXCEPTION As String = "ASCII File Exception: {0}"


        Public Sub New(ByVal _FullFileName As String)
            
            _Error = False
            _ErrorMessage = String.Empty
            If _FullFileName <> String.Empty Then

                Me._FullFileName = _FullFileName
            Else
                'Write Log
                _Error = True
                _ErrorMessage = AsciiFile.Error_Msg_EMPTYFILE
            End If
            Initizial()
        End Sub

#Region "Process"

        Private Function Initizial() As Boolean
            Try
                _Fi = New FileInfo(_FullFileName)
                If _Fi.Exists Then
                    _FilePath = _Fi.Directory.FullName
                    _FileName = _Fi.Name
                    _ReadOnly = _Fi.IsReadOnly
                    ' Get Content when file exists
                    ReadFileContent()
                Else
                    ' File Does Not Found

                    '_Fi.Create()
                    If _Fi.Directory.Exists = False Then
                        _Fi.Directory.Create()

                    End If
                End If

            Catch ex As Exception
                ' Throw CPSLIB Exception
                _Error = True
                _ErrorMessage = String.Format(AsciiFile.Error_Msg_EXCEPTION, ex.Message)
            End Try
        End Function



        Private Sub ReadFileContent()

            _lineContent = New ArrayList
            Try

                _sr = New StreamReader(_Fi.FullName, System.Text.Encoding.ASCII)

                While _sr.Peek() > -1
                    _lineContent.Add(_sr.ReadLine())

                End While
                _sr.Close()
            Catch ex As Exception
                ' Throw CPSLib Exception
                _Error = True
                _ErrorMessage = String.Format(AsciiFile.Error_Msg_EXCEPTION, ex.Message)
            End Try

        End Sub

        Public Sub WriteLine(ByVal s As String)
            Dim _sw As StreamWriter
            Try

                'If Not _Fi.IsReadOnly Then
                _sw = New StreamWriter(_Fi.FullName, True, System.Text.Encoding.UTF8)
                _sw.WriteLine(s)
                _sw.Close()
                'End If
            Catch ioex As IOException
                _Error = True
                _ErrorMessage = String.Format(AsciiFile.Error_Msg_EXCEPTION, ioex.Message)
            Catch ex As Exception
                _Error = True
                _ErrorMessage = String.Format(AsciiFile.Error_Msg_EXCEPTION, ex.Message)
            End Try




        End Sub
#End Region
#Region "Property"

        Private _Error As Boolean

        Private _ErrorMessage As String
        Public Property ErrorMessage() As String
            Get
                Return _ErrorMessage
            End Get
            Set(ByVal value As String)
                _ErrorMessage = value
            End Set
        End Property

        Public Property HasError() As Boolean
            Get
                Return _Error
            End Get
            Set(ByVal value As Boolean)
                _Error = value
            End Set
        End Property

        Public ReadOnly Property Information() As FileInfo
            Get
                Return _Fi
            End Get
        End Property

        Public ReadOnly Property StreamReader() As StreamReader
            Get
                Return _sr
            End Get
        End Property

        Public ReadOnly Property FileContent() As ArrayList
            Get
                Return _lineContent
            End Get
        End Property

#End Region
    End Class
End Namespace
