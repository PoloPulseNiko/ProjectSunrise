@ModelType Conversation
@Code
    ViewBag.Title = "Direct messages"
End Code
<h1>Conversation with @ViewBag.OtherName</h1>
@For Each letter As Letter In ViewBag.Letters
    @<article id="direct-message-@letter.Id"><strong>@letter.Sender.Nickname</strong> <small>@letter.Created.ToString("yyyy-MM-dd HH:mm") UTC</small><p class="message">@letter.Body</p></article>
Next
<form method="post" action="@Url.Action("Send",New With {.id = Model.Id})">@Html.AntiForgeryToken()
<label>Private message <textarea id="compose" name="body" required="required"></textarea></label><button>Send</button></form>
<script src="@Url.Content("~/Scripts/chat.js")"></script>
