Namespace Data
    Public Class SAPUDF : Inherits Data.Connection.SQLServerInfo

        Dim _TBLName As String
        Dim _Debug As CPSLIB.Debug
        Dim _CPSException As CPSException
        Dim _htUDF As Hashtable
        Public Sub New(ByVal _Server As String, ByVal _DBName As String, ByVal _SQLUserName As String, ByVal _SQLPassword As String, ByVal _TBLName As String)
            MyBase.New(_Server, _SQLUserName, _SQLPassword, _DBName)
            Me._TBLName = _TBLName
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException

            setUDF()
        End Sub

        Private Sub setUDF()
            _htUDF = New Hashtable
            Dim _sql As String = "select 'U_' + AliasID as AliasID,TypeID from CUFD where TableID = '{0}'"
            Try
                _htUDF = MyBase.ExecuteHashTable("AliasID", String.Format(_sql, _TBLName))

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub

        Public Function GetUDF(ByVal _DocEntry As String, Optional ByVal _LineNum As String = "-1", Optional ByVal _IgnoreColumn As ArrayList = Nothing)
            Dim _htValue As New Hashtable
            Dim _dt As DataTable
            Dim _sql As String = "SELECT * FROM {0} WHERE DocEntry = '{1}' AND {2}"
            Try
                _sql = String.Format(_sql, _TBLName, _DocEntry, IIf(_LineNum = "-1", "1 = 1", String.Format("LineNum = '{0}'", _LineNum)))
                _Debug.Write(_sql, "Execute SQL")
                _dt = MyBase.ExecuteDatatable(_sql)
                If _dt.Rows.Count > 0 Then
                    For Each o As Object In _htUDF.Keys
                        If Not _IgnoreColumn.Contains(o) Then
                            Try
                                If Not IsDBNull(_dt(0)(o)) Then
                                    _htValue(o) = _dt(0)(o).ToString
                                End If

                            Catch ex As Exception
                                _CPSException.ExecuteHandle(ex)
                            End Try

                        End If
                    Next
                End If

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

            Return _htValue
        End Function

    End Class
End Namespace