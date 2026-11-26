@ModelType String
@Code
    ViewBag.Title = "Confirm deletion"
End Code
<h1>Confirm deletion</h1><p>Permanently delete @Model?</p>
<form method="post">@Html.AntiForgeryToken()<button>Delete</button></form>
