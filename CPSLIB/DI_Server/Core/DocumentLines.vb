Namespace DIServer

    Public Class DocumentLines

        Public Const _ItemCode As String = "ItemCode"
        Public Const _Quantity As String = "Quantity"
        Public Const _Price As String = "Price"
        Public Const _WhsCode As String = "WarehouseCode"
        Public Const _LineNum As String = "LineNum"


        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _htLines As Hashtable
        Private _htBatch As Hashtable
        Private _htSerial As Hashtable

        Public Const FieldPrefix As String = "<{0}>{1}</{0}>"

        Public Sub New()
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException()
            _htLines = New Hashtable
            _htBatch = New Hashtable
            _htSerial = New Hashtable
        End Sub
#Region "Property"
        Public Sub setValue(ByVal strFieldName As String, ByVal value As Object)
            _htLines(strFieldName) = value
        End Sub
        Public Function getValue(ByVal strFieldName As String) As Object
            Return _htLines(strFieldName)

        End Function
#End Region
        Public Function GetBatches() As Hashtable
            Return _htBatch
        End Function

        Public Function GetSerial() As Hashtable
            Return _htSerial
        End Function

        Public Sub AddBatch(ByVal _Batch As BatchNumbers)
            _htBatch.Add(_htBatch.Count, _Batch)
        End Sub

        Public Sub AddSerial(ByVal _Serial As SerialNumbers)
            _htSerial.Add(_htBatch.Count, _Serial)
        End Sub

#Region "Operation"
        Public Sub Clear()
            _htLines.Clear()
        End Sub

        Public Function GenerateBatchLine() As String
            Dim _Cmd As String = String.Empty

            If Not _htBatch Is Nothing Then
                For Each o As Object In _htBatch.Keys
                    _Cmd = _Cmd & CType(_htBatch(o), BatchNumbers).GenerateLine
                Next
            End If
            Return _Cmd
        End Function

        Public Function GenerateSerialLine() As String
            Dim _Cmd As String = String.Empty

            If Not _htSerial Is Nothing Then
                For Each o As Object In _htSerial.Keys
                    _Cmd = _Cmd & CType(_htSerial(o), BatchNumbers).GenerateLine
                Next
            End If
            Return _Cmd
        End Function

        Public Function GenerateLine() As String
            Dim _Cmd As String = String.Empty

            If Not _htLines Is Nothing Then
                For Each o As Object In _htLines.Keys
                    _Cmd = _Cmd & String.Format(FieldPrefix, o.ToString, _htLines(o.ToString))
                Next
            End If

            Return String.Format(DIServer.DI_Object.DocRowXML, _Cmd)
        End Function
#End Region

#Region "Property"
        Public Property Price() As Decimal
            Get
                Return Data.Validation.isNumeric(getValue(_Price))
            End Get
            Set(ByVal value As Decimal)
                setValue(_Price, value)
            End Set
        End Property

        Public Property Quantity() As Decimal
            Get
                Return Data.Validation.isNumeric(getValue(_Quantity))
            End Get
            Set(ByVal value As Decimal)
                setValue(_Quantity, value)
            End Set
        End Property

        Public Property ItemCode() As String
            Get
                Return Data.Validation.isNumeric(getValue(_ItemCode))
            End Get
            Set(ByVal value As String)
                setValue(_ItemCode, value)
            End Set
        End Property

        Public Property WhsCode As String
            Get
                Return getValue(_WhsCode)
            End Get
            Set(ByVal value As String)
                setValue(_WhsCode, value)
            End Set
        End Property
#End Region

    End Class
End Namespace
