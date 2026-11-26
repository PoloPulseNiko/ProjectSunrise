Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Public Class RoomsController
    Inherits CommunityController

    Public Function Index(search As String, category As Integer?, tag As Integer?, sort As String) As ActionResult
        Dim rooms = Db.VisibleRooms(Uid, Staff)
        If Not String.IsNullOrWhiteSpace(search) Then rooms = rooms.Where(Function(r) r.Name.Contains(search))
        If category.HasValue Then rooms = rooms.Where(Function(r) r.CategoryId = category)
        If tag.HasValue Then rooms = rooms.Where(Function(r) r.Tags.Any(Function(t) t.Id = tag))
        Catalog()
        Return View(If(sort = "newest", rooms.OrderByDescending(Function(r) r.Id), rooms.OrderBy(Function(r) r.Name)).Include("Owner").ToList())
    End Function

    Private Sub Catalog()
        ViewBag.Categories = Db.Categories.OrderBy(Function(x) x.Name).ToList()
        ViewBag.Tags = Db.Tags.OrderBy(Function(x) x.Name).ToList()
    End Sub

    <AllowAnonymous>
    Public Function Details(id As Integer) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        Dim rights = Db.Access(room, Uid, Staff)
        If Not rights.Visible Then Return Denied()
        ViewBag.Rights = rights
        If rights.Manage Then
            ViewBag.Invitations = Db.Seats.Where(Function(s) s.RoomId = id AndAlso s.Pending).Select(Function(s) s.User.Nickname).OrderBy(Function(n) n).ToList()
        End If
        If rights.ReadPosts Then
            ViewBag.Posts = Db.Posts.Where(Function(p) p.RoomId = id).Include("Sender").Include("Reactions").OrderBy(Function(p) p.Id).ToList()
        End If
        Return View(room)
    End Function

    Public Function Create() As ActionResult
        Return RedirectToAction("Edit", New With {.id = 0})
    End Function

    Public Function Edit(Optional id As Integer = 0) As ActionResult
        Dim room = If(id = 0, New Room(), Db.Rooms.Find(id))
        If room Is Nothing Then Return HttpNotFound()
        If id <> 0 AndAlso Not Db.Access(room, Uid, Staff).Manage Then Return Denied()
        Catalog()
        Return View(room)
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Save(id As Integer, name As String, description As String, categoryId As Integer?, tagIds As Integer(),
                         joining As String, visibility As String, posting As String) As ActionResult
        Dim room = If(id = 0, New Room With {.OwnerId = Uid}, Db.Rooms.Find(id))
        If room Is Nothing Then Return HttpNotFound()
        If id <> 0 AndAlso Not Db.Access(room, Uid, Staff).Manage Then Return Denied()
        If id = 0 Then room.Name = If(name, "").Trim()
        room.Description = If(description, "").Trim()
        If room.Name.Length < 3 OrElse room.Name.Length > 100 OrElse room.Description.Length = 0 OrElse room.Description.Length > 500 Then
            ModelState.AddModelError("", "Use a room name of 3 to 100 characters and a description of 1 to 500 characters.")
            Catalog()
            Return View("Edit", room)
        End If
        If joining <> "open" AndAlso joining <> "invite" Then Return Invalid("Unknown join policy.")
        room.Joining = joining
        If Staff Then
            If Not {"public", "private", "staff"}.Contains(visibility) OrElse Not {"members", "moderators", "staff"}.Contains(posting) Then Return Invalid("Unknown access policy.")
            room.Visibility = visibility
            room.Posting = posting
        End If
        If categoryId.HasValue AndAlso Db.Categories.Find(categoryId.Value) Is Nothing Then Return Invalid("Unknown category.")
        room.CategoryId = categoryId
        Dim ids = If(tagIds, New Integer() {}).Distinct().ToArray()
        Dim tags = Db.Tags.Where(Function(t) ids.Contains(t.Id)).ToList()
        If tags.Count <> ids.Length Then Return Invalid("Unknown tag.")
        room.Tags.Clear()
        For Each item In tags
            room.Tags.Add(item)
        Next
        If id = 0 Then
            Db.Rooms.Add(room)
            Db.Seats.Add(New RoomSeat With {.Room = room, .UserId = Uid, .Moderator = True})
        Else
            Db.NotifyMembers(room, Uid, "room", room.Name & " has been updated.")
        End If
        Db.SaveChanges()
        Return RedirectToAction("Details", New With {.id = room.Id})
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Join(id As Integer) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        Dim rights = Db.Access(room, Uid, Staff)
        If Not rights.Join Then Return Denied()
        Dim seat = Db.Seats.Find(id, Uid)
        If seat Is Nothing Then
            Db.Seats.Add(New RoomSeat With {.RoomId = id, .UserId = Uid})
        Else
            seat.Pending = False
            Dim recipient = Uid
            For Each n In Db.Notices.Where(Function(x) x.RecipientId = recipient AndAlso x.RoomId = id AndAlso x.Kind = "invite")
                n.IsRead = True
            Next
        End If
        Try
            Db.SaveChanges()
        Catch ex As DbUpdateException
            If Not Db.Seats.AsNoTracking().Any(Function(s) s.RoomId = id AndAlso s.UserId = Uid AndAlso Not s.Pending) Then Throw
        End Try
        Return RedirectToAction("Details", New With {.id = id})
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Leave(id As Integer) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        If room.OwnerId = Uid Then Return Denied()
        Dim seat = Db.Seats.Find(id, Uid)
        If seat IsNot Nothing Then
            Db.Seats.Remove(seat)
            Db.SaveChanges()
        End If
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Invite(id As Integer, nickname As String) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        If Not Db.Access(room, Uid, Staff).Manage Then Return Denied()
        Dim target = Db.Users.SingleOrDefault(Function(u) u.Nickname = nickname)
        If target Is Nothing Then
            TempData("Feedback") = "No profile has that nickname."
        ElseIf Db.Seats.Find(id, target.Id) Is Nothing Then
            Db.Seats.Add(New RoomSeat With {.Room = room, .UserId = target.Id, .Pending = True})
            Db.Notices.Add(New Notice With {.Room = room, .RecipientId = target.Id, .ActorId = Uid, .Kind = "invite", .Body = "You were invited to " & room.Name & "."})
            Try
                Db.SaveChanges()
            Catch ex As DbUpdateException
                If Not Db.Seats.AsNoTracking().Any(Function(s) s.RoomId = id AndAlso s.UserId = target.Id) Then Throw
            End Try
        End If
        Return RedirectToAction("Details", New With {.id = id})
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Send(id As Integer, input As ChatMessageInput) As ActionResult
        Dim body = input.Body
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        If Not Db.Access(room, Uid, Staff).WritePosts Then Return Denied()
        If String.IsNullOrWhiteSpace(body) Then Return Invalid("Enter a message.")
        Dim post = Db.Posts.Add(New RoomPost With {.Room = room, .SenderId = Uid, .Body = body.Trim()})
        Db.NotifyMembers(room, Uid, "message", "New message in " & room.Name & ".", post)
        Db.SaveChanges()
        Return Redirect(Url.Action("Details", New With {.id = id}) & "#message-" & post.Id)
    End Function

    Public Function Delete(id As Integer) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        If Not Db.Access(room, Uid, Staff).Manage Then Return Denied()
        Return View("ConfirmDelete", CType(room.Name, Object))
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Delete")>
    Public Function DeleteRoom(id As Integer) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        If Not Db.Access(room, Uid, Staff).Manage Then Return Denied()
        Using tx = Db.Database.BeginTransaction()
            Db.Database.ExecuteSqlCommand("DELETE FROM dbo.Notices WHERE RoomId = @p0", id)
            Db.Rooms.Remove(room)
            Db.SaveChanges()
            tx.Commit()
        End Using
        Return RedirectToAction("Index")
    End Function
End Class
