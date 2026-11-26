@ModelType Object
@Code
    Dim descriptor As AdminTable = ViewBag.Table
    Dim collection As String = ViewBag.Collection
    Dim rowKey As String = ViewBag.Key
    ViewBag.Title = collection
End Code
<h1>@collection</h1>@Html.ValidationSummary()
<form method="post">@Html.AntiForgeryToken()
@For Each name In descriptor.Fields
    Dim prop = descriptor.EntityType.GetProperty(name)
    Dim value = prop.GetValue(Model)
    If rowKey IsNot Nothing AndAlso descriptor.Keys.Contains(name) Then
        @<p>@name &middot; @value</p>
    ElseIf prop.PropertyType = GetType(Boolean) Then
        @<label>@name @Html.DropDownList("field_" & name,New SelectList({"True","False"},CStr(value)))</label>
    Else
        @<label>@name @Html.TextArea("field_" & name, Convert.ToString(value))</label>
    End If
Next
<button>Save record</button></form>
@If rowKey IsNot Nothing Then
    @Html.ActionLink("Delete record","Remove",New With {.collection = collection, .key = rowKey})
End If
