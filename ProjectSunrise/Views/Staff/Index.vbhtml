@ModelType List(Of Room)
@Code
    ViewBag.Title = "Staff panel"
    Dim totals As Dictionary(Of String,Integer) = ViewBag.Totals
End Code
<h1>Staff panel</h1>
<table><tbody>
@For Each metric In totals
    @<tr><th>@metric.Key</th><td>@metric.Value</td></tr>
Next
</tbody></table>
<h2>Room access</h2><ul>
@For Each room In Model
    @<li>@Html.ActionLink(room.Name, "Edit", "Rooms", New With {.id = room.Id}, Nothing) &middot; @room.Visibility / @room.Posting / @room.Joining</li>
Next
</ul>
<form method="post" action="@Url.Action("AccessPolicy")">@Html.AntiForgeryToken()
<label>Room @Html.DropDownList("room",New SelectList(Model,"Id","Name"))</label>
<label>Visibility @Html.DropDownList("visibility",New SelectList({"public","private","staff"}))</label>
<label>Who can post @Html.DropDownList("posting_policy",New SelectList({"members","moderators","staff"}))</label>
<label>Joining @Html.DropDownList("join_policy",New SelectList({"open","invite"}))</label><button>Update access</button></form>
<h2>Broadcast</h2>
<form method="post" action="@Url.Action("Broadcast")">@Html.AntiForgeryToken()
<label>Room @Html.DropDownList("id",New SelectList(Model,"Id","Name"))</label>
<label>Announcement (10â€“255 characters)<textarea name="body" maxlength="255" required="required"></textarea></label><button>Send broadcast</button></form>
<h2>Room moderators</h2>
<form method="post" action="@Url.Action("Moderator")">@Html.AntiForgeryToken()
<label>Room @Html.DropDownList("id",New SelectList(Model,"Id","Name"))</label>
<label>Member nickname <input name="nickname" required="required" /></label>
<label>Role <select name="enabled"><option value="true">Moderator</option><option value="false">Member</option></select></label><button>Save role</button></form>
<h2>Categories and tags</h2>
<form method="post" action="@Url.Action("Topic")">@Html.AntiForgeryToken()
<label>Type <select name="kind"><option>category</option><option>tag</option></select></label>
<label>Name <input name="name" maxlength="40" required="required" /></label><button>Add</button></form>
<h2>Recent profiles</h2><ul>
@For Each person As ApplicationUser In ViewBag.People
    @<li>@Html.ActionLink(person.Nickname, "Details", "Profiles", New With {.id = person.Id}, Nothing)</li>
Next
</ul><h2>Recent room messages</h2><ul>
@For Each post As RoomPost In ViewBag.Posts
    @<li>@Html.ActionLink(post.Room.Name, "Details", "Rooms", New With {.id = post.RoomId}, Nothing) &middot; @post.Sender.Nickname &middot; @post.Body</li>
Next
</ul><h2>Recent notifications</h2><ul>
@For Each n As Notice In ViewBag.Notices
    @<li>@n.Recipient.Nickname &middot; @n.Body</li>
Next
</ul>
