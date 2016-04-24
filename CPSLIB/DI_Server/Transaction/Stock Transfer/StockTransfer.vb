Namespace DIServer
    Public Class StockTransfer
        Inherits DIServer.Document

        Private _diServerConnection As DIServerConnection
        Public Const Fld_FromWhse As String = "FromWarehouse"

        Public Sub New(ByVal _diServerConnection As DIServerConnection)
            MyBase.New(_diServerConnection, SAPbobsCOM.BoObjectTypes.oStockTransfer)
            Me._diServerConnection = _diServerConnection

        End Sub

        Public Property FromWarehouse
            Get
                Return GetValue(Fld_FromWhse)
            End Get
            Set(ByVal value)
                SetValue(Fld_FromWhse, value)
            End Set
        End Property

    End Class
End Namespace
