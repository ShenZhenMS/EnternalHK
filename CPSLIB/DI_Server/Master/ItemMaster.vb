Namespace DIServer.Master
    Public Class ItemMaster : Inherits DIServer.DI_Object

        Dim _htMaster As Hashtable
        Dim _htPrice As Hashtable
        Dim _htWarehouse As Hashtable
        Dim _Debug As CPSLIB.Debug
        Dim _CPSException As CPSException
        Public Sub New(_diServerConnection As DIServer.DIServerConnection)
            MyBase.New(SAPbobsCOM.BoObjectTypes.oItems, _diServerConnection)

            _htMaster = New Hashtable
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException

        End Sub

        Public Function Post(_Cmd As Command) As CommandStatus
            Select Case _Cmd
                Case Command.AddObject

                Case Command.UpdateObject

                Case Command.RemoveObject

            End Select
        End Function

        Public Sub setValue(ByVal strName As String, ByVal Value As Object)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            If _htMaster Is Nothing Then
                _htMaster = New Hashtable
            End If
            _htMaster(strName) = Value
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Function getValue(strName As String) As Object
            Return _htMaster(strName)
        End Function

    End Class
End Namespace
