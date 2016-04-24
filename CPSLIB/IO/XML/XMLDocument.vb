Namespace XML
    Public Class XMLDocument : Inherits System.Xml.XmlDocument
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _File As IO.Ascii.AsciiFile
        Private _StrCmd As String
        Private _dt As DataTable
        Private _ds As DataSet
        Private rootElement As System.Xml.XmlElement


        Public Sub New(ByVal _File As IO.Ascii.AsciiFile, Optional ByVal _toDataTable As Boolean = False)
            MyBase.New()
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            Me._File = _File
            Try
                MyBase.Load(_File.Information.FullName)
                If _toDataTable Then
                    _dt = Me.toDataTable
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub

        Public Function GetDocument() As XMLDocument
            Return Me
        End Function

        Public Sub New(ByVal _rootElement As String)
            MyBase.New()
            rootElement = MyBase.CreateElement(_rootElement)
            MyBase.AppendChild(rootElement)
        End Sub

        Public Sub New(ByVal _strCmd As String, ByVal _toDataTable As Boolean)
            MyBase.New()
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            Me._StrCmd = _strCmd

            Try
                MyBase.LoadXml(_strCmd)

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
            If _toDataTable Then
                _dt = Me.toDataTable
            End If

        End Sub

        Public Sub New(ByVal _ds As DataSet)
            MyBase.new()
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            Me._ds = _ds
            Me._dt = _ds.Tables(0)
            Try
                MyBase.LoadXml(_ds.GetXml)
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        Public Sub New(ByVal _dt As DataTable)
            MyBase.New()
            _CPSException = New CPSException
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            'Me._StrCmd = _StrCmd
            Me._dt = _dt
            Me._ds = _dt.DataSet

            Try
                MyBase.LoadXml(_dt.DataSet.GetXml)

            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try
        End Sub

        Public Sub WriteElement(ByVal _ElementName As String, ByVal Value As String)
            Dim newNode As System.Xml.XmlNode
            If rootElement Is Nothing Then
                rootElement = MyBase.DocumentElement

            End If
            newNode = rootElement.AppendChild(MyBase.CreateElement(_ElementName))
            newNode.InnerText = Value
            rootElement.AppendChild(newNode)

        End Sub
#Region "Search/Get/Assign"
        ' Read Node List in all element
        Public Function FindNodeList(ByVal strName As String) As ArrayList
            Return FindNodeList(MyBase.DocumentElement, strName, String.Empty)
        End Function
        ' Read Single Node in all element
        Public Function FindNode(ByVal strName As String) As System.Xml.XmlNode
            Dim al As ArrayList = FindNodeList(MyBase.DocumentElement, strName, String.Empty)
            If al.Count > 0 Then
                Return CType(al(0), System.Xml.XmlNode)
            Else
                Return Nothing
            End If
        End Function
        ' Read Node List inside parent node
        Public Function FindNodeList(ByVal strName As String, ByVal strParent As String) As ArrayList
            Return FindNodeList(MyBase.DocumentElement, strName, strParent)
        End Function
        ' Read Single Node inside parent node
        Public Function FindNode(ByVal strName As String, ByVal strParent As String) As System.Xml.XmlNode
            Dim _al As ArrayList = FindNodeList(MyBase.DocumentElement, strName, strParent)
            If _al.Count > 0 Then
                Return _al(0)
            Else
                Return Nothing
            End If
        End Function
        ' Assign Value 
        Public Sub SetInnerText(ByVal strName As String, ByVal strValue As Object)
            Dim _Node As System.Xml.XmlNode
            Try
                _Node = FindNode(strName)
                If Not _Node Is Nothing Then
                    _Node.InnerText = strValue
                End If
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex)
            End Try

        End Sub
        Private Function FindNodeList(ByVal _XmlElement As XML.XMLElement, ByVal strName As String, ByVal strParent As String) As ArrayList
            Dim _alNode As New ArrayList
            Dim t_alNode As ArrayList
            Dim _startFind As Boolean = False
            If strParent = String.Empty Then
                _startFind = True
            End If
            For Each o As System.Xml.XmlNode In _XmlElement.ChildNodes
                If o.Name = strName Then
                    If _startFind Then
                        _alNode.Add(o)
                    End If

                Else
                    If o.HasChildNodes Then
                        If strParent <> String.Empty Then
                            If o.Name = strParent Then
                                _startFind = True
                            Else
                                _startFind = False
                            End If
                        End If

                        t_alNode = FindNodeList(o, strName, strParent, _startFind)
                        For Each _o As Object In t_alNode.ToArray
                            _alNode.Add(_o)
                        Next

                    End If
                End If

            Next
            Return _alNode
        End Function
        Private Function FindNodeList(ByVal _XMLNode As System.Xml.XmlNode, ByVal strName As String, ByVal strParent As String, ByRef _startFind As Boolean) As ArrayList
            Dim _alNode As New ArrayList
            Dim t_alNode As ArrayList
            For Each o As System.Xml.XmlNode In _XMLNode.ChildNodes
                If o.Name = strName Then
                    If _startFind Then
                        _alNode.Add(o)
                    End If

                Else
                    If o.HasChildNodes Then
                        If strParent <> String.Empty Then
                            If o.Name = strParent Then
                                _startFind = True
                            Else
                                _startFind = False
                            End If
                        End If
                        t_alNode = FindNodeList(o, strName, strParent, _startFind)
                        For Each _o As Object In t_alNode.ToArray
                            _alNode.Add(_o)
                        Next
                    End If
                End If
            Next
            Return _alNode
        End Function

        
#End Region
#Region "Datatable"
        Private Function toDataTable() As DataTable





            If _ds Is Nothing Then
                _ds = New DataSet

            End If
            _ds.ReadXml(New System.IO.StringReader(MyBase.OuterXml), XmlReadMode.Auto)
            Return _ds.Tables(0)
        End Function

        Public Function DataTable() As DataTable
            Return Me._dt
        End Function

#End Region

        Public Sub Export(ByVal _Path As String, ByVal _file As String)

            Dim _di As System.IO.DirectoryInfo
            Try
                _di = New System.IO.DirectoryInfo(_Path)
                If _di.Exists = False Then
                    _di.Create()
                End If
                
                MyBase.Save(_Path & "\" & _file)
            Catch ex As Exception
                _CPSException.ExecuteHandle(ex, System.Reflection.MethodBase.GetCurrentMethod.Name)
            End Try
        End Sub
    End Class
End Namespace
