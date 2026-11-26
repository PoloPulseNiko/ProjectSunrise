Imports Microsoft.AspNet.Identity

<Authorize>
Public MustInherit Class CommunityController
    Inherits Controller
    Protected ReadOnly Db As New ApplicationDbContext

    Protected ReadOnly Property Uid As String
        Get
            Return User.Identity.GetUserId()
        End Get
    End Property

    Protected ReadOnly Property Staff As Boolean
        Get
            Return User.IsInRole("Staff")
        End Get
    End Property

    Protected Overrides Sub OnResultExecuting(context As ResultExecutingContext)
        MyBase.OnResultExecuting(context)
        If User.Identity.IsAuthenticated AndAlso TypeOf context.Result Is ViewResult Then
            Dim owner = Uid
            ViewBag.UnreadTotal = Db.Notices.Count(Function(n) n.RecipientId = owner AndAlso Not n.IsRead)
        End If
    End Sub

    Protected Function Denied() As ActionResult
        Return New HttpStatusCodeResult(403)
    End Function

    Protected Function Invalid(message As String) As ActionResult
        Return New HttpStatusCodeResult(400, message)
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then Db.Dispose()
        MyBase.Dispose(disposing)
    End Sub
End Class
