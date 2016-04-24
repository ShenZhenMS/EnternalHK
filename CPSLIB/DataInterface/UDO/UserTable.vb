Imports CPSLIB.DataInterface.Company
Imports SAPbobsCOM
Namespace DataInterface.UDO

    Public Class UserTable
        Private _diCompany As DICompany
        Private _Debug As CPSLIB.Debug
        Private _strUDT As String
        Private _tblName As String
        Private _CPSException As CPSException
        Private _UDTs As UserTables
        Private _UDT As UserTable
        Private _UTType As BoUTBTableType
        Private _UTDesc As String
        Private _hasError As Boolean
        Private _Message As Boolean
        Private _htField As Hashtable

        Public Sub New(ByVal _diCompany As DICompany, ByVal strUDT As String, ByVal UTType As SAPbobsCOM.BoUTBTableType)
            _hasError = False
            _Message = String.Empty
            Me._strUDT = strUDT
            Me._UTType = UTType
            Me._diCompany = _diCompany
            Me._htField = New Hashtable
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            If strUDT.Length > 1 Then
                If strUDT.Substring(0, 1) = "@" Then
                    _tblName = strUDT.Substring(1)
                Else
                    _tblName = strUDT
                End If
            Else
                _tblName = strUDT
            End If
            ReadUserTable()
        End Sub

#Region "Property"

        Public Sub setUDF(ByVal _UDF As DataInterface.UDF.UDF)
            If _htField Is Nothing Then
                _htField = New Hashtable
            End If
            _htField.Add(_htField.Count + 1, _UDF)
        End Sub

        Public Property Description() As String
            Get
                Return _UTDesc
            End Get
            Set(ByVal value As String)
                _UTDesc = value
            End Set
        End Property
        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property
        Public ReadOnly Property hasError() As Boolean
            Get
                Return _hasError
            End Get
        End Property
#End Region
#Region "Execute"
        Private Sub ReadUserTable()
            _UDTs = _diCompany.Company.UserTables
        End Sub
        Public Function Exists() As Boolean
            Dim _ret As Boolean
            Dim _sqlClient As Data.Connection.MSSQLClient
            Try
                _sqlClient = New Data.Connection.MSSQLClient(_diCompany.ServerName, _diCompany.CompanyDB, _diCompany.DBUserName, _diCompany.DBPassword)
                If _sqlClient.isObjectExists(Data.Connection.MSSQLClient.SysObjectType.USER_TABLE, _strUDT) Then
                    _ret = True
                Else
                    _ret = False
                End If
            Catch ex As Exception
                _ret = False
                _CPSException.ExecuteHandle(ex)
            End Try

            Return _ret

        End Function

        Public Function Create() As Integer
            Dim _UTMD As UserTablesMD


            Dim _ret As Integer = -1
            If Exists() = False Then
                Try
                    _diCompany.SetTransaction()
                    _UTMD = _diCompany.Company.GetBusinessObject(BoObjectTypes.oUserTables)
                    _UTMD.TableName = _tblName
                    If Data.Validation.IsNull(_UTDesc) = String.Empty Then
                        _UTMD.TableDescription = _UTDesc
                    End If
                    _UTMD.TableType = _UTType
                    _ret = _UTMD.Add()
                    If _ret <> 0 Then
                        _Message = _diCompany.Company.GetLastErrorCode & " : " & _diCompany.Company.GetLastErrorDescription

                    Else
                        If Not _htField Is Nothing Then
                            _ret = CreateUDF()
                        End If
                    End If
                    If _ret <> 0 Then
                        _diCompany.RollbackTransaction()
                    Else
                        _diCompany.CommitTransaction()
                    End If
                Catch ex As Exception
                    _CPSException.ExecuteHandle(ex)
                    _Message = "Exception: " & ex.Message
                    _diCompany.RollbackTransaction()
                End Try
            Else
                _Message = String.Format(New Logging.MessageCode().Read(Logging.MessageCode.MessageCode.OBJECT_EXISTS), _strUDT)
            End If
            Return _ret
        End Function
        Public Function CreateUDF(Optional ByVal inTranacton As Boolean = False) As Integer
            Dim _UFMD As UserFieldsMD
            Dim _UDF As DataInterface.UDF.UDF
            Dim _UFVV As ValidValuesMD
            Dim _ret As Integer
            ' Create User Define Field
            If inTranacton = False Then
                _diCompany.SetTransaction()
            End If
            If Not _htField Is Nothing Then
                Try


                    For Each o As Object In _htField.Keys
                        _UDF = CType(_htField(o), DataInterface.UDF.UDF)
                        _UFMD = _diCompany.Company.GetBusinessObject(BoObjectTypes.oUserFields)
                        _UFMD.TableName = _tblName
                        _UFMD.Name = _UDF.UDFName
                        _UFMD.Type = _UDF.UDFType
                        _UFMD.Description = _UDF.UDFDesc
                        _UFMD.Size = _UDF.UDFSize
                        If Not _UDF.Value Is Nothing Then
                            For Each _o As Object In _UDF.Value.Keys
                                _UFMD.ValidValues.Value = _o.ToString()
                                _UFMD.ValidValues.Description = _UDF.Value(_o).ToString
                                _UFMD.Add()
                            Next

                        End If
                        _UFMD.DefaultValue = _UDF.UDFDFTValue
                        _UFMD = Nothing
                        _ret = _UFMD.Add
                        If _ret <> 0 Then
                            _Message = _diCompany.Company.GetLastErrorCode & " : " & _diCompany.Company.GetLastErrorDescription
                            Exit For
                        End If
                    Next
                Catch ex As Exception
                    _CPSException.ExecuteHandle(ex)
                    If inTranacton = False Then
                        ' Rollback Tansaction
                        _diCompany.RollbackTransaction()
                    End If
                End Try

            End If
            If _ret <> 0 Then
                If inTranacton = False Then
                    ' Rollback Tansaction
                    _diCompany.RollbackTransaction()
                End If
            Else
                If inTranacton = False Then
                    _diCompany.CommitTransaction()
                End If
            End If

        End Function
#End Region
    End Class
End Namespace

