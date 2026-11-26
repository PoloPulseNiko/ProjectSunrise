@ModelType List(Of ApplicationUser)
@Code
    ViewBag.Title = "Administration"
End Code
<h1>Administration</h1>
<ul>
@For Each name In AdminTable.Tables.Keys
    @<li>@Html.ActionLink(name, "Records", New With {.collection = name})</li>
Next
</ul>
<h2>Accounts and profiles</h2><p>@Html.ActionLink("Create account", "Account")</p>
<form method="get"><label>Search accounts @Html.TextBox("q",Request("q"))</label><button>Search</button></form>
<table><tr><th>Username</th><th>Profile</th><th>User ID</th></tr>
@For Each person In Model
    @<tr><td>@Html.ActionLink(person.UserName, "Account", New With {.id = person.Id})</td><td>@Html.ActionLink(person.Nickname,"Details","Profiles",New With {.id = person.Id},Nothing)</td><td>@person.Id</td></tr>
Next
</table>
<h2>Account roles</h2>
<form method="post" action="@Url.Action("AccountRole")">@Html.AntiForgeryToken()
<label>Account @Html.DropDownList("id", New SelectList(Model,"Id","UserName"))</label>
<label>Role @Html.DropDownList("role",New SelectList({"Staff","Administrators","Moderators","Room Managers"}))</label>
<label>Change <select name="enabled"><option value="true">Grant</option><option value="false">Revoke</option></select></label><button>Save role</button></form>
<h2>Rooms</h2><p>@Html.ActionLink("Create room", "Edit", "Rooms")</p><ul>
@For Each room As Room In ViewBag.Rooms
    @<li>@room.Id &middot; @Html.ActionLink(room.Name,"Details","Rooms",New With {.id = room.Id},Nothing)</li>
Next
</ul>
