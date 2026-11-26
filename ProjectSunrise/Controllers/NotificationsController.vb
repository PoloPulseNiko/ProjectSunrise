Imports System.Data.Entity
Public Class NotificationsController
    Inherits CommunityController

    Public Function Index(kind As String, Optional unread As Boolean = False) As ActionResult
        Dim owner = Uid
        Dim notices = Db.Notices.Where(Function(n) n.RecipientId = owner)
        ViewBag.Unread = notices.Count(Function(n) Not n.IsRead)
        If unread Then notices = notices.Where(Function(n) Not n.IsRead)
        If Not String.IsNullOrEmpty(kind) Then notices = notices.Where(Function(n) n.Kind = kind)
        Return View(notices.Include("Letter").OrderByDescending(Function(n) n.Id).ToList())
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function MarkRead(id As Integer) As ActionResult
        Dim owner = Uid
        Dim n = Db.Notices.SingleOrDefault(Function(x) x.Id = id AndAlso x.RecipientId = owner)
        If n Is Nothing Then Return HttpNotFound()
        n.IsRead = True
        Db.SaveChanges()
        Return RedirectToAction("Index")
    End Function

    <HttpPost, ValidateAntiForgeryToken>
    Public Function MarkAll() As ActionResult
        Db.Database.ExecuteSqlCommand("UPDATE dbo.Notices SET IsRead=1 WHERE RecipientId=@p0 AND IsRead=0", Uid)
        Return RedirectToAction("Index")
    End Function
End Class
