Imports System.ComponentModel
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Validation
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin

<Authorize(Roles:="Administrators")>
Public Class AdminController
    Inherits CommunityController

    Public Function Index(q As String) As ActionResult
        Dim accounts = Db.Users.AsQueryable()
        If Not String.IsNullOrWhiteSpace(q) Then accounts = accounts.Where(Function(u) u.UserName.Contains(q) OrElse u.Nickname.Contains(q))
        ViewBag.Rooms = Db.Rooms.OrderBy(Function(r) r.Name).ToList()
        Return View(accounts.OrderBy(Function(u) u.UserName).Take(50).ToList())
    End Function

    Public Function Account(id As String) As ActionResult
        If String.IsNullOrEmpty(id) Then Return View(New AdminAccountInput())
        Dim person = Db.Users.Find(id)
        If person Is Nothing Then Return HttpNotFound()
        Return View(New AdminAccountInput With {.Id = person.Id, .Username = person.UserName, .Email = person.Email,
            .Nickname = person.Nickname, .Disabled = person.LockoutEndDateUtc.GetValueOrDefault() > DateTime.UtcNow.AddYears(1)})
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Account")>
    Public Function SaveAccount(input As AdminAccountInput) As ActionResult
        Dim create = String.IsNullOrEmpty(input.Id)
        If create AndAlso String.IsNullOrEmpty(input.NewPassword) Then ModelState.AddModelError("NewPassword", "Enter a password for the new account.")
        If String.Equals(input.Id, Uid, StringComparison.OrdinalIgnoreCase) AndAlso input.Disabled Then ModelState.AddModelError("Disabled", "You cannot disable your own sign-in.")
        If Not ModelState.IsValid Then Return View("Account", input)
        Dim context = HttpContext.GetOwinContext()
        Dim manager = context.GetUserManager(Of ApplicationUserManager)()
        Dim person = If(create, New ApplicationUser(), manager.FindById(input.Id))
        If person Is Nothing Then Return HttpNotFound()
        Using tx = context.Get(Of ApplicationDbContext)().Database.BeginTransaction()
            person.UserName = input.Username
            person.Email = input.Email
            person.Nickname = input.Nickname.Trim()
            person.LockoutEnabled = True
            If input.Disabled Then
                person.LockoutEndDateUtc = DateTime.UtcNow.AddYears(100)
            ElseIf person.LockoutEndDateUtc.GetValueOrDefault() > DateTime.UtcNow.AddYears(1) Then
                person.LockoutEndDateUtc = Nothing
            End If
            Try
                Dim result = If(create, manager.Create(person, input.NewPassword), manager.Update(person))
                If result.Succeeded AndAlso Not create AndAlso Not String.IsNullOrEmpty(input.NewPassword) Then
                    result = manager.ResetPassword(person.Id, manager.GeneratePasswordResetToken(person.Id), input.NewPassword)
                End If
                If result.Succeeded AndAlso Not create Then result = manager.UpdateSecurityStamp(person.Id)
                If Not result.Succeeded Then
                    For Each issue In result.Errors
                        ModelState.AddModelError("", issue)
                    Next
                    Return View("Account", input)
                End If
                tx.Commit()
            Catch ex As DbUpdateException
                ModelState.AddModelError("", "That username, nickname or email is already in use.")
                Return View("Account", input)
            End Try
        End Using
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function AccountRole(id As String, role As String, enabled As Boolean) As ActionResult
        If String.Equals(id, Uid, StringComparison.OrdinalIgnoreCase) OrElse Not {"Staff","Administrators","Moderators","Room Managers"}.Contains(role) Then Return Invalid("Choose another account and a recognised role.")
        Dim manager = HttpContext.GetOwinContext().GetUserManager(Of ApplicationUserManager)()
        If manager.FindById(id) Is Nothing Then Return HttpNotFound()
        If manager.IsInRole(id, role) <> enabled Then
            Dim result = If(enabled, manager.AddToRole(id, role), manager.RemoveFromRole(id, role))
            If Not result.Succeeded Then Return Invalid("The role could not be changed.")
            manager.UpdateSecurityStamp(id)
        End If
        Return RedirectToAction("Index")
    End Function

    Private Function Table(collection As String) As AdminTable
        Dim result As AdminTable = Nothing
        AdminTable.Tables.TryGetValue(If(collection,""), result)
        ViewBag.Collection = collection
        ViewBag.Table = result
        Return result
    End Function

    Public Function Records(collection As String, Optional page As Integer = 1) As ActionResult
        Dim descriptor = Table(collection)
        If descriptor Is Nothing Then Return HttpNotFound()
        If page < 1 OrElse page > 1000000 Then Return Invalid("Invalid page.")
        ViewBag.Page = page
        Return View(descriptor.Rows(Db, page))
    End Function

    Public Function Record(collection As String, key As String) As ActionResult
        Dim descriptor = Table(collection)
        If descriptor Is Nothing Then Return HttpNotFound()
        Dim row = If(key Is Nothing, Activator.CreateInstance(descriptor.EntityType), descriptor.Find(Db, key))
        If row Is Nothing Then Return HttpNotFound()
        ViewBag.Key = key
        Return View(row)
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Record")>
    Public Function SaveRecord(collection As String, key As String) As ActionResult
        Dim descriptor = Table(collection)
        If descriptor Is Nothing Then Return HttpNotFound()
        Dim row = If(key Is Nothing, Activator.CreateInstance(descriptor.EntityType), descriptor.Find(Db, key))
        If row Is Nothing Then Return HttpNotFound()
        ViewBag.Key = key
        For Each name In descriptor.Fields
            If key IsNot Nothing AndAlso descriptor.Keys.Contains(name) Then Continue For
            Dim prop = descriptor.EntityType.GetProperty(name)
            Dim value = Request.Form("field_" & name)
            Try
                If prop.PropertyType = GetType(String) Then
                    prop.SetValue(row, If(value, "").Trim())
                Else
                    prop.SetValue(row, TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(If(value, "")))
                End If
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is NotSupportedException
                ModelState.AddModelError("", "Invalid value for " & name & ".")
            End Try
        Next
        If TypeOf row Is RoomSeat Then
            Dim seat = DirectCast(row, RoomSeat)
            If seat.Pending AndAlso Db.Rooms.Any(Function(r) r.Id = seat.RoomId AndAlso r.OwnerId = seat.UserId) Then ModelState.AddModelError("", "A room owner must remain a member.")
        ElseIf TypeOf row Is Reaction Then
            If Not {"like","love","laugh"}.Contains(DirectCast(row, Reaction).Kind) Then ModelState.AddModelError("", "Choose like, love or laugh.")
        ElseIf TypeOf row Is Conversation Then
            Dim pair = DirectCast(row, Conversation)
            If pair.FirstId = pair.SecondId Then ModelState.AddModelError("", "Choose two different people.")
            If String.CompareOrdinal(pair.FirstId, pair.SecondId) > 0 Then
                Dim first = pair.FirstId
                pair.FirstId = pair.SecondId
                pair.SecondId = first
            End If
            If key IsNot Nothing AndAlso Db.Letters.Any(Function(l) l.ConversationId = pair.Id AndAlso l.SenderId <> pair.FirstId AndAlso l.SenderId <> pair.SecondId) Then ModelState.AddModelError("", "Existing messages must belong to the participants.")
        ElseIf TypeOf row Is Letter Then
            Dim letter = DirectCast(row, Letter)
            If Not Db.Conversations.Any(Function(c) c.Id = letter.ConversationId AndAlso (c.FirstId = letter.SenderId OrElse c.SecondId = letter.SenderId)) Then ModelState.AddModelError("", "Choose a sender in this conversation.")
        ElseIf TypeOf row Is Notice Then
            Dim n = DirectCast(row, Notice)
            If Not {"message","room","invite","direct_message"}.Contains(n.Kind) Then ModelState.AddModelError("", "Choose a valid notification type.")
            If n.PostId.HasValue AndAlso Not Db.Posts.Any(Function(p) p.Id = n.PostId AndAlso p.RoomId = n.RoomId) Then ModelState.AddModelError("", "Choose a message from the target room.")
            If n.LetterId.HasValue AndAlso (n.RoomId.HasValue OrElse Not Db.Letters.Any(Function(l) l.Id = n.LetterId AndAlso (l.Conversation.FirstId = n.RecipientId OrElse l.Conversation.SecondId = n.RecipientId))) Then ModelState.AddModelError("", "Choose a private message for this recipient.")
        End If
        If Not ModelState.IsValid Then Return View("Record", row)
        If key Is Nothing Then Db.Set(descriptor.EntityType).Add(row)
        Try
            Db.SaveChanges()
        Catch ex As DbEntityValidationException
            For Each issue In ex.EntityValidationErrors.SelectMany(Function(e) e.ValidationErrors)
                ModelState.AddModelError("", issue.ErrorMessage)
            Next
            Return View("Record", row)
        Catch ex As DbUpdateException
            ModelState.AddModelError("", "Check that referenced records exist and unique values are available.")
            Return View("Record", row)
        End Try
        Return RedirectToAction("Records", New With {.collection = collection})
    End Function

    Public Function Remove(collection As String, key As String) As ActionResult
        Dim descriptor = Table(collection)
        If descriptor Is Nothing OrElse descriptor.Find(Db, key) Is Nothing Then Return HttpNotFound()
        Return View("ConfirmDelete", CType(collection & " record " & key, Object))
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Remove")>
    Public Function RemoveRecord(collection As String, key As String) As ActionResult
        Dim descriptor = Table(collection)
        If descriptor Is Nothing Then Return HttpNotFound()
        Dim row = descriptor.Find(Db, key)
        If row Is Nothing Then Return HttpNotFound()
        Dim seat = TryCast(row, RoomSeat)
        If seat IsNot Nothing AndAlso seat.UserId = seat.Room.OwnerId Then Return Invalid("The room owner must remain a member.")
        Using tx = Db.Database.BeginTransaction()
            Select Case collection.ToLowerInvariant()
                Case "messages"
                    Db.Database.ExecuteSqlCommand("DELETE FROM dbo.Notices WHERE PostId=@p0", DirectCast(row, RoomPost).Id)
                Case "categories"
                    Db.Database.ExecuteSqlCommand("UPDATE dbo.Rooms SET CategoryId=NULL WHERE CategoryId=@p0", DirectCast(row, Category).Id)
                Case "direct-messages"
                    Db.Database.ExecuteSqlCommand("DELETE FROM dbo.Notices WHERE LetterId=@p0", DirectCast(row, Letter).Id)
                Case "conversations"
                    Db.Database.ExecuteSqlCommand("DELETE FROM dbo.Notices WHERE LetterId IN (SELECT Id FROM dbo.Letters WHERE ConversationId=@p0)", DirectCast(row, Conversation).Id)
            End Select
            Db.Set(descriptor.EntityType).Remove(row)
            Db.SaveChanges()
            tx.Commit()
        End Using
        Return RedirectToAction("Records", New With {.collection = collection})
    End Function
End Class
