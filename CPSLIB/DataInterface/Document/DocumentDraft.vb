Namespace DataInterface.Document
    Public Class DocumentDraft : Inherits Document

        Public Sub New(ByVal _DICompany As DataInterface.Company.DICompany, ByVal _ObjType As SAPbobsCOM.BoObjectTypes)
            MyBase.New(_DICompany, SAPbobsCOM.BoObjectTypes.oDrafts, _ObjType)
        End Sub
    End Class
End Namespace
