Imports H = System.Web.Http
Imports System.Data.Entity
Imports Microsoft.AspNet.Identity

Public Class ChatApiController
    Inherits H.ApiController
    Private ReadOnly Db As New ApplicationDbContext

    Private Function RoomData(room As Room) As Object
        Return New With {.id = room.Id, .name = room.Name, .description = room.Description, .creator = room.Owner.Nickname,
            .category = If(room.Category Is Nothing, Nothing, room.Category.Name),
            .tags = room.Tags.Select(Function(t) New With {.id = t.Id, .name = t.Name}).ToList(),
            .members_count = room.Seats.Where(Function(s) Not s.Pending).Count()}
    End Function

    <H.HttpGet, H.Route("rooms/api")>
    Public Function Rooms() As H.IHttpActionResult
        Dim available = Db.VisibleRooms(User.Identity.GetUserId(), User.IsInRole("Staff")).Include("Owner").Include("Category").Include("Tags").Include("Seats").OrderBy(Function(r) r.Id).ToList()
        Return Json(available.Select(AddressOf RoomData).ToList())
    End Function

    <H.HttpGet, H.Route("rooms/api/{id:int}")>
    Public Function RoomInfo(id As Integer) As H.IHttpActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return NotFound()
        If Not Db.Access(room, User.Identity.GetUserId(), User.IsInRole("Staff")).Visible Then Return StatusCode(Net.HttpStatusCode.Forbidden)
        Return Json(RoomData(room))
    End Function

    <H.HttpGet, H.Route("rooms/api/{id:int}/messages")>
    Public Function Messages(id As Integer, Optional latest As Boolean = False) As H.IHttpActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return NotFound()
        If Not Db.Access(room, User.Identity.GetUserId(), User.IsInRole("Staff")).ReadPosts Then Return StatusCode(Net.HttpStatusCode.Forbidden)
        Dim posts = Db.Posts.Where(Function(p) p.RoomId = id).OrderByDescending(Function(p) p.Id)
        If latest Then Return Json(New With {.id = posts.Select(Function(p) p.Id).FirstOrDefault()})
        Return Json(posts.Select(Function(p) New With {.id = p.Id, .sender = p.Sender.Nickname, .room = p.Room.Name, .text = p.Body, .created_at = p.Created}).ToList())
    End Function

    <H.Authorize, H.HttpGet, H.Route("notifications/api")>
    Public Function Notifications() As H.IHttpActionResult
        Dim uid = User.Identity.GetUserId()
        Dim items = Db.Notices.Where(Function(n) n.RecipientId = uid).Include("Room").Include("Recipient").Include("Actor").Include("Letter").OrderByDescending(Function(n) n.Id).ToList()
        Return Json(items.Select(Function(n) New With {.id = n.Id, .recipient = n.Recipient.Nickname, .actor = n.Actor.Nickname,
            .room = If(n.Room Is Nothing, Nothing, n.Room.Name), .notification_type = n.Kind, .text = n.Body,
            .is_read = n.IsRead, .created_at = n.Created, .target_url = n.TargetUrl()}).ToList())
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then Db.Dispose()
        MyBase.Dispose(disposing)
    End Sub
End Class
