Public Class BinLocationForSAP : Inherits SAPSQLConnections

    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _sqlUpdateBinLocation As String = "EXEC CPS_PROC_UPDATEBINLOCATION '{0}', '{1}', '{2}','{3}','{4}'"


    '<ItemCode>106-06</ItemCode> <BatchNum>20130930002</BatchNum> <FromLocCode>1002</FromLocCode> <ToLocCode>1005</ToLocCode> <Quantity>1000.000000</Quantity> <USER/> <WhsCode>OFFICE</WhsCode>
    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)

        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException

    End Sub
    Public Function Test() As Boolean
        Return False
    End Function

    Public Function UpdateBinLocation(ByVal _dt As DataTable) As Boolean
        Dim _status As Boolean
        Dim _rollback As Boolean = False
        MyBase.StartTansaction()
        For Each _dr As DataRow In _dt.Rows
            _status = UpdateBinLocation(_dr)
            If _status = False Then
                _rollback = True
            End If
        Next

        'If _rollback = True Then
        '    MyBase.RollbackTransaction()
        'End If
        MyBase.ComitTransaction()

        Return _rollback
    End Function

    'Modified by MK, 20130830
    Private Function UpdateBinLocation(ByVal _dr As DataRow) As Boolean
        Dim _ret As Integer = -1
        Dim _status As Boolean = False
        Try
            'Dim fld1 As String = _dr(0)
            'Dim fld2 As String = _dr(1)
            'Dim fld3 As String = _dr(2)
            'Dim fld4 As String = _dr(3)
            'MyBase.ExecuteUpdate()
            _ret = MyBase.ExecuteUpdate(String.Format(_sqlUpdateBinLocation, _dr(BinLocationForWMS.Fld_ItemCode), _dr(BinLocationForWMS.Fld_WhsCode), _dr(BinLocationForWMS.Fld_ToLocCode), _dr(BinLocationForWMS.Fld_BatchNum), _dr(BinLocationForWMS.Fld_FromLocCode)))
            If _ret > 0 Then

                ' Success
                _status = True

            Else

                ' Failure
                _status = False

            End If
        Catch ex As Exception

            _CPSException.ExecuteHandle(ex)

        End Try

        Return _status
    End Function

End Class