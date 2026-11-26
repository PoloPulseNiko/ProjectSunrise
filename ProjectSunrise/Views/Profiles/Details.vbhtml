@Imports Microsoft.AspNet.Identity
@ModelType ApplicationUser
@Code
    ViewBag.Title = Model.Nickname
End Code
<h1>@Model.Nickname</h1>
@If Model.Avatar IsNot Nothing Then
    @<img class="avatar" src="@Url.Action("Picture",New With {.id = Model.Id})" alt="Profile picture" />
End If
<p>Member since @Model.Joined.ToString("dd MMM yyyy")</p><p class="message">@Model.Bio</p>
@If User.Identity.GetUserId() = Model.Id OrElse User.IsInRole("Staff") Then
    @Html.ActionLink("Edit profile", "Edit", New With {.id = Model.Id})
    @Html.ActionLink("Delete account", "Delete", New With {.id = Model.Id})
End If
@If Request.IsAuthenticated AndAlso User.Identity.GetUserId() <> Model.Id Then
    @<form method="post" action="@Url.Action("Start","Direct", New With {.id = Model.Id})">@Html.AntiForgeryToken()<button>Send a private message</button></form>
End If
