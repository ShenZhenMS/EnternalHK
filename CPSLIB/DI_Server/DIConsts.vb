Imports SAPbobsCOM
Namespace DIServer
    Public Class DIConst

        Public Shared ReadOnly Property LoginCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "<dis:Login xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "<DatabaseServer>{0}</DatabaseServer>"
                _Cmd = _Cmd & "<DatabaseName>{1}</DatabaseName>"
                _Cmd = _Cmd & "<DatabaseType>{2}</DatabaseType>"
                _Cmd = _Cmd & "<DatabaseUsername>{3}</DatabaseUsername>"
                _Cmd = _Cmd & "<DatabasePassword>{4}</DatabasePassword>"
                _Cmd = _Cmd & "<CompanyUsername>{5}</CompanyUsername>"
                _Cmd = _Cmd & "<CompanyPassword>{6}</CompanyPassword>"
                _Cmd = _Cmd & "<Language>{7}</Language>"
                _Cmd = _Cmd & "<LicenseServer>{8}</LicenseServer>"
                _Cmd = _Cmd & "</dis:Login></env:Body></env:Envelope>"

                Return _Cmd
            End Get
        End Property


        Public Shared ReadOnly Property LogoutCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID></env:Header><env:Body>"
                _Cmd = _Cmd & "<dis:Logout xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "</dis:Logout></env:Body></env:Envelope>"


                Return _Cmd
            End Get
        End Property

        Public Shared ReadOnly Property AddObjectCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID>"
                _Cmd = _Cmd & "</env:Header>"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "dis:AddObject xmlns:dis=""http://www.sap.com/SBO/DIS"" CommandID=""{1}"">"
                _Cmd = _Cmd & "<BOM>"
                _Cmd = _Cmd & "<BO>"
                _Cmd = _Cmd & "<AdmInfo>"
                _Cmd = _Cmd & "<Object>{2}</Object>"
                _Cmd = _Cmd & "</AdmInfo>"
                _Cmd = _Cmd & "{3}"
                _Cmd = _Cmd & "</BO>"
                _Cmd = _Cmd & "</BOM>"
                _Cmd = _Cmd & "</dis:AddObject>"
                _Cmd = _Cmd & "</env:Body>"
                _Cmd = _Cmd & "</env:Envelope>"

                Return _Cmd
            End Get
        End Property
        Public Shared ReadOnly Property UpdateObjectCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID>"
                _Cmd = _Cmd & "</env:Header>"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "<dis:UpdateObject xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "<BOM>"
                _Cmd = _Cmd & "<BO>"
                _Cmd = _Cmd & "<AdmInfo>"
                _Cmd = _Cmd & "<Object>{1}</Object>"
                _Cmd = _Cmd & "</AdmInfo>"
                _Cmd = _Cmd & "<QueryParams>"
                _Cmd = _Cmd & "{2}"
                _Cmd = _Cmd & "</QueryParams>"
                _Cmd = _Cmd & "{3}"
                _Cmd = _Cmd & "</BO>"
                _Cmd = _Cmd & "</BOM>"
                _Cmd = _Cmd & "</dis:UpdateObject>"
                _Cmd = _Cmd & "</env:Body>"
                _Cmd = _Cmd & "</env:Envelope>"


                Return _Cmd
            End Get
        End Property
        Public Shared ReadOnly Property RemoveObjectCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID>"
                _Cmd = _Cmd & "</env:Header>"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "<dis:RemoveObject xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "<BOM>"
                _Cmd = _Cmd & "<BO>"
                _Cmd = _Cmd & "<AdmInfo>"
                _Cmd = _Cmd & "<Object>{1}</Object>"
                _Cmd = _Cmd & "</AdmInfo>"
                _Cmd = _Cmd & "<QueryParams>"
                _Cmd = _Cmd & "{2}"
                _Cmd = _Cmd & "</QueryParams>"
                _Cmd = _Cmd & "</BO>"
                _Cmd = _Cmd & "</BOM>"
                _Cmd = _Cmd & "</dis:RemoveObject>"
                _Cmd = _Cmd & "</env:Body>"
                _Cmd = _Cmd & "</env:Envelope>"


                Return _Cmd
            End Get
        End Property
        Public Shared ReadOnly Property CancelObjectCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID>"
                _Cmd = _Cmd & "</env:Header>"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "<dis:CancelObject xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "<BOM>"
                _Cmd = _Cmd & "<BO>"
                _Cmd = _Cmd & "<AdmInfo>"
                _Cmd = _Cmd & "<Object>{1}</Object>"
                _Cmd = _Cmd & "</AdmInfo>"
                _Cmd = _Cmd & "<QueryParams>"
                _Cmd = _Cmd & "{2}"
                _Cmd = _Cmd & "</QueryParams>"
                _Cmd = _Cmd & "</BO>"
                _Cmd = _Cmd & "</BOM>"
                _Cmd = _Cmd & "</dis:CancelObject>"
                _Cmd = _Cmd & "</env:Body>"
                _Cmd = _Cmd & "</env:Envelope>"


                Return _Cmd
            End Get
        End Property
        Public Shared ReadOnly Property CloseObjectCmd() As String
            Get
                Dim _Cmd As String = String.Empty
                _Cmd = _Cmd & "<?xml version=""1.0"" encoding=""UTF-16""?>"
                _Cmd = _Cmd & "<env:Envelope xmlns:env=""http://schemas.xmlsoap.org/soap/envelope/"">"
                _Cmd = _Cmd & "<env:Header>"
                _Cmd = _Cmd & "<SessionID>{0}</SessionID>"
                _Cmd = _Cmd & "</env:Header>"
                _Cmd = _Cmd & "<env:Body>"
                _Cmd = _Cmd & "<dis:CloseObject xmlns:dis=""http://www.sap.com/SBO/DIS"">"
                _Cmd = _Cmd & "<BOM>"
                _Cmd = _Cmd & "<BO>"
                _Cmd = _Cmd & "<AdmInfo>"
                _Cmd = _Cmd & "<Object>{1}</Object>"
                _Cmd = _Cmd & "</AdmInfo>"
                _Cmd = _Cmd & "<QueryParams>"
                _Cmd = _Cmd & "{2}"
                _Cmd = _Cmd & "</QueryParams>"
                _Cmd = _Cmd & "</BO>"
                _Cmd = _Cmd & "</BOM>"
                _Cmd = _Cmd & "</dis:CloseObject>"
                _Cmd = _Cmd & "</env:Body>"
                _Cmd = _Cmd & "</env:Envelope>"

                Return _Cmd
            End Get
        End Property


    End Class
End Namespace
