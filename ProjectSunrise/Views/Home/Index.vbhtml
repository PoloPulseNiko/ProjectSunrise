@Code
    ViewBag.Title = "Home"
End Code
<h1>Welcome to NTChat</h1>
<p>Find a room, join a conversation or catch up with someone privately.</p>
<p>@Html.ActionLink("Browse rooms", "Index", "Rooms") &middot; @Html.ActionLink("Browse profiles", "Index", "Profiles")</p>
@If Request.IsAuthenticated Then
    @<p>@Html.ActionLink("Open your dashboard", "Dashboard", "Profiles")</p>
Else
    @<p>@Html.ActionLink("Create an account", "Register", "Account") or @Html.ActionLink("sign in", "Login", "Account") to join a room.</p>
End If
