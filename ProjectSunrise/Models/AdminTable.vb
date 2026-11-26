Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.Globalization
Imports System.Linq.Expressions
Imports System.Data.Entity

' Only fields listed here are available to the administrator forms.
Public MustInherit Class AdminTable
    Public ReadOnly EntityType As Type
    Public ReadOnly Keys As String()
    Public ReadOnly Fields As String()

    Protected Sub New(entity As Type, keyNames As String, fieldNames As String)
        EntityType = entity
        Keys = keyNames.Split(" "c)
        Fields = fieldNames.Split(" "c)
    End Sub

    Public Shared ReadOnly Tables As New Dictionary(Of String, AdminTable)(StringComparer.OrdinalIgnoreCase) From {
        {"categories", New AdminTable(Of Category)(Function(x) x.Id, "Id", "Name Description")},
        {"tags", New AdminTable(Of Tag)(Function(x) x.Id, "Id", "Name")},
        {"memberships", New AdminTable(Of RoomSeat)(Function(x) x.RoomId, "RoomId UserId", "RoomId UserId Pending Moderator")},
        {"messages", New AdminTable(Of RoomPost)(Function(x) x.Id, "Id", "RoomId SenderId Body")},
        {"reactions", New AdminTable(Of Reaction)(Function(x) x.PostId, "PostId UserId Kind", "PostId UserId Kind")},
        {"conversations", New AdminTable(Of Conversation)(Function(x) x.Id, "Id", "FirstId SecondId")},
        {"direct-messages", New AdminTable(Of Letter)(Function(x) x.Id, "Id", "ConversationId SenderId Body")},
        {"notifications", New AdminTable(Of Notice)(Function(x) x.Id, "Id", "RecipientId ActorId RoomId PostId LetterId Kind Body IsRead")}
    }

    Public MustOverride Function Rows(db As ApplicationDbContext, page As Integer) As List(Of Object)

    Public Function KeyOf(row As Object) As String
        Return String.Join("~", Keys.Select(Function(k) Convert.ToString(EntityType.GetProperty(k).GetValue(row), CultureInfo.InvariantCulture)))
    End Function

    Public Function Find(db As ApplicationDbContext, key As String) As Object
        If String.IsNullOrEmpty(key) Then Return Nothing
        Dim parts = key.Split("~"c)
        If parts.Length <> Keys.Length Then Return Nothing
        Dim values(Keys.Length - 1) As Object
        For i = 0 To Keys.Length - 1
            Try
                values(i) = Convert.ChangeType(parts(i), EntityType.GetProperty(Keys(i)).PropertyType, CultureInfo.InvariantCulture)
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is OverflowException
                Return Nothing
            End Try
        Next
        Return db.Set(EntityType).Find(values)
    End Function
End Class

Public Class AdminTable(Of T As Class)
    Inherits AdminTable
    Private ReadOnly Order As Expression(Of Func(Of T,Integer))
    Public Sub New(orderBy As Expression(Of Func(Of T,Integer)), keys As String, fields As String)
        MyBase.New(GetType(T), keys, fields)
        Order = orderBy
    End Sub
    Public Overrides Function Rows(db As ApplicationDbContext, page As Integer) As List(Of Object)
        Return db.Set(Of T)().OrderByDescending(Order).Skip((page - 1) * 50).Take(50).ToList().Cast(Of Object)().ToList()
    End Function
End Class
