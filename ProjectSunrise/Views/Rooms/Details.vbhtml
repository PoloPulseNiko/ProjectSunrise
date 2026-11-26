@Imports Microsoft.AspNet.Identity
@ModelType Room
@Code
    ViewBag.Title = Model.Name
    Dim rights As RoomAccess = ViewBag.Rights
End Code
<h1>@Model.Name</h1><p>@Model.Description</p>
@If Model.Owner.Avatar IsNot Nothing Then
    @<img width="42" height="42" alt="" src="@Url.Action("Picture","Profiles",New With {.id = Model.OwnerId})" />
End If
<p>Created by @Html.ActionLink(Model.Owner.Nickname, "Details", "Profiles", New With {.id = Model.OwnerId}, Nothing)
 &middot; @Model.Visibility &middot; @Model.Joining</p>
@if Model.Category IsNot Nothing Then
    @<p>Category &middot; @Model.Category.Name</p>
End If
@If Model.Tags.Any() Then
    @<p>Tags &middot; @String.Join(", ", Model.Tags.Select(Function(t) t.Name))</p>
End If
@If rights.Manage Then
    Dim invitations As List(Of String) = ViewBag.Invitations
    If invitations.Count > 0 Then
        @<p>Pending invitations &middot; @String.Join(", ", invitations)</p>
    End If
    @<p>@Html.ActionLink("Edit room", "Edit", New With {.id = Model.Id}) &middot; @Html.ActionLink("Delete room", "Delete", New With {.id = Model.Id})</p>
    @<form method="post" action="@Url.Action("Invite",New With {.id = Model.Id})">@Html.AntiForgeryToken()
        <label>Invite by nickname <input name="nickname" required="required" /></label><button>Send invitation</button></form>
End If
@If rights.Join Then
    @<form method="post" action="@Url.Action("Join", New With {.id = Model.Id})">@Html.AntiForgeryToken()<button>Join room</button></form>
ElseIf rights.Member AndAlso Model.OwnerId <> User.Identity.GetUserId() Then
    @<form method="post" action="@Url.Action("Leave", New With {.id = Model.Id})">@Html.AntiForgeryToken()<button>Leave room</button></form>
End If
@If rights.ReadPosts Then
    Dim posts As List(Of RoomPost) = ViewBag.Posts
    @<h2>Conversation</h2>
    @<div id="thread" data-check="@Url.Content("~/rooms/api/" & Model.Id & "/messages?latest=true")" data-last="@(If(posts.Count = 0, 0, posts.Last().Id))">
    @For Each post In posts
        @<article id="message-@post.Id"><strong>@Html.ActionLink(post.Sender.Nickname, "Details", "Profiles", New With {.id = post.SenderId}, Nothing)</strong>
        @If post.Sender.Avatar IsNot Nothing Then
            @<img width="32" height="32" alt="" src="@Url.Action("Picture","Profiles",New With {.id = post.SenderId})" />
        End If
        <small>@post.Created.ToString("yyyy-MM-dd HH:mm") UTC</small>
        <p class="message">@post.Body</p>
        <form class="inline" method="post" action="@Url.Action("React", "Posts", New With {.id = post.Id})">@Html.AntiForgeryToken()
        @For Each kind In {"like", "love", "laugh"}
            Dim count = post.Reactions.Where(Function(r) r.Kind = kind).Count()
            @<button name="kind" value="@kind">@kind (@count)</button>
        Next
        </form>
        @If post.SenderId = User.Identity.GetUserId() Then
            @Html.ActionLink("Edit", "Edit", "Posts", New With {.id = post.Id}, Nothing)
            @Html.ActionLink("Delete", "Delete", "Posts", New With {.id = post.Id}, Nothing)
        End If
        </article>
    Next
    </div>
    If rights.WritePosts Then
        @<form method="post" action="@Url.Action("Send", New With {.id = Model.Id})">@Html.AntiForgeryToken()
        <label>Message <textarea id="compose" name="body" required="required"></textarea></label><button>Send</button></form>
    End If
    @<p><a href="@Url.Action("Details", New With {.id = Model.Id})">Refresh conversation</a></p>
    @<script src="@Url.Content("~/Scripts/chat.js")"></script>
Else
    @<p>Join this room to read messages.</p>
End If
