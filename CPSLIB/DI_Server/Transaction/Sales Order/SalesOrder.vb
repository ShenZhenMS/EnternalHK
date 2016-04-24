Namespace DIServer
    Public Class SalesOrder
        Inherits DIServer.Document

        Private _diServerConnection As DIServerConnection

      

        Public Sub New(ByVal _diServerConnection As DIServerConnection)
            MyBase.New(_diServerConnection, SAPbobsCOM.BoObjectTypes.oOrders)
            Me._diServerConnection = _diServerConnection

        End Sub

        
     


        
    End Class
End Namespace
