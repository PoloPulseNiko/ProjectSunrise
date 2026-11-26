Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure

Public Class DirectController
    Inherits CommunityController

    Public Function Index(q As String) As ActionResult
        Dim owner = Uid
        Dim users = Db.Users.Where(Function(u) u.Id <> owner)
        If Not String.IsNullOrWhiteSpace(q) Then users = users.Where(Function(u) u.Nickname.Contains(q) OrElse u.UserName.Contains(q))
        ViewBag.People = users.OrderBy(Function(u) u.Nickname).Select(Function(u) New ProfileSummary With {.Id = u.Id, .Nickname = u.Nickname}).Take(12).ToList()
        Return View(Db.Conversations.Where(Function(c) c.FirstId = owner OrElse c.SecondId = owner).
            OrderByDescending(Function(c) c.Updated).Select(Function(c) New ConversationSummary With {
                .Id = c.Id, .Updated = c.Updated, .OtherName = If(c.FirstId = owner, c.Second.Nickname, c.First.Nickname),
                .LastBody = c.Letters.OrderByDescending(Function(l) l.Id).Select(Function(l) l.Body).FirstOrDefault()
            }).ToList())
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Start(id As String) As ActionResult
        Dim target = Db.Users.Where(Function(u) u.Id = id).Select(Function(u) u.Id).SingleOrDefault()
        If target Is Nothing OrElse target = Uid Then Return HttpNotFound()
        Dim ordered = {Uid, target}.OrderBy(Function(s) s, StringComparer.Ordinal).ToArray()
        Dim firstId = ordered(0), secondId = ordered(1)
        Dim pair = Db.Conversations.Where(Function(c) c.FirstId = firstId AndAlso c.SecondId = secondId)
        Dim conversation = pair.FirstOrDefault()
        If conversation Is Nothing Then
            conversation = Db.Conversations.Add(New Conversation With {.FirstId = firstId, .SecondId = secondId})
            Try
                Db.SaveChanges()
            Catch ex As DbUpdateException
                Db.Entry(conversation).State = EntityState.Detached
                conversation = pair.FirstOrDefault()
                If conversation Is Nothing Then Throw
            End Try
        End If
        Return RedirectToAction("Details", New With {.id = conversation.Id})
    End Function

    Public Function Details(id As Integer) As ActionResult
        Dim owner = Uid
        Dim conversation = Db.Conversations.SingleOrDefault(Function(c) c.Id = id AndAlso (c.FirstId = owner OrElse c.SecondId = owner))
        If conversation Is Nothing Then Return HttpNotFound()
        ViewBag.OtherName = If(conversation.FirstId = owner, conversation.Second.Nickname, conversation.First.Nickname)
        ViewBag.Letters = Db.Letters.Where(Function(l) l.ConversationId = id).Include("Sender").OrderBy(Function(l) l.Id).ToList()
        Return View(conversation)
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function Send(id As Integer, input As ChatMessageInput) As ActionResult
        Dim body = input.Body
        Dim owner = Uid
        Dim conversation = Db.Conversations.SingleOrDefault(Function(c) c.Id = id AndAlso (c.FirstId = owner OrElse c.SecondId = owner))
        If conversation Is Nothing Then Return HttpNotFound()
        If String.IsNullOrWhiteSpace(body) Then Return Invalid("Enter a message.")
        Dim recipient = If(conversation.FirstId = owner, conversation.SecondId, conversation.FirstId)
        Dim letter = Db.Letters.Add(New Letter With {.Conversation = conversation, .SenderId = owner, .Body = body.Trim()})
        conversation.Updated = DateTime.UtcNow
        Db.Notices.Add(New Notice With {.ActorId = owner, .RecipientId = recipient, .Kind = "direct_message", .Body = Db.Users.Find(owner).Nickname & " sent you a private message.", .Letter = letter})
        Db.SaveChanges()
        Return Redirect(Url.Action("Details", New With {.id = id}) & "#direct-message-" & letter.Id)
    End Function
End Class

Public Class ConversationSummary
    Public Property Id As Integer
    Public Property OtherName As String
    Public Property Updated As DateTime
    Public Property LastBody As String
End Class
