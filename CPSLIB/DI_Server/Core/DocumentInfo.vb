Imports SAPbobsCOM
Namespace DIServer

    Public Class DocumentInfo : Inherits Document
        Private _SessionID As String
        Private _BOObjectType As SAPbobsCOM.BoObjectTypes
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
       
        Private _ActionStatus As DI_Object.Command


       
        Private _diServerConnection As DIServerConnection
       
       
        'Public Sub New(ByVal _diServerConnection As DIServerConnection)

        '    Me.New(_diServerConnection, Nothing)

        'End Sub

       
       


       
        Public Sub New(ByVal _diServerConnecton As DIServerConnection, ByVal _ObjectType As SAPbobsCOM.BoObjectTypes)

            MyBase.New(_diServerConnecton, _ObjectType)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Me._diServerConnection = _diServerConnecton



            Me._SessionID = SessionID

            Me._BOObjectType = _ObjectType


            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub




#Region "Property"
        Public Property BOObjectType() As SAPbobsCOM.BoObjectTypes
            Get
                Return Me._BOObjectType
            End Get
            Set(ByVal value As SAPbobsCOM.BoObjectTypes)
                Me._BOObjectType = value
            End Set
        End Property

        Public Property ActionStatus() As Command
            Get
                Return _ActionStatus
            End Get
            Set(ByVal value As Command)
                _ActionStatus = value
            End Set
        End Property
      
#End Region

#Region "Execute"


        Public Function GetSchema(ByVal _Cmd As Command) As CommandStatus
            Dim _PostResult As XML.XMLDocument

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus = CommandStatus.Fail
            If MyBase.Post(Command.GetServiceDataXMLSchema) = CommandStatus.Success Then
                _PostResult = PostResult

            End If
            Return _ret
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Function




        Private Function GenerateObject() As String
            Dim _ret As String = String.Empty
            If Not Me.BOObjectType = Nothing Then
                _ret = String.Format(ObjectXML, [Enum].GetName(Me.BOObjectType.GetType, Me.BOObjectType))
            End If

            Return _ret
        End Function








#End Region

#Region "Property"


#End Region

        Public Function Logout() As Boolean
            Return DIServerConn.Logout
        End Function
    End Class
End Namespace

