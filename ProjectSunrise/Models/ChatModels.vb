Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity

Public Class Room
    Public Property Id As Integer
    <Required, StringLength(100)>
    Public Property Name As String
    <Required, StringLength(500)>
    Public Property Description As String
    <Required>
    Public Property OwnerId As String
    Public Overridable Property Owner As ApplicationUser
    Public Property CategoryId As Integer?
    Public Overridable Property Category As Category
    Public Overridable Property Tags As ICollection(Of Tag) = New List(Of Tag)
    Public Overridable Property Seats As ICollection(Of RoomSeat) = New List(Of RoomSeat)
    <Required, StringLength(20)>
    Public Property Visibility As String = "public"
    <Required, StringLength(20)>
    Public Property Posting As String = "members"
    <Required, StringLength(20)>
    Public Property Joining As String = "open"
End Class

Public Class Category
    Public Property Id As Integer
    <Required, StringLength(40), Index(IsUnique:=True)>
    Public Property Name As String
    Public Property Description As String
End Class

Public Class Tag
    Public Property Id As Integer
    <Required, StringLength(30), Index(IsUnique:=True)>
    Public Property Name As String
    Public Overridable Property Rooms As ICollection(Of Room)
End Class

' A pending seat represents an invitation until the recipient joins.
Public Class RoomSeat
    <Key, Column(Order:=0)>
    Public Property RoomId As Integer
    Public Overridable Property Room As Room
    <Key, Column(Order:=1), MaxLength(128)>
    Public Property UserId As String
    Public Overridable Property User As ApplicationUser
    Public Property Pending As Boolean
    Public Property Moderator As Boolean
End Class

Public Class RoomPost
    Public Property Id As Integer
    <Index>
    Public Property RoomId As Integer
    Public Overridable Property Room As Room
    <Required>
    Public Property SenderId As String
    Public Overridable Property Sender As ApplicationUser
    <Required>
    Public Property Body As String
    Public Property Created As DateTime = DateTime.UtcNow
    Public Overridable Property Reactions As ICollection(Of Reaction) = New List(Of Reaction)
End Class

Public Class Reaction
    <Key, Column(Order:=0)>
    Public Property PostId As Integer
    Public Overridable Property Post As RoomPost
    <Key, Column(Order:=1), MaxLength(128)>
    Public Property UserId As String
    Public Overridable Property User As ApplicationUser
    <Key, Column(Order:=2), StringLength(10)>
    Public Property Kind As String
End Class

Public Class Conversation
    Public Property Id As Integer
    <Required, MaxLength(128), Index("IX_Pair", 1, IsUnique:=True)>
    Public Property FirstId As String
    Public Overridable Property First As ApplicationUser
    <Required, MaxLength(128), Index("IX_Pair", 2, IsUnique:=True)>
    Public Property SecondId As String
    Public Overridable Property Second As ApplicationUser
    Public Property Updated As DateTime = DateTime.UtcNow
    Public Overridable Property Letters As ICollection(Of Letter)
End Class

Public Class Letter
    Public Property Id As Integer
    Public Property ConversationId As Integer
    Public Overridable Property Conversation As Conversation
    <Required>
    Public Property SenderId As String
    Public Overridable Property Sender As ApplicationUser
    <Required>
    Public Property Body As String
    Public Property Created As DateTime = DateTime.UtcNow
End Class

Public Class Notice
    Public Property Id As Integer
    <Required, Index>
    Public Property RecipientId As String
    Public Overridable Property Recipient As ApplicationUser
    <Required>
    Public Property ActorId As String
    Public Overridable Property Actor As ApplicationUser
    Public Property RoomId As Integer?
    Public Overridable Property Room As Room
    Public Property PostId As Integer?
    Public Overridable Property Post As RoomPost
    Public Property LetterId As Integer?
    Public Overridable Property Letter As Letter
    <Required, StringLength(20)>
    Public Property Kind As String
    <Required, StringLength(255)>
    Public Property Body As String
    Public Property IsRead As Boolean
    Public Property Created As DateTime = DateTime.UtcNow
    Public Function TargetUrl() As String
        If Letter IsNot Nothing Then Return VirtualPathUtility.ToAbsolute("~/Direct/Details/" & Letter.ConversationId) & "#direct-message-" & Letter.Id
        If RoomId.HasValue Then Return VirtualPathUtility.ToAbsolute("~/Rooms/Details/" & RoomId.Value) & If(PostId.HasValue, "#message-" & PostId.GetValueOrDefault(), "")
        Return VirtualPathUtility.ToAbsolute("~/Notifications")
    End Function
End Class

Public Class RoomAccess
    Public ReadOnly Visible As Boolean
    Public ReadOnly Member As Boolean
    Public ReadOnly ReadPosts As Boolean
    Public ReadOnly WritePosts As Boolean
    Public ReadOnly Manage As Boolean
    Public ReadOnly Join As Boolean

    Public Sub New(room As Room, userId As String, staff As Boolean, seat As RoomSeat)
        Member = seat IsNot Nothing AndAlso Not seat.Pending
        Dim invited = seat IsNot Nothing AndAlso seat.Pending
        Manage = staff OrElse (userId IsNot Nothing AndAlso room.OwnerId = userId)
        Visible = staff OrElse room.Visibility = "public" OrElse
            (room.Visibility = "private" AndAlso (Member OrElse invited))
        ReadPosts = Visible AndAlso (staff OrElse Member)
        WritePosts = ReadPosts AndAlso (staff OrElse room.Posting = "members" OrElse
            (room.Posting = "moderators" AndAlso (Manage OrElse seat.Moderator)))
        Join = Visible AndAlso userId IsNot Nothing AndAlso Not staff AndAlso Not Member AndAlso
            (room.Joining = "open" OrElse invited)
    End Sub
End Class

Partial Public Class ApplicationDbContext
    Public Property Rooms As DbSet(Of Room)
    Public Property Seats As DbSet(Of RoomSeat)
    Public Property Categories As DbSet(Of Category)
    Public Property Tags As DbSet(Of Tag)
    Public Property Posts As DbSet(Of RoomPost)
    Public Property Reactions As DbSet(Of Reaction)
    Public Property Conversations As DbSet(Of Conversation)
    Public Property Letters As DbSet(Of Letter)
    Public Property Notices As DbSet(Of Notice)

    Protected Overrides Sub OnModelCreating(builder As DbModelBuilder)
        MyBase.OnModelCreating(builder)
        builder.Entity(Of Room)().ToTable("Rooms")
        builder.Entity(Of Category)().ToTable("Categories")
        builder.Entity(Of RoomSeat)().ToTable("RoomSeats")
        builder.Entity(Of Tag)().ToTable("Tags")
        builder.Entity(Of RoomPost)().ToTable("RoomPosts")
        builder.Entity(Of Notice)().ToTable("Notices")
        builder.Entity(Of Reaction)().ToTable("Reactions")
        builder.Entity(Of Conversation)().ToTable("Conversations")
        builder.Entity(Of Letter)().ToTable("Letters")
        ' SQL Server rejects multiple cascade paths to the same user or notice.
        builder.Conventions.Remove(Of ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention)()
        builder.Entity(Of Tag)().HasMany(Function(t) t.Rooms).WithMany(Function(r) r.Tags).Map(Sub(link)
            link.ToTable("TagRooms")
            link.MapLeftKey("Tag_Id")
            link.MapRightKey("Room_Id")
        End Sub)
        builder.Entity(Of RoomSeat)().HasRequired(Function(x) x.Room).WithMany(Function(x) x.Seats).HasForeignKey(Function(x) x.RoomId).WillCascadeOnDelete(True)
        builder.Entity(Of RoomPost)().HasRequired(Function(x) x.Room).WithMany().HasForeignKey(Function(x) x.RoomId).WillCascadeOnDelete(True)
        builder.Entity(Of Reaction)().HasRequired(Function(x) x.Post).WithMany(Function(x) x.Reactions).HasForeignKey(Function(x) x.PostId).WillCascadeOnDelete(True)
        builder.Entity(Of Letter)().HasRequired(Function(x) x.Conversation).WithMany(Function(x) x.Letters).HasForeignKey(Function(x) x.ConversationId).WillCascadeOnDelete(True)
        builder.Entity(Of ApplicationUser)().Property(Function(x) x.Email).HasColumnAnnotation("Index", New Infrastructure.Annotations.IndexAnnotation(New IndexAttribute("IX_Email") With {.IsUnique = True}))
    End Sub

    Public Function VisibleRooms(uid As String, staff As Boolean) As IQueryable(Of Room)
        Return Rooms.Where(Function(r) staff OrElse r.Visibility = "public" OrElse
            (r.Visibility = "private" AndAlso r.Seats.Any(Function(s) s.UserId = uid AndAlso Not s.Pending)))
    End Function

    Public Function Access(room As Room, uid As String, staff As Boolean) As RoomAccess
        Dim seat = If(uid Is Nothing, Nothing, Seats.Find(room.Id, uid))
        Return New RoomAccess(room, uid, staff, seat)
    End Function

    Public Sub NotifyMembers(room As Room, actorId As String, kind As String, body As String, Optional post As RoomPost = Nothing)
        Dim recipients = Seats.Where(Function(s) s.RoomId = room.Id AndAlso Not s.Pending AndAlso s.UserId <> actorId).Select(Function(s) s.UserId).ToList()
        For Each uid In recipients
            Notices.Add(New Notice With {.RecipientId = uid, .ActorId = actorId, .Room = room, .Kind = kind, .Body = body, .Post = post})
        Next
    End Sub
End Class

Public Class ChatMessageInput
    <AllowHtml>
    Public Property Body As String
End Class
