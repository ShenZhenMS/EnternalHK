Public Class SalesCreditMemoValidation : Inherits Validation


    Dim _Setting As Settings
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dt As DataTable
    Public Sub New(ByVal _Setting As Settings, ByVal _dt As DataTable)
        MyBase.New(_Setting, Settings.WMSModule.ARCREDITMEMO)
        Me._Setting = _Setting
        Me._dt = _dt
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        MyBase.ValidateTableStructure(_dt)
    End Sub

    Public Function AdjustedTable() As DataTable
        Return _dt
    End Function



End Class
