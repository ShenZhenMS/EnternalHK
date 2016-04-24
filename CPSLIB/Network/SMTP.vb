Imports System.Net
Namespace Network
    Public Class SMTP : Inherits System.Net.Mail.SmtpClient

        Public Shared Default_Port_Number As Integer = 25
        Private _Server As String
        Private _Login As String
        Private _Password As String
        Private _Body As String
        Private _alAttachment As ArrayList
        Private _alMailTo As ArrayList
        Private _Subject As String
        Private _alMailCC As ArrayList
        Private _alMailBCC As ArrayList
        Private _PortNumber As Integer
        Private _MailFrom As String
        Dim _CPSException As CPSException
        Dim _Debug As CPSLIB.Debug

        Private _isError As Boolean
        Private _isHTMLFormat As Boolean
        Public Property isHTMLFormat() As Boolean
            Get
                Return _isHTMLFormat
            End Get
            Set(ByVal value As Boolean)
                _isHTMLFormat = value
            End Set
        End Property

        Private _Message As String
        Public Property Message() As String
            Get
                Return _Message
            End Get
            Set(ByVal value As String)
                _Message = value
            End Set
        End Property

        Public Property isError() As Boolean
            Get
                Return _isError
            End Get
            Set(ByVal value As Boolean)
                _isError = value
            End Set
        End Property

        Public Property MailFrom() As String
            Get
                Return _MailFrom
            End Get
            Set(ByVal value As String)
                _MailFrom = value
            End Set
        End Property

        Public Property MailBCC() As ArrayList
            Get
                Return _alMailBCC
            End Get
            Set(ByVal value As ArrayList)
                _alMailBCC = value
            End Set
        End Property

        Public Sub SetMailBCC(ByVal email As String)
            If _alMailBCC Is Nothing Then
                _alMailBCC = New ArrayList
            End If
            _alMailBCC.Add(email)
        End Sub

        Public Sub SetMailCC(ByVal email As String)
            If _alMailCC Is Nothing Then
                _alMailCC = New ArrayList
            End If
            _alMailCC.Add(email)
        End Sub

        Public Property MailCC() As ArrayList
            Get
                Return _alMailCC
            End Get
            Set(ByVal value As ArrayList)
                _alMailCC = value
            End Set
        End Property

        Public Property Subject() As String
            Get
                Return _Subject
            End Get
            Set(ByVal value As String)
                _Subject = value
            End Set
        End Property

        Public Sub setMailTo(ByVal email As String)
            If _alMailTo Is Nothing Then
                _alMailTo = New ArrayList
            End If
            _alMailTo.Add(email)
        End Sub
        Public Property MailTo() As ArrayList
            Get
                Return _alMailTo
            End Get
            Set(ByVal value As ArrayList)

                _alMailTo = value
            End Set
        End Property


        Public Sub SetAttachment(ByVal _file As String)
            If _alAttachment Is Nothing Then
                _alAttachment = New ArrayList
            End If
            _alAttachment.Add(_file)
        End Sub

        Public Property Attachment() As ArrayList
            Get
                Return _alAttachment
            End Get
            Set(ByVal value As ArrayList)
                _alAttachment = value
            End Set
        End Property

        Public Property Body() As String
            Get
                Return _Body
            End Get
            Set(ByVal value As String)
                _Body = value
            End Set
        End Property


        Public Property Password() As String
            Get
                Return _Password
            End Get
            Set(ByVal value As String)
                _Password = value
            End Set
        End Property

        Public Property Login() As String
            Get
                Return _Login
            End Get
            Set(ByVal value As String)
                _Login = value
            End Set
        End Property

        Public Property Server() As String
            Get
                Return _Server
            End Get
            Set(ByVal value As String)
                _Server = value
            End Set
        End Property



        Public Property PortNumner() As Integer
            Get
                Return _PortNumber
            End Get
            Set(ByVal value As Integer)
                _PortNumber = value
            End Set
        End Property

        Public Sub New(ByVal host As String, ByVal Login As String, ByVal Password As String, ByVal PortNumber As Integer)
            MyBase.New(host, PortNumber)
            MyBase.Credentials = New System.Net.NetworkCredential(Login, Password)

            Me._Server = host
            Me._Login = Login
            Me._Password = Password
            Me._PortNumber = PortNumber
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
        End Sub
        Public Sub New()
            MyBase.New()
            _PortNumber = 25 ' Default SMTP Port
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
        End Sub

        Public Function ExecuteMail() As Boolean
            Dim _ret As Boolean = True
            Dim mail As System.Net.Mail.MailMessage
            Try

            
            If _PortNumber > 0 Then
                MyBase.Port = _PortNumber
            End If

                MyBase.Host = _Server
                If _Login <> String.Empty Then

                    MyBase.Credentials = New System.Net.NetworkCredential(_Login, _Password)
                End If


                mail = New System.Net.Mail.MailMessage
                mail.IsBodyHtml = _isHTMLFormat
            mail.From = New System.Net.Mail.MailAddress(_MailFrom)
            If Not _alMailTo Is Nothing Then
                For Each o As Object In _alMailTo.ToArray
                    mail.To.Add(New System.Net.Mail.MailAddress(o))
                Next
            End If
            If Not _alMailCC Is Nothing Then
                For Each o As Object In _alMailCC.ToArray()
                    mail.CC.Add(New System.Net.Mail.MailAddress(o))
                Next
            End If
            If Not _alMailBCC Is Nothing Then
                For Each o As Object In _alMailBCC.ToArray
                    mail.Bcc.Add(New System.Net.Mail.MailAddress(o))
                Next
            End If

            mail.Subject = _Subject
            mail.Body = _Body
                MyBase.Send(mail)

                _Message = String.Empty

            Catch ex As Exception
                _ret = False
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function
    End Class
End Namespace
