Imports System.Data.Entity

<Authorize(Roles:="Staff")>
Public Class StaffController
    Inherits CommunityController

    Public Function Index() As ActionResult
        ViewBag.Totals = New Dictionary(Of String, Integer) From {
            {"Accounts", Db.Users.Count()}, {"Profiles", Db.Users.Count()}, {"Rooms", Db.Rooms.Count()},
            {"Categories", Db.Categories.Count()}, {"Memberships", Db.Seats.Count(Function(s) Not s.Pending)},
            {"Room messages", Db.Posts.Count()}, {"Reactions", Db.Reactions.Count()},
            {"Notifications", Db.Notices.Count()}, {"Unread notifications", Db.Notices.Count(Function(n) Not n.IsRead)}}
        ViewBag.People = Db.Users.OrderByDescending(Function(u) u.Joined).Take(6).ToList()
        ViewBag.Posts = Db.Posts.Include("Room").Include("Sender").OrderByDescending(Function(p) p.Id).Take(5).ToList()
        ViewBag.Notices = Db.Notices.Include("Recipient").OrderByDescending(Function(n) n.Id).Take(5).ToList()
        Return View(Db.Rooms.OrderBy(Function(r) r.Name).ToList())
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function AccessPolicy(room As Integer, visibility As String, posting_policy As String, join_policy As String) As ActionResult
        Dim target = Db.Rooms.Find(room)
        If target Is Nothing Then Return HttpNotFound()
        If Not {"public","private","staff"}.Contains(visibility) OrElse Not {"members","moderators","staff"}.Contains(posting_policy) OrElse Not {"open","invite"}.Contains(join_policy) Then Return Invalid("Unknown room policy.")
        target.Visibility = visibility
        target.Posting = posting_policy
        target.Joining = join_policy
        Db.SaveChanges()
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Broadcast(id As Integer, body As String) As ActionResult
        Dim room = Db.Rooms.Find(id)
        If room Is Nothing Then Return HttpNotFound()
        body = If(body, "").Trim()
        If body.Length < 10 OrElse body.Length > 255 Then Return Invalid("Use 10 to 255 characters.")
        Db.NotifyMembers(room, Uid, "room", body)
        Db.SaveChanges()
        TempData("Feedback") = "Broadcast sent to room members."
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Topic(kind As String, name As String) As ActionResult
        name = If(name, "").Trim()
        If kind = "category" AndAlso name.Length > 0 AndAlso name.Length <= 40 Then
            If Not Db.Categories.Any(Function(c) c.Name = name) Then Db.Categories.Add(New Category With {.Name = name})
        ElseIf kind = "tag" AndAlso name.Length > 0 AndAlso name.Length <= 30 Then
            If Not Db.Tags.Any(Function(t) t.Name = name) Then Db.Tags.Add(New Tag With {.Name = name})
        Else
            Return Invalid("Enter a valid category or tag name.")
        End If
        Db.SaveChanges()
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Moderator(id As Integer, nickname As String, enabled As Boolean) As ActionResult
        Dim seat = Db.Seats.SingleOrDefault(Function(s) s.RoomId = id AndAlso Not s.Pending AndAlso s.User.Nickname = nickname)
        If seat Is Nothing Then Return HttpNotFound()
        seat.Moderator = enabled
        Db.SaveChanges()
        Return RedirectToAction("Index")
    End Function
End Class
