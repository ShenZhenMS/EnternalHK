Public Class COASetting : Inherits CPSLIB.Data.Connection.SQLServerInfo

    Dim _Setting As Settings
    Dim _Debug As CPSLIB.CPSLIB.Debug
    Dim _CPSException As CPSLIB.CPSException
    Dim _COA As String


    Private _CostCode1Mandatory As Boolean
    Public Property CostCode1Mandatory() As Boolean
        Get
            Return _CostCode1Mandatory
        End Get
        Set(ByVal value As Boolean)
            _CostCode1Mandatory = value
        End Set
    End Property

    Private _CostCode5Mandatory As Boolean
    Public Property CostCode5Mandatory() As Boolean
        Get
            Return _CostCode5Mandatory
        End Get
        Set(ByVal value As Boolean)
            _CostCode5Mandatory = value
        End Set
    End Property

    Private _CostCode2Mandatory As Boolean
    Public Property CostCode2Mandatory() As Boolean
        Get
            Return _CostCode2Mandatory
        End Get
        Set(ByVal value As Boolean)
            _CostCode2Mandatory = value
        End Set
    End Property

    Private _CostCode3Mandatory As Boolean
    Public Property CostCode3Mandatory() As Boolean
        Get
            Return _CostCode3Mandatory
        End Get
        Set(ByVal value As Boolean)
            _CostCode3Mandatory = value
        End Set
    End Property

    Private _CostCode4Mandatory As Boolean
    Public Property CostCode4Mandatory() As Boolean
        Get
            Return _CostCode4Mandatory
        End Get
        Set(ByVal value As Boolean)
            _CostCode4Mandatory = value
        End Set
    End Property


    Public Sub New(ByVal _Setting As Settings, ByVal _COA As String)
        MyBase.New(_Setting.ServerName, _Setting.SQLUserName, _Setting.SQLPasswd, _Setting.Database)
        Me._Setting = _Setting
        _Debug = New CPSLIB.CPSLIB.Debug(Me.GetType.ToString)
        _CPSException = New CPSLIB.CPSException
        Me._COA = _COA
        CheckCOA()
    End Sub

    Private Sub CheckCOA()
        Dim _sql As String = "SELECT isNull(U_Dept,'N') as Dept, isNull(U_Brand,'N') as Brand, isNull(U_CounterCustomer,'N') as 'Counter', isNull(U_Location,'N') as Location, isNull(U_Team,'N') as team  FROM OACT WHERE ACCTCODE = '{0}'"

        Dim _dt As DataTable
        'U_Dept()
        'U_Brand()
        'U_CounterCustomer()
        'U_Location()
        'U_Team()
        _CostCode1Mandatory = False
        _CostCode2Mandatory = False
        _CostCode3Mandatory = False
        _CostCode4Mandatory = False
        _CostCode5Mandatory = False
        Try
            _dt = MyBase.ExecuteDatatable(String.Format(_sql, _COA.Replace("'", "''")))
            If _dt.Rows.Count > 0 Then
                _CostCode1Mandatory = (_dt.Rows(0)("Dept") = "Y")
                _CostCode2Mandatory = (_dt.Rows(0)("Brand") = "Y")
                _CostCode3Mandatory = (_dt.Rows(0)("Counter") = "Y")
                _CostCode4Mandatory = (_dt.Rows(0)("Location") = "Y")
                _CostCode5Mandatory = (_dt.Rows(0)("Team") = "Y")

            End If
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
    End Sub

    Public Function GetDepartmentCode(ByVal _ItemCode As String) As String
        Dim _sql As String = "select Ocrcode from OOCR where OcrCode = (select isNull(U_ProductType,'') from OITM where ItemCode = '{0}' ) and active = 'Y'"
        Dim _ret As String = ""
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

    Public Function GetBrand(ByVal _ItemCode As String) As String
        Dim _sql As String = "select Ocrcode from OOCR where OcrCode = (select isNull(U_Brand,'') from OITM where ItemCode = '{0}' ) and active = 'Y'"
        Dim _ret As String = ""
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function getCounter(ByVal _ItemCode As String) As String
        Dim _sql As String = "select Ocrcode from OOCR where OcrCode = (select isNull(U_ProductType,'') from OITM where ItemCode = '{0}' ) and active = 'Y'"
        Dim _ret As String = "HQ_CUST"
        'Try
        '    _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))
        'Catch ex As Exception
        '    _CPSException.ExecuteHandle(ex)
        'End Try
        Return _ret
    End Function
    Public Function getLocation(ByVal _ItemCode As String) As String
        Dim _sql As String = "SELECT ocrcode from OOCR where ocrcode = 'HK'"
        Dim _ret As String = ""
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function
    Public Function getTeam(ByVal _ItemCode As String) As String
        Dim _sql As String = "select Ocrcode from OOCR where OcrCode = (select isNull(U_ProductType,'') from OITM where ItemCode = '{0}' ) and active = 'Y'"
        Dim _ret As String = ""
        Try
            _ret = MyBase.ExecuteValue(String.Format(_sql, _ItemCode.Replace("'", "''")))
        Catch ex As Exception
            _CPSException.ExecuteHandle(ex)
        End Try
        Return _ret
    End Function

End Class
