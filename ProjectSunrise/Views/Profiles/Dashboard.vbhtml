@ModelType ApplicationUser
@Code
    ViewBag.Title = "Dashboard"
End Code
<h1>Dashboard for @Model.Nickname</h1>
<p>@Html.ActionLink("My profile", "Details", New With {.id = Model.Id}) &middot; @Html.ActionLink("Unread notifications (" & CInt(ViewBag.Unread) & ")", "Index", "Notifications", New With {.unread = True}, Nothing)</p>
<h2>Rooms you created</h2><ul>
@For Each room As Room In ViewBag.Created
    @<li>@Html.ActionLink(room.Name, "Details", "Rooms", New With {.id = room.Id}, Nothing)</li>
Next
</ul><h2>Rooms you joined</h2><ul>
@For Each room As Room In ViewBag.Joined
    @<li>@Html.ActionLink(room.Name, "Details", "Rooms", New With {.id = room.Id}, Nothing)</li>
Next
</ul><h2>Your recent messages</h2><ul>
@For Each post As RoomPost In ViewBag.Posts
    @<li><a href="@Url.Action("Details","Rooms",New With {.id = post.RoomId})#message-@post.Id">@post.Room.Name</a> &middot; @post.Body</li>
Next
</ul><h2>Recent notifications</h2><ul>
@For Each n As Notice In ViewBag.Notices
    @<li><a href="@n.TargetUrl()">@n.Body</a></li>
Next
</ul>
