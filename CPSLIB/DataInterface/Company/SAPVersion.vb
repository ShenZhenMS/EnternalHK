Imports CPSLIB.Data
Namespace DataInterface.Company

    Public Class SAPVersion : Inherits Data.Connection.MSSQLClient

        Public Shared SBOCOMMON_DATABASENAME As String = "SBO-COMMON"
        Public Shared Field_DBNAME As String = "dbName"
        Public Shared Field_CompanyName As String = "cmpName"
        Public Shared Field_VersionNum As String = "VERSSTR"
        Public Shared Field_DBUser As String = "dbUser"
        Public Shared Field_Location As String = "Loc"

        Public Shared QUERY_SAPVersion As String = "SELECT {0} FROM [{1}].DBO.SRGC WHERE 1 = 1 {2}"
        Private _Message As String

        Private _dtVersion As DataTable
        Private _htVersion As Hashtable
        Private _Debug As CPSLib.Debug
        Private _CPSException As CPSException

        Public Sub New(ByVal _ServerName As String, ByVal _LoginID As String, ByVal _Password As String)
            Me.New(_ServerName, _LoginID, _Password, SBOCOMMON_DATABASENAME)

        End Sub
        Public Sub New(ByVal _ServerName As String, ByVal _LoginID As String, ByVal _Password As String, ByVal _CommonDB As String)

            MyBase.New(_ServerName, _CommonDB, _LoginID, _Password)
            _Debug = New CPSLib.Debug(Me.GetType.ToString)
            _CPSException = New CPSException

            If MyBase.Connect() = False Then

            End If
            _htVersion = New Hashtable
            Execute()
            MyBase.Close()
            _Debug.Finish()
        End Sub
#Region "Execute"
        Private Sub Execute()
            Try
                _htVersion.Clear()
                _dtVersion = MyBase.ExecuteDatatable(String.Format(QUERY_SAPVersion, Field_DBNAME & "," & Field_VersionNum, SBOCOMMON_DATABASENAME, String.Empty))
                _Debug.Write(String.Format(QUERY_SAPVersion, Field_DBNAME & "," & Field_VersionNum, SBOCOMMON_DATABASENAME, String.Empty), "Query", CPSLIB.Debug.LineType.Information)
                If Not _dtVersion Is Nothing Then
                    If _dtVersion.Rows.Count > 0 Then
                        For Each _dr As DataRow In _dtVersion.Rows
                            If _htVersion.ContainsKey(_dr.Item(Field_DBNAME)) Then
                                _htVersion(_dr.Item(Field_DBNAME)) = _dr.Item(Field_VersionNum)
                            Else
                                _htVersion.Add(_dr.Item(Field_DBNAME), _dr.Item(Field_VersionNum))
                            End If

                        Next
                    End If
                End If
            Catch ex As Exception
                _Debug.Write(ex.Message, "Execute Exception", CPSLIB.Debug.LineType.Error)
                _CPSException.ExecuteHandle(ex)
                _Message = ex.Message
            End Try
        End Sub
#End Region
#Region "Property"
        Public ReadOnly Property Count()
            Get
                Return _htVersion.Count
            End Get
        End Property

        Public ReadOnly Property VersionNumber(ByVal dbname As String) As Integer

            Get
                _Debug.Write(dbname, "Retreive Version Number", CPSLIB.Debug.LineType.Information)
                If _htVersion.ContainsKey(dbname) = False Then
                    Return -1
                Else
                    Return Convert.ToInt32(_htVersion(dbname))
                End If
            End Get
        End Property

        Public Function GetDatabase() As Hashtable
            Return _htVersion
        End Function
        Public Function GetTable() As DataTable
            Return _dtVersion
        End Function
#End Region
    End Class

End Namespace
