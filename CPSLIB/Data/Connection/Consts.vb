Namespace Data.Connection
    Public Class Consts
        'Connection String
        ''' <summary>
        ''' TEMPLATE_CONNECTIONSTRING Parameter
        ''' {0}: Server
        ''' {1}: Database
        ''' {2}: UserID
        ''' {3}: Password
        ''' {4}: Persist Security Info
        ''' {5}: Pooling
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared TEMPLATE_CONNECTIONSTRING = "Data Source={0};Initial Catalog={1};Persist Security Info={4};User ID={2};Password={3};Pooling={5};"
        Public Shared DEFAULT_PersisSecurityInfo As Boolean = True
        Public Shared DEFAULT_Pooling As Boolean = False


    End Class
End Namespace
