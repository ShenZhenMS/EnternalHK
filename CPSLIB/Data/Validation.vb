Namespace Data
    Public Class Validation



        Public Shared Function DBNull(ByVal _value As Object) As Object
            Return DBNull(_value, String.Empty)
        End Function
        Public Shared Function DBNull(ByVal _value As Object, ByVal _dftValue As Object) As Object
            If IsDBNull(_value) Then
                Return _dftValue
            Else
                Return _value
            End If
        End Function
        Public Shared Function DBNull(ByVal _value As Object, ByVal _dbtype As DbType) As Object
            If IsDBNull(_value) Then
                Select Case _dbtype
                    Case DbType.Currency
                        Return 0
                    Case DbType.Date
                        Return Nothing
                    Case DbType.Decimal
                        Return 0
                    Case DbType.Double
                        Return 0
                    Case DbType.Int32
                        Return 0
                    Case DbType.Object
                        Return 0
                    Case DbType.AnsiString
                        Return String.Empty
                    Case Else
                        Return String.Empty
                End Select
            Else
                Return _value

            End If
        End Function
        Public Shared Function IsNull(ByVal _value As String) As String
            Return IsNull(_value, DbType.AnsiString)
        End Function
        Public Shared Function isNull(ByVal _value As DateTime) As DateTime
            Return isNull(_value, DbType.Date)
        End Function
        Public Shared Function isNull(ByVal _value As Integer) As Integer
            Return isNull(_value, DbType.Int32)
        End Function
        Public Shared Function isNull(ByVal _value As Decimal) As Decimal
            Return isNull(_value, DbType.Currency)
        End Function
        Public Shared Function isNull(ByVal _value As Object, ByVal _dbtype As DbType) As Object
            If _value Is Nothing Then
                Select Case _dbtype
                    Case DbType.Currency
                        Return 0
                    Case DbType.Date
                        Return Nothing
                    Case DbType.Decimal
                        Return 0
                    Case DbType.Double
                        Return 0
                    Case DbType.Int32
                        Return 0
                    Case DbType.Object
                        Return 0
                    Case DbType.AnsiString
                        Return String.Empty
                    Case Else
                        Return String.Empty
                End Select
            Else
                Return _value

            End If
        End Function
#Region "Date"
        Public Shared Function minDate() As DateTime
            Return New DateTime(1900, 1, 1)
        End Function
        Public Shared Function maxDate() As DateTime
            Return New DateTime(9999, 12, 31)
        End Function

        Public Shared Function isDate(ByVal _o As Object) As Boolean
            Dim ret As Boolean = True
            Dim _d As Date
            Try
                _d = Convert.ToDateTime(_o)
            Catch ex As Exception
                ret = False
            End Try
            Return ret
        End Function

#End Region
#Region "Numeric"
        Public Shared Function isNumeric(ByVal o As Object) As Boolean
            Dim _ret As Boolean = True
            Dim _n As Decimal
            Try
                _n = Convert.ToDecimal(o)
            Catch ex As Exception
                _ret = False
            End Try
            Return _ret
        End Function
#End Region
    End Class
End Namespace
