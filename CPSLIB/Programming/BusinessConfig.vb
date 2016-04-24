Namespace Programming
    Public Class Business_Config : Inherits Settings.File.Files

        Public Shared _ConfigFile As String = "BusinessConfig.ini"

        Public Const _SECTION_EXCHANGERATES = "EXCHANGE RATES"

        Public Const _KEY_ER_AVERAGEMETHOD = "AVERAGEMETHOD"

        Public Const _KEY_ER_MONTHENDMETHOD = "MONTHENDMETHOD"

        Public Const _KEY_ER_AVERAGEVIEW = "AVERAGETABLE"

        Public Const _KEY_ER_MONTHENDVIEW = "MonthEndTable"


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
