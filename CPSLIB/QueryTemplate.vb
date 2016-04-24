Namespace CPSLIB
    Public Class QueryTemplate
        Public Shared SAPVersion As String = "SELECT dbName,cmpName,VERSSTR,dbUser,Loc FROM [{0}].DBO.SRGC WHERE DBNAME = {1}"
        Public Shared SAPCOMMONDB As String = "SBO-COMMON"
    End Class
End Namespace
