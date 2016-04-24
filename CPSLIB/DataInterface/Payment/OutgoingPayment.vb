Imports SAPbobsCOM
Imports CPSLIB.DataInterface.Company

Namespace DataInterface.Payment
    Public Class OutgoingPayment
        Private _diCompany As DataInterface.Company.DICompany
        Private _Doc As Documents
        Public Sub New()
            _Doc = _diCompany.Company.GetBusinessObject(BoObjectTypes.oVendorPayments)

        End Sub
    End Class
End Namespace
