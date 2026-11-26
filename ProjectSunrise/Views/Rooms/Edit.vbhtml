@ModelType Room
@Code
    ViewBag.Title = If(Model.Id = 0, "Create room", "Edit room")
End Code
<h1>@ViewBag.Title</h1>
@Html.ValidationSummary()
<form method="post" action="@Url.Action("Save")">
@Html.AntiForgeryToken() @Html.Hidden("id", Model.Id)
@if Model.Id = 0 Then
    @<label>Room name @Html.TextBox("name", Model.Name, New With {.maxlength = 100, .required = "required"})</label>
Else
    @<h2>@Model.Name</h2>
End If
<label>Description @Html.TextArea("description", Model.Description, New With {.maxlength = 500, .required = "required"})</label>
<label>Category @Html.DropDownList("categoryId", New SelectList(ViewBag.Categories, "Id", "Name", Model.CategoryId), "Uncategorised")</label>
<label>Tags @Html.ListBox("tagIds", New MultiSelectList(ViewBag.Tags, "Id", "Name", Model.Tags.Select(Function(t) t.Id)))</label>
<label>Joining @Html.DropDownList("joining", New SelectList({"open","invite"},Model.Joining))</label>
@If User.IsInRole("Staff") Then
    @<label>Visibility @Html.DropDownList("visibility", New SelectList({"public","private","staff"}, Model.Visibility))</label>
    @<label>Who can post @Html.DropDownList("posting", New SelectList({"members","moderators","staff"}, Model.Posting))</label>
End If
<button>Save room</button>
</form>
