Imports System.Web.Http
Imports System.Web.Optimization

Public Class MvcApplication
    Inherits System.Web.HttpApplication

    Protected Sub Application_Start()
        System.Data.Entity.Database.SetInitializer(Of ProjectSunrise.ApplicationDbContext)(New ProjectSunrise.ChatInitializer())
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(AddressOf WebApiConfig.Register)
        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters)
        RouteConfig.RegisterRoutes(RouteTable.Routes)
        BundleConfig.RegisterBundles(BundleTable.Bundles)
    End Sub
    Protected Sub Application_EndRequest()
        Dim status = Response.StatusCode
        If status <> 404 AndAlso status <> 500 Then Return
        If Request.Path.IndexOf("/api", StringComparison.OrdinalIgnoreCase) >= 0 Then Return
        If Not String.IsNullOrEmpty(Response.RedirectLocation) Then Return
        If status = 500 AndAlso Not Context.IsCustomErrorEnabled Then Return
        Response.Clear()
        Response.StatusCode = status
        Response.TrySkipIisCustomErrors = True
        Response.ContentType = "text/html; charset=utf-8"
        Dim title = If(status = 404, "Page not found", "Something went wrong")
        Dim detail = If(status = 404, "This page may have moved or been deleted.", "Please try again in a moment.")
        Response.Write("<!DOCTYPE html><html lang=""en""><meta charset=""utf-8""><title>" & title &
            " · NTChat</title><h1>" & title & "</h1><p>" & detail & "</p><a href=""" &
            HttpUtility.HtmlAttributeEncode(VirtualPathUtility.ToAbsolute("~/")) & """>Back to NTChat</a></html>")
    End Sub
End Class
