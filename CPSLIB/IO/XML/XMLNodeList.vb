Namespace XML.XMLNodeList
    Public MustInherit Class XMLNodeList : Inherits System.Xml.XmlNodeList

        Public Sub New()
            MyBase.New()
        End Sub

#Region "Get Value"
        Public Function FirstValue() As Object
            Dim _ret As Object
            _ret = Me.Item(0).InnerText
            Return _ret
        End Function
        Public Function LastValue() As Object
            Dim _ret As Object
            _ret = Me.Item(Me.Count - 1).InnerText
            Return _ret
        End Function

#End Region


    End Class
End Namespace
