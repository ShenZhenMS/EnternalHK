Imports System.Net
Imports System.IO
Namespace Network
    Public Class FTPClient

        Dim _Debug As CPSLIB.Debug
        Dim _CPSException As CPSException


        Private _isError As Boolean
        Public Property isError() As Boolean
            Get
                Return _isError
            End Get
            Set(ByVal value As Boolean)
                _isError = value
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


        Private _ftpserver As String

        Private _UserName As String

        Private _Password As String

        Private _status As String
        Public Property FTPStatus() As String
            Get
                Return _status
            End Get
            Set(ByVal value As String)
                _status = value
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

        Public Property UserName() As String
            Get
                Return _UserName
            End Get
            Set(ByVal value As String)
                _UserName = value
            End Set
        End Property

        Public Property FTPServer() As String
            Get
                Return _ftpserver
            End Get
            Set(ByVal value As String)
                _ftpserver = value
            End Set
        End Property


        Public Sub New(ByVal _FTPServer As String, ByVal username As String, ByVal Password As String)
            _Debug = New CPSLIB.Debug(Me.GetType.ToString)
            _CPSException = New CPSException
            Me._ftpserver = _FTPServer
            Me._UserName = username
            Me._Password = Password
            _isError = False
        End Sub

        Public Function Download(ByVal _filename As String, ByVal _TargetPath As String) As Boolean
            Dim myFtpWebRequest As FtpWebRequest
            Dim myFtpWebResponse As FtpWebResponse
            Dim myStreamWriter As StreamWriter
            Dim _ret As Boolean = True
            Try
                _Debug.Write(String.Format("ftp://{0}/{1}", _ftpserver, _filename), "Download Process")
                myFtpWebRequest = WebRequest.Create(String.Format("ftp://{0}/{1}", _ftpserver, _filename))

                If _UserName Is Nothing = False Then
                    myFtpWebRequest.Credentials = New NetworkCredential(_UserName, _Password)
                End If


                myFtpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile
                myFtpWebRequest.UseBinary = True

                myFtpWebResponse = myFtpWebRequest.GetResponse()

                myStreamWriter = New StreamWriter(String.Format("{0}/{1}", _TargetPath, _filename))
                myStreamWriter.Write(New StreamReader(myFtpWebResponse.GetResponseStream()).ReadToEnd)
                myStreamWriter.Close()

                _status = myFtpWebResponse.StatusDescription

                myFtpWebResponse.Close()
                _ret = True
            Catch ex As Exception
                _ret = False
                _isError = True
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function

        Public Function Upload(ByVal _filename As String, ByVal tgtFileName As String) As Boolean
            Dim myFtpWebRequest As FtpWebRequest
            Dim myFtpWebResponse As FtpWebResponse
            Dim _f As IO.Ascii.AsciiFile
            Dim sr As StreamReader
            Dim _stream As Stream
            Dim fc As Byte()
            Dim _ret As Boolean = True
            Try
                '_f = New IO.Ascii.AsciiFile(_filename)

                _Debug.Write(String.Format("ftp://{0}/{1}", _ftpserver, tgtFileName), "Upload Process")
                myFtpWebRequest = WebRequest.Create(String.Format("ftp://{0}/{1}", _ftpserver, tgtFileName))

                If _UserName Is Nothing = False Then
                    myFtpWebRequest.Credentials = New NetworkCredential(_UserName, _Password)
                End If



                myFtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile
                myFtpWebRequest.UseBinary = True


                sr = New StreamReader(_filename)
                fc = System.Text.Encoding.UTF8.GetBytes(sr.ReadToEnd())
                myFtpWebRequest.ContentLength = fc.Length
                sr.Close()
                _stream = myFtpWebRequest.GetRequestStream
                _stream.Write(fc, 0, fc.Length)
                _stream.Close()
                myFtpWebResponse = myFtpWebRequest.GetResponse

                _ret = True
            Catch ex As Exception
                _ret = False
                _isError = True
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret
        End Function

        Public Function Delete(ByVal _filename As String) As Boolean
            Dim myFtpWebRequest As FtpWebRequest
            Dim myFtpWebResponse As FtpWebResponse
            Dim myStreamWriter As StreamWriter
            Dim _ret As Boolean = True
            Try
                _Debug.Write(String.Format("ftp://{0}/{1}", _ftpserver, _filename), "Download Process")
                myFtpWebRequest = WebRequest.Create(String.Format("ftp://{0}/{1}", _ftpserver, _filename))

                If _UserName Is Nothing = False Then
                    myFtpWebRequest.Credentials = New NetworkCredential(_UserName, _Password)
                End If
                myFtpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile
                myFtpWebResponse = myFtpWebRequest.GetResponse()

                _status = myFtpWebResponse.StatusDescription

                myFtpWebResponse.Close()
                _ret = True
            Catch ex As Exception
                _ret = False
                _isError = True
                _Message = ex.Message
                _CPSException.ExecuteHandle(ex)
            End Try
            Return _ret



        End Function

    End Class
End Namespace
