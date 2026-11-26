@ModelType List(Of Room)
@Code
    ViewBag.Title = "Rooms"
End Code
<h1>Rooms</h1>
<p>@Html.ActionLink("Create a room", "Edit", New With {.id = 0})</p>
<form method="get" class="filters">
<label>Search @Html.TextBox("search", Request("search"))</label>
<label>Category @Html.DropDownList("category", New SelectList(ViewBag.Categories,"Id","Name", Request("category")), "All categories")</label>
<label>Tag @Html.DropDownList("tag", New SelectList(ViewBag.Tags,"Id","Name", Request("tag")), "All tags")</label>
<label>Sort @Html.DropDownList("sort", New SelectList({"name","newest"}, Request("sort")))</label>
<button>Filter</button>
</form>
@For Each room In Model
    @<article><h2>@Html.ActionLink(room.Name, "Details", New With {.id = room.Id})</h2>
    <p>@room.Description</p><small>Created by @room.Owner.Nickname &middot; @room.Visibility &middot; @room.Joining</small></article>
Next
@If Model.Count = 0 Then
    @<p>No rooms match.</p>
End If
