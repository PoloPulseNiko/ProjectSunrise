@ModelType List(Of Object)
@Code
    Dim descriptor As AdminTable = ViewBag.Table
    Dim collection As String = ViewBag.Collection
    Dim pageNumber As Integer = ViewBag.Page
    ViewBag.Title = collection
End Code
<h1>@collection</h1><p>@Html.ActionLink("Administration", "Index") &middot; @Html.ActionLink("Add record","Record",New With {.collection = collection})</p>
<div class="scroll"><table><thead><tr><th>Key</th>
@For Each name In descriptor.Fields
    @<th>@name</th>
Next
</tr></thead><tbody>
@For Each row In Model
    @<tr><td>@Html.ActionLink(descriptor.KeyOf(row),"Record",New With {.collection = collection, .key = descriptor.KeyOf(row)})</td>
    @For Each name In descriptor.Fields
        @<td>@descriptor.EntityType.GetProperty(name).GetValue(row)</td>
    Next
    </tr>
Next
</tbody></table></div>
@If pageNumber > 1 Then
    @Html.ActionLink("Previous","Records",New With {.collection = collection, .page = pageNumber - 1})
End If
@If Model.Count = 50 Then
    @Html.ActionLink("Next","Records",New With {.collection = collection, .page = pageNumber + 1})
End If
