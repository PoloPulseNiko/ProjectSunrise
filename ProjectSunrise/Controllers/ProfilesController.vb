Imports System.Data.Entity
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.AspNet.Identity

Public Class ProfilesController
    Inherits CommunityController

    Public Function Create() As ActionResult
        Return RedirectToAction("Edit", New With {.id = Uid})
    End Function

    <AllowAnonymous>
    Public Function Index(q As String) As ActionResult
        Dim users = Db.Users.AsQueryable()
        If Not String.IsNullOrWhiteSpace(q) Then users = users.Where(Function(u) u.Nickname.Contains(q) OrElse u.UserName.Contains(q))
        Return View(users.OrderBy(Function(u) u.Nickname).Select(Function(u) New ProfileSummary With {.Id = u.Id, .Nickname = u.Nickname}).ToList())
    End Function

    <AllowAnonymous>
    Public Function Details(id As String) As ActionResult
        If id Is Nothing Then Return HttpNotFound()
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        Return View(person)
    End Function

    <AllowAnonymous>
    Public Function Picture(id As String) As ActionResult
        Dim bytes = Db.Users.Where(Function(u) u.Id = id).Select(Function(u) u.Avatar).FirstOrDefault()
        If bytes Is Nothing Then Return HttpNotFound()
        Return File(bytes, "image/jpeg")
    End Function

    Public Function Edit(id As String) As ActionResult
        If Not Staff AndAlso id <> Uid Then Return Denied()
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        Return View(person)
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Edit")>
    Public Function UpdateProfile(id As String, bio As String, avatar As HttpPostedFileBase, Optional clearAvatar As Boolean = False) As ActionResult
        If Not Staff AndAlso id <> Uid Then Return Denied()
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        If If(bio, "").Length > 500 Then ModelState.AddModelError("bio", "Use up to 500 characters for your bio.")
        If clearAvatar Then person.Avatar = Nothing
        If avatar IsNot Nothing AndAlso avatar.ContentLength > 0 Then
            Try
                If avatar.ContentLength > 2097152 Then Throw New InvalidDataException()
                Using original = Image.FromStream(avatar.InputStream, False, True)
                    If CLng(original.Width) * original.Height > 16000000 Then Throw New InvalidDataException()
                    Dim scale = Math.Min(1.0, 256.0 / Math.Max(original.Width, original.Height))
                    Using thumb As New Bitmap(Math.Max(1, CInt(original.Width * scale)), Math.Max(1, CInt(original.Height * scale))),
                        canvas = Graphics.FromImage(thumb), output As New MemoryStream()
                        canvas.Clear(Color.White)
                        canvas.DrawImage(original, 0, 0, thumb.Width, thumb.Height)
                        thumb.Save(output, ImageFormat.Jpeg)
                        person.Avatar = output.ToArray()
                    End Using
                End Using
            Catch ex As Exception When TypeOf ex Is InvalidDataException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is OutOfMemoryException OrElse TypeOf ex Is Runtime.InteropServices.ExternalException
                ModelState.AddModelError("avatar", "Choose an image under 2 MB and 16 megapixels.")
            End Try
        End If
        person.Bio = bio
        If Not ModelState.IsValid Then Return View("Edit", person)
        Db.SaveChanges()
        Return RedirectToAction("Details", New With {.id = id})
    End Function

    Public Function Dashboard() As ActionResult
        Dim owner = Uid
        ViewBag.Created = Db.Rooms.Where(Function(r) r.OwnerId = owner).OrderBy(Function(r) r.Name).ToList()
        ViewBag.Joined = Db.VisibleRooms(owner, Staff).Where(Function(r) r.OwnerId <> owner AndAlso r.Seats.Any(Function(s) s.UserId = owner AndAlso Not s.Pending)).OrderBy(Function(r) r.Name).ToList()
        ViewBag.Posts = Db.Posts.Where(Function(p) p.SenderId = owner).Include("Room").OrderByDescending(Function(p) p.Id).Take(5).ToList()
        ViewBag.Notices = Db.Notices.Where(Function(n) n.RecipientId = owner).OrderByDescending(Function(n) n.Id).Take(5).ToList()
        ViewBag.Unread = Db.Notices.Count(Function(n) n.RecipientId = owner AndAlso Not n.IsRead)
        Return View(Db.Users.Find(owner))
    End Function

    Private Function CanDeleteAccount(id As String) As Boolean
        If id = Uid Then Return True
        If Not Staff Then Return False
        Dim adminRole = Db.Roles.Where(Function(r) r.Name = "Administrators").Select(Function(r) r.Id)
        Return User.IsInRole("Administrators") OrElse Not Db.Users.Any(Function(u) u.Id = id AndAlso u.Roles.Any(Function(r) adminRole.Contains(r.RoleId)))
    End Function

    Public Function Delete(id As String) As ActionResult
        If Not CanDeleteAccount(id) Then Return Denied()
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        Return View("ConfirmDelete", CType(person.Nickname & "'s account, rooms and messages", Object))
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Delete")>
    Public Function DeleteAccount(id As String) As ActionResult
        If Not CanDeleteAccount(id) Then Return Denied()
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        Using tx = Db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable)
            Db.Database.ExecuteSqlCommand("DELETE FROM dbo.Notices WHERE RecipientId=@p0 OR ActorId=@p0 OR RoomId IN (SELECT Id FROM dbo.Rooms WHERE OwnerId=@p0) OR PostId IN (SELECT Id FROM dbo.RoomPosts WHERE SenderId=@p0) OR LetterId IN (SELECT l.Id FROM dbo.Letters l JOIN dbo.Conversations c ON c.Id=l.ConversationId WHERE c.FirstId=@p0 OR c.SecondId=@p0)", id)
            Db.Conversations.RemoveRange(Db.Conversations.Where(Function(c) c.FirstId = id OrElse c.SecondId = id))
            Db.Reactions.RemoveRange(Db.Reactions.Where(Function(r) r.UserId = id))
            Db.Seats.RemoveRange(Db.Seats.Where(Function(s) s.UserId = id))
            Db.Posts.RemoveRange(Db.Posts.Where(Function(p) p.SenderId = id))
            Db.SaveChanges()
            Db.Rooms.RemoveRange(Db.Rooms.Where(Function(r) r.OwnerId = id))
            Db.SaveChanges()
            Db.Database.ExecuteSqlCommand("DELETE FROM dbo.AspNetUserClaims WHERE UserId=@p0; DELETE FROM dbo.AspNetUserLogins WHERE UserId=@p0; DELETE FROM dbo.AspNetUserRoles WHERE UserId=@p0", id)
            Db.Users.Remove(person)
            Db.SaveChanges()
            tx.Commit()
        End Using
        If Uid = id Then Request.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie)
        Return RedirectToAction("Index")
    End Function
End Class

Public Class ProfileSummary
    Public Property Id As String
    Public Property Nickname As String
End Class
