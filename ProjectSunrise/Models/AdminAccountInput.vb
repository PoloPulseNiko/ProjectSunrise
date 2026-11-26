Imports System.ComponentModel.DataAnnotations

Public Class AdminAccountInput
    Public Property Id As String
    <Required, StringLength(150), RegularExpression("^[\w.@+\-]+$")>
    Public Property Username As String
    <Required, EmailAddress, StringLength(256)>
    Public Property Email As String
    <Required, StringLength(30)>
    Public Property Nickname As String
    <DataType(DataType.Password), StringLength(100, MinimumLength:=6)>
    Public Property NewPassword As String
    Public Property Disabled As Boolean
End Class
