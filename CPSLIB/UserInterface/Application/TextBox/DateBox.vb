Public Class DateBox : Inherits System.Windows.Forms.DateTimePicker

    Private _BlankValue As DateTime


    Public Sub New()
        MyBase.New()
        'Default Setting
        MyBase.ShowCheckBox = True
        MyBase.Format = Windows.Forms.DateTimePickerFormat.Short
    End Sub

#Region "Property"

    Public Property CurrentValue() As DateTime
        Get
            If MyBase.ShowCheckBox Then
                If Checked Then
                    Return Value
                Else
                    Return _BlankValue
                End If
            Else
                Return Value
            End If
        End Get
        Set(ByVal value As DateTime)
            MyBase.Value = value
        End Set
    End Property
    Public WriteOnly Property BlankValue() As DateTime
        Set(ByVal value As DateTime)
            _BlankValue = value
        End Set
    End Property

#End Region
End Class
