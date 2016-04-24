Namespace Settings.File
    Public Class Consts
        Public Shared _Default_Setting_File_Name As String = "Settings.ini"
        Public Shared _Default_Value_Sperator As String = "="

        Public Shared ReadOnly Property _Default_Setting_File_Path()
            Get
                Return System.Environment.CurrentDirectory
            End Get
        End Property

        Public Shared ReadOnly Property _Default_Setting_FullPath()
            Get
                Return Consts._Default_Setting_File_Path & "\" & Consts._Default_Setting_File_Name
            End Get
        End Property

    End Class
End Namespace
