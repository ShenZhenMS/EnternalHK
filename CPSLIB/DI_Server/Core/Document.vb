Imports SAPbobsCOM
Namespace DIServer

    Public Class Document : Inherits DIServer.DI_Object
        Private _SessionID As String
        Private _htDocHeader As Hashtable
        Private _htDocLineProp As Hashtable
        Private _htDocLines As Hashtable
        Private _htOtherLines As Hashtable
        Private _BOObjectType As SAPbobsCOM.BoObjectTypes
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _StrXMLCmd As String
        Private _CurrentLine As Integer
        Dim _htHeaderFieldMapping As Hashtable
        Dim _htLineFieldMapping As Hashtable
        Private _DocLines As DocumentLines
        Private _ActionStatus As DI_Object.Command

        Public Const _DocDueDate As String = "DocDueDate"
        Public Const _DocDate As String = "DocDate"
        Public Const _TaxDate As String = "TaxDate"
        Public Const _CardCode As String = "CardCode"

        Dim _NewEntry As String
        Dim _NewObjectType As String

        Dim _htHeader As Hashtable
        Dim _htDetail As Hashtable

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



        Public Sub NewDocument()
            _CurrentLine = 0
            _htDocHeader = New Hashtable
            _htDocHeader(_DocDate) = DateTime.Now.ToString("yyyyMMdd")
            _htDocLines = New Hashtable
            _htOtherLines = New Hashtable
            _DocLines = New DocumentLines
        End Sub



        Public Sub New(ByVal _diServerConnecton As DIServerConnection, ByVal _ObjectType As SAPbobsCOM.BoObjectTypes)

            MyBase.New(_ObjectType, _diServerConnecton)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Me._diServerConnection = _diServerConnecton
            Me._SessionID = _diServerConnecton.SessionID  'SessionID
            Me._BOObjectType = _ObjectType

            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _htHeader = New Hashtable
            _htDetail = New Hashtable

            FieldMapping()
            NewDocument()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Sub ClearStandardTable()
            _htHeader = New Hashtable
            _htDetail = New Hashtable

        End Sub

        Private Sub FieldMapping()
            _htHeaderFieldMapping = New Hashtable


            _htHeaderFieldMapping("Comments") = "Comments"
            _htHeaderFieldMapping("DocDueDate") = "DocDueDate"
            '_htHeaderFieldMapping("TaxDate") = "TaxDate"
            _htHeaderFieldMapping("DocDate") = "DocDate"
            _htHeaderFieldMapping("SlpCode") = "SalesPersonCode"
            _htHeaderFieldMapping("Address") = "Address"
            _htHeaderFieldMapping("JrnlMemo") = "JournalMemo"
            _htHeaderFieldMapping("SlpCode") = "SalesPersonCode"
            _htHeaderFieldMapping("DocCur") = "DocCurrency"
            _htHeaderFieldMapping("OwnerCode") = "DocumentsOwner"
            _htHeaderFieldMapping("DiscPrcnt") = "DiscountPercent"
            ' For AP Credit Memo
            _htHeaderFieldMapping("NumAtCard") = "NumAtCard"


            _htLineFieldMapping = New Hashtable

            _htLineFieldMapping("FreeTxt") = "FreeText"
            _htLineFieldMapping("CogsOcrCod") = "COGSCostingCode"
            _htLineFieldMapping("CogsOcrCo2") = "COGSCostingCode2"
            _htLineFieldMapping("CogsOcrCo3") = "COGSCostingCode3"
            _htLineFieldMapping("CogsOcrCo4") = "COGSCostingCode4"
            _htLineFieldMapping("CogsOcrCo5") = "COGSCostingCode5"
            _htLineFieldMapping("WhsCode") = "WarehouseCode"

            _htLineFieldMapping("OcrCode") = "CostingCode"
            _htLineFieldMapping("OcrCode2") = "CostingCode2"
            _htLineFieldMapping("OcrCode3") = "CostingCode3"
            _htLineFieldMapping("OcrCode4") = "CostingCode4"
            _htLineFieldMapping("OcrCode5") = "CostingCode5"
            _htLineFieldMapping("LineTotal") = "LineTotal"
            _htLineFieldMapping("PriceBefDi") = "UnitPrice"
            _htLineFieldMapping("AcctCode") = "AccountCode"
            _htLineFieldMapping("DiscPrcnt") = "DiscountPercent"
            _htLineFieldMapping("Currency") = "Currency"

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
            setRowsValue(DocumentLines._LineNum, Me._CurrentLine)
            Me._CurrentLine = _htDocLines.Count + 1
            _htDocLines.Add(_htDocLines.Count, _DocLines)

            _DocLines = New DocumentLines

            _Debug.Write(Me._CurrentLine, "Current LineNumber")
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Sub setRowsValue(ByVal strName As String, ByVal Value As Object)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
           
            _DocLines.setValue(strName, Value)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

        Public Sub setRowsValue(ByVal _htValue As Hashtable)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            For Each o As Object In _htValue.Keys
                _DocLines.setValue(o, _htValue(o))
            Next

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
            _Debug.Write(_CurrentLine, "Set Batch, Current Line")
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

        Public Function GetDocDate(ByVal _TBLName As String, ByVal _key As String) As Date
            Dim _ret As Date
            Dim _dt As DataTable
            Try
                _dt = Read(_TBLName, _key)
                If _dt.Rows.Count > 0 Then
                    _ret = Convert.ToDateTime(_dt.Rows(0)("DocDate"))
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            _Debug.Write(_ret, "DocDate Result")
            Return _ret
        End Function

        Public Function GetDueDate(ByVal _TBLName As String, ByVal _key As String) As Date
            Dim _ret As Date
            Dim _dt As DataTable
            Try
                _dt = Read(_TBLName, _key)
                If _dt.Rows.Count > 0 Then
                    _ret = Convert.ToDateTime(_dt.Rows(0)("DocDueDate"))
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            _Debug.Write(_ret, "DocDueDate Result")
            Return _ret
        End Function

        Public Function GetDueDate(ByVal _TBLName As String, ByVal _key As String, ByVal _CurrDate As Date) As Date

            Dim _dt As DataTable
            Dim _OrgDate As Date
            Dim _OrgDueDate As Date
            Dim _DateDiff As Integer

            Dim _ret As Date
            Try
                _dt = Read(_TBLName, _key)
                If _dt.Rows.Count > 0 Then
                    _OrgDate = Convert.ToDateTime(_dt.Rows(0)("DocDate"))
                    _OrgDueDate = Convert.ToDateTime(_dt.Rows(0)("DocDueDate"))
                    _DateDiff = DateDiff(DateInterval.Day, _OrgDate, _CurrDate)
                    _ret = _OrgDueDate.AddDays(_DateDiff)
                Else

                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function



        Public Sub SetDoctotalByDraft_ARCMTemp(ByVal _TblName As String, ByVal _Key As String)
            Dim _dt As DataTable
            Try
                _dt = Read(_TblName, _Key)
                If _dt.Rows.Count > 0 Then
                    Try
                        If IsDBNull(_dt(0)("DocTotal")) = False Then

                            SetValue("DocTotal", _dt(0)("DocTotal"))
                        End If

                    Catch ex As Exception
                        _CPSException.ExecuteHandle(ex)
                    End Try


                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        Public Sub SetHeaderStandardField(ByVal _TblName As String, ByVal _key As String, ByVal ColumnName As String)

            Dim _dt As DataTable
            Try
                _dt = Read(_TblName, _key)
                If _dt.Rows.Count > 0 Then
                    Try
                        If IsDBNull(_dt(0)(ColumnName)) = False Then
                            If ColumnName = "DocDate" Or ColumnName = "DocDueDate" Or ColumnName = "TaxDate" Then
                                SetValue(_htHeaderFieldMapping(ColumnName), Convert.ToDateTime(_dt(0)(ColumnName)).ToString("yyyyMMdd"))
                            Else
                                SetValue(_htHeaderFieldMapping(ColumnName), _dt(0)(ColumnName))
                            End If
                            SetValue(_htHeaderFieldMapping(ColumnName), _dt(0)(ColumnName))
                        End If

                    Catch ex As Exception
                        _CPSException.ExecuteHandle(ex)
                    End Try


                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        Public Sub SetAllHeaderStandardField(ByVal _TblName As String, ByVal _key As String)

            Dim _dt As DataTable
            Try
                _dt = Read(_TblName, _key)
                If _dt.Rows.Count > 0 Then
                    For Each o As Object In _htHeaderFieldMapping.Keys
                        Try
                            If IsDBNull(_dt(0)(o)) = False Then
                                If o.ToString = "DocDate" Or o.ToString = "DocDueDate" Or o.ToString = "TaxDate" Then
                                    SetValue(_htHeaderFieldMapping(o), Convert.ToDateTime(_dt(0)(o)).ToString("yyyyMMdd"))
                                Else
                                    If o.ToString <> "DocCur" Then
                                        SetValue(_htHeaderFieldMapping(o), _dt(0)(o))
                                    End If

                                End If

                            End If

                        Catch ex As Exception
                            _CPSException.ExecuteHandle(ex)
                        End Try

                    Next



                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub


        Public Sub SetLineStandardField(ByVal _TblName As String, ByVal _key As String, ByVal _LineNum As Integer, ByVal ColumnName As String)

            Dim _dt As DataTable
            Try
                _dt = Read(_TblName, _key, _LineNum)
                If _dt.Rows.Count > 0 Then
                    Try
                        If IsDBNull(_dt(0)(ColumnName)) = False Then
                            setRowsValue(_htLineFieldMapping(ColumnName), _dt(0)(ColumnName))
                        End If

                    Catch ex As Exception
                        _CPSException.ExecuteHandle(ex)
                    End Try


                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        Public Sub SetALLLineStandardField(ByVal _TblName As String, ByVal _key As String, ByVal _LineNum As Integer)

            Dim _dt As DataTable
            Try
                _dt = Read(_TblName, _key, _LineNum)
                If _dt.Rows.Count > 0 Then
                    For Each o As Object In _htLineFieldMapping.Keys
                        Try
                            If IsDBNull(_dt(0)(o)) = False Then
                                If o.ToString <> "Currency" Then
                                    setRowsValue(_htLineFieldMapping(o), _dt(0)(o))
                                End If

                            End If

                        Catch ex As Exception
                            _CPSException.ExecuteHandle(ex)
                        End Try

                    Next


                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub


        Public Sub setUDF(ByVal _TblName As String, ByVal _Key As String)
            Dim _ht As Hashtable
            Dim _dt As DataTable
            Try
                _ht = GetUDF(_TblName)
                _dt = Read(_TblName, _Key)
                If _dt.Rows.Count > 0 Then
                    For Each o As Object In _ht.Keys
                        Try
                            If IsDBNull(_dt(0)(o)) = False Then
                                SetValue(o.ToString, _dt(0)(o))
                            End If

                        Catch ex As Exception
                            _CPSException.ExecuteHandle(ex)
                        End Try

                    Next
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub


        Public Sub setLineField(ByVal _TBLName As String, ByVal _Key As String, ByVal _LineNum As String, ByVal _ColumnName As String, ByVal XMLColumn As String)

            Dim _dt As DataTable
            Try
                _dt = Read(_TBLName, _Key, _LineNum)
                If _dt.Rows.Count > 0 Then
                    setRowsValue(XMLColumn, _dt.Rows(0)(_ColumnName))

                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub

        Public Sub setUDF(ByVal _TBLName As String, ByVal _Key As String, ByVal _LineNum As String)
            Dim _ht As Hashtable
            Dim _dt As DataTable
            Try
                _ht = GetUDF(_TBLName)
                _dt = Read(_TBLName, _Key, _LineNum)
                If _dt.Rows.Count > 0 Then
                    For Each o As Object In _ht.Keys
                        Try
                            If IsDBNull(_dt(0)(o)) = False Then
                                setRowsValue(o.ToString, _dt(0)(o))
                            End If

                        Catch ex As Exception
                            _CPSException.ExecuteHandle(ex)
                        End Try

                    Next
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub

        Public Function GetUDF(ByVal _tblName As String) As Hashtable
            Dim _SQLConn As Data.Connection.SQLServerInfo
            Dim _ht As New Hashtable
            Dim _sql As String = "select 'U_' + AliasID as AliasID,TypeID from CUFD where TableID = '{0}'"

            Try
                _SQLConn = New Data.Connection.SQLServerInfo(_diServerConnection.Server, _diServerConnection.DBUserName, _diServerConnection.DBPassword, _diServerConnection.CompanyDB)

                _ht = _SQLConn.ExecuteHashTable("AliasID", String.Format(_sql, _tblName))

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ht
        End Function

        Private Function Read(ByVal _tblname As String, ByVal _Key As String) As DataTable
            Dim _SQLConn As Data.Connection.SQLServerInfo
            Dim _dt As DataTable
            Dim _sql As String = "select * from {0} Where DocEntry = '{1}'"

            Try
                _SQLConn = New Data.Connection.SQLServerInfo(_diServerConnection.Server, _diServerConnection.DBUserName, _diServerConnection.DBPassword, _diServerConnection.CompanyDB)
                _dt = _SQLConn.ExecuteDatatable(String.Format(_sql, _tblname, _Key))

                If _dt.Rows.Count > 0 Then
                    Return _dt
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Function

        Private Function Read(ByVal _tblname As String, ByVal _Key As String, ByVal _LineNUm As String) As DataTable
            Dim _SQLConn As Data.Connection.SQLServerInfo
            Dim _dt As DataTable
            Dim _sql As String = "select * from {0} Where DocEntry = '{1}' and LineNum = '{2}'"

            Try
                _SQLConn = New Data.Connection.SQLServerInfo(_diServerConnection.Server, _diServerConnection.DBUserName, _diServerConnection.DBPassword, _diServerConnection.CompanyDB)
                _dt = _SQLConn.ExecuteDatatable(String.Format(_sql, _tblname, _Key, _LineNUm))

                If _dt.Rows.Count > 0 Then
                    _Debug.WriteTable(_dt, "Line")
                    Return _dt
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Function


        Public Sub SetValue(ByVal _htValue As Hashtable)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            If _htDocHeader Is Nothing Then
                _htDocHeader = New Hashtable
            End If
            For Each o As Object In _htValue.Keys
                _htDocHeader(o) = _htValue(o)
            Next
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub

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
        
        Public Function Post(ByVal _Cmd As Command) As CommandStatus
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus = CommandStatus.Fail
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
                    _htDocLines = Tools.ReverseHashTable(_htDocLines)
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
