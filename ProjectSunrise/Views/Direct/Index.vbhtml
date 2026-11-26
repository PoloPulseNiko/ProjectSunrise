@ModelType List(Of ConversationSummary)
@Code
    ViewBag.Title = "Direct messages"
End Code
<h1>Direct messages</h1>
@For Each thread In Model
    @<article>@Html.ActionLink(thread.OtherName, "Details", New With {.id = thread.Id}) <small>@thread.Updated.ToString("yyyy-MM-dd HH:mm") UTC</small><p class="message">@thread.LastBody</p></article>
Next
<h2>Start a conversation</h2>
<form method="get"><label>Find someone @Html.TextBox("q",Request("q"))</label><button>Search</button></form>
@For Each person As ProfileSummary In ViewBag.People
    @<form class="inline" method="post" action="@Url.Action("Start",New With {.id = person.Id})">@Html.AntiForgeryToken()<button>@person.Nickname</button></form>
Next
