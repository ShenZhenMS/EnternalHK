Public Class DIServerTagConfig : Inherits Settings.File.Files

    Public Shared ConfigFile As String = "DIServerTag.ini"
    Public Shared HeaderCategory As String = "HeaderLevel"
    Public Shared LineCategory As String = "LineLevel"
    Public Shared BatchCategory As String = "BatchLevel"
    Public Shared SerialCategory As String = "SerialLevel"
    Public Shared DefaultHeaderTag As String = "Documents"
    Public Shared DefaultLineTag As String = "Document_Lines"
    Public Shared DefaultBatchTag As String = "BatchNumbers"
    Public Shared DefaultSerialTag As String = "SerialNumbers"


    Private _otherTag As Hashtable
    Public Property OtherTag() As Hashtable
        Get
            Return _otherTag
        End Get
        Set(ByVal value As Hashtable)
            _otherTag = value
        End Set
    End Property


    Private _objtype As SAPbobsCOM.BoObjectTypes


    Private _HeaderTag As String

    Private _LineTag As String

    Private _BatchTag As String

    Private _SerialTag As String
    Public Property SerialLevelTag() As String
        Get
            Return _SerialTag
        End Get
        Set(ByVal value As String)
            _SerialTag = value
        End Set
    End Property

    Public Property BatchLevelTag() As String
        Get
            Return _BatchTag
        End Get
        Set(ByVal value As String)
            _BatchTag = value
        End Set
    End Property

    Public Property LineLevelTag() As String
        Get
            Return _LineTag
        End Get
        Set(ByVal value As String)
            _LineTag = value
        End Set
    End Property

    Public Property HeaderTag() As String
        Get
            Return _HeaderTag
        End Get
        Set(ByVal value As String)
            _HeaderTag = value
        End Set
    End Property

    Public Property ObjectType() As SAPbobsCOM.BoObjectTypes
        Get
            Return _objtype
        End Get
        Set(ByVal value As SAPbobsCOM.BoObjectTypes)
            _objtype = value
        End Set
    End Property

    Public Sub New(ByVal _objtype As SAPbobsCOM.BoObjectTypes)
        MyBase.New(System.Environment.CurrentDirectory & "\" & ConfigFile)
        Me._objtype = _objtype
        _HeaderTag = getValue(HeaderCategory, _objtype.ToString)
        If _HeaderTag = String.Empty Then
            _HeaderTag = DefaultHeaderTag
        End If
        _LineTag = getValue(LineCategory, _objtype.ToString)
        If _LineTag = String.Empty Then
            _LineTag = DefaultLineTag
        End If
        _BatchTag = getValue(BatchCategory, _objtype.ToString)
        If _BatchTag = String.Empty Then
            _BatchTag = DefaultBatchTag
        End If
        _SerialTag = getValue(SerialCategory, _objtype.ToString)
        If _SerialTag = String.Empty Then
            _SerialTag = DefaultSerialTag
        End If
        _otherTag = MyBase.GetSectionValue(_objtype.ToString)


    End Sub

End Class
