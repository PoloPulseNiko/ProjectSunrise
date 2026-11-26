@ModelType AdminAccountInput
@Code
    ViewBag.Title = If(Model.Id Is Nothing, "Create account", "Edit account")
End Code
<h1>@ViewBag.Title</h1>
@Html.ValidationSummary()
<form method="post">@Html.AntiForgeryToken() @Html.HiddenFor(Function(m) m.Id)
<label>Username @Html.TextBoxFor(Function(m) m.Username)</label>
<label>Email @Html.TextBoxFor(Function(m) m.Email)</label>
<label>Nickname @Html.TextBoxFor(Function(m) m.Nickname)</label>
<label>New password @Html.PasswordFor(Function(m) m.NewPassword, New With {.autocomplete = "new-password"})</label>
<p>Existing accounts keep their password when this field is empty.</p>
<label>@Html.CheckBoxFor(Function(m) m.Disabled) Disable sign-in</label>
<button>Save account</button></form>
