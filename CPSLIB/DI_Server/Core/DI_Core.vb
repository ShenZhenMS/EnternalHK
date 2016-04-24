Namespace DIServer
    Public Class Core
        Inherits DIServer.DI_Node

       

        Public Const xmlns As String = "http://www.sap.com/SBO/DIS"
        ''' <summary>
        ''' {0} Session ID
        ''' </summary>
        ''' <remarks></remarks>
        Public Const SessionXML As String = "<SessionID>{0}</SessionID>"
        ''' <summary>
        ''' {0} Command 
        ''' {1} Command Name
        ''' {2} XMLNS
        ''' </summary>
        ''' <remarks></remarks>
        '''
        Public Const SchemaSCommandXML As String = "<dis:{0} xmlns:dis=""{2}"" CommandID=""{1}"">"

        Public Const SchemaECommandXML As String = "</dis:{0}>"


        Public Const SCommandXML As String = "<dis:{0} xmlns:dis=""{2}"" CommandID=""{1}"">" & _
                                           "<BOM>" & _
                                            "<BO>"
        Public Const ECommandXML As String = "</BO></BOM></dis:{0}>"

        ''' <summary>
        ''' {0} Object Type Name
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Public Const SchemaObjectXML As String = "<Object>{0}</Object>"
        Public Const ObjectXML As String = "<AdmInfo>" & _
                                            "<Object>{0}</Object>" & _
                                            "</AdmInfo>"

        Public Const SBodyXML As String = "<env:Body>"
        Public Const EBodyXML As String = "</env:Body>"


        ''' <summary>
        ''' {0} Header Property: e.g. Session ID
        ''' {1} Body Content
        '''
        ''' </summary>
        ''' <remarks></remarks>
        ''' 
        Public Const RequestXML As String = "<?xml version=""1.0"" encoding=""UTF-16""?>" & _
            "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">" & _
            "{0}" & _
            "<env:Body>{1}</env:Body>" & _
            "</env:Envelope>"
        
        ''' <summary>
        ''' {0} xmlns
        ''' {1} Server
        ''' {2} CompanyDB
        ''' {3} DBType
        ''' {4} DBUserName
        ''' {5} DBPassword
        ''' {6} Username
        ''' {7} Password
        ''' {8} Language
        ''' {9} DBServerType
        ''' </summary>
        ''' <remarks></remarks>
        Public Const RequestLoginXML As String = "<dis:Login xmlns:dis=""{0}"">" & _
                                                 "<DatabaseServer>{1}</DatabaseServer>" & _
                                                 "<DatabaseName>{2}</DatabaseName>" & _
                                                 "<DatabaseType>{3}</DatabaseType>" & _
                                                 "<DatabaseUsername>{4}</DatabaseUsername>" & _
                                                 "<DatabasePassword>{5}</DatabasePassword>" & _
                                                 "<CompanyUsername>{6}</CompanyUsername>" & _
                                                 "<CompanyPassword>{7}</CompanyPassword>" & _
                                                 "<Language>{8}</Language>" & _
                                                 "<LicenseServer>{9}</LicenseServer>" & _
                                                 "</dis:Login>"

        Public Const RequestLogoutXML As String = "<dis:Logout xmlns:dis=""{0}"">" & _
                                                  "</dis:Logout>"


        Public Const RequestHeaderXML As String = "<env:Header>{0}</env:Header>"

        'Public Const DocumentLinesPrefix As String = "<Document_Lines>{0}</Document_Lines>"


        Public Const HeaderLevelPrefix As String = "<{0}><row>{1}</row></{0}>"
        Public Const NonHeaderLevelPrefix As String = "<{0}>{1}</{0}>"
        Public Const DocRowXML As String = "<row>{0}</row>"
        'Public Const DocHeaderXML As String = "<Documents><row>{0}</row></Documents>"

        Private _htCmd As Hashtable

      

      

        Public Sub New(_CommandName As String)
            MyBase.New(_CommandName)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)

        End Sub



#Region "Property"
        Public ReadOnly Property CmdString() As Hashtable
            Get
                Return _htCmd
            End Get
        End Property
#End Region

#Region "Operation"
        Public Function SessionString() As String
            Return String.Format(Core.SessionXML, SessionID)
        End Function
        Public Function GenerateBodyHeader() As String
            Dim _ret As String = String.Empty
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            If SessionID <> String.Empty Then
                Return String.Format(RequestHeaderXML, SessionString)
            End If

            Return _ret
        End Function
        Public Sub SetCommandString(ByVal strCmd As String)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            If _htCmd Is Nothing Then
                _htCmd = New Hashtable
            End If
            _htCmd.Add(_htCmd.Count + 1, strCmd)
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
        End Sub
        Public Overloads Function Execute() As CommandStatus

            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As CommandStatus
            _ret = MyBase.Execute(String.Format(Core.RequestXML, GenerateBodyHeader, GenerateCommand))
            _htCmd.Clear()
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function

        Private Function GenerateCommand() As String
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Start)
            Dim _ret As String = String.Empty
            If _htCmd Is Nothing = False Then
                For Each o As Object In _htCmd.Keys
                    _ret = _ret & _htCmd(o).ToString & vbCrLf
                Next
            Else
                _ret = String.Empty
            End If
            TimeSet.Log(System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.FullName & "." & System.Reflection.MethodBase.GetCurrentMethod.Name, TimeSet.Status.Finish)
            Return _ret
        End Function
#End Region

    End Class
End Namespace
