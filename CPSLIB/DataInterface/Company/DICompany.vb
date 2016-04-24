Imports SAPbobsCOM
Imports CPSLIB.Logging

Namespace DataInterface.Company

    Public Class DICompany
        Private _Debug As CPSLIB.Debug

        Private _CPSException As CPSException
        Private _Message As String = ""
        Private _oCompany As SAPbobsCOM.Company
        Private _CompanyDB As String
        Private _ServerName As String
        Private _LicenseServer As String
        Private _UserName As String
        Private _Password As String
        Private _DBUserName As String
        Private _DBPassword As String
        Private _ServerDBType As DataInterface.Company.DICompany.DataBaseType
        Private _ret As Integer
        Private _isError As Boolean
        Private _hasException As Boolean
        Private _CompanyInfo As CompanyInfo

        Public Enum DataBaseType
            MSSQL = 1
            DB2 = 2
            SYBASE = 3
            MSSQL2005 = 4
            MAXDB = 5
            MSSQL2008 = 6
            MSSQL2012 = 7

        End Enum

        Public Sub New()
            _isError = False
            _hasException = False
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _oCompany = New SAPbobsCOM.Company
        End Sub

        Public Sub New(ByVal _ServerName As String, ByVal _CompanyDB As String,
                       ByVal _LicenseServer As String, ByVal _UserName As String,
                       ByVal _Password As String, ByVal _DBUserName As String,
                       ByVal _DBPassword As String, ByVal _ServerDBType As DICompany.DataBaseType)

            _isError = False

            _hasException = False
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _oCompany = New SAPbobsCOM.Company
            ServerName = _ServerName
            CompanyDB = _CompanyDB
            LicenseServer = _LicenseServer
            UserName = _UserName
            Password = _Password

            If _DBUserName <> String.Empty Then
                DBUserName = _DBUserName
            End If
            If _DBPassword <> String.Empty Then
                DBPassword = _DBPassword
            End If

            ServerDBType = _ServerDBType
        End Sub
#Region "Process"
        Public Function ExecuteProcedure(ByVal strSql As String) As String
            _Debug.Write(strSql, "Procedure Query", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            Dim rs As SAPbobsCOM.Recordset
            Dim _ret As Object = String.Empty
            Dim _connectStatus As Boolean = _oCompany.Connected

            Try
                If _oCompany.Connected = False Then
                    Connect()

                End If
                If Ret <> 0 Then
                    Return Message
                Else
                    rs = RecordSet
                    rs.DoQuery(strSql)
                    If _connectStatus = False Then
                        _oCompany.Disconnect()
                    End If
                End If

                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _Message = _Message & "Exception (ExecuteProcedure): " & ex.Message & vbCrLf
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                _ret = ex.Message
            End Try
            Return _ret
        End Function

        Public Function ExecuteValue(ByVal strSql As String, ByVal strField As String) As Object
            _Debug.Write(strSql, "Execute Value Query", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            _Debug.Write(strField, "Field", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
            Dim rs As SAPbobsCOM.Recordset = RecordSet
            Dim _ret As Object = String.Empty
            Try
                rs.DoQuery(strSql)
                If Not rs.EoF Then
                    _ret = rs.Fields().Item(strField).Value
                End If
                rs = Nothing
                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _Message = _Message & "Exception (ExecuteRecordSet): " & CompanyDB & ":" & ex.Message & vbCrLf
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
            End Try

            Return _ret
        End Function

        Public Function TestConnection() As String

            Dim oMessage As String = CompanyDB & vbTab & ":"
            Connect()
            If Connected Then
                oMessage = oMessage & New Logging.MessageCode().Read(MessageCode.MessageCode.SUCCESS)
                Disconnect()
            Else
                oMessage = oMessage & _Message
            End If

            Return oMessage
        End Function
        Public Sub Connect()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & " " & _oCompany.CompanyDB, TimeSet.Status.Start)

            Try
                _Debug.Write("try to connect company")
                _ret = _oCompany.Connect
                If _ret <> 0 Then
                    _isError = True
                    _Debug.Write(_oCompany.CompanyDB, "Connection Failure: " & _oCompany.GetLastErrorDescription, System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Warning)
                    _Message = _oCompany.GetLastErrorCode & ":" & _oCompany.GetLastErrorDescription
                Else
                    _Debug.Write(_oCompany.CompanyDB, "Connection Success", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLIB.Debug.LineType.Information)
                    _CompanyInfo = New CompanyInfo(Me)
                End If

            Catch ex As Exception
                _isError = True
                _hasException = True
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                'Karrson: Remark: throw new CPSException(ex)
            End Try
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name & "," & " " & _oCompany.CompanyDB, TimeSet.Status.Finish)
        End Sub
        Public Sub Disconnect()
            Try
                If _oCompany.Connected Then
                    _oCompany.Disconnect()
                End If
                _hasException = False
            Catch ex As Exception
                _isError = True
                _hasException = True
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                'Karrson: Remark: throw new CPSException(ex)
            End Try
        End Sub
#End Region
#Region "Transaction"
        Public Sub SetTransaction()
            _Debug.Write(_oCompany.CompanyDB, "Start Transaction", CPSLIB.Debug.LineType.Information)
            _oCompany.StartTransaction()

        End Sub
        Public Sub CommitTransaction()
            _Debug.Write(_oCompany.CompanyDB, "Comit Transaction", CPSLIB.Debug.LineType.Information)
            EndTransaction(BoWfTransOpt.wf_Commit)
        End Sub
        Public Sub RollbackTransaction()
            _Debug.Write(_oCompany.CompanyDB, "Rollback Transaction", CPSLIB.Debug.LineType.Information)
            EndTransaction(BoWfTransOpt.wf_RollBack)
        End Sub
        Public Sub EndTransaction(ByVal _endType As SAPbobsCOM.BoWfTransOpt)

            If _oCompany.InTransaction Then
                Try
                    _oCompany.EndTransaction(_endType)
                    _hasException = False
                Catch ex As Exception
                    _isError = True
                    _hasException = True
                    _Message = _Message & "Exception (EndTransaction): " & ex.Message & vbCrLf
                    _CPSException.ExecuteHandle(ex, Reflection.MethodBase.GetCurrentMethod.Name)
                End Try

            End If

        End Sub
#End Region

#Region "Property"
        Public ReadOnly Property CompInfomation() As CompanyInfo
            Get
                Return _CompanyInfo
            End Get
        End Property
        Public ReadOnly Property Ret() As Integer
            Get
                Return _ret

            End Get
        End Property
        Public ReadOnly Property inTransaction() As Boolean
            Get
                Return _oCompany.InTransaction
            End Get
        End Property
        Public ReadOnly Property RecordSet() As Recordset
            Get
                If _oCompany.Connected = False Then
                    Me.Connect()
                End If
                Return _oCompany.GetBusinessObject(BoObjectTypes.BoRecordset)
            End Get
        End Property
        Public Property ServerName() As String
            Get
                Return _ServerName
            End Get
            Set(ByVal value As String)
                _ServerName = value
                _oCompany.Server = value
            End Set
        End Property
        Public Property LicenseServer() As String
            Get
                Return _LicenseServer
            End Get
            Set(ByVal value As String)
                _LicenseServer = value
                _oCompany.LicenseServer = value
            End Set
        End Property
        Public Property UserName() As String
            Get
                Return _UserName
            End Get
            Set(ByVal value As String)
                _UserName = value
                _oCompany.UserName = value
            End Set
        End Property
        Public Property Password() As String
            Get
                Return _Password
            End Get
            Set(ByVal value As String)
                _Password = value
                _oCompany.Password = value
            End Set
        End Property
        Public Property DBUserName() As String
            Get
                Return _DBUserName
            End Get
            Set(ByVal value As String)
                _DBUserName = value
                _oCompany.DbUserName = value
            End Set
        End Property
        Public Property DBPassword() As String
            Get
                Return _DBPassword
            End Get
            Set(ByVal value As String)
                _DBPassword = value
                _oCompany.DbPassword = value
            End Set
        End Property

        Public Property CompanyDB() As String
            Get
                Return _CompanyDB
            End Get
            Set(ByVal value As String)
                _CompanyDB = value
                _oCompany.CompanyDB = value
            End Set
        End Property

        Public Property ServerDBType() As DataInterface.Company.DICompany.DataBaseType
            Get
                Return _ServerDBType
            End Get
            Set(ByVal value As DataInterface.Company.DICompany.DataBaseType)
                _ServerDBType = value
                _oCompany.DbServerType = value
            End Set
        End Property
        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property
        Public ReadOnly Property Connected() As Boolean
            Get

                Return _oCompany.Connected
            End Get
        End Property
        Public ReadOnly Property Company() As SAPbobsCOM.Company
            Get
                Return _oCompany
            End Get
        End Property
        Public ReadOnly Property isError() As Boolean
            Get
                Return isError
            End Get
        End Property
#End Region
#Region "Company Information"
        Public Function SystemCurrency() As String
            Dim ret As String
            Dim strSql
        End Function

#End Region
    End Class
End Namespace
