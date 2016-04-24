Imports Settings
Namespace Logging
    ''' <summary>
    ''' Version 1: HardCode Message Code in the Class and allow add new message in coding.
    ''' Defination of Message Code: 1: Debug
    '''                             2: Error
    '''                             3: Warning
    '''                             4: Information
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MessageCode
#Region "Library default message code"
        'PLEASE APPEND MESSAGE CODE BY FOR VERSION CONTROL
        'Version1: 2010-05-05
        Public Enum MessageCode
            ' Companies is blank
            BLANK_COMPANY = 200001
            NO_RECORD_FOUND = 200002
            OBJECT_NOT_EXISTS = 200003
            OBJECT_EXISTS = 200004

            ' Success
            SUCCESS = 400001

        End Enum

#End Region
#Region "Settings"




        Private _ReadFromDatabase As Boolean
        Private _ReadFromXML As Boolean

        'Database Information
        Private _MessageServer As String
        Private _MessageDatabase As String
        Private _MessageUserName As String
        Private _MessagePassword As String
        ' File Information
        Private _MessageFilePath As String

#End Region
        Public Enum MessageType
            debug = 1
            [error] = 2
            warning = 3
            information = 4
        End Enum


        Dim _HTMessage As Hashtable
        Public Sub New()
            Setup()
        End Sub



#Region "Property"

#End Region
#Region "Init Message Code"
        Private Sub SetDefaultMessage()
            If _HTMessage Is Nothing Then
                _HTMessage = New Hashtable
            End If
            If _HTMessage.Count > 0 Then
                _HTMessage.Clear()
            End If
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            ' Default Message Setup in here
            ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
            _HTMessage.Add(MessageCode.BLANK_COMPANY, "No companies selected!")
            _HTMessage.Add(MessageCode.SUCCESS, "Success")
            _HTMessage.Add(MessageCode.NO_RECORD_FOUND, "No Record Found!")
            _HTMessage.Add(MessageCode.OBJECT_NOT_EXISTS, "Object {0} doesn't exists!")
            _HTMessage.Add(MessageCode.OBJECT_EXISTS, "Object {0} already exists!")

        End Sub
        Private Sub Setup()
            SetDefaultMessage()
            ' Karrson: Comment following code for read ini file directly in next version

            '_ReadFromDatabase = _MainSetting.MessageReadFromDatabase
            '_ReadFromXML = _MainSetting.MessageReadFromXML
            '_MessageDatabase = _MainSetting.MessageSQLDatabase
            '_MessageFilePath = _MainSetting.MessageFilePath
            '_MessageServer = _MainSetting.MessageSQLServer
            '_MessageUserName = _MainSetting.MessageSQLLogin
            '_MessagePassword = _MainSetting.MessageSQLPassword


            If _ReadFromDatabase Then

            Else
                If _ReadFromXML Then

                End If

            End If
        End Sub
#End Region
#Region "Processing"

        Public Sub Read(ByVal _MessageCode As Logging.MessageCode.MessageCode, ByRef s As String, ByVal sperater As String)
            s = s & _MessageCode & " : " & _HTMessage(_MessageCode).ToString() & sperater
        End Sub
        Public Function Read(ByVal _MessageCode As Logging.MessageCode.MessageCode) As String
            Return _MessageCode & " : " & _HTMessage(_MessageCode).ToString()
        End Function

        Public Function Add(ByVal Message As String, ByVal _MessageType As Logging.MessageCode.MessageType) As Integer

        End Function


#End Region
#Region "Message Import/Export"
        Public Function ExportDatabase() As Boolean

        End Function

        Public Function ExportFile() As Boolean

        End Function

        Public Function ExportXML() As Boolean

        End Function

        Public Function ImportDatabase() As Boolean

        End Function

        Public Function ImportFile() As Boolean

        End Function

        Public Function ImportXML() As Boolean

        End Function

#End Region

    End Class
End Namespace
