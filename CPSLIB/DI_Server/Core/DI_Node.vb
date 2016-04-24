Imports CPSLIB.XML
Namespace DIServer
    Public Class DI_Node

        Public Enum CommandStatus
            Success = 1
            Fail = 2
        End Enum

        Private _DI_Node As SBODI_Server.Node
        Private _SessionID As String
        Private _ResponseString As String
        Private _ResponseElement As System.Xml.XmlElement
        Private _ResponseDocument As XMLDocument
        Private _ResponseStatus As CommandStatus
        Private _ResponseMessage As String

        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException

        Private _CommandName As String
        Public Property CommandName() As String
            Get
                Return _CommandName
            End Get
            Set(ByVal value As String)
                _CommandName = value
            End Set
        End Property

       
        Public ReadOnly Property PostResult As XMLDocument
            Get
                Return _ResponseDocument
            End Get

        End Property

        Public Sub New()
            Me.New(String.Empty)
        End Sub

        Public Sub New(_CommandName As String)
            _DI_Node = New SBODI_Server.Node
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            Me._CommandName = _CommandName
        End Sub
        Public Function Execute(ByVal strCmd As String) As CommandStatus
            strCmd = strCmd.Replace("&", "&amp;")
            Dim _ret As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            _Debug.Write(strCmd, "Execute Command String", CPSLIB.Debug.LineType.Information)
            _CPSException.ExecuteHandle(New Exception(strCmd))
            Try
                _Debug.Write(New XMLDocument(strCmd, False), "DIServer", _CommandName, _CommandName, False)
                _ResponseString = _DI_Node.Interact(strCmd)
                _ResponseDocument = New XMLDocument(_ResponseString, False)
                _ResponseElement = _ResponseDocument.DocumentElement
                _Debug.Write(_ResponseDocument, "DIServerResult", _CommandName, _CommandName, False)
                If InStr(_ResponseElement.InnerXml, "<env:Fault>") Then 'And (Not (sret.StartsWith("Error"))) Then
                    _ResponseMessage = "Error: " & _ResponseElement.InnerText
                    _Debug.Write(_ResponseMessage, "Error")
                    _ResponseStatus = CommandStatus.Fail
                Else
                    _ResponseStatus = CommandStatus.Success
                    _ResponseMessage = String.Empty
                End If

            Catch ex As Exception
                _ResponseStatus = CommandStatus.Fail
                _ResponseMessage = "Exception: " & ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try

            _ret = _ResponseStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function

#Region "Property"

        Public Property SessionID() As String
            Get
                Return _SessionID
            End Get
            Set(ByVal value As String)
                _SessionID = value
            End Set
        End Property

        Public ReadOnly Property CmdStatus() As CommandStatus
            Get
                Return _ResponseStatus
            End Get

        End Property

        Public ReadOnly Property CmdMessage() As String
            Get
                Return _ResponseMessage
            End Get
        End Property

        Public ReadOnly Property ResponseElement() As System.Xml.XmlElement
            Get
                Return _ResponseElement
            End Get
        End Property

#End Region


    End Class
End Namespace
