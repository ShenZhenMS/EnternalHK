Imports CPSLIB.IO
Imports CPSLIB.Programming
Namespace CPSLIB
    Public Class Debug
        'Private _AsciiFile As Ascii.AsciiFile
        Public Enum FileType
            Ascii = 1
            HTML = 2
        End Enum


        Private _DF As Programming.Debug.File
        Private _HDF As Programming.Debug.HtmlFile
        Private _ConfigFile As Programming.Program_Config
        Private _Path As String
        Private _FileFormat As DebugFileFormat
        Private _SubFolderMethod As SubFolderMethod
        Private _Enable As Boolean
        Private _DebugFileName As String
        Private _DirInfo As IO.Directory.Directory
        Private _CurrentFunction As String
        Private _SeqCnt As Integer
        Private _ClassName As String
        Private _FileType As FileType
        Private _Extension As String = ""
        Private _XMLEnable As Boolean
        Private _XMLPath As String = ""


        Private _Database As String
        Public Property Database() As String
            Get
                Return _Database
            End Get
            Set(ByVal value As String)
                _Database = value
            End Set
        End Property


        Public Enum SubFolderMethod
            None = 0
            System_User = 1
            System_Date = 2
            Database_SystemDate = 4
            ' No Effect Below
            Other = 3
        End Enum

        Public Enum DebugFileFormat

            Sequence_HourMinutes = 1
            Sequence_HourMinutesSecond = 2
            Sequence_UserName = 3
            ' No Effect Below
            Sequence_Other = 4
            ' New
            Class_Hour = 5

        End Enum

        Public Enum LineType

            Information = 1
            Warning = 2
            [Error] = 3
        End Enum

        Public Enum LineInformation
            LineType_DateTime = 1
            LineType_Other = 2
        End Enum
        'Public Sub New()
        '    Me.New(String.Empty)
        'End Sub


        Public Sub New(ByVal _ClassName As String, Optional ByVal _Database As String = "")
            Me._Database = _Database
            _ConfigFile = New Programming.Program_Config
            If _ConfigFile.hasFile Then
                ReadConfig()
            Else
                ' Load Default Parameter
                _Enable = False
                _Path = Settings.File.Consts._Default_Setting_File_Path
                _SubFolderMethod = SubFolderMethod.Database_SystemDate
                _FileFormat = DebugFileFormat.Sequence_HourMinutes
                _FileType = FileType.HTML
                _XMLEnable = False
                _XMLPath = ""
            End If
            If _Enable Then
                Select Case _FileType
                    Case FileType.Ascii
                        _Extension = ".debug"
                    Case FileType.HTML
                        _Extension = ".html"
                End Select
                ' Debug File Path
                _DebugFileName = _Path & "\"
                ' SubDirectory

                Select Case _SubFolderMethod
                    Case SubFolderMethod.System_Date
                        _DebugFileName = _DebugFileName & DateTime.Now.ToString("yyyy-MM-dd") & "\"
                    Case SubFolderMethod.System_User
                        _DebugFileName = _DebugFileName & System.Environment.UserName & "\"
                    Case SubFolderMethod.Database_SystemDate
                        _DebugFileName = _DebugFileName & IIf(_Database <> String.Empty, _Database & "\", "") & DateTime.Now.ToString("yyyy-MM-dd") & "\"
                    Case SubFolderMethod.Other

                    Case SubFolderMethod.None
                    Case Else

                End Select

                _DirInfo = New IO.Directory.Directory(_DebugFileName, True)

                ' Debug File Name
                Select Case _FileFormat
                    Case DebugFileFormat.Sequence_HourMinutes
                        _SeqCnt = _DirInfo.FileInfo.Count + 1
                        If _ClassName <> String.Empty Then
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & DateTime.Now.ToString("yyyyMMddhhmm") & "_" & _ClassName & _Extension
                        Else
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & DateTime.Now.ToString("yyyyMMddhhmm") & _Extension
                        End If

                    Case DebugFileFormat.Sequence_HourMinutesSecond
                        _SeqCnt = _DirInfo.FileInfo.Count + 1
                        If _ClassName <> String.Empty Then
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & DateTime.Now.ToString("yyyyMMddhhmmss") & "_" & _ClassName & _Extension
                        Else
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & DateTime.Now.ToString("yyyyMMddhhmmss") & _Extension
                        End If


                    Case DebugFileFormat.Sequence_Other

                    Case DebugFileFormat.Sequence_UserName
                        _SeqCnt = _DirInfo.FileInfo.Count + 1
                        If _ClassName <> String.Empty Then
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & System.Environment.UserName & "_" & _ClassName & _Extension
                        Else
                            _DebugFileName = _DebugFileName & Convert.ToInt32(_SeqCnt).ToString("d5") & "_" & System.Environment.UserName & _Extension
                        End If
                    Case DebugFileFormat.Class_Hour

                        If _ClassName <> String.Empty Then
                            _DebugFileName = _DebugFileName & _ClassName & "_" & DateTime.Now.ToString("yyyyMMddhh0000") & _Extension
                        Else
                            _DebugFileName = _DebugFileName & DateTime.Now.ToString("yyyyMMddhh0000") & _Extension
                        End If
                End Select
                ' Set Debug File
                Select Case _FileType
                    Case FileType.Ascii
                        _DF = New Programming.Debug.File(_DebugFileName)
                    Case FileType.HTML
                        _HDF = New Programming.Debug.HtmlFile(_DebugFileName)
                End Select

                '_AsciiFile = New Ascii.AsciiFile(_DebugFile)

            End If ' _Enable = false

        End Sub
#Region "Config"
        Private Sub ReadConfig()


            Dim strEnable As String = _ConfigFile.getValue(Programming.Program_Config._SECTION_DEBUG, Programming.Program_Config._KEY_DEBUG_ENABLE)
            Select Case strEnable

                Case "Y"
                    _Enable = True

                Case Else
                    _Enable = False
            End Select

            _Path = _ConfigFile.getValue(Programming.Program_Config._SECTION_DEBUG, Programming.Program_Config._KEY_DEBUG_PATH)
            _FileFormat = _ConfigFile.getValue(Programming.Program_Config._SECTION_DEBUG, Programming.Program_Config._KEY_DEBUG_FILE_FORMAT)
            Select Case _ConfigFile.getValue(Programming.Program_Config._SECTION_DEBUG, Programming.Program_Config._KEY_DEBUG_FILE_TYPE).ToUpper
                Case "FILE"
                    _FileType = FileType.Ascii
                Case "HTML"
                    _FileType = FileType.HTML
                Case Else
                    _FileType = FileType.HTML
            End Select

            Select Case _ConfigFile.getValue(Programming.Program_Config._SECTION_DEBUG, Programming.Program_Config._KEY_DEBUG_SUBFOLDERTYPE)
                Case 0
                    _SubFolderMethod = SubFolderMethod.None
                Case 1
                    _SubFolderMethod = SubFolderMethod.System_User
                Case 2
                    _SubFolderMethod = SubFolderMethod.System_Date
                Case 3
                    _SubFolderMethod = SubFolderMethod.Other
                Case 4
                    _SubFolderMethod = SubFolderMethod.Database_SystemDate
            End Select

            _XMLEnable = (_ConfigFile.getValue(Programming.Program_Config._SECTION_XMLEXPORT, Programming.Program_Config._KEY_XMLEXPORT_ENABLE) = "Y")
            _XMLPath = _ConfigFile.getValue(Programming.Program_Config._SECTION_XMLEXPORT, Programming.Program_Config._KEY_XMLEXPORT_PATH)
        End Sub
#End Region
#Region "Debug"
        Public Sub Start(ByVal _ProcessName As String)
            If _Enable Then
                If _FileType = FileType.Ascii Then
                    _DF.Start(_ProcessName)
                End If

            End If


        End Sub
        Public Sub StartFunction(ByVal _Function As String)
            If _Enable Then
                If _FileType = FileType.Ascii Then
                    _DF.StartFunction(_Function)
                End If
            End If

        End Sub

        Public Sub EndFunction()
            If _Enable Then
                If _FileType = FileType.Ascii Then
                    _DF.EndFunction()
                End If

            End If

        End Sub

        Public Sub Finish()
            If _Enable Then
                If _FileType = FileType.Ascii Then
                    _DF.Finish()
                End If

            End If

        End Sub

        Public Sub Write(ByVal _xml As XML.XMLDocument, ByVal _SubDir As String, ByVal _b As Boolean)
            Write(_xml, _SubDir, String.Empty, String.Empty, _b)
        End Sub

        Public Sub Write(ByVal _xml As XML.XMLDocument, ByVal _SubDir As String, _SubSubDir As String, _CommandName As String, ByVal _b As Boolean)
            Dim _di As System.IO.DirectoryInfo
            Dim _dis As System.IO.DirectoryInfo
            If _XMLEnable Then
                Try
                    _di = New System.IO.DirectoryInfo(_XMLPath)
                    If _di.Exists = False Then
                        _di.Create()
                    End If
                    
                    _dis = New System.IO.DirectoryInfo(_XMLPath & "\" & _SubDir)
                    If _dis.Exists = False Then
                        _dis.Create()
                    End If

                    If _SubSubDir <> String.Empty Then
                        _dis.CreateSubdirectory(_SubSubDir)
                        _xml.Save(_dis.FullName & "\" & _SubSubDir & "\" & _CommandName & "_" & DateTime.Now.ToString("yyyy-MM-dd_hhmmss") & ".xml")
                    Else

                        _xml.Save(_dis.FullName & "\" & _CommandName & "_" & DateTime.Now.ToString("yyyy-MM-dd_hhmmss") & ".xml")
                    End If

                Catch ex As Exception

                End Try
            End If
        End Sub

        Public Sub Write(ByVal _value As Object)
            Write(_value, String.Empty, -1, CPSLIB.Debug.LineType.Information)
        End Sub
        Public Sub Write(ByVal _value As Object, ByVal Title As String)
            Write(_value, Title, LineType.Information)
        End Sub
        Public Sub Write(ByVal _value As Object, ByVal _Title As String, ByVal _linetype As CPSLIB.Debug.LineType)
            Write(_value, _Title, String.Empty, -1, _linetype)
        End Sub
        Public Sub Write(ByVal _value As Object, ByVal _function As String, ByVal _Line As Integer, ByVal _type As CPSLIB.Debug.LineType)
            Write(_value, String.Empty, _function, -1, CPSLIB.Debug.LineType.Information)
        End Sub
        Public Sub Write(ByVal _value As Object, ByVal _Title As String, ByVal _function As String, ByVal _type As CPSLIB.Debug.LineType)
            Write(_value, _Title, _function, -1, CPSLIB.Debug.LineType.Information)
        End Sub
        Public Sub Write(ByVal _value As Object, ByVal _Title As String, ByVal _function As String, ByVal _Line As Integer, ByVal _type As CPSLIB.Debug.LineType)
            If _Enable Then
                Select Case _FileType
                    Case FileType.Ascii
                        _DF.Write(_value, _Title, _function, _Line, _type)
                    Case FileType.HTML
                        _HDF.RecordLog(_value, _Title)
                End Select


            End If


        End Sub

        Public Sub WriteException(ByVal ex As Exception, ByVal title As Object)
            If _Enable And _FileType = FileType.HTML Then
                _HDF.RecordException(ex, title)
            End If
        End Sub
        Public Sub WriteResult(ByVal _Cmd As Object, ByVal title As Object, ByVal _SQLServerInfo As Data.Connection.SQLServerInfo)
            If _Enable And _FileType = FileType.HTML Then
                _HDF.RecordResult(_Cmd, title, _SQLServerInfo)
            End If
        End Sub
        Public Sub WriteTable(ByVal _dt As DataTable, ByVal title As String)
            If _Enable And _FileType = FileType.HTML Then

                _HDF.RecordTable(_dt, title)

            End If
        End Sub
#End Region

    End Class

End Namespace