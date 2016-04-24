Imports CPSLIB.Settings
Imports CPSLIB.Programming

Public Class CPSException

    Private _Program_Config As Programming.Program_Config
    Private _Path As String
    Private _SubFolderMethod As SubFolderMethod
    Private _Enable As Boolean
    Private _ExceptionFile As String
    Private _DirInfo As IO.Directory.Directory
    Private _AsciiFile As IO.Ascii.AsciiFile
    Private _HTMLFile As Programming.Debug.HtmlFile
    Private _FileType As CPSLIB.Debug.FileType
    Public Enum SubFolderMethod
        None = 0
        System_User = 1
        System_Date = 2
        ' No Effect Below
        Other = 3
    End Enum

    Public Sub New()

        _Program_Config = New Programming.Program_Config
        If _Program_Config.hasFile Then
            Select Case _Program_Config.getValue(Programming.Program_Config._SECTION_EXCEPTION, Programming.Program_Config._KEY_EXCEPTION_SUBFOLDERTYPE)
                Case 0
                    _SubFolderMethod = SubFolderMethod.None
                Case 1
                    _SubFolderMethod = SubFolderMethod.System_User
                Case 2
                    _SubFolderMethod = SubFolderMethod.System_Date
                Case 3
                    _SubFolderMethod = SubFolderMethod.Other
                Case Else

            End Select
            _Path = _Program_Config.getValue(Programming.Program_Config._SECTION_EXCEPTION, Programming.Program_Config._KEY_EXCEPTION_PATH)
            _Enable = (_Program_Config.getValue(Programming.Program_Config._SECTION_EXCEPTION, Programming.Program_Config._KEY_EXCEPTION_ENABLE) = "Y")
            Select Case _Program_Config.getValue(Programming.Program_Config._SECTION_EXCEPTION, Programming.Program_Config._KEY_DEBUG_FILE_TYPE).ToUpper
                Case "FILE"
                    _FileType = CPSLIB.Debug.FileType.Ascii
                Case "HTML"
                    _FileType = CPSLIB.Debug.FileType.HTML
                Case Else
                    _FileType = CPSLIB.Debug.FileType.Ascii
            End Select

        Else
            'Default Parameter
            _SubFolderMethod = SubFolderMethod.System_Date

            _Path = Settings.File.Consts._Default_Setting_File_Path
            _Enable = False
        End If
        If _Enable Then
            ' Debug File Path
            _ExceptionFile = _Path & "\"
            ' SubDirectory
            Select Case _SubFolderMethod
                Case SubFolderMethod.System_Date
                    _ExceptionFile = _ExceptionFile & DateTime.Now.ToString("yyyy-MM-dd") & "\"
                Case SubFolderMethod.System_User
                    _ExceptionFile = _ExceptionFile & System.Environment.UserName & "\"
                Case SubFolderMethod.Other

                Case SubFolderMethod.None
                Case Else

            End Select
            _DirInfo = New IO.Directory.Directory(_ExceptionFile, True)
            ' Exception File Name
            '_ExceptionFile = _ExceptionFile & "Exception-" & Convert.ToInt32(_DirInfo.FileCount + 1).ToString("C5") & ".excpt"
            Select Case _FileType
                Case CPSLIB.Debug.FileType.Ascii
                    _ExceptionFile = _ExceptionFile & "ExceptionLog" & DateTime.Now.ToString("yyyyMMddhh0000") & ".ext"
                Case CPSLIB.Debug.FileType.HTML
                    _ExceptionFile = _ExceptionFile & "ExceptionLog" & DateTime.Now.ToString("yyyyMMddhh0000") & ".html"
            End Select





            ' Set Debug File
            Select Case _FileType
                Case CPSLIB.Debug.FileType.Ascii
                    _AsciiFile = New IO.Ascii.AsciiFile(_ExceptionFile)
                Case CPSLIB.Debug.FileType.HTML
                    _HTMLFile = New Programming.Debug.HtmlFile(_ExceptionFile)
            End Select


        End If
    End Sub
    Public Sub ExecuteHandle(ByVal ex As Exception)
        Write(ex, String.Empty)
    End Sub
    Public Sub ExecuteHandle(ByVal ex As Exception, ByVal Title As String)
        Write(ex, Title)
    End Sub
    Private Sub Write(ByVal ex As Exception, ByVal Title As String)
        If _Enable Then
            Select Case _FileType
                Case CPSLIB.Debug.FileType.Ascii

                    _AsciiFile.WriteLine("###############################################")
                    _AsciiFile.WriteLine("#EXCEPTION")
                    If Title <> String.Empty Then
                        _AsciiFile.WriteLine("#Title: " & Title)
                    Else

                    End If
                    _AsciiFile.WriteLine("###############################################")
                    _AsciiFile.WriteLine("Message: " & vbTab & ex.Message)
                    _AsciiFile.WriteLine("Trace: " & vbTab & ex.StackTrace)
                    _AsciiFile.WriteLine("Source: " & vbTab & ex.Source)
                    _AsciiFile.WriteLine("###############################################")
                    _AsciiFile.WriteLine("###############################################")
                Case CPSLIB.Debug.FileType.Ascii
                    If Title <> String.Empty Then
                        _HTMLFile.RecordException(ex, "Exception: " & Title)
                    Else
                        _HTMLFile.RecordException(ex, "Exception")
                    End If

            End Select

        End If
    End Sub
End Class

