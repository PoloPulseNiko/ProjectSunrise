@ModelType RoomPost
@Code
    ViewBag.Title = "Edit message"
End Code
<h1>Edit message</h1>
<form method="post">@Html.AntiForgeryToken()<label>Message @Html.TextArea("body", Model.Body)</label><button>Save changes</button></form>
