@ModelType ApplicationUser
@Code
    ViewBag.Title = "Edit profile"
End Code
<h1>Edit @Model.Nickname</h1>@Html.ValidationSummary()
<form method="post" enctype="multipart/form-data">@Html.AntiForgeryToken()
<label>Bio @Html.TextArea("bio",Model.Bio, New With {.maxlength = 500})</label>
<label>Profile picture <input type="file" name="avatar" accept="image/*" /></label>
<label><input type="checkbox" value="true" name="clearAvatar" /> Remove picture</label>
<button>Save profile</button></form>
