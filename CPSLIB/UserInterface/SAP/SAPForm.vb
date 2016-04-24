Imports B1WizardBase
Namespace SAPUI
    Public Class SAPForm
        Private _Debug As CPSLIB.Debug
        Private _CPSException As CPSException
        Private _oForm As SAPbouiCOM.Form
        Private _FormUID As String

        Public Sub New(ByVal _FormUID As String)
            Me._FormUID = _FormUID
            _oForm = B1Connections.theAppl.Forms.Item(_FormUID)
        End Sub

        Public Sub Create(ByVal strPath As String)
            Dim _oForms As SAPbouiCOM.Forms = B1Connections.theAppl.Forms
            Dim oFormParams As SAPbouiCOM.FormCreationParams = B1Connections.theAppl.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams)

            Try
                Dim oXmlDoc As New XML.XMLDocument(New IO.Ascii.AsciiFile(strPath))

                oFormParams.XmlData = oXmlDoc.InnerXml
                _oForm = _oForms.AddEx(oFormParams)

                _oForm.Visible = True



            Catch comEx As Runtime.InteropServices.COMException

                If comEx.ErrorCode = -7010 Then
                    _oForm = _oForm.Item(oFormParams.UniqueID)

                    _oForm.Visible = True
                    _oForm.Select()
                End If



            Catch ex As Exception
                _oForm = _oForms.Item(oFormParams.UniqueID)

                _oForm.Visible = True
                _oForm.Select()

            End Try
        End Sub
#Region "Property"
        Public ReadOnly Property Exists() As Boolean
            Get
                If _oForm Is Nothing Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        Public ReadOnly Property UID() As String
            Get

                Return _oForm.UniqueID


            End Get
        End Property

        Public ReadOnly Property _Object() As SAPbouiCOM.Form
            Get
                Return _oForm
            End Get
        End Property

#End Region
    End Class
End Namespace
