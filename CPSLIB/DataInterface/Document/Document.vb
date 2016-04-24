Namespace DataInterface.Document
    Public Class Document
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _isError As Boolean
        Private _Message As String
        Private _DICompany As DataInterface.Company.DICompany
        Private _DocStatus As DocumentStatus
        Private _htUDF As Hashtable
        Private _htLines As Hashtable

        Private _DocEntry As Integer
        Private _DocNum As Integer
        Private _CardCode As String
        Private _DocDate As DateTime
        Private _DocDueDate As DateTime
        Private _Remarks As String
        Private _PostDate As DateTime
        Private _Currency As String
        Private _Series As Integer
        Private _SeriesString As String
        Private _NumAtCard As String
        Private _ShipToCode As String


        Private _OwnerID As String
        Public Property OwnerID() As String
            Get
                Return _OwnerID
            End Get
            Set(ByVal value As String)
                _OwnerID = value
            End Set
        End Property


        Public Property ShipToCode() As String
            Get
                Return _ShipToCode
            End Get
            Set(ByVal value As String)
                _ShipToCode = value
            End Set
        End Property


        Public ReadOnly Property DICompany() As DataInterface.Company.DICompany
            Get
                Return _DICompany
            End Get
        End Property
        Public ReadOnly Property isError() As Boolean
            Get
                Return _isError
            End Get
        End Property

        Public ReadOnly Property Message() As String
            Get
                Return _Message
            End Get
        End Property

        Public Enum DocumentType
            Item = 1
            Service = 2
            Other = 3
        End Enum

        Public Enum DocumentStatus
            Add = 1
            Update = 2
        End Enum

        Public Enum PostStatus
            Ready = 0
            Success = 1
            Fail = 2
        End Enum

       
        Private _DocType As DocumentType
        Public Property DocType() As DocumentType
            Get
                Return _DocType
            End Get
            Set(ByVal value As DocumentType)
                _DocType = value
            End Set
        End Property

        Public Property NumAtCard() As String
            Get
                Return _NumAtCard
            End Get
            Set(ByVal value As String)
                _NumAtCard = value
            End Set
        End Property

        Public Property SeriesString() As String
            Get
                Return _SeriesString
            End Get
            Set(ByVal value As String)
                _SeriesString = value
            End Set
        End Property

        Public Property Series() As Integer
            Get
                Return _Series
            End Get
            Set(ByVal value As Integer)
                _Series = value
            End Set
        End Property

       
        Public Property Currency() As String
            Get
                Return _Currency
            End Get
            Set(ByVal value As String)
                _Currency = value
            End Set
        End Property

        Public Property PostDate() As DateTime
            Get
                Return _PostDate
            End Get
            Set(ByVal value As DateTime)
                _PostDate = value
            End Set
        End Property

        Public Property Remarks() As String
            Get
                Return _Remarks
            End Get
            Set(ByVal value As String)
                _Remarks = value
            End Set
        End Property

        Public Property DocDueDate() As DateTime
            Get
                Return _DocDueDate
            End Get
            Set(ByVal value As DateTime)
                _DocDueDate = value
            End Set
        End Property

        Public Property DocDate() As DateTime
            Get
                Return _DocDate
            End Get
            Set(ByVal value As DateTime)
                _DocDate = value
            End Set
        End Property

        Public Property CardCode() As String
            Get
                Return _CardCode
            End Get
            Set(ByVal value As String)

                _CardCode = value
            End Set
        End Property

        Public Property DocNum() As Integer
            Get
                Return _DocNum
            End Get
            Set(ByVal value As Integer)
                _DocNum = value
            End Set
        End Property

        Public Property DocEntry() As Integer
            Get
                Return _DocEntry
            End Get
            Set(ByVal value As Integer)
                _DocEntry = value
            End Set
        End Property


        Private _Doc As SAPbobsCOM.Documents

        Public ReadOnly Property Document() As SAPbobsCOM.Documents
            Get
                Return _Doc
            End Get

        End Property


        Public Sub New(ByVal _DICompany As DataInterface.Company.DICompany, ByVal _ObjType As SAPbobsCOM.BoObjectTypes, Optional ByVal _TargetType As SAPbobsCOM.BoObjectTypes = Nothing)
            Me.New(_DICompany, 0, _ObjType, _TargetType)
        End Sub

        Public Sub New(ByVal _DICompany As DataInterface.Company.DICompany, ByVal _DocEntry As Integer, ByVal _ObjType As SAPbobsCOM.BoObjectTypes, Optional ByVal _TargetType As SAPbobsCOM.BoObjectTypes = Nothing)
            Me._DICompany = _DICompany
            _Debug = New CPSLIB.Debug(Me.GetType.ToString())
            _CPSException = New CPSException
            Try
                _Debug.Write("New Instance")
                _Debug.Write("Objtype: " & _ObjType)
                If _DICompany Is Nothing Then
                    _Debug.Write("DICompany is nothing")
                Else
                    _Debug.Write("DI Connected: " & _DICompany.Connected)
                End If
                If _DICompany.Connected = False Then
                    _DICompany.Connect()
                End If
                _Doc = _DICompany.Company.GetBusinessObject(_ObjType)
                _Debug.Write("_Doc is nothing: " & (_Doc Is Nothing))

                _DocStatus = DocumentStatus.Add
                If _DocEntry > 0 Then
                    If _Doc.GetByKey(_DocEntry) Then
                        _DocStatus = DocumentStatus.Update
                    End If
                End If
                If _ObjType = SAPbobsCOM.BoObjectTypes.oDrafts Then
                    _Doc.DocObjectCode = _TargetType
                End If
            Catch ex As Exception
                _isError = True
                _Message = "Exception: " & ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        ' User Defined Field
        Public Sub SetUDF(ByVal name As String, ByVal value As Object)
            If _htUDF Is Nothing Then
                _htUDF = New Hashtable
            End If
            _htUDF(name) = value
        End Sub

        Public ReadOnly Property UserField() As Hashtable
            Get
                Return _htUDF
            End Get
        End Property

        Public Function DocumentLineCount() As Integer
            If _htLines Is Nothing Then
                Return 0
            Else
                Return _htLines.Count
            End If
        End Function

        ' Set Document Rows
        Public Sub SetDocumentLine(ByVal _DL As Document_Line)
            If _htLines Is Nothing Then
                _htLines = New Hashtable
            End If
            _htLines(_htLines.Count + 1) = _DL
        End Sub
        ' Post Document
        Public Function Post() As Boolean
            Dim _ret As Boolean = True
            Dim _DocLine As Document_Line
            Dim _RowCnt As Integer
            Dim _retNum As Integer
            Try

                ' Assign Header Property
                _Doc.CardCode = _CardCode
                _Doc.DocDate = _DocDate
                If Not _DocDueDate = Nothing Then
                    _Doc.DocDueDate = _DocDueDate
                End If
                If _Currency <> String.Empty Then
                    _Doc.DocCurrency = _Currency
                End If
                Select Case _DocType
                    Case DocumentType.Item
                        _Doc.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items
                    Case DocumentType.Service
                        _Doc.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Service
                    Case Else
                        _Doc.DocType = SAPbobsCOM.BoDocumentTypes.dDocument_Items

                End Select
                If _NumAtCard <> String.Empty Then
                    _Doc.NumAtCard = _NumAtCard
                End If

                If Not _PostDate = Nothing Then
                    _Doc.TaxDate = _PostDate
                End If
                If Not _Series = Nothing Then
                    _Doc.Series = _Series
                End If
                If _SeriesString <> String.Empty Then
                    _Doc.SeriesString = _SeriesString
                End If
                If _Remarks <> String.Empty Then
                    _Doc.Comments = _Remarks
                End If

                If _ShipToCode <> String.Empty Then
                    _Doc.ShipToCode = _ShipToCode
                End If

                If _OwnerID <> String.Empty Then
                    _Doc.DocumentsOwner = _OwnerID
                End If
                ' User Defined Field
                If _htUDF Is Nothing = False Then
                    For Each o As Object In _htUDF.Keys
                        _Doc.UserFields.Fields.Item(o.ToString).Value = _htUDF(o)
                    Next
                End If

                WriteDataTable()
                ' Row Level
                _RowCnt = _Doc.Lines.Count


                For i As Integer = 1 To _htLines.Count
                    _DocLine = _htLines(i)
                    If _DocStatus = DocumentStatus.Add Then
                        If i > 1 Then
                            _Doc.Lines.Add()
                        End If
                    Else
                        _Doc.Lines.Add()
                    End If
                    With _Doc.Lines
                        If _DocLine.ItemCode <> String.Empty Then
                            .ItemCode = _DocLine.ItemCode
                        End If
                        If Not _DocLine.Quantity = Nothing Then
                            .Quantity = _DocLine.Quantity
                        End If
                        If Not _DocLine.UnitPrice = Nothing Then
                            .UnitPrice = _DocLine.UnitPrice
                        End If
                        If Not _DocLine.Discount = Nothing Then
                            .DiscountPercent = _DocLine.Discount
                        End If
                        If _DocLine.Warehouse <> String.Empty Then
                            .WarehouseCode = _DocLine.Warehouse
                        End If
                        If _DocLine.Account <> String.Empty Then
                            .AccountCode = _DocLine.Account
                        End If
                        If _DocLine.Project <> String.Empty Then
                            .ProjectCode = _DocLine.Project
                        End If
                        If _DocLine.ProfitCode1 <> String.Empty Then
                            .CostingCode = _DocLine.ProfitCode1
                        End If
                        If _DocLine.ProfitCode2 <> String.Empty Then
                            .CostingCode2 = _DocLine.ProfitCode2
                        End If
                        If _DocLine.ProfitCode3 <> String.Empty Then
                            .CostingCode3 = _DocLine.ProfitCode3
                        End If
                        If _DocLine.ProfitCode4 <> String.Empty Then
                            .CostingCode4 = _DocLine.ProfitCode4
                        End If
                        If _DocLine.ProfitCode5 <> String.Empty Then
                            .CostingCode5 = _DocLine.ProfitCode5
                        End If
                        If _DocLine.UserFields Is Nothing = False Then
                            For Each o As Object In _DocLine.UserFields.Keys
                                .UserFields.Fields.Item(o.ToString).Value = _DocLine.UserFields(o)
                            Next
                        End If

                    End With
                Next
                ' Post Document
                Select Case _DocStatus
                    Case DocumentStatus.Add
                        _retNum = _Doc.Add
                    Case DocumentStatus.Update
                        _retNum = _Doc.Update
                End Select
                If _retNum <> 0 Then
                    _ret = False
                    _isError = True
                    _Message = _DICompany.Company.GetLastErrorCode & " : " & _DICompany.Company.GetLastErrorDescription
                Else
                    _DocEntry = _DICompany.Company.GetNewObjectKey
                    _ret = True
                    _isError = False
                    _Message = String.Empty
                End If
            Catch ex As Exception
                _ret = False
                _isError = True
                _Message = "Exception: " & ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function

        Public Sub WriteDataTable()

            Dim _DL As Document_Line
            Dim _dtDocLine As DataTable
            Dim _dr As DataRow
            Try


                If _htLines.Count > 0 Then
                    For Each o As Object In _htLines.Keys
                        _DL = CType(_htLines(o), Document_Line)
                        Exit For
                    Next
                    _dtDocLine = New DataTable
                    _dtDocLine.Columns.Add("ItemCode")
                    _dtDocLine.Columns.Add("Quantity")
                    _dtDocLine.Columns.Add("UnitPrice")
                    _dtDocLine.Columns.Add("Discount")
                    _dtDocLine.Columns.Add("Warehouse")
                    _dtDocLine.Columns.Add("Account")
                    _dtDocLine.Columns.Add("Project")
                    _dtDocLine.Columns.Add("CostingCode1")
                    _dtDocLine.Columns.Add("CostingCode2")
                    _dtDocLine.Columns.Add("CostingCode3")
                    _dtDocLine.Columns.Add("CostingCode4")
                    _dtDocLine.Columns.Add("CostingCode5")
                    

                    
                    For Each _UDF As Object In _DL.UserFields.Keys
                        _dtDocLine.Columns.Add(_UDF)
                    Next

                    ' Assign Data 
                    For Each o As Object In _htLines.Keys
                        _dr = _dtDocLine.NewRow
                        _DL = _htLines(o)

                        _dr("ItemCode") = _DL.ItemCode
                        _dr("Quantity") = _DL.Quantity
                        _dr("UnitPrice") = _DL.UnitPrice
                        _dr("Discount") = _DL.Discount
                        _dr("Warehouse") = _DL.Warehouse
                        _dr("Account") = _DL.Account
                        _dr("Project") = _DL.Project
                        _dr("CostingCode1") = _DL.ProfitCode1
                        _dr("CostingCode2") = _DL.ProfitCode2
                        _dr("CostingCode3") = _DL.ProfitCode3
                        _dr("CostingCode4") = _DL.ProfitCode4
                        _dr("CostingCode5") = _DL.ProfitCode5

                        
                        For Each _UDF As Object In _DL.UserFields.Keys
                            _dr.Item(_UDF) = Convert.ToString(_DL.UserFields(_UDF))
                        Next

                        _dtDocLine.Rows.Add(_dr)
                    Next

                    _Debug.WriteTable(_dtDocLine, "Generated Document")

                End If
            Catch ex As Exception
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub
    End Class
End Namespace
