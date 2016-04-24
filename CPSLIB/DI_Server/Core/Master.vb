Imports SAPbobsCOM
Namespace DIServer

    Public Class Master : Inherits DIServer.DI_Object
        Private _SessionID As String
        Private _htDocHeader As Hashtable
        Private _htDocLineProp As Hashtable
        Private _htDocLines As Hashtable
        Private _BOObjectType As SAPbobsCOM.BoObjectTypes
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _StrXMLCmd As String
        Private _CurrentLine As Integer

        Private _DocLines As DocumentLines
        Private _ActionStatus As DI_Object.Command

      

        Dim _NewEntry As String
        Dim _NewObjectType As String

        Private _diServerConnection As DIServerConnection
        Public Property DIServerConn() As DIServerConnection
            Get
                Return _diServerConnection
            End Get
            Set(ByVal value As DIServerConnection)
                _diServerConnection = value
            End Set
        End Property

        Public Const FieldPrefix As String = "<{0}>{1}</{0}>"

        'Public Sub New(ByVal _diServerConnection As DIServerConnection)

        '    Me.New(_diServerConnection, Nothing)

        'End Sub

        Public ReadOnly Property NewEntry As String
            Get
                Return _NewEntry
            End Get
        End Property

        Public ReadOnly Property NewObjectCode As String
            Get
                Return _NewObjectType
            End Get
        End Property

        Public Sub New(ByVal _diServerConnecton As DIServerConnection, ByVal _ObjectType As SAPbobsCOM.BoObjectTypes)

            MyBase.New(_ObjectType, _diServerConnecton)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Me._diServerConnection = _diServerConnecton


            _CurrentLine = 0
            Me._SessionID = SessionID

            Me._BOObjectType = _ObjectType


            _htDocHeader = New Hashtable
            _htDocLines = New Hashtable
            _DocLines = New DocumentLines
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub


        Public Sub SelectLine(ByVal i As Integer)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Me._CurrentLine = i
            _DocLines = CType(_htDocLines(i), DocumentLines)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub


        Public Sub AddRow()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            If _htDocLines Is Nothing Then
                _htDocLines = New Hashtable
            End If
            _htDocLines.Add(_htDocLines.Count, _DocLines)
            _DocLines = New DocumentLines
            Me._CurrentLine = _htDocLines.Count + 1
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Sub setRowsValue(ByVal strName As String, ByVal Value As Object)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            _DocLines.setValue(strName, Value)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Sub setRow(ByVal _DocLine As DocumentLines)
            If _htDocLines Is Nothing Then
                _htDocLines = New Hashtable
            End If
            _htDocLines.Add(_htDocLines.Count, _DocLine)
            Me._CurrentLine = _htDocLines.Count + 1
        End Sub

        Public Sub setBatchNumberRow(ByVal _BatchNum As BatchNumbers)
            _BatchNum.BaseLineNumber = _CurrentLine
            _DocLines.AddBatch(_BatchNum)
        End Sub

        Public Sub setSerialNumberRow(ByVal _SerialNumber As SerialNumbers)
            _SerialNumber.BaseLineNumber = _CurrentLine
            _DocLines.AddSerial(_SerialNumber)
        End Sub

        Public Function getRow(ByVal index As Integer) As DocumentLines
            Dim _ret As DocumentLines = Nothing
            Try
                If _htDocLines Is Nothing = False Then
                    _ret = CType(_htDocLines(index), DocumentLines)
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function


#Region "Set Header Field"
        Public Sub setValue(ByVal strName As String, ByVal Value As Object)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            If _htDocHeader Is Nothing Then
                _htDocHeader = New Hashtable
            End If
            _htDocHeader(strName) = Value
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub
        Public Function GetValue(ByVal strName As String) As Object
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As Object = String.Empty
            Try
                _ret = _htDocHeader(strName)
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
#End Region

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
        Public Function LineCount() As Integer
            If _htDocLines Is Nothing = False Then
                Return _htDocLines.Count
            Else
                Return 0
            End If
        End Function
#End Region

#Region "Execute"
        Public Function Post(_Cmd As Command) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus = False
            ' Generate SOAP XML
            Select Case _Cmd
                Case Command.AddObject
                    _ret = Add(GenerateXML)
                    If _ret = CommandStatus.Success Then
                        _NewEntry = ResponseElement.FirstChild("AddObjectResponse").ChildNodes(0).InnerText
                        _NewObjectType = ResponseElement.FirstChild("AddObjectResponse").ChildNodes(1).InnerText

                    End If
                Case Command.CancelObject
                    _ret = Cancel(GenerateXML)
                Case Command.CloseObject
                    _ret = Close(GenerateXML)
                Case Command.RemoveObject
                    _ret = Remove(GenerateXML)
                Case Command.UpdateObject
                    _ret = Update(GenerateXML)
                Case Else

            End Select
            ' Generate Document
            Return _ret
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Function
        Private Function GenerateXML() As String
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As String = String.Empty
            _ret = GenerateObject() & GenerateHeader() & GenerateLines() & GenerateBatches() & GenerateSerialNumbers()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
        Private Function GenerateObject() As String
            Dim _ret As String = String.Empty
            If Not Me.BOObjectType = Nothing Then
                _ret = String.Format(ObjectXML, [Enum].GetName(Me.BOObjectType.GetType, Me.BOObjectType))
            End If

            Return _ret
        End Function
        Private Function GenerateHeader() As String
            Dim _ret As String = String.Empty
            Try


                If Not _htDocHeader Is Nothing Then

                    For Each o As Object In _htDocHeader.Keys
                        _ret = _ret & String.Format(FieldPrefix, o.ToString, _htDocHeader(o.ToString))
                    Next
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            If _ret = String.Empty Then
                Return _ret
            Else
                Return String.Format(DIServer.Core.HeaderLevelPrefix, DIServerTag.HeaderTag, _ret)
            End If

        End Function
        Private Function GenerateLines() As String
            Dim _ret As String = String.Empty
            Try
                If Not _htDocLines Is Nothing Then
                    For Each o As Object In _htDocLines.Keys

                        _ret = _ret & CType(_htDocLines(o), DocumentLines).GenerateLine
                    Next
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            If _htDocLines Is Nothing Then
                Return _ret
            Else
                Return String.Format(DIServer.Core.NonHeaderLevelPrefix, DIServerTag.LineLevelTag, _ret)
            End If

        End Function


        Private Function GenerateBatches() As String
            Dim _ret As String = String.Empty

            Try
                If Not _htDocLines Is Nothing Then
                    For Each o As Object In _htDocLines.Keys

                        _ret = _ret & CType(_htDocLines(o), DocumentLines).GenerateBatchLine


                    Next
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            If _htDocLines Is Nothing Then
                Return _ret
            Else
                If _ret = String.Empty Then
                    Return _ret
                Else
                    Return String.Format(DIServer.Core.NonHeaderLevelPrefix, DIServerTag.BatchLevelTag, _ret)
                End If

            End If
        End Function

        Private Function GenerateSerialNumbers() As String
            Dim _ret As String = String.Empty

            Try
                If Not _htDocLines Is Nothing Then
                    For Each o As Object In _htDocLines.Keys

                        _ret = _ret & CType(_htDocLines(o), DocumentLines).GenerateSerialLine


                    Next
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            If _htDocLines Is Nothing Then
                Return _ret
            Else
                If _ret = String.Empty Then
                    Return _ret
                Else
                    Return String.Format(DIServer.Core.NonHeaderLevelPrefix, DIServerTag.SerialLevelTag, _ret)
                End If

            End If
        End Function

#End Region

#Region "Property"

        Public Property DocDate() As DateTime
            Get
                Return GetValue(_DocDate)
            End Get
            Set(ByVal value As DateTime)
                setValue(_DocDate, value)
            End Set
        End Property

        Public Property TaxDate() As DateTime
            Get
                Return GetValue(_TaxDate)
            End Get
            Set(ByVal value As DateTime)
                setValue(_TaxDate, value)
            End Set
        End Property

        Public Property DeliveryDate() As DateTime
            Get
                Return GetValue(_DocDueDate)
            End Get
            Set(ByVal value As DateTime)
                setValue(_DocDueDate, value)
            End Set
        End Property

        Public Property CardCode() As String
            Get
                Return GetValue(_CardCode)
            End Get
            Set(ByVal value As String)
                setValue(_CardCode, value)

            End Set

        End Property
#End Region

        Public Function Logout() As Boolean
            Return DIServerConn.Logout
        End Function
    End Class
End Namespace
