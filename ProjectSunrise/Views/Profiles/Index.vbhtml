@ModelType List(Of ProfileSummary)
@Code
    ViewBag.Title = "Profiles"
End Code
<h1>Profiles</h1>
<form method="get"><label>Find a profile @Html.TextBox("q", Request("q"))</label><button>Search</button></form>
<ul>
@For Each person In Model
    @<li>@Html.ActionLink(person.Nickname, "Details", New With {.id = person.Id})</li>
Next
</ul>
