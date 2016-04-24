Namespace DataInterface.Document
    Public Class SalesOrderDocumentDraft : Inherits DataInterface.Document.DocumentDraft
        Public Sub New(ByVal _diCompany As DataInterface.Company.DICompany)
            MyBase.New(_diCompany, SAPbobsCOM.BoObjectTypes.oOrders)
        End Sub

    End Class
End Namespace
