Public Class Sales : Inherits WMSSQLConnections

    Public Shared Fld_DocType As String = "DocType"
    Public Shared Fld_DocEntry As String = "DocEntry"
    Public Shared Fld_LineNum As String = "LineNum"
    Public Shared Fld_DocNum As String = "DocNum"
    Public Shared Fld_DocDueDate As String = "DocDueDate"
    Public Shared Fld_CardCode As String = "CardCode"
    Public Shared Fld_CardName As String = "CardName"
    Public Shared Fld_ItemCode As String = "ItemCode"
    Public Shared Fld_ItemName As String = "ItemName"
    Public Shared Fld_Quantity As String = "Quantity"
    Public Shared Fld_UOM As String = "UOM"
    Public Shared Fld_WhsCode As String = "WhsCode"
    Public Shared Fld_WhsName As String = "WhsName"
    Public Shared Fld_BatchNum As String = "BatchNum"
    Public Shared Fld_LocCode As String = "LocCode"
    Public Shared Fld_TgtEntry As String = "TgtEntry"
    Public Shared Fld_TgtNum As String = "TgtNum"
    Public Shared Fld_ErrCode As String = "ErrCode"
    Public Shared Fld_ErrDscr As String = "ErrDscr"
    Public Shared Fld_TrtCreateDate As String = "TrtCreateDate"
    Public Shared Fld_ReceiveEntry As String = "ReceiveEntry"
    Public Shared Fld_LastRunDate As String = "LastRunDate"
    Public Shared Fld_TrxStatus As String = "TrxStatus"
    Public Shared Fld_PickNum As String = "PickNum"
    Public Shared Fld_ReceiveLineNum As String = "ReceiveLineNum"
    Public Shared Fld_LineQuantity As String = "LineQuantity"
    Public Shared Fld_NewDocDueDate As String = "NewDocDueDate"

    Public Shared TBL_ORDR As String = "CPS_TBL_ORDR"
    Dim _sqlUpdate_Suc As String = "UPDATE {0} SET TrxStatus = 'S', LastRunDate = getDate(), TrtCreateDate = getDate(), ErrCode = '',ErrDscr = '', TgtEntry = '{1}',TgtNum = '{2}' Where DocEntry = {3} and isNull(TrxStatus,'') = ''"
    Dim _sqlUpdate_Err As String = "UPDATE {0} SET TrxStatus = 'F', LastRunDate = getDate(), TrtCreateDate = null, ErrCode = '{1}',ErrDscr = '{2}', TgtEntry = null,TgtNum = null  Where DocEntry = {3} and isNull(TrxStatus,'') = ''"
    Dim _sqlOpenSO As String = "SELECT {0} {1} FROM CPS_TBL_ORDR WHERE ISNULL(TRXSTATUS,'') IN ('F','') ORDER BY DOCENTRY ASC"
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _dtOpenSO As DataTable
    Dim _dtDistinctOpenSO As DataTable
    Dim _Setting As Settings
    Dim _SAPDIConn As SAPDIConnections
    Dim _SAPSQLConn As SAPSQLConnections
    Dim _SalesConfig As SalesConfig
    Private _ErrorMsg As String

    Public Property ErrorMessage() As String
        Get
            Return _ErrorMsg
        End Get
        Set(ByVal value As String)
            _ErrorMsg = value
        End Set
    End Property


    Public Sub New(ByVal _Setting As Settings, ByVal _SAPDIConn As SAPDIConnections)
        MyBase.New(_Setting)
        Me._Setting = _Setting
        Me._SAPDIConn = _SAPDIConn
        Me._SAPSQLConn = _SAPSQLConn
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        _SalesConfig = New SalesConfig(_Setting)
        If Not _SalesConfig.isActive Then
            MyBase.isError = True
            _ErrorMsg = _SalesConfig.Message
        End If
    End Sub

    Public Function OpenSalesOrder() As Boolean
        _dtDistinctOpenSO = MyBase.ExecuteDatatable(String.Format(_sqlOpenSO, "Distinct", Fld_DocEntry))
        _dtOpenSO = MyBase.ExecuteDatatable(String.Format(_sqlOpenSO, "", "*"))
        If MyBase.isError Then
            _ErrorMsg = MyBase.Message
        End If
        Return Not MyBase.isError
    End Function

    Public Function Generate() As Boolean
        Generate = True
        Try
            If _SAPDIConn.Connected Then
                _SAPDIConn.Connect()
            End If
            If _SAPDIConn.Connected = False Then
                Generate = False
                _ErrorMsg = _SAPDIConn.Message
            Else
                If _dtDistinctOpenSO.Rows.Count > 0 Then
                    For Each dr As DataRow In _dtDistinctOpenSO.Rows
                        Generate = Generate(dr(Fld_DocEntry))
                    Next
                End If
            End If

        Catch ex As Exception
            Generate = False
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
        End Try
    End Function

    Public Function Generate(ByVal _DocEntry As String) As Boolean
        Dim drSOLine As DataRow()
        Dim oDoc As SAPbobsCOM.Documents
        ' Module Configuration Check
        Dim _newDocNum As String
        Dim _newDocEntry As String
        Dim PrevLineNum As Integer = -1

        Generate = True
        Try
            drSOLine = _dtOpenSO.Select(String.Format(" {0} = '{1}'", Fld_DocEntry, _DocEntry), String.Format("{0} asc", Fld_LineNum))
            If drSOLine.Length > 0 Then

                Select Case _SalesConfig.trtDocType

                    Case "13"
                        oDoc = _SAPDIConn.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
                    Case "15"
                        oDoc = _SAPDIConn.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes)
                End Select

                oDoc.CardCode = drSOLine(0)(Fld_CardCode)
                oDoc.DocDate = drSOLine(0)(Fld_DocDueDate)
                oDoc.DocDueDate = drSOLine(0)(Fld_DocDueDate)
                oDoc.Comments = "WMS Sales"
                PrevLineNum = -1
                Dim _isFirst As Boolean = True
                Dim _AccQty As Decimal = 0
                For Each dr As DataRow In drSOLine
                    If PrevLineNum <> dr(Fld_LineNum) And Not _isFirst Then
                        oDoc.Lines.Quantity = _AccQty
                        oDoc.Lines.Add()
                        _AccQty = 0
                    End If
                    _isFirst = False
                    _AccQty = _AccQty + dr(Fld_Quantity)
                    oDoc.Lines.BaseType = "17"
                    oDoc.Lines.BaseEntry = dr(Fld_DocEntry)
                    oDoc.Lines.BaseLine = dr(Fld_LineNum)

                    If _SalesConfig.isBatchItem(dr(Fld_ItemCode)) Then
                        oDoc.Lines.BatchNumbers.BatchNumber = dr(Fld_BatchNum)
                        'oDoc.Lines.BatchNumbers.ManufacturingDate = dr(Fld_MfrDate)
                        oDoc.Lines.BatchNumbers.Location = dr(Fld_LocCode)
                        'oDoc.Lines.BatchNumbers.ExpiryDate = dr(Fld_ExpireDate)
                        oDoc.Lines.BatchNumbers.Quantity = dr(Fld_Quantity)
                        oDoc.Lines.BatchNumbers.Add()
                    End If

                    PrevLineNum = dr(Fld_LineNum)
                Next

                If oDoc.Add <> 0 Then
                    ' Error 
                    Generate = False
                    If UpdateErrorStatus(_DocEntry, _SAPDIConn.Company.GetLastErrorCode, _SAPDIConn.Company.GetLastErrorDescription) = False Then

                    End If
                Else
                    ' Success
                    Generate = True
                    _newDocEntry = _SAPDIConn.Company.GetNewObjectKey
                    _SAPDIConn.Company.GetNewObjectCode(_newDocNum)
                    If UpdateSuccessStatus(_DocEntry, _newDocEntry, _newDocNum) = False Then

                    End If

                End If

            Else
                UpdateErrorStatus(_DocEntry, "-1", "Internal Error(No Row Lines)")
                Generate = False
            End If

        Catch ex As Exception
            Generate = False
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod.Name)
            ' Update Error Status by DocEntry
        End Try


    End Function

    Public Function UpdateSuccessStatus(ByVal _DocEntry As String, ByVal _TgtDocEntry As String, ByVal _TgtDocNum As String) As Boolean
        UpdateSuccessStatus = True
        Dim mSql As String

        Try
            mSql = String.Format(_sqlUpdate_Suc, Sales.TBL_ORDR, _TgtDocEntry, _TgtDocNum, _DocEntry)
            MyBase.ExecuteUpdate(mSql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateSuccessStatus = False
        End Try

    End Function

    Public Function UpdateErrorStatus(ByVal _DocEntry As String, ByVal _ErrCode As String, ByVal _ErrorDesc As String) As Boolean
        UpdateErrorStatus = True
        Dim mSql As String
        Try
            mSql = String.Format(_sqlUpdate_Err, Sales.TBL_ORDR, _ErrCode, _ErrorDesc, _DocEntry)
            MyBase.ExecuteUpdate(mSql)
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().Name)
            UpdateErrorStatus = False
        End Try

    End Function

End Class
