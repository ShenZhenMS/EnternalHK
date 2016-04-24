Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System.Xml
Imports System.Data.SqlClient

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class WebService
    Inherits System.Web.Services.WebService

    Dim _DIServerConn As DIServer
    Public DB_Server = "COMPASS88"
    Public DB_Name = "LCHK800"
    Public DB_Type = "dst_MSSQL2008"
    Public DB_UserName = "sa"
    Public DB_UserPassword = "compass2008"
    Public B1_UserName = ""
    Public B1_UserPassword = ""
    Public B1_Language = "ln_English"
    Public B1_LicenseServer = ""
    Public Midd_DB_Name = ""
    Public SessionID As String = ""


#Region "SAP"
    Public Function ConnectDIServer() As String
        Dim _Setting As New Settings
        Dim _DiConn As New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
        If _DiConn.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Success Then
            Return _DiConn.SessionID
            _DiConn.Logout()
        Else
            Return _DiConn.CmdMessage
        End If
    End Function

    Public Function DisconnectDIServer() As String
        If _DIServerConn Is Nothing Then
            Return "NO SAP Connection exists."
        Else
            If _DIServerConn.isConnected = True Then
                If _DIServerConn.Logout() Then
                    Return "Success"
                Else
                    Return "Fail: " & _DIServerConn.Message
                End If
            Else
                Return "NO SAP Connection Exists"
            End If
        End If
    End Function

    <WebMethod(Description:="SAP LogIn")> _
    Public Function SAP_LogIn() As String
        Return ConnectDIServer()
    End Function

    <WebMethod(Description:="SAP LogOut")> _
    Public Function SAP_LogOut() As String
        Return DisconnectDIServer()
    End Function
#End Region

#Region "Purchase"

    <WebMethod(Description:="Import into CPS_TBL_OPOR")> _
    Public Function CPS_PURCHASE_IMPORT_ADD_GRPO(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSPurchase As PurchaseForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _PurValid As PurchaseValidation
        Dim _WSGRPO As DIServer_GRPO
        Dim _ret As String
        Dim _diConn As CPSLIB.DIServer.DIServerConnection
        _XMLResult = New CPSLIB.XML.XMLDocument("xml")

        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSPurchase = New PurchaseForWMS(_Setting, _SAPDIConn)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.PURCHASE.ToString, True)
            _dt = _XML.DataTable

            _diConn = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
            If Not _dt Is Nothing Then
                If _diConn.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Success Then

                    _PurValid = New PurchaseValidation(_Setting, _dt)
                    _dt = _PurValid.AdjustedTable
                    If _WMSPurchase.ToPurchaseTable(_dt) Then
                        _WSGRPO = New DIServer_GRPO(_Setting, _diConn)
                        If _WSGRPO.Generate(_dt(0)(PurchaseForWMS.Fld_ReceiveEntry)) = False Then
                            _Debug.Write("Fail")
                            _XMLResult.WriteElement(Message.XML_ERROR_TAG, _WSGRPO.CmdMessage)

                        Else
                            _Debug.Write("Success")
                            _XMLResult.WriteElement(Message.XML_INFORMATION_TAG, "Success")
                        End If

                    End If
                    _diConn.Logout()
                Else

                    _XMLResult.WriteElement(Message.XML_ERROR_TAG, _diConn.CmdMessage)
                End If
            Else
                _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Internal Error")
            End If

        Catch ex As Exception

            _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Failed: " & ex.Message)
        End Try

        Return _XMLResult

    End Function

    <WebMethod(Description:="Update Batch Information")> _
    Public Function CPS_INVENTORY_BATCHINFO_IMPORT(ByVal _Itemcode As String, ByVal _BatchNum As String,
                                                   ByVal _WhsCode As String, ByVal _MfrDate As String,
                                                   ByVal _ExpDate As String) As XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As MasterForSAP
        Dim _WMSMaster As MasterForWMS
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Try
            _SAPMaster = New MasterForSAP(_Setting, Nothing)
            _WMSMaster = New MasterForWMS(_Setting)
            _WMSMaster.ToBatchTable(_Itemcode, _BatchNum, _WhsCode, _MfrDate, _ExpDate)
            If _SAPMaster.UpdateBatchInfo(_Itemcode, _BatchNum, _WhsCode, _MfrDate, _ExpDate) Then
                _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Success")
            Else
                _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Fail")
            End If
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XMLResult.WriteElement(Message.XML_ERROR_TAG, ex.Message)
        End Try
        Return _XMLResult
    End Function

    <WebMethod(Description:="Get GRPO Result")> _
    Public Function CPS_PURCHASE_EXPORT_RECEVESTATUS(ByVal _ReceiveEntry As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _Purchase As PurchaseForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _Purchase = New PurchaseForWMS(_Setting, Nothing)
            _dt = _Purchase.PurchaseResult(_ReceiveEntry)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PURCHASE.ToString & "_RESULT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try

    End Function

    <WebMethod(Description:="Get All Open Production Order FG")> _
    Public Function CPS_PRODUCTION_EXPORT_ALL_OpenProductionList_FG() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPProduction As ProductionForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPProduction = New ProductionForSAP(_Setting, _SAPDIConn)
            _dt = _SAPProduction.OpenListFG
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PRODUCTION.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get All Open Production Order Component")> _
    Public Function CPS_PRODUCTION_EXPORT_ALL_OpenProductionList_Component() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPProduction As ProductionForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPProduction = New ProductionForSAP(_Setting, _SAPDIConn)
            _dt = _SAPProduction.OpenListChild
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PRODUCTION.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Open Production Order FG")> _
    Public Function CPS_PRODUCTION_EXPORT_OpenProductionList_FG(ByVal _FromDate As String,
                                                                ByVal _ToDate As String,
                                                                ByVal _DocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPProduction As ProductionForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPProduction = New ProductionForSAP(_Setting, _SAPDIConn)
            _dt = _SAPProduction.OpenListFG(_FromDate, _ToDate, _DocNum)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PRODUCTION.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Open Production Order Componet")> _
    Public Function CPS_PRODUCTION_EXPORT_OpenProductionList_Component(ByVal _FromDate As String, ByVal _ToDate As String, ByVal _DocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPProduction As ProductionForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPProduction = New ProductionForSAP(_Setting, _SAPDIConn)
            _dt = _SAPProduction.OpenListChild(_FromDate, _ToDate, _DocNum)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PRODUCTION.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function


    <WebMethod(Description:="Get Stock Take Status")> _
    Public Function CPS_STOCKTAKE_EXPORT_StockTakeStatus(ByVal RefNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPStockTake As StockTakeForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _ret As String = String.Empty
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPStockTake = New StockTakeForSAP(_Setting, _SAPDIConn)
            _ret = _SAPStockTake.GetResult(RefNum)

            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, _ret)


            _Debug.Write(_XML, Settings.WMSModule.PURCHASE.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_ERROR_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function


    <WebMethod(Description:="Get All Open Purchase Order")> _
    Public Function CPS_PURCHASE_EXPORT_ALL_OpenPurchaseList() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPPurchase As PurchaseForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPPurchase = New PurchaseForSAP(_Setting, _SAPDIConn)
            _dt = _SAPPurchase.OpenPurchaseList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PURCHASE.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Return Open Purchase Order")> _
    Public Function CPS_PURCHASE_EXPORT_OpenPurchaseList(ByVal pCardCode As String,
                                                         ByVal pFromDate As String,
                                                         ByVal pToDate As String,
                                                         ByVal pDocNum As String,
                                                         ByVal _ASNNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPPurchase As PurchaseForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPPurchase = New PurchaseForSAP(_Setting, _SAPDIConn)
            _dt = _SAPPurchase.OpenPurchaseList(pCardCode, pFromDate, pToDate, pDocNum, _ASNNum)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.PURCHASE.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function
#End Region

#Region "Sales"
    <WebMethod(Description:="Get Open Sales Order List")> _
    Public Function CPS_SALES_EXPORT_ALL_OPENSALESLIST() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.OpenSalesList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message)
            Return _XML.GetDocument
        End Try

    End Function


    <WebMethod(Description:="Get Open Pick List for Pick List Report")> _
    Public Function CPS_SALES_EXPORT_OPENPickListReport() As System.Xml.XmlDocument

        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.PickListReport
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_PICKLISTREPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_PICKLISTREPORT", True)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)

            Return _XML.GetDocument
        End Try

    End Function


    <WebMethod(Description:="Get Open Sales Order with Criteria")> _
    Public Function CPS_SALES_EXPORT_OPENSALESLIST(ByVal CardCode As String,
                                                   ByVal FromDocDate As String,
                                                   ByVal ToDocDate As String,
                                                   ByVal DocNum As String) As System.Xml.XmlDocument

        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.OpenSalesList(CardCode, FromDocDate, ToDocDate, DocNum)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Address List By Sales Order")> _
    Public Function CPS_SALES_EXPORT_ADDRESSLIST(ByVal DocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.AddressList(DocNum)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Open Sales Order with Criteria")> _
    Public Function CPS_SALES_EXPORT_SALESLIST(ByVal CardCode As String,
                                               ByVal FromDocDate As String,
                                               ByVal ToDocDate As String,
                                               ByVal DocNum As String) As System.Xml.XmlDocument

        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.SalesList(CardCode, FromDocDate, ToDocDate, DocNum)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Validate Batch Shelf Life By Customer and Item")> _
    Public Function CPS_SALES_EXPORT_ValidateBatch(ByVal CardCode As String,
                                                   ByVal ItemCode As String,
                                                   ByVal BatchNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Try
            _SAPSales = New SalesForSAP(_Setting, Nothing)
            If _SAPSales.ValidateBatchNumber(CardCode, ItemCode, BatchNum) Then
                _XMLResult = New CPSLIB.XML.XMLDocument("xml")
                _XMLResult.WriteElement("Message", "True")
            Else
                _XMLResult = New CPSLIB.XML.XMLDocument("xml")
                _XMLResult.WriteElement("Message", "False")
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XMLResult.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XMLResult.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Delivery Result")> _
    Public Function CPS_SALES_EXPORT_DELIVERYSTATUS(ByVal _ReceiveEntry As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _Sales As SalesForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _Sales = New SalesForWMS(_Setting, Nothing)
            _dt = _Sales.SalesResult(_ReceiveEntry)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_RESULT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to genreate target sales document in SAP ")> _
    Public Function CPS_SALES_IMPORT_PICKLIST(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSSales As SalesForWMS
        '20130718
        Dim _XML As CPSLIB.XML.XMLDocument

        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _SlsValid As SalesValidation
        Dim _DO As DeliveryOperation
        Dim _DiServerConnection As CPSLIB.DIServer.DIServerConnection
        Try
            _WMSSales = New SalesForWMS(_Setting, Nothing)
            '20130718:
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.SALES.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then
                _SlsValid = New SalesValidation(_Setting, _dt)
                _dt = _SlsValid.AdjustedTable
                'Jerry Add Check the Login State If faile give up insert data to middle db
                _DiServerConnection = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName, _Setting.LicServer, _Setting.Database, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Username, _Setting.Password, CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
                If _DiServerConnection.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Success Then
                    If _WMSSales.ToSalesTable(_dt) Then
                        If _dt.Rows.Count > 0 Then
                            _DO = New DeliveryOperation(_Setting)
                            If _DO.Start(_DiServerConnection) Then
                                _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                                _XMLResult.WriteElement(Message.XML_INFORMATION_TAG, "Success")
                            Else
                                _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                                _XMLResult.WriteElement(Message.XML_ERROR_TAG, _DO.Message)
                            End If
                        End If
                    Else
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _WMSSales.Message)
                    End If
                    If Not String.IsNullOrEmpty(_DiServerConnection.SessionID) Then
                        _DiServerConnection.Logout()
                    End If
                Else
                    Throw New Exception(_DiServerConnection.CmdMessage)
                End If
            End If
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XMLResult.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XMLResult.GetDocument
        Finally
            If Not String.IsNullOrEmpty(_DiServerConnection.SessionID) Then
                _DiServerConnection.Logout()
            End If
        End Try
        Return _XMLResult.GetDocument
    End Function
#End Region

#Region "InventoryTransaction"
    <WebMethod(Description:="Get Open Inventory Transfer List")> _
    Public Function CPS_INVENTORYTRANSACTION_EXPORT_ALL_OPENLIST() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPInvtran As InventoryTransactionForSAP

        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPInvtran = New InventoryTransactionForSAP(_Setting, Nothing)
            _dt = _SAPInvtran.OpenInventoryTransferList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.STOCKTRN.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Reason Code For Stock Adjustment (Parameter: DocType (GI - Goods Issue/ GR - Goods Receipt / TR - Standalone Inventory Transfer")> _
    Public Function CPS_MASTER_EXPORT_REASON(ByVal _DocType As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPExport As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPExport = New SAPMasterExport(_Setting)
            Select Case _DocType
                Case "GI"
                    _dt = _SAPExport.ReasonCode("60")
                Case "GR"
                    _dt = _SAPExport.ReasonCode("59")
                Case "TR"
                    _dt = _SAPExport.ReasonCode("67")
            End Select
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.REASON.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Return Open Inventory Transfer based on criteria")> _
    Public Function CPS_INVENTORYTRANSACTION_EXPORT_OPENLIST(ByVal pFromDate As String,
                                                             ByVal pToDate As String,
                                                             ByVal pDocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPInvtran As InventoryTransactionForSAP

        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPInvtran = New InventoryTransactionForSAP(_Setting, Nothing)
            _dt = _SAPInvtran.OpenInventoryTransferList(pFromDate, pToDate, pDocNum)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.STOCKTRN.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Inventory Transfer Status")> _
    Public Function CPS_INVENTORYTRANSACTION_EXPORT_INVENTORYTRANSSTATUS(ByVal _ReceiveEntry As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _InvTran As InventoryTransactionForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _InvTran = New InventoryTransactionForWMS(_Setting, Nothing)
            _dt = _InvTran.InventoryTransResult(_ReceiveEntry)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.STOCKTRN.ToString & "_RESULT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to Update Bin Location in SAP")> _
    Public Function CPS_INVENTORYTRANSACTION_BINLOCATION(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSBinLocation As BinLocationForWMS
        Dim _SAPBinLocation As BinLocationForSAP
        Dim _StsBinLocation As BinLocationUpdateStatus
        Dim _Status As Boolean
        Dim _Rollback As Boolean

        Dim _SAPDIConn As SAPDIConnections
        Dim _SAPWMSConn As WMSSQLConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _BinLocValid As BinLocationValidation

        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSBinLocation = New BinLocationForWMS(_Setting)
            _SAPBinLocation = New BinLocationForSAP(_Setting, _SAPDIConn)
            _StsBinLocation = New BinLocationUpdateStatus(_Setting)

            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.BINLOCATION.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then

                _BinLocValid = New BinLocationValidation(_Setting, _dt)
                _dt = _BinLocValid.AdjustedTable
                _Status = _WMSBinLocation.ToBinLocationTransferTable(_dt)
                _SAPBinLocation.Test()
                _Rollback = _SAPBinLocation.UpdateBinLocation(_dt)
                For Each dr As DataRow In _dt.Rows
                    _StsBinLocation.UpdateSuccessStatus(dr(0), dr(2), dr(1), dr(3))
                Next
                _XMLResult = New CPSLIB.XML.XMLDocument("xml")
                _XMLResult.WriteElement("Message", "Success")
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XMLResult.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to genreate Inventory Transfer in SAP")> _
    Public Function CPS_INVENTORYTRANSACTION_IMPORT_ADDINVENTORYTRANSFER(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _CPSException As New CPSLIB.CPSException
        Dim _Setting As Settings = New Settings
        Dim _WMSInventoryTransaction As InventoryTransactionForWMS

        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _ITRValid As InventoryTransactionValidation
        Dim WSStockTransfer As DIServer_StockTransfer
        Dim _DiConn As CPSLIB.DIServer.DIServerConnection
        Dim _docEntry As String

        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _Debug.Write("Check Point 1")
            _WMSInventoryTransaction = New InventoryTransactionForWMS(_Setting, _SAPDIConn)
            _Debug.Write("Check Point 2")
            _Debug.Write(doc.OuterXml)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write("Check Point 3")
            _Debug.Write(_XML, Settings.WMSModule.STOCKTRN.ToString, True)
            _dt = _XML.DataTable

            _DiConn = New CPSLIB.DIServer.DIServerConnection(_Setting.ServerName,
                                                             _Setting.LicServer,
                                                             _Setting.Database,
                                                             _Setting.SQLUserName,
                                                             _Setting.SQLPasswd,
                                                             _Setting.Username,
                                                             _Setting.Password,
                                                             CPSLIB.DataInterface.Company.DICompany.DataBaseType.MSSQL2008)
            If _DiConn.Login = CPSLIB.DIServer.DI_Node.CommandStatus.Fail Then
                _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Internal Error")
            Else

                If Not _dt Is Nothing Then
                    _Debug.Write("Ready to create inventory transfer")
                    _ITRValid = New InventoryTransactionValidation(_Setting, _dt)
                    _dt = _ITRValid.AdjustedTable
                    _WMSInventoryTransaction.ToInventoryTransferTable(_dt)
                    If _dt.Rows.Count > 0 Then
                        WSStockTransfer = New DIServer_StockTransfer(_Setting, _DiConn)
                        If WSStockTransfer.Create(_dt(0)(InventoryTransactionForWMS.Fld_ReceiveEntry)) Then
                            _Debug.Write("Success")
                            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                            _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Success")
                        Else
                            _Debug.Write("Fail")
                            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                            _XMLResult.WriteElement(Message.XML_ERROR_TAG, WSStockTransfer.CmdMessage)
                        End If
                    Else
                        _Debug.Write("Internal Error")
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Internal Error")
                    End If
                End If
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to genreate Goods Issue in SAP")> _
    Public Function CPS_INVENTORY_IMPORT_GOODISSUE(ByVal doc As XmlDocument) As System.Xml.XmlDocument

        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSInventoryInOut As InventoryInoutForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _ITRValid As InventoryInOutValidation
        Dim _StockInOut As InventoryInoutOperation
        Dim _docEntry As String
        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSInventoryInOut = New InventoryInoutForWMS(_Setting, _SAPDIConn, InventoryInoutForWMS._DocumentType.GI)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.STOCKIO.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then
                _ITRValid = New InventoryInOutValidation(_Setting, _dt)
                _dt = _ITRValid.AdjustedTable
                _WMSInventoryInOut.ToWMSTable(_dt)
                If _dt.Rows.Count > 0 Then
                    _StockInOut = New InventoryInoutOperation(_Setting, InventoryInoutForWMS._DocumentType.GI)
                    If _StockInOut.Start(_dt) Then
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_INFORMATION_TAG, "Success")
                    Else
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _StockInOut.Message)
                    End If
                End If
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to genreate Goods Receive in SAP")> _
    Public Function CPS_INVENTORY_IMPORT_GOODRECEIVE(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSInventoryInOut As InventoryInoutForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _ITRValid As InventoryInOutValidation
        Dim _StockInOut As InventoryInoutOperation
        Dim _docEntry As String
        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSInventoryInOut = New InventoryInoutForWMS(_Setting, _SAPDIConn, InventoryInoutForWMS._DocumentType.GR)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.STOCKIO.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then
                _ITRValid = New InventoryInOutValidation(_Setting, _dt)
                _dt = _ITRValid.AdjustedTable
                _WMSInventoryInOut.ToWMSTable(_dt)
                If _dt.Rows.Count > 0 Then
                    _StockInOut = New InventoryInoutOperation(_Setting, InventoryInoutForWMS._DocumentType.GR)
                    If _StockInOut.Start(_dt) Then
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_INFORMATION_TAG, "Success")
                    Else
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _StockInOut.Message)
                    End If
                End If
                Return _XMLResult.GetDocument
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to Import Stock Take to SAP")> _
    Public Function CPS_STOCKTAKE_IMPORT(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings
        Dim _WMSStockTake As StockTakeForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _StockTakeValid As StockTakeValidation
        Dim _docEntry As String
        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSStockTake = New StockTakeForWMS(_Setting, _SAPDIConn)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.STOCKTAKE.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then
                _StockTakeValid = New StockTakeValidation(_Setting, _dt)
                _dt = _StockTakeValid.AdjustedTable
                _WMSStockTake.ToLogTable(_dt)
                If _dt.Rows.Count > 0 Then
                    If _WMSStockTake.isError Then
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _WMSStockTake.Message)
                    Else
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_INFORMATION_TAG, "Success")
                    End If
                Else
                    _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                    _XMLResult.WriteElement(Message.XML_ERROR_TAG, "No Data Found")
                End If
                Return _XMLResult.GetDocument
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function
#End Region

#Region "Master"
    <WebMethod(Description:="Item Barcode List")> _
    Public Function CPS_MASTER_EXPORT_BARCODE() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.BarCodeInfo
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.EXPBARCODE.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)
                _Debug.Write(_XML, Settings.WMSModule.EXPBARCODE & "_EXPORT", True)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Batch Information By Item By Warehouse By Location")> _
    Public Function CPS_MASTER_EXPORT_BATCHBYITEMLOCATION(ByVal ItemCode As String,
                                                          ByVal WhsCode As String,
                                                          ByVal LocCode As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPExp As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPExp = New SAPMasterExport(_Setting)
            _dt = _SAPExp.BatchInformation(ItemCode, WhsCode, LocCode)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.STOCKTRN & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG, False)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Warehouse List")> _
    Public Function CPS_MASTER_EXPORT_WAREHOUSE() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.WarehouseInfo
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.EXPWAREHOUSE.ToString, True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)
                _Debug.Write(_XML, Settings.WMSModule.EXPWAREHOUSE & "_EXPORT", True)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="BP List")> _
    Public Function CPS_MASTER_EXPORT_BPList() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.BPList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.BP.ToString, True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument("Error: " & ex.Message, False)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Item List")> _
    Public Function CPS_MASTER_EXPORT_ITEM() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.ItemInfo()
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.EXPITEM & "_EXPORT", True)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Item List")> _
    Public Function CPS_MASTER_EXPORT_ITEM_BY_RANGE(ByVal pFromDate As String, ByVal pToDate As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.ItemInfo_Range(pFromDate, pToDate)

            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.EXPITEM & "_EXPORT", True)
                Return _XML.GetDocument
            End If
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try

    End Function

    <WebMethod(Description:="Item Barcode List")> _
    Public Function CPS_MASTER_EXPORT_BARCODE_BY_RANGE(ByVal pFromDate As String, ByVal pToDate As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.BarCodeInfo_Range(pFromDate, pToDate)

            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.EXPBARCODE & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try

    End Function

    <WebMethod(Description:="Item Barcode List")> _
    Public Function CPS_MASTER_EXPORT_BARCODE_BY_CODE(ByVal pItemCode As String, ByVal pBarCode As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPMaster As SAPMasterExport
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPMaster = New SAPMasterExport(_Setting)
            _dt = _SAPMaster.BarCodeInfo_Code(pItemCode, pBarCode)

            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.SALES.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.EXPBARCODE & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get All Item Groups")> _
    Public Function CPS_MASTER_EXPORT_ITEMGROUP() As System.Xml.XmlDocument
        Dim DataSet_XElement As XElement = New XElement("DataSet")
        Dim Message_XElement As XElement = New XElement("Message")
        Dim Result_XElement As XElement = New XElement("Result")
        Dim ErrMessage_XElement As XElement = New XElement("ErrorMessage")
        Dim ds_Result As DataSet = New DataSet("DataSet")
        Dim flag As DataTable = New DataTable("Row")

        Try
            flag = ITEMGROUP_MASTER()
            ds_Result.Tables.Add(flag)

        Catch ex As Exception
            ErrMessage_XElement.Add(ex.Message)
        End Try

        '----------Start Create DataSet for XML----------

        Dim xmlDocument As System.Xml.XmlDocument

        xmlDocument = New System.Xml.XmlDocument()
        xmlDocument.LoadXml(ds_Result.GetXml)

        Return xmlDocument
    End Function

#End Region
#Region "Return"
    <WebMethod(Description:="Export A/P Invoice List for Purchase Return")> _
    Public Function CPS_PURCHASERETURN_EXPORT_ALL() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPPurchaseReturn As PurchaseCreditMemoForSAP
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPPurchaseReturn = New PurchaseCreditMemoForSAP(_Setting, Nothing)
            _dt = _SAPPurchaseReturn.OpenList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.APCREDITMEMO.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Export A/P Invoice List for Purchase Return")> _
    Public Function CPS_PURCHASERETURN_EXPORT(ByVal CardCode As String,
                                              ByVal DocNum As String,
                                              ByVal _FromDate As String,
                                              ByVal _ToDate As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPPurchaseReturn As PurchaseCreditMemoForSAP
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _Debug.Write("Check Point 1")
            _SAPPurchaseReturn = New PurchaseCreditMemoForSAP(_Setting, Nothing)
            _dt = _SAPPurchaseReturn.OpenList(DocNum, CardCode, _FromDate, _ToDate)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.APCREDITMEMO.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Read XML to genreate Standalone Purchase Return Draft in SAP")> _
    Public Function CPS_PURCHASERETURN_IMPORT(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Dim _Setting As Settings = New Settings

        Dim _WMSPR As PurchaseCreditMemoForWMS

        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _PRValid As PurchaseCreditMemoValidation
        Dim _PR As ReturnOperation
        Dim _docEntry As String

        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSPR = New PurchaseCreditMemoForWMS(_Setting, Nothing, PurchaseCreditMemoForWMS._DocumentType.PR)
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.APCREDITMEMO.ToString, True)
            _dt = _XML.DataTable

            If Not _dt Is Nothing Then
                _PRValid = New PurchaseCreditMemoValidation(_Setting, _dt)
                _dt = _PRValid.AdjustedTable
                _WMSPR.ToWMSTable(_dt)

                If _dt.Rows.Count > 0 Then
                    _PR = New ReturnOperation(_Setting, SalesCreditMemoForWMS._DocumentType.PR)
                    If _PR.Start(_dt) Then
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Success")
                    Else
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _PR.Message)
                    End If
                End If
            End If
            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XMLResult.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Export A/R Invoice List for Sales Return")> _
    Public Function CPS_SALESRETURN_EXPORT_ALL() As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSalesReturn As SalesCreditMemoForSAP
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try

            _SAPSalesReturn = New SalesCreditMemoForSAP(_Setting, Nothing)
            _dt = _SAPSalesReturn.OpenList
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.ARCREDITMEMO.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception

            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Export A/R Invoice List for Sales Return")> _
    Public Function CPS_SALESRETURN_EXPORT(ByVal CardCode As String, ByVal DocNum As String, ByVal _FromDate As String, ByVal _ToDate As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSalesReturn As SalesCreditMemoForSAP
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPSalesReturn = New SalesCreditMemoForSAP(_Setting, Nothing)
            _dt = _SAPSalesReturn.OpenList(DocNum, CardCode, _FromDate, _ToDate)
            _dt.TableName = "Row"
            _XML = New CPSLIB.XML.XMLDocument(_dt)
            _Debug.Write(_XML, Settings.WMSModule.ARCREDITMEMO.ToString & "_EXPORT", True)
            Return _XML.GetDocument
        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get Goods Issue Status")> _
    Public Function CPS_INVENTORY_EXPORT_GI_STATUS(ByVal _WMSDocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _IOforSAP As New InventoryInoutForSAP(_Setting, Nothing)
        Dim _XML As CPSLIB.XML.XMLDocument
        _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
        _XML.WriteElement(Message.XML_INFORMATION_TAG, _IOforSAP.DocumentStatus(_WMSDocNum))
        Return _XML.GetDocument
    End Function

    <WebMethod(Description:="Get Return Status")> _
    Public Function CPS_INVENTORY_EXPORT_RETURN_STATUS(ByVal _WMSDocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _IOforSAP As New InventoryInoutForSAP(_Setting, Nothing)
        Dim _XML As CPSLIB.XML.XMLDocument
        _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
        _XML.WriteElement(Message.XML_INFORMATION_TAG, _IOforSAP.DocumentStatus(_WMSDocNum))
        Return _XML.GetDocument
    End Function

    <WebMethod(Description:="Get Goods Receive Status")> _
    Public Function CPS_INVENTORY_EXPORT_GR_STATUS(ByVal _WMSDocNum As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _IOforSAP As New InventoryInoutForSAP(_Setting, Nothing)
        Dim _XML As CPSLIB.XML.XMLDocument
        _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
        _XML.WriteElement(Message.XML_INFORMATION_TAG, _IOforSAP.DocumentStatus(_WMSDocNum))
        Return _XML.GetDocument
    End Function

    <WebMethod(Description:="Read XML to genreate Standalone Sales Return Draft in SAP")> _
    Public Function CPS_SALESRETURN_IMPORT(ByVal doc As XmlDocument) As System.Xml.XmlDocument
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        _Debug.Write("Start")
        Dim _Setting As Settings = New Settings
        Dim _WMSSR As SalesCreditMemoForWMS
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _XMLResult As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _SRValid As SalesCreditMemoValidation
        Dim _SR As ReturnOperation
        Dim _docEntry As String
        Try
            _XMLResult = New CPSLIB.XML.XMLDocument("xml")
            _SAPDIConn = New SAPDIConnections(_Setting)
            _WMSSR = New SalesCreditMemoForWMS(_Setting, Nothing, SalesCreditMemoForWMS._DocumentType.SR)
            _Debug.Write(doc.OuterXml, "XML")
            _XML = New CPSLIB.XML.XMLDocument(doc.OuterXml, True)
            _Debug.Write(_XML, Settings.WMSModule.ARCREDITMEMO.ToString, True)
            _dt = _XML.DataTable
            If Not _dt Is Nothing Then
                _SRValid = New SalesCreditMemoValidation(_Setting, _dt)
                _dt = _SRValid.AdjustedTable
                _WMSSR.ToWMSTable(_dt)
                If _dt.Rows.Count > 0 Then
                    _SR = New ReturnOperation(_Setting, SalesCreditMemoForWMS._DocumentType.SR)
                    _Debug.Write("Check Point 10")
                    If _SR.Start(_dt) Then
                        _Debug.Write("Check Point 11")
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, "Success")
                    Else
                        _Debug.Write("Check Point 12")
                        _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                        _XMLResult.WriteElement(Message.XML_ERROR_TAG, _SR.Message)
                    End If
                End If
            End If

            Return _XMLResult.GetDocument
        Catch ex As Exception
            _XMLResult = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XMLResult.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

#End Region
    Public Function ToXmlDocument(ByVal xDoc As XDocument) As XmlDocument

        Dim xmlDocument As XmlDocument = New XmlDocument()
        Dim xReader As XmlReader = xDoc.CreateReader

        xmlDocument.Load(xReader)
        Return xmlDocument
    End Function


    <WebMethod(Description:="Test Method ONLY")> _
    Public Function LoginandLogout() As String
        Dim _Setting As Settings = New Settings
        SAP_LogIn()
        SAP_LogOut()
        Return ""
    End Function


    <WebMethod(Description:="SQL to XML")> _
    Public Function CPS_SQL_TO_XML(ByVal mSQL As String) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSQLConn As SAPSQLConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _debug As New CPSLIB.CPSLIB.Debug("MK_Debug")

        Try
            _SAPSQLConn = New SAPSQLConnections(_Setting)


            _dt = _SAPSQLConn.ExecuteDatatable(mSQL)

            _XML = New CPSLIB.XML.XMLDocument(_dt)

            _debug.Write(_XML, "MK_Test", True)

            Return _XML.GetDocument

        Catch ex As Exception
            _debug.Write(ex.Message)

        End Try

    End Function

    <WebMethod(Description:="SQL to XML")> _
    Public Function CPS_DT_TO_XML(ByVal dt As DataTable) As System.Xml.XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _debug As New CPSLIB.CPSLIB.Debug("MK_Debug")

        Try


            _XML = New CPSLIB.XML.XMLDocument(dt)

            _debug.Write(_XML, "MK_Test", True)

            Return _XML.GetDocument

        Catch ex As Exception
            _debug.Write(ex.Message)

        End Try

    End Function

    <WebMethod(Description:="XML Compression Test")> _
    Public Function CPS_INT_PROGRAMCONFIGEXIST() As System.Xml.XmlDocument
        Dim _XML As CPSLIB.XML.XMLDocument
        _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
        _XML.WriteElement("EnvPath", System.Environment.CurrentDirectory)
        Return _XML
    End Function

#Region "Store_Procedures"

    Public Sub SP_IMPORT_OPOR(ByVal pDocEntry As String, _
                              ByVal pLineNum As String, _
                              ByVal pDocNum As String, _
                              ByVal pDocDate As String, _
                              ByVal pDocDueDate As String, _
                              ByVal pCardCode As String, _
                              ByVal pCardName As String, _
                              ByVal pNumAtCard As String, _
                              ByVal pItemCode As String, _
                              ByVal pItemName As String, _
                              ByVal pQuantity As String, _
                              ByVal pUOM As String, _
                              ByVal pWhsCode As String, _
                              ByVal pWhsName As String, _
                              ByVal pBatchNumber As String, _
                              ByVal pExpireDate As String, _
                              ByVal pMfrDate As String, _
                              ByVal pBarCode As String, _
                              ByVal pLocCode As String)


        Dim SQL_Connection As String = "Data Source=" & DB_Server & ";" & _
                        "Initial Catalog=" & Midd_DB_Name & ";" & _
                        "User ID=" & DB_UserName & ";" & _
                        "Password=" & DB_UserPassword & ";"

        Dim cn As SqlConnection
        cn = New SqlConnection
        cn.ConnectionString = SQL_Connection
        cn.Open()

        Dim Para_DocEntry As SqlClient.SqlParameter
        Dim Para_LineNum As SqlClient.SqlParameter
        Dim Para_DocNum As SqlClient.SqlParameter
        Dim Para_DocDate As SqlClient.SqlParameter
        Dim Para_DocDueDate As SqlClient.SqlParameter
        Dim Para_CardCode As SqlClient.SqlParameter
        Dim Para_CardName As SqlClient.SqlParameter
        Dim Para_NumAtCard As SqlClient.SqlParameter
        Dim Para_ItemCode As SqlClient.SqlParameter
        Dim Para_ItemName As SqlClient.SqlParameter
        Dim Para_Quantity As SqlClient.SqlParameter
        Dim Para_UOM As SqlClient.SqlParameter
        Dim Para_WhsCode As SqlClient.SqlParameter
        Dim Para_WhsName As SqlClient.SqlParameter
        Dim Para_BatchNumber As SqlClient.SqlParameter
        Dim Para_ExpireDate As SqlClient.SqlParameter
        Dim Para_MfrDate As SqlClient.SqlParameter
        Dim Para_BarCode As SqlClient.SqlParameter
        Dim Para_LocCode As SqlClient.SqlParameter

        Dim cmd As New SqlCommand
        cmd.CommandText = "dbo.CPS_SP_IMPORT_OPOR"
        cmd.CommandType = CommandType.StoredProcedure
        cmd.Connection = cn
        cmd.Parameters.Clear()

        Para_DocEntry = New SqlClient.SqlParameter("@DocEntry", SqlDbType.Int)
        Para_LineNum = New SqlClient.SqlParameter("@LineNum", SqlDbType.Int)
        Para_DocNum = New SqlClient.SqlParameter("@DocNum", SqlDbType.Int)
        Para_DocDate = New SqlClient.SqlParameter("@DocDate", SqlDbType.Date)
        Para_DocDueDate = New SqlClient.SqlParameter("@DocDueDate", SqlDbType.Date)
        Para_CardCode = New SqlClient.SqlParameter("@CardCode", SqlDbType.NVarChar, 15)
        Para_CardName = New SqlClient.SqlParameter("@CardName", SqlDbType.NVarChar, 100)
        Para_NumAtCard = New SqlClient.SqlParameter("@NumAtCard", SqlDbType.NVarChar, 100)
        Para_ItemCode = New SqlClient.SqlParameter("@ItemCode", SqlDbType.NVarChar, 20)
        Para_ItemName = New SqlClient.SqlParameter("@ItemName", SqlDbType.NVarChar, 100)
        Para_Quantity = New SqlClient.SqlParameter("@Quantity", SqlDbType.Decimal)
        Para_UOM = New SqlClient.SqlParameter("@UOM", SqlDbType.NVarChar, 20)
        Para_WhsCode = New SqlClient.SqlParameter("@WhsCode", SqlDbType.NVarChar, 8)
        Para_WhsName = New SqlClient.SqlParameter("@WhsName", SqlDbType.NVarChar, 20)
        Para_BatchNumber = New SqlClient.SqlParameter("@BatchNumber", SqlDbType.NVarChar, 100)
        Para_ExpireDate = New SqlClient.SqlParameter("@ExpireDate", SqlDbType.DateTime)
        Para_MfrDate = New SqlClient.SqlParameter("@MfrDate", SqlDbType.DateTime)
        Para_BarCode = New SqlClient.SqlParameter("@BarCode", SqlDbType.NVarChar, 20)
        Para_LocCode = New SqlClient.SqlParameter("@LocCode", SqlDbType.NVarChar, 20)

        Para_DocEntry.Value = pDocEntry
        cmd.Parameters.Add(Para_DocEntry)
        Para_LineNum.Value = pLineNum
        cmd.Parameters.Add(Para_LineNum)
        Para_DocNum.Value = pDocNum
        cmd.Parameters.Add(Para_DocNum)
        Para_DocDate.Value = pDocDate
        cmd.Parameters.Add(Para_DocDate)
        Para_DocDueDate.Value = pDocDueDate
        cmd.Parameters.Add(Para_DocDueDate)
        Para_CardCode.Value = pCardCode
        cmd.Parameters.Add(Para_CardCode)
        Para_CardName.Value = pCardName
        cmd.Parameters.Add(Para_CardName)
        Para_NumAtCard.Value = pNumAtCard
        cmd.Parameters.Add(Para_NumAtCard)
        Para_ItemCode.Value = pItemCode
        cmd.Parameters.Add(Para_ItemCode)
        Para_ItemName.Value = pItemName
        cmd.Parameters.Add(Para_ItemName)
        Para_Quantity.Value = pQuantity
        cmd.Parameters.Add(Para_Quantity)
        Para_UOM.Value = pUOM
        cmd.Parameters.Add(Para_UOM)
        Para_WhsCode.Value = pWhsCode
        cmd.Parameters.Add(Para_WhsCode)
        Para_WhsName.Value = pWhsName
        cmd.Parameters.Add(Para_WhsName)
        Para_BatchNumber.Value = pBatchNumber
        cmd.Parameters.Add(Para_BatchNumber)
        Para_ExpireDate.Value = pExpireDate
        cmd.Parameters.Add(Para_ExpireDate)
        Para_MfrDate.Value = pMfrDate
        cmd.Parameters.Add(Para_MfrDate)
        Para_BarCode.Value = pBarCode
        cmd.Parameters.Add(Para_BarCode)
        Para_LocCode.Value = pLocCode
        cmd.Parameters.Add(Para_LocCode)

        cmd.ExecuteNonQuery()

        Try
            cmd.ExecuteReader()
        Catch ex As Exception
            Throw New Exception("SQL Error: SP_IMPORT_OPOR - " & ex.Message)
        Finally
            cn.Close()
            cn.Dispose()
            cmd.Dispose()
        End Try

    End Sub

#End Region

#Region "Functions"

    Public Function ITEM_BARCODE() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_BARCODE] Order By ItemCode asc")

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function ITEMGROUP_MASTER() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_OITB]")

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function WAREHOUSE() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_WAREHOUSE]")

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function ITEM_MASTER() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_OITM] Order By ItemCode asc")

        dt.TableName = "Row"
        Return dt
    End Function


    Public Function Get_CPS_VIEW_ORDR() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_ORDR] Order By Docentry asc")

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function Get_CPS_VIEW_ORDR(ByVal CardCode As String, ByVal FromDocDate As String, ByVal ToDocDate As Date, ByVal DocNum As String) As DataTable
        initInformation()
        Dim dt As DataTable
        Dim _sql As String = "select * from [dbo].[CPS_VIEW_ORDR] where 1 = 1"
        If CardCode <> String.Empty Then
            _sql = _sql & String.Format(" AND CARDCODE = '{0}'", CardCode)
        End If
        If FromDocDate <> String.Empty Then
            _sql = _sql & String.Format(" AND FromDocDate >= '{0}'", FromDocDate)
        End If
        If ToDocDate <> String.Empty Then
            _sql = _sql & String.Format(" AND ToDocDate <= '{0}'", ToDocDate)
        End If
        If DocNum <> String.Empty Then
            _sql = _sql & String.Format(" AND DocNum = '{0}'", DocNum)
        End If
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        _sql)

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function Get_CPS_VIEW_OPOR() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                           "select * from [dbo].[CPS_VIEW_OPOR]")
        dt.TableName = "Row"
        Return dt

    End Function

    Public Function Get_CPS_VIEW_ORDR_BYDOCNUM(ByVal DocNum As String) As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        String.Format("select * from [dbo].[CPS_VIEW_ORDR] where DocNum = {0}", DocNum))

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function Get_Open_PurchaseOrder(ByVal pVendorCode As String, ByVal pFromDate As String, ByVal pToDate As String, ByVal pDocNum As String)
        initInformation()

        Dim dt As DataTable
        Dim _sql As String = "select * from [dbo].[CPS_VIEW_OPOR] Where 1 = 1"


        If pVendorCode <> String.Empty Then
            _sql = _sql & String.Format(" AND CardCode = '{0}'", pVendorCode.Replace("'", "''"))

        End If
        If pFromDate <> String.Empty Then
            _sql = _sql & String.Format(" AND DocDueDate >= '{0}'", pFromDate)

        End If

        If pToDate <> String.Empty Then
            _sql = _sql & String.Format(" AND DocDueDate <= '{0}'", pToDate)
        End If

        If pDocNum <> String.Empty Then
            _sql = _sql & String.Format(" AND DocNum = '{0}'", pDocNum.Replace("'", "''"))
        End If



        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                           _sql)
        dt.TableName = "Row"
        Return dt

    End Function

    Public Function Get_CPS_VIEW_OWTR() As DataTable
        initInformation()
        Dim dt As DataTable
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        "select * from [dbo].[CPS_VIEW_OWTR] Order By DocEntry asc")

        dt.TableName = "Row"
        Return dt
    End Function

    Public Function Get_CPS_VIEW_OWTR(ByVal FromDocDate As String, ByVal ToDocDate As Date, ByVal DocNum As String) As DataTable
        initInformation()
        Dim dt As DataTable
        Dim _sql As String = "select * from [dbo].[CPS_VIEW_OWTR] where 1 = 1"
        
        If FromDocDate <> String.Empty Then
            _sql = _sql & String.Format(" AND FromDocDate >= '{0}'", FromDocDate)
        End If
        If ToDocDate <> String.Empty Then
            _sql = _sql & String.Format(" AND ToDocDate <= '{0}'", ToDocDate)
        End If
        If DocNum <> String.Empty Then
            _sql = _sql & String.Format(" AND DocNum = '{0}'", DocNum)
        End If
        dt = SQL_DataTable(DB_Server, _
                           DB_Name, _
                           DB_UserName, _
                           DB_UserPassword, _
                        _sql)

        dt.TableName = "Row"
        Return dt
    End Function

#End Region

#Region "SQL Functions"

    Public Function SQL_DataTable(ByVal pDataSource As String, _
                                  ByVal pInitialCatalog As String, _
                                  ByVal pUserID As String, _
                                  ByVal pPassword As String, _
                                  ByVal pSQL As String) As System.Data.DataTable
        Dim cn As SqlConnection
        Dim da As SqlDataAdapter
        cn = New SqlConnection
        Dim dt As DataTable = New DataTable
        Dim SQL_Connection As String = "Data Source=" & pDataSource & ";" & _
                        "Initial Catalog=" & pInitialCatalog & ";" & _
                        "User ID=" & pUserID & ";" & _
                        "Password=" & pPassword & ";"
        cn.ConnectionString = SQL_Connection
        cn.Open()

        Try

            da = New SqlDataAdapter(pSQL, cn)
            da.Fill(dt)
        Catch ex As Exception
            Throw New Exception("SQL Exception Catch : " & pSQL & " - " & ex.Message)
        Finally
            cn.Close()
            cn.Dispose()
        End Try
        Return dt

    End Function

#End Region

    Public Sub initInformation()
        DB_Server = System.Configuration.ConfigurationManager.AppSettings("DB_Server").ToString
        DB_Name = System.Configuration.ConfigurationManager.AppSettings("DB_Name").ToString
        DB_UserName = System.Configuration.ConfigurationManager.AppSettings("DB_UserName").ToString
        DB_UserPassword = System.Configuration.ConfigurationManager.AppSettings("DB_UserPassword").ToString
        B1_UserName = System.Configuration.ConfigurationManager.AppSettings("B1_UserName").ToString
        B1_UserPassword = System.Configuration.ConfigurationManager.AppSettings("B1_UserPassword").ToString
        B1_LicenseServer = System.Configuration.ConfigurationManager.AppSettings("B1_LicenseServer").ToString
        Midd_DB_Name = System.Configuration.ConfigurationManager.AppSettings("Midd_DB_Name").ToString
    End Sub

    <WebMethod(Description:="Get All POS STOCK OUT")> _
    Public Function GET_POS_STOCKOUT(ByVal docDate As String) As XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.PosStockOut(docDate)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function

    <WebMethod(Description:="Get All POS SALES RETURN")> _
    Public Function GET_POS_SALES_RETURN(ByVal docDate As String) As XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.PosSalesReturn(docDate)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function



    <WebMethod(Description:="Get All POS SHOP TO SHOP")> _
    Public Function GET_POS_SHOP_TO_SHOP(ByVal docDate As String) As XmlDocument
        Dim _Setting As Settings = New Settings
        Dim _SAPSales As SalesForSAP
        Dim _SAPDIConn As SAPDIConnections
        Dim _XML As CPSLIB.XML.XMLDocument
        Dim _dt As DataTable
        Dim _Debug As New CPSLIB.CPSLIB.Debug(System.Reflection.MethodBase.GetCurrentMethod.Name)
        Try
            _SAPDIConn = New SAPDIConnections(_Setting)
            _SAPSales = New SalesForSAP(_Setting, _SAPDIConn)
            _dt = _SAPSales.PosShopToShop(docDate)
            If _dt Is Nothing = False Then
                _dt.TableName = Message.XML_TABLE_NAME
                _XML = New CPSLIB.XML.XMLDocument(_dt)
                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            Else
                _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
                _XML.WriteElement(Message.XML_ERROR_TAG, Message.ERROR_DATA_NOT_FOUND)

                _Debug.Write(_XML, Settings.WMSModule.POSTRAN.ToString & "_EXPORT", True)
                Return _XML.GetDocument
            End If

        Catch ex As Exception
            _XML = New CPSLIB.XML.XMLDocument(Message.XML_DEFAULT_TAG)
            _XML.WriteElement(Message.XML_ERROR_TAG, ex.Message)
            Return _XML.GetDocument
        End Try
    End Function
End Class