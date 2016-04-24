Namespace Programming
    Public Class Program_Config : Inherits Settings.File.Files

        Public Shared _ConfigFile As String = "ProgramConfig.ini"

        Public Const _SECTION_XMLEXPORT As String = "XMLEXPORT"

        Public Const _SECTION_DEBUG As String = "DEBUG"

        Public Const _SECTION_EXCEPTION As String = "EXCEPTION"

        Public Const _KEY_XMLEXPORT_ENABLE As String = "ENABLE"

        Public Const _KEY_XMLEXPORT_PATH As String = "XMLEXPORT PATH"

        Public Const _KEY_DEBUG_ENABLE As String = "ENABLE"

        Public Const _KEY_DEBUG_PATH As String = "DEBUG PATH"

        Public Const _KEY_DEBUG_FILE_FORMAT As String = "DEBUG FORMAT"

        Public Const _KEY_DEBUG_FILE_TYPE As String = "DEBUG TYPE"

        Public Const _KEY_DEBUG_SUBFOLDERTYPE As String = "DEBUG SUBFOLDER TYPE"


        Public Const _KEY_EXCEPTION_ENABLE As String = "ENABLE"

        Public Const _KEY_EXCEPTION_PATH As String = "EXCEPTION PATH"

        Public Const _KEY_EXCEPTION_SUBFOLDERTYPE As String = "DEBUG SUBFOLDER TYPE"

        Public Sub New()
            MyBase.New(Settings.File.Consts._Default_Setting_File_Path & "\" & _ConfigFile)
        End Sub

#Region "Property"
        Public ReadOnly Property hasFile() As Boolean
            Get
                Return MyBase.Information.Exists
            End Get
        End Property
#End Region





    End Class

End Namespace
