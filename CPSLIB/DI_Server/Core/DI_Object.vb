Namespace DIServer
    Public Class DI_Object
        Inherits DIServer.Core
        Private _ObjectXML As String = String.Empty
        Private _CommandString As String = String.Empty
        Private _CommandName As String = String.Empty
        Private _SessionID As String = String.Empty
        Private _objtype As SAPbobsCOM.BoObjectTypes
        Private _Action As Command
        Private _DIServerConnecton As DIServerConnection
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Public Enum Command
            AddObject = 1
            UpdateObject = 2
            RemoveObject = 3
            CancelObject = 4
            CloseObject = 5
            GetBusinessObjectXmlSchema = 6
            GetServiceDataXMLSchema = 7
        End Enum
    
        Private _DIServerTag As DIServerTagConfig
        Public Property DIServerTag() As DIServerTagConfig
            Get
                Return _DIServerTag
            End Get
            Set(ByVal value As DIServerTagConfig)
                _DIServerTag = value
            End Set
        End Property

        Public Sub New(ByVal _objtype As SAPbobsCOM.BoObjectTypes, ByVal _diServerConnection As DIServerConnection)
            MyBase.New(_objtype.ToString)
            MyBase.SessionID = _diServerConnection.SessionID
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            _DIServerTag = New DIServerTagConfig(_objtype)
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _Action = New Command
            Me._objtype = _objtype
            Try
                If _diServerConnection.SessionID = String.Empty Then
                    _diServerConnection.Execute()
                End If
                Me._SessionID = _diServerConnection.SessionID
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Private Function GenerateObject() As String
            Dim _ret As String = String.Empty
            If Not _objtype = Nothing Then
                _ret = String.Format(ObjectXML, [Enum].GetName(Me._objtype.GetType, Me._objtype))
            End If

            Return _ret
        End Function

        Private Function GenerateSchemaObject() As String
            Dim _ret As String = String.Empty
            If Not _objtype = Nothing Then
                _ret = String.Format(SchemaObjectXML, [Enum].GetName(Me._objtype.GetType, Me._objtype))
            End If

            Return _ret
        End Function

        Public Function GetServiceData() As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SchemaSCommandXML, [Enum].GetName(_Action.GetType, Command.GetServiceDataXMLSchema), [Enum].GetName(_Action.GetType, Command.GetServiceDataXMLSchema), xmlns)
            _CommandString = _CommandString & GenerateSchemaObject() & String.Format(Core.SchemaECommandXML, [Enum].GetName(_Action.GetType, Command.GetServiceDataXMLSchema))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function

        Public Function GetObjectSchema() As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SchemaSCommandXML, [Enum].GetName(_Action.GetType, Command.GetBusinessObjectXmlSchema), [Enum].GetName(_Action.GetType, Command.GetBusinessObjectXmlSchema), xmlns)
            _CommandString = _CommandString & GenerateSchemaObject() & String.Format(Core.SchemaECommandXML, [Enum].GetName(_Action.GetType, Command.GetBusinessObjectXmlSchema))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function

        Public Function Add(ByVal StrContent As String) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SCommandXML, [Enum].GetName(_Action.GetType, Command.AddObject), [Enum].GetName(_Action.GetType, Command.AddObject), xmlns)
            _CommandString = _CommandString & StrContent & String.Format(Core.ECommandXML, [Enum].GetName(_Action.GetType, Command.AddObject))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
        Public Function Update(ByVal StrContent As String) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SCommandXML, [Enum].GetName(_Action.GetType, Command.UpdateObject), [Enum].GetName(_Action.GetType, Command.UpdateObject), xmlns)
            _CommandString = _CommandString & StrContent & String.Format(Core.ECommandXML, [Enum].GetName(_Action.GetType, Command.UpdateObject))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
        Public Function Remove(ByVal StrContent As String) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SCommandXML, [Enum].GetName(_Action.GetType, Command.RemoveObject), [Enum].GetName(_Action.GetType, Command.RemoveObject), xmlns)
            _CommandString = _CommandString & StrContent & String.Format(Core.ECommandXML, [Enum].GetName(_Action.GetType, Command.RemoveObject))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
        Public Function Cancel(ByVal StrContent As String) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SCommandXML, [Enum].GetName(_Action.GetType, Command.CancelObject), [Enum].GetName(_Action.GetType, Command.CancelObject), xmlns)
            _CommandString = _CommandString & StrContent & String.Format(Core.ECommandXML, [Enum].GetName(_Action.GetType, Command.CancelObject))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
        Public Function Close(ByVal StrContent As String) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _CommandString = String.Format(Core.SCommandXML, [Enum].GetName(_Action.GetType, Command.CloseObject), [Enum].GetName(_Action.GetType, Command.CloseObject), xmlns)
            _CommandString = _CommandString & StrContent & String.Format(Core.ECommandXML, [Enum].GetName(_Action.GetType, Command.AddObject))
            SetCommandString(_CommandString)
            _ret = Execute()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function

#Region "Operation"
        
#End Region
        
    End Class
End Namespace
