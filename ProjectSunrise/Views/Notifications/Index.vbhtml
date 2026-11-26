@ModelType List(Of Notice)
@Code
    ViewBag.Title = "Notifications"
End Code
<h1>Notifications</h1><p>@ViewBag.Unread unread</p>
<form method="get" class="filters"><label>Type @Html.DropDownList("kind",New SelectList({"message","room","invite","direct_message"},Request("kind")), "All types")</label>
<label>@Html.CheckBox("unread",String.Equals(Request("unread"),"true",StringComparison.OrdinalIgnoreCase)) Unread only</label><button>Filter</button></form>
<form method="post" action="@Url.Action("MarkAll")">@Html.AntiForgeryToken()<button>Mark all as read</button></form>
@For Each n In Model
    @<article><a href="@n.TargetUrl()">@n.Body</a> <small>@n.Created.ToString("yyyy-MM-dd HH:mm") UTC</small>
    @If Not n.IsRead Then
        @<form class="inline" method="post" action="@Url.Action("MarkRead",New With {.id = n.Id})">@Html.AntiForgeryToken()<button>Mark as read</button></form>
    End If
    </article>
Next
