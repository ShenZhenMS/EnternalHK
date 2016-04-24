Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CompilerServices

Public Class InventoryInoutOperation
    'jerry  Add Fields DocDueDate CardCode
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _WMSConn As WMSSQLConnections
    Dim _InventoryInOutConfig As InventoryInoutConfig
    Dim _StockIn As InventoryReceive.InventoryGenEntryService
    Dim _StockOut As InventoryIssue.InventoryGenExitService
    Dim _InventoryInOut As InventoryInoutForWMS
    Dim _Setting As Settings
    Dim _DISI As DIServer_StockIn
    Dim _WSSO As DIServer_StockOut

    Dim oDoc As Object
    Private _Message As String
    Private _isError As Boolean
    Dim _WSSI As WS_StockIn
    Public Property isError() As Boolean
        Get
            Return _isError
        End Get
        Set(ByVal value As Boolean)
            _isError = value
        End Set
    End Property

    Public Property Message() As String
        Get
            Return _Message
        End Get
        Set(ByVal value As String)
            _Message = value
        End Set
    End Property

    Public Sub New(ByVal _Setting As Settings, ByVal _DocType As InventoryInoutForWMS._DocumentType)
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _WMSConn = New WMSSQLConnections(_Setting)
        _InventoryInOutConfig = New InventoryInoutConfig(_Setting)
        _InventoryInOut = New InventoryInoutForWMS(_Setting, Nothing, _DocType)
    End Sub


    Public Function Start(ByVal _dt As DataTable) As Boolean
        Dim _ret As Boolean = True
        Dim _DIConn As CPSLIB.DIServer.DIServerConnection
        Try
            Dim _htKeyValue As Hashtable
            Dim _htDocStatus As Hashtable
            _DIConn = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
            If _DIConn.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Fail Then
                _ret = False
                _isError = True
                _Message = _DIConn.CmdMessage
            Else
                Select Case _InventoryInOut.DocumentType
                    Case InventoryInoutForWMS._DocumentType.GI
                        _WSSO = New DIServer_StockOut(_Setting, _DIConn)

                        If _WSSO.Start(_dt) = False Then
                            _ret = False
                            _isError = True
                            _Message = _WSSO.CmdMessage
                        Else
                            _ret = True
                            _isError = False
                            _Message = String.Empty
                        End If
                    Case InventoryInoutForWMS._DocumentType.GR
                        _htKeyValue = New Hashtable
                        _htDocStatus = New Hashtable
                       
                        Try
                            If _dt.Rows.Count > 0 Then
                                For Each dr As DataRow In _dt.Rows
                                    If dr(Inventory_Inout.Fld_isDraft) = "N" Then
                                        _htKeyValue(dr(_InventoryInOutConfig.KeyField)) = "Y"
                                    Else
                                        _htKeyValue(dr(_InventoryInOutConfig.KeyField)) = "N"
                                    End If
                                Next
                            Else
                                Return False
                            End If

                            If _htKeyValue.Count > 0 Then
                                For Each o As Object In _htKeyValue.Keys
                                    If _htKeyValue(o) = "Y" Then
                                        _DISI = New DIServer_StockIn(_Setting, _DIConn)
                                        _htDocStatus(o) = _DISI.Start(o, _dt)
                                    Else
                                        _DISI = New DIServer_StockIn(_Setting, _DIConn, True)
                                        _htDocStatus(o) = _DISI.Start(o, _dt)
                                    End If
                                Next

                            End If
                        Catch ex As Exception
                            _ret = False
                            _CPSException.ExecuteHandle(ex)
                        End Try
                End Select
            End If

        Catch ex As Exception
            _Debug.Write("Exception: " & ex.Message)
            _ret = False
            _isError = True
            _Message = ex.Message
        End Try
        Return _ret
    End Function
End Class



