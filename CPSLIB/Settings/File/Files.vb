Imports CPSLIB.IO

Namespace Settings.File

    Public Class Files : Inherits IO.Ascii.AsciiFile

        Dim SettingFile As String
        Dim _asciifile As Ascii.AsciiFile
        Dim _Config As Hashtable
#Region "Constructor"
        Public Sub New(ByVal fullFilePath As String)
            MyBase.New(fullFilePath)
            If MyBase.Information.Exists Then
                Read()
            Else

            End If

        End Sub
#End Region
#Region "Process"


        Private Sub Read()
            _Config = New Hashtable
            Dim _FileContent As ArrayList = MyBase.FileContent
            Dim _Section As String = String.Empty
            For Each o As Object In _FileContent
                If o.ToString.Trim <> String.Empty Then
                    If o.ToString().Substring(0, 1) = "[" And o.ToString().Substring(o.ToString().Trim().Length - 1, 1) = "]" Then
                        ' Section
                        _Section = o.ToString().Replace("[", "").Replace("]", "").Trim
                    Else
                        ' Value
                        If o.ToString().IndexOf(Settings.File.Consts._Default_Value_Sperator) > 0 Then
                            ' Valid
                            _Config.Add(_Section & "," & o.ToString().Substring(0, o.ToString().IndexOf(Settings.File.Consts._Default_Value_Sperator)), o.ToString.Substring(o.ToString.IndexOf(Settings.File.Consts._Default_Value_Sperator) + 1))

                        Else
                            'Invalid -- Skip Line

                        End If
                    End If

                End If
            Next
        End Sub

        Public Function getValue(ByVal _Section As String, ByVal _Name As String) As String
            Dim ret As String = ""
            If _Config.ContainsKey(_Section.Trim & "," & _Name.Trim) Then
                ret = _Config(_Section.Trim & "," & _Name.Trim).ToString
            Else

            End If
            Return ret
        End Function

        Public Function GetSectionValue(ByVal _Section As String) As Hashtable
            Dim _ht As New Hashtable
            For Each o As String In _Config.Keys
                If o.Contains(_Section) Then
                    _ht(o) = _Config(o)
                End If
            Next
            Return _ht
        End Function

#End Region





    End Class
End Namespace
