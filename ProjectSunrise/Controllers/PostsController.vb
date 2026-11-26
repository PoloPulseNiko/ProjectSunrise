Public Class PostsController
    Inherits CommunityController

    Public Function Edit(id As Integer) As ActionResult
        Dim post = Db.Posts.Find(id)
        If post Is Nothing Then Return HttpNotFound()
        If post.SenderId <> Uid OrElse Not Db.Access(post.Room, Uid, Staff).ReadPosts Then Return Denied()
        Return View(post)
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Edit")>
    Public Function UpdatePost(id As Integer, input As ChatMessageInput) As ActionResult
        Dim body = input.Body
        Dim post = Db.Posts.Find(id)
        If post Is Nothing Then Return HttpNotFound()
        If post.SenderId <> Uid OrElse Not Db.Access(post.Room, Uid, Staff).ReadPosts Then Return Denied()
        If String.IsNullOrWhiteSpace(body) Then Return Invalid("Enter a message.")
        post.Body = body.Trim()
        Db.SaveChanges()
        Return RedirectToAction("Details", "Rooms", New With {.id = post.RoomId})
    End Function

    Public Function Delete(id As Integer) As ActionResult
        Dim post = Db.Posts.Find(id)
        If post Is Nothing Then Return HttpNotFound()
        If post.SenderId <> Uid Then Return Denied()
        Return View("ConfirmDelete", CType("this message", Object))
    End Function

    <HttpPost, ValidateAntiForgeryToken, ActionName("Delete")>
    Public Function DeletePost(id As Integer) As ActionResult
        Dim post = Db.Posts.Find(id)
        If post Is Nothing Then Return HttpNotFound()
        If post.SenderId <> Uid Then Return Denied()
        Dim roomId = post.RoomId
        Db.Notices.RemoveRange(Db.Notices.Where(Function(n) n.PostId = id))
        Db.Posts.Remove(post)
        Db.SaveChanges()
        Return RedirectToAction("Details", "Rooms", New With {.id = roomId})
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function React(id As Integer, kind As String) As ActionResult
        Dim post = Db.Posts.Find(id)
        If post Is Nothing Then Return HttpNotFound()
        If Not Db.Access(post.Room, Uid, Staff).ReadPosts Then Return Denied()
        If Not {"like", "love", "laugh"}.Contains(kind) Then Return Invalid("Unknown reaction.")
        Using tx = Db.Database.BeginTransaction()
            ' Serialize toggles of the same composite key, including a missing key.
            Db.Database.ExecuteSqlCommand(
                "IF EXISTS (SELECT 1 FROM dbo.Reactions WITH (UPDLOCK,HOLDLOCK) WHERE PostId=@p0 AND UserId=@p1 AND Kind=@p2) " &
                "DELETE FROM dbo.Reactions WHERE PostId=@p0 AND UserId=@p1 AND Kind=@p2 " &
                "ELSE INSERT INTO dbo.Reactions(PostId,UserId,Kind) VALUES (@p0,@p1,@p2)", id, Uid, kind)
            tx.Commit()
        End Using
        Return Redirect(Url.Action("Details", "Rooms", New With {.id = post.RoomId}) & "#message-" & id)
    End Function
End Class
