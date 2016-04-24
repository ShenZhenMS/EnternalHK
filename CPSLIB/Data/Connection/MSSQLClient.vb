Imports System.Data.SqlClient
Imports System.Data

Imports CPSLIB.Settings
Imports CPSLIB

Namespace Data.Connection
    Public Class MSSQLClient
        Public Shared TransactionName As String = "trans"
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        ' Following is read by config file on next version

        Public ConnectionTimeout As Integer = 5000
        Public _isConnected As Boolean
        ' 2010-07-26
        Private _isError As Boolean
        Private _hasException As Boolean
        Private _Message As String

        Public Shared _ObjectQuery = "SELECT 1 FROM SYS.OBJECTS WHERE TYPE = '{0}' AND [NAME] = '{1}'"

        Public Enum SysObjectType
            SQL_SCALAR_FUNCTION = 1 'FN
            SERVICE_QUEUE = 2 'SQ
            USER_TABLE = 3 'U
            DEFAULT_CONSTRAINT = 4 'D
            PRIMARY_KEY_CONSTRAINT = 5 'PK
            VIEW = 6 'V
            SYSTEM_TABLE = 7 'S
            INTERNAL_TABLE = 8 'IT
            SQL_STORED_PROCEDURE = 9 'P
            SQL_TABLE_VALUED_FUNCTION = 10 'TF'

        End Enum
        Public Enum CommandType
            Query = 0
            Update = 1
            Delete = 2
            Insert = 3
            StoreProcedure = 4

        End Enum


        Private _SqlConnection As SqlConnection
        Private _SqlCommand As SqlCommand
        Private _SqlDataAdapter As SqlDataAdapter
        Private _SqlDataReader As SqlDataReader
        Private _SqlDataSet As DataSet

        Private _DataTable As DataTable
        Private _Column As ArrayList
        Private _ReturnResult As Integer
        Private _Command As String
        Private _Parameter As Hashtable

        Private _SqlServer As String
        Private _SqlDatabase As String
        Private _SqlUserLogin As String
        Private _SqlUserPassword As String
        Private _SqlPersisSecurityInfo As Boolean = Data.Connection.Consts.DEFAULT_PersisSecurityInfo
        Private _SqlPooling As Boolean = Data.Connection.Consts.DEFAULT_Pooling
        Private _ConnectionString As String
        Private _SQLTransaction As SqlTransaction
        Private _InTransaction As Boolean

        Private _Identity As Integer
        Public Property Identity() As Integer
            Get
                Return _Identity
            End Get
            Set(ByVal value As Integer)
                _Identity = value
            End Set
        End Property

#Region "Constructor"
        Public Sub New(ByVal SqlServer As String, ByVal SqlDatabase As String, ByVal SqlUserName As String, ByVal SqlPassword As String)
            _hasException = False
            _isConnected = False
            _InTransaction = False
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)

            _CPSException = New CPSException
            _SqlServer = SqlServer
            _SqlDatabase = SqlDatabase
            _SqlUserLogin = SqlUserName
            _SqlUserPassword = SqlPassword

            _ConnectionString = String.Format(Data.Connection.Consts.TEMPLATE_CONNECTIONSTRING, _SqlServer, _SqlDatabase, _SqlUserLogin, _SqlUserPassword, Data.Connection.Consts.DEFAULT_PersisSecurityInfo, Data.Connection.Consts.DEFAULT_Pooling)
            _Debug.Write(_ConnectionString, "ConnectionString", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
        End Sub
#End Region
#Region "Property"
        Public Property SQLServer() As String
            Get
                Return _SqlServer
            End Get
            Set(ByVal value As String)
                _SqlServer = value
            End Set
        End Property

        Public Property SQLDatabase() As String
            Get
                Return _SqlDatabase
            End Get
            Set(ByVal value As String)
                _SqlDatabase = value
            End Set
        End Property

        Public Property SQLUserName() As String
            Get
                Return _SqlUserLogin
            End Get
            Set(ByVal value As String)
                _SqlUserLogin = value
            End Set
        End Property

        Public Property SQLPassword() As String
            Get
                Return _SqlUserPassword
            End Get
            Set(ByVal value As String)
                _SqlUserPassword = value
            End Set
        End Property
        Public ReadOnly Property ConnectionString() As String
            Get
                Return _ConnectionString
            End Get
        End Property

        Public ReadOnly Property SQLConnection() As SqlConnection
            Get
                Return _SqlConnection
            End Get
        End Property
        Public Property hasException() As Boolean
            Get
                Return _hasException
            End Get
            Set(ByVal value As Boolean)
                _hasException = value
            End Set
        End Property
        Public Property isError() As Boolean
            Get
                Return _isError
            End Get
            Set(ByVal value As Boolean)
                _isError = value
            End Set
        End Property

        Public ReadOnly Property SQLCommand() As SqlCommand
            Get
                Return _SqlCommand
            End Get
        End Property

        Public Property Message() As String
            Get
                Return _Message
            End Get
            Set(ByVal value As String)
                _Message = value
            End Set
        End Property

        Public ReadOnly Property isConnected() As Boolean
            Get
                Return _isConnected
            End Get
        End Property

#End Region
#Region "Connection"
        Public Function Connect() As Boolean
            Try
                _Debug.Write("Server Connect")
                _SqlConnection = New SqlConnection(_ConnectionString)
                ' OpenConnection
                _SqlConnection.Open()
                _isConnected = True
                _hasException = False
            Catch ex As System.Exception
                ' Throw Exception
                _hasException = True
                _Message = _Message & "Exception: " & ex.Message
                ' Log
                _Debug.Write(ex.Message, "Exception", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Error)
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)

            End Try
            Return _isConnected
        End Function


#End Region
#Region "Process"
        Public Function isObjectExists(ByVal _SysObjType As SysObjectType, ByVal _Name As String) As Boolean

            Dim _ret As Boolean
            Dim _type As String = String.Empty
            Dim _StrSql As String
            Select Case _SysObjType
                Case SysObjectType.DEFAULT_CONSTRAINT
                    _type = "D"
                Case SysObjectType.INTERNAL_TABLE
                    _type = "IT"
                Case SysObjectType.PRIMARY_KEY_CONSTRAINT
                    _type = "PK"
                Case SysObjectType.SERVICE_QUEUE
                    _type = "SQ"
                Case SysObjectType.SQL_SCALAR_FUNCTION
                    _type = "FN"
                Case SysObjectType.SQL_STORED_PROCEDURE
                    _type = "P"
                Case SysObjectType.SYSTEM_TABLE
                    _type = "S"
                Case SysObjectType.USER_TABLE
                    _type = "U"
                Case SysObjectType.VIEW
                    _type = "V"
                Case SysObjectType.SQL_TABLE_VALUED_FUNCTION
                    _type = "TF"

            End Select
            _StrSql = String.Format(MSSQLClient._ObjectQuery, _type, _Name)
            Try
                If isConnected = False Then
                    Connect()
                End If
                _SqlCommand = NewSQLCommand(_StrSql, System.Data.CommandType.Text)
                _SqlDataReader = _SqlCommand.ExecuteReader()

                If _SqlDataReader.HasRows Then
                    _ret = True
                Else
                    _ret = False

                End If
                _SqlDataReader.Close()
                Close()
                Return _ret
            Catch ex As Exception
                _Message = _Message & " Exception (isObjectExists) : " & ex.Message
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                Return False
            End Try
        End Function
        Public ReadOnly Property NewSQLCommand(ByVal strCmd As String, ByVal CmdType As System.Data.CommandType) As SqlCommand
            Get
                If isConnected = False Then
                    Connect()
                End If
                _SqlCommand = New SqlCommand
                _SqlCommand.Connection = _SqlConnection
                _SqlCommand.CommandType = CmdType
                _SqlCommand.CommandText = strCmd
                _SqlCommand.CommandTimeout = ConnectionTimeout
                If _InTransaction Then
                    _SqlCommand.Transaction = _SQLTransaction
                End If
                Return _SqlCommand
            End Get
        End Property
        Public ReadOnly Property Datatable()
            Get
                Return _DataTable
            End Get
        End Property
        Public ReadOnly Property ColumnName()
            Get
                Return _Column
            End Get
        End Property
        Public ReadOnly Property ColumnCount()
            Get
                Return _Column.Count
            End Get
        End Property
        Public ReadOnly Property ReturnResult()
            Get
                Return _ReturnResult
            End Get
        End Property
        Public Sub SetCommand(ByVal strCommand As String)
            _Command = strCommand
        End Sub
        Public Sub SetParameter(ByVal _ParName As String, ByVal _Val As String, ByVal _SQLDBType As SqlDbType, Optional ByVal _SQLSize As Decimal = Nothing)
            Dim _htValue As New Hashtable
            _htValue.Add("SQLValue", _Val)
            _htValue.Add("SQLDBType", _SQLDBType)
            _htValue.Add("SQLLength", _SQLSize)

            If _Parameter Is Nothing Then
                _Parameter = New Hashtable

            End If
            _Parameter.Add(_ParName, _htValue)
        End Sub

        Public Sub ClearParameter()
            If Not _Parameter Is Nothing Then
                _Parameter.Clear()
            End If

        End Sub

        Public Sub ExecuteCommand(ByVal strCommand As String, ByVal CmdType As MSSQLClient.CommandType)
            Select Case CmdType
                Case CommandType.Query
                    _DataTable = ExecuteDatatable(strCommand)
                    _Column = New ArrayList
                    For Each _dc As DataColumn In _DataTable.Columns
                        _Column.Add(_dc)
                    Next
                Case CommandType.Delete
                    _ReturnResult = ExecuteUpdate(strCommand)
                Case CommandType.Update
                    _ReturnResult = ExecuteUpdate(strCommand)
                Case CommandType.StoreProcedure


            End Select
        End Sub

        Public Function ExecuteProcedure() As Integer
            Return ExecuteProcedure(_Command)
        End Function

        Public Function ExecuteProcedure(ByVal strCommand As String) As Object
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strCommand, TimeSet.Status.Start)
            _Debug.Write(strCommand, "Execute Command: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim _ret As Object = Nothing
            If Not isConnected Then
                Connect()
            End If
            _SqlCommand = NewSQLCommand(strCommand, System.Data.CommandType.StoredProcedure)
            ' Define Parameter
            Try
                If _Parameter Is Nothing = False Then
                    For Each _o As Object In _Parameter.Keys
                        Dim _htValue As Hashtable = CType(_Parameter(_o), Hashtable)

                        'Select Case CType(_htValue("SQLDBType"), SqlDbType)

                        'End Select
                        If _htValue("SQLSize") <> Nothing Then
                            _SqlCommand.Parameters.Add(_o.ToString(), CType(_htValue("SQLDBType"), SqlDbType))
                        Else
                            _SqlCommand.Parameters.Add(_o.ToString(), CType(_htValue("SQLDBType"), SqlDbType), _htValue("SQLSize"))
                        End If
                        _SqlCommand.Parameters(_o.ToString()).Value = _htValue("SQLValue")
                    Next
                End If

                _SqlCommand.CommandType = System.Data.CommandType.StoredProcedure
                _ret = _SqlCommand.ExecuteScalar()

            Catch ex As Exception
                _Message = _Message & "Exception (ExecuteProcedure) : " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                _ret = -1
            End Try
            If Not _InTransaction Then
                Close()
            End If
            Dim _IO As System.IO.FileInfo

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strCommand, TimeSet.Status.Finish)
            Return _ret
        End Function

        Public Function ExecuteUpdate(ByVal strCommand As String, Optional ByVal _GetIdentity As Boolean = False) As Integer
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strCommand, TimeSet.Status.Start)
            _Debug.Write(strCommand, "Execute Update: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            _isError = False
            If Not isConnected Then
                Connect()
            End If

            _SqlCommand = New SqlCommand(strCommand, _SqlConnection)

            If _InTransaction Then
                _SqlCommand.Transaction = _SQLTransaction
            End If

            Try
                _ReturnResult = _SqlCommand.ExecuteNonQuery()
                If _GetIdentity Then
                    _Identity = ExecuteValue("SELECT @@IDENTITY AS 'Identity'")
                End If
            Catch ex As Exception
                _isError = True
                _ReturnResult = 0
                _Message = _Message & "Exception (ExecuteUpdate): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strCommand)
            End Try

            If Not _InTransaction Then
                Close()
            End If

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strCommand, TimeSet.Status.Finish)
            Return _ReturnResult
        End Function

        Public Function Exists(ByVal strQuery) As Boolean
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery, TimeSet.Status.Start)
            _Debug.Write(strQuery, "Execute Exists: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim ret As Boolean = True
            Dim _dt As DataTable = Nothing
            Dim _ds As New DataSet
            If Not isConnected Then
                Connect()
            End If
            _SqlDataAdapter = New SqlDataAdapter
            Try
                _SqlDataAdapter.SelectCommand = NewSQLCommand(strQuery, System.Data.CommandType.Text)
                _SqlDataAdapter.Fill(_ds)
                _dt = _ds.Tables(0)
                If _dt.Rows.Count > 0 Then
                    ret = True
                Else
                    ret = False
                End If
            Catch ex As Exception
                ret = False
                _Message = _Message & "Exception (ExecuteDatatable): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strQuery)
            End Try
            If Not _InTransaction Then
                Close()
            End If
            Return ret
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery, TimeSet.Status.Finish)
        End Function

        Public Function ExecuteValue(ByVal strQuery As String) As Object
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery, TimeSet.Status.Start)
            _Debug.Write(strQuery, "Execute Value: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim ret As Object = Nothing
            Dim _dt As DataTable = Nothing
            Dim _ds As New DataSet
            If Not isConnected Then
                Connect()
            End If
            _SqlDataAdapter = New SqlDataAdapter
            Try
                _SqlDataAdapter.SelectCommand = NewSQLCommand(strQuery, System.Data.CommandType.Text)
                _SqlDataAdapter.Fill(_ds)
                _dt = _ds.Tables(0)
                If _dt.Rows.Count > 0 Then
                    ret = _dt.Rows(0)(0)
                End If
            Catch ex As Exception
                'Throw New CPSLIB.CPSException(ex, _MainSettings)
                _Message = _Message & "Exception (ExecuteDatatable): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strQuery)
            End Try
            If Not _InTransaction Then
                Close()
            End If

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & ",", TimeSet.Status.Finish)
            Return ret
        End Function

        Public Function ExecuteHashTable(ByVal strKeyField As String, ByVal strQuery As String) As Hashtable
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery & "," & Me.ConnectionString, TimeSet.Status.Start)
            _Debug.Write(strQuery, "Execute Hashtable: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim _ht As Hashtable
            Dim _dt As DataTable
            Dim _ds As New DataSet
            If Not isConnected Then
                Connect()
            End If
            _SqlDataAdapter = New SqlDataAdapter
            Try
                _ht = New Hashtable

                _SqlDataAdapter.SelectCommand = NewSQLCommand(strQuery, System.Data.CommandType.Text)
                _SqlDataAdapter.Fill(_ds)
                _dt = _ds.Tables(0)
                For Each _dr As DataRow In _dt.Rows
                    _ht.Add(_dr(strKeyField), _dr)
                Next
            Catch ex As Exception
                'Throw New CPSLIB.CPSException(ex, _MainSettings)
                _Message = _Message & "Exception (ExecuteDatatable): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strQuery)
            End Try
            If Not _InTransaction Then
                Close()
            End If

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ht
        End Function

        Public Function ExecuteCSV(ByVal strQuery As String, ByVal s As String, ByVal _FilePath As String) As Boolean
            Dim _dt As DataTable
            Dim _ret As IO.Ascii.AsciiFile
            Dim _str As String
            Dim _isFirstLine As Boolean = True
            Try
                _dt = ExecuteDatatable(strQuery)
                _Debug.WriteTable(_dt, strQuery)
                _ret = New IO.Ascii.AsciiFile(_FilePath)

                If _dt.Rows.Count > 0 Then
                    For Each dr As DataRow In _dt.Rows
                        _str = String.Empty
                        If _isFirstLine Then

                            For Each o As DataColumn In _dt.Columns
                                _str = _str & o.ColumnName.ToString & s
                            Next

                            _str = _str.Substring(0, _str.Length - 1)
                            _ret.WriteLine(_str)
                            _isFirstLine = False
                        End If

                        _str = String.Empty

                        For Each o As DataColumn In _dt.Columns

                            If IsDBNull(dr(o.ColumnName)) Then
                                _str = _str & "" & s
                            Else
                                _str = _str & dr(o.ColumnName).ToString().Replace(vbCr, String.Empty).Replace(vbCrLf, String.Empty).Replace("\", "\|") & s
                            End If
                        Next

                        _str = _str.Substring(0, _str.Length - 1)
                        _ret.WriteLine(_str)
                    Next

                    Return True
                Else
                    _Message = "No data found"
                    Return False
                End If

            Catch ex As Exception
                _Message = "Exception (ExecuteCSV): " & ex.Message
                _CPSException.ExecuteHandle(ex)
                Return False
            End Try

        End Function

        Public Function ExecuteStoreProcedure(ByVal strQuery As String, ByVal _htParameter As Hashtable) As SqlDataReader

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery, TimeSet.Status.Start)
            _Debug.Write(strQuery, "Execute StoreProcedure: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim _dt As DataTable = Nothing
            Dim _ds As New DataSet
            Dim _sdr As SqlDataReader
            Dim _sqlCommand As SqlCommand
            Dim param As SqlParameter
            _isError = False

            If Not isConnected Then
                Connect()
            End If

            '_SqlDataAdapter = New SqlDataAdapter
            Try
                _sqlCommand = NewSQLCommand(strQuery, System.Data.CommandType.StoredProcedure)
                _Debug.Write("parameter")

                For Each o As Object In _htParameter.Keys
                    _Debug.Write(_htParameter(o).ToString, o.ToString)
                    param = New SqlParameter
                    param = _sqlCommand.Parameters.Add(o.ToString, SqlDbType.NVarChar, 100)
                    param.Value = _htParameter(o).ToString
                Next

                _sdr = _sqlCommand.ExecuteReader

            Catch ex As Exception
                _isError = True
                'Throw New CPSLIB.CPSException(ex, _MainSettings)
                _Message = _Message & "Exception (ExecuteStoreProcedure): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strQuery)
            End Try
            If Not _InTransaction Then
                Close()
            End If

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _sdr
        End Function


        Public Function ExecuteDatatable(ByVal strQuery As String) As DataTable

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & strQuery, TimeSet.Status.Start)
            _Debug.Write(strQuery, "Execute Datatable: " & _SqlDatabase, CPSLIB.Debug.LineType.Information)
            Dim _dt As DataTable = Nothing
            Dim _ds As New DataSet
            _isError = False
            If Not isConnected Then
                Connect()
            End If
            _SqlDataAdapter = New SqlDataAdapter
            Try
                _SqlDataAdapter.SelectCommand = NewSQLCommand(strQuery, System.Data.CommandType.Text)
                _SqlDataAdapter.Fill(_ds)
                _dt = _ds.Tables(0)

            Catch ex As Exception
                _isError = True
                'Throw New CPSLIB.CPSException(ex, _MainSettings)
                _Message = _Message & "Exception (ExecuteDatatable): " & ex.Message & vbCrLf & ex.StackTrace
                _CPSException.ExecuteHandle(ex, strQuery)
            End Try
            If Not _InTransaction Then
                Close()
            End If

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _dt
        End Function


        Public Function ExecuteDatatable(ByVal _dr As DataRow(), ByVal _dc As DataColumnCollection) As DataTable
            Dim _dtRet As DataTable
            Dim newDataRow As DataRow
            _dtRet = New DataTable
            ' Create DataStructure
            Try


                For Each dc As DataColumn In _dc
                    _dtRet.Columns.Add(dc.ColumnName, dc.DataType)
                Next
                ' Loop Row Level
                For Each dr As DataRow In _dr
                    newDataRow = _dtRet.NewRow
                    For Each dc As DataColumn In _dc
                        newDataRow(dc.ColumnName) = dr(dc.ColumnName)
                    Next
                    _dtRet.Rows.Add(newDataRow)
                Next
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _dtRet
        End Function

        Public Sub Close()
            Try
                _SqlConnection.Close()
                _isConnected = False
            Catch ex As Exception
                'Karrson: Remark: throw new CPSException(ex)
                _hasException = True
                _Message = _Message & "Exception: " & ex.Message
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
            End Try
        End Sub

        Public Function StartTansaction() As Boolean
            Dim _ret As Boolean
            Try
                _SQLTransaction = _SqlConnection.BeginTransaction(TransactionName)

                _ret = True
                _InTransaction = True
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                _ret = False
            End Try
            Return _ret
        End Function
        Public Function ComitTransaction() As Boolean
            Dim _ret As Boolean
            Try
                _SQLTransaction.Commit()
                _ret = True
                _InTransaction = False
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                _ret = False
            End Try
            Return _ret
        End Function
        Public Function RollbackTransaction() As Boolean
            Dim _ret As Boolean
            Try
                _SQLTransaction.Rollback()
                _ret = True
                _InTransaction = False
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                _ret = False
            End Try
            Return _ret
        End Function
#End Region
    End Class
End Namespace
