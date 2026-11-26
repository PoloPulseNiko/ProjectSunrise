@Imports Microsoft.AspNet.Identity
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@ViewBag.Title Â· NTChat</title>
    <link rel="stylesheet" href="@Url.Content("~/Content/Site.css")" />
</head>
<body>
<header><div class="wrap mast">
    <a class="brand" href="@Url.Action("Index","Home")">NT<span>Chat</span></a>
    <div class="session">
    @If Request.IsAuthenticated Then
        @<a href="@Url.Action("Details","Profiles", New With {.id = User.Identity.GetUserId()})">@User.Identity.Name</a>
        @<form method="post" action="@Url.Action("LogOff","Account")">@Html.AntiForgeryToken()<button>Sign out</button></form>
    Else
        @Html.ActionLink("Sign in", "Login", "Account")
        @Html.ActionLink("Register", "Register", "Account")
    End If
    </div>
</div></header>
<nav><div class="wrap">
    @Html.ActionLink("Rooms", "Index", "Rooms")
    @Html.ActionLink("Profiles", "Index", "Profiles")
    @If Request.IsAuthenticated Then
        @Html.ActionLink("Direct messages", "Index", "Direct")
        @Html.ActionLink("Notifications" & If(ViewBag.UnreadTotal Is Nothing, "", " (" & CStr(ViewBag.UnreadTotal) & ")"), "Index", "Notifications")
        @Html.ActionLink("Dashboard", "Dashboard", "Profiles")
    End If
    @If User.IsInRole("Staff") Then
        @Html.ActionLink("Staff panel", "Index", "Staff")
    End If
    @If User.IsInRole("Administrators") Then
        @Html.ActionLink("Administration", "Index", "Admin")
    End If
</div></nav>
<main class="wrap page">
    @If TempData("Feedback") IsNot Nothing Then
        @<p class="feedback">@TempData("Feedback")</p>
    End If
    @RenderBody()
</main>
<footer class="wrap">NTChat &middot; @Html.ActionLink("About", "About", "Home") &middot; @Html.ActionLink("FAQ", "Faq", "Home") &middot; @Html.ActionLink("Community guidelines", "Guidelines", "Home")</footer>
@If IsSectionDefined("scripts") Then
    @Scripts.Render("~/bundles/jquery")
    @RenderSection("scripts", required:=False)
End If
</body></html>
