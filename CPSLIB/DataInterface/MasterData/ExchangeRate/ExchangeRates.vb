Imports CPSLIB.DataInterface.Company
Imports SAPbobsCOM

Namespace DataInterface.MasterData.ExchangeRate
    Public Class ExchangeRates
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Public Shared _DEFAULT_COPYEXPRESS_PROCEDURE As String = "CPS_SP_CopyExchangeRates"
        Dim _Status As DataInterface.Document.Document.PostStatus
        Dim _Doc As SBObob
        Dim _UpdateRecord As Boolean = False
        Dim _Message As String
        Dim _ConnectStatus As Boolean
        Dim _diCompany As DataInterface.Company.DICompany


        Public Sub New(ByVal _diCompany As DataInterface.Company.DICompany)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            _Status = DataInterface.Document.Document.PostStatus.Ready
            _Doc = _diCompany.Company.GetBusinessObject(BoObjectTypes.BoBridge)
            Me._diCompany = _diCompany
        End Sub

        Public Sub Execute(ByVal _Currency As String, ByVal _Date As Date, ByVal _value As Decimal)
            Try
                _Debug.Write("", "", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLib.Debug.LineType.Information)
                _Doc.SetCurrencyRate(_Currency, _Date, _value, _UpdateRecord)
            Catch ex As Exception
                _Debug.Write(ex.Message, "Exception", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLib.Debug.LineType.Error)
                _CPSException.ExecuteHandle(ex)
                _Message = _Message & "Exception (Execute) :" & ex.Message
            End Try
        End Sub

        Public Function Copy(ByVal SourceCompany As DataInterface.Company.DICompany, ByVal DateFrom As String, ByVal DateTo As String, ByVal Currency As String) As String
            Dim _ret As String = String.Empty
            Dim _retid As Integer
            Dim _sqlclient As Data.Connection.MSSQLClient
            Dim _ExecuteAsProcedure As Boolean = False
            Try
                _sqlclient = New Data.Connection.MSSQLClient(_diCompany.ServerName, _diCompany.CompanyDB, _diCompany.DBUserName, _diCompany.DBPassword)
                _sqlclient.Connect()
                '           @DATABASE NVARCHAR(20),
                '@V_CURRENCY NVARCHAR(MAX),
                '@DATEFROM NVARCHAR(10),
                '@DATETO NVARCHAR(10)
                If _sqlclient.isObjectExists(Data.Connection.MSSQLClient.SysObjectType.SQL_STORED_PROCEDURE, _DEFAULT_COPYEXPRESS_PROCEDURE) Then
                    _ExecuteAsProcedure = True
                    _sqlclient.ClearParameter()
                    _sqlclient.SetCommand(_DEFAULT_COPYEXPRESS_PROCEDURE)
                    _sqlclient.SetParameter("@DATABASE", SourceCompany.CompanyDB, SqlDbType.NVarChar, 20)
                    _sqlclient.SetParameter("@V_CURRENCY", Currency, SqlDbType.NVarChar, 255)
                    _sqlclient.SetParameter("@DATEFROM", DateFrom, SqlDbType.NVarChar, 10)
                    _sqlclient.SetParameter("@DATETO", DateTo, SqlDbType.NVarChar, 10)
                    _retid = _sqlclient.ExecuteProcedure()

                End If
                _sqlclient.Close()
            Catch ex As Exception
                _Debug.Write(ex.Message, "Exception", System.Reflection.MethodBase.GetCurrentMethod.ToString, CPSLib.Debug.LineType.Error)
                _CPSException.ExecuteHandle(ex)
                _Message = _Message & "Exception (Copy) : " & ex.Message
                _ret = ex.Message
            End Try
            If _ExecuteAsProcedure = False Then
                'Under Construction
            End If
            Return _ret
        End Function



#Region "Property"
        Public Property UpdateRecord() As Boolean
            Get
                Return _UpdateRecord
            End Get
            Set(ByVal value As Boolean)
                _UpdateRecord = value
            End Set
        End Property
        Public Property Message() As String
            Get
                Return _Message
            End Get
            Set(ByVal value As String)

            End Set
        End Property
#End Region
    End Class
End Namespace
