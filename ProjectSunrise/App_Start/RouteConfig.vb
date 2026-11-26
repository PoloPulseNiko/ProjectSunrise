Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc
Imports System.Web.Routing

Public Module RouteConfig
    Public Sub RegisterRoutes(ByVal routes As RouteCollection)
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}")

        routes.MapRoute("HomeAbout", "about", New With {.controller = "Home", .action = "About"})
        routes.MapRoute("HomeFaq", "faq", New With {.controller = "Home", .action = "Faq"})
        routes.MapRoute("HomeGuidelines", "guidelines", New With {.controller = "Home", .action = "Guidelines"})
        routes.MapRoute("AccountRegister", "accounts/signup", New With {.controller = "Account", .action = "Register"})
        routes.MapRoute("AccountLogin", "accounts/login", New With {.controller = "Account", .action = "Login"})
        routes.MapRoute("AccountLogOff", "accounts/logout", New With {.controller = "Account", .action = "LogOff"})
        routes.MapRoute("ProfilesIndex", "profiles/list", New With {.controller = "Profiles", .action = "Index"})
        routes.MapRoute("ProfilesCreate", "profiles/create", New With {.controller = "Profiles", .action = "Create"})
        routes.MapRoute("ProfilesDashboard", "profiles/dashboard", New With {.controller = "Profiles", .action = "Dashboard"})
        routes.MapRoute("ProfilesEdit", "profiles/{id}/edit", New With {.controller = "Profiles", .action = "Edit"})
        routes.MapRoute("ProfilesDelete", "profiles/{id}/delete", New With {.controller = "Profiles", .action = "Delete"})
        routes.MapRoute("PublicProfile", "profiles/{id}", New With {.controller = "Profiles", .action = "Details"}, New With {.id = "[a-fA-F0-9-]{36}"})
        routes.MapRoute("RoomsCreate", "rooms/create", New With {.controller = "Rooms", .action = "Create"})
        routes.MapRoute("RoomsJoin", "rooms/{id}/join", New With {.controller = "Rooms", .action = "Join"}, New With {.id = "\d+"})
        routes.MapRoute("RoomsLeave", "rooms/{id}/leave", New With {.controller = "Rooms", .action = "Leave"}, New With {.id = "\d+"})
        routes.MapRoute("RoomsInvite", "rooms/{id}/invite", New With {.controller = "Rooms", .action = "Invite"}, New With {.id = "\d+"})
        routes.MapRoute("RoomsEdit", "rooms/{id}/edit", New With {.controller = "Rooms", .action = "Edit"}, New With {.id = "\d+"})
        routes.MapRoute("RoomsDelete", "rooms/{id}/delete", New With {.controller = "Rooms", .action = "Delete"}, New With {.id = "\d+"})
        routes.MapRoute("RoomsDetails", "rooms/{id}", New With {.controller = "Rooms", .action = "Details"}, New With {.id = "\d+"})
        routes.MapRoute("DirectIndex", "messages/direct", New With {.controller = "Direct", .action = "Index"})
        routes.MapRoute("DirectDetails", "messages/direct/{id}", New With {.controller = "Direct", .action = "Details"}, New With {.id = "\d+"})
        routes.MapRoute("PostsEdit", "messages/{id}/edit", New With {.controller = "Posts", .action = "Edit"}, New With {.id = "\d+"})
        routes.MapRoute("PostsDelete", "messages/{id}/delete", New With {.controller = "Posts", .action = "Delete"}, New With {.id = "\d+"})
        routes.MapRoute("PostsReact", "messages/{id}/react/{kind}", New With {.controller = "Posts", .action = "React"}, New With {.id = "\d+"})
        routes.MapRoute("NotificationsMarkRead", "notifications/{id}/read", New With {.controller = "Notifications", .action = "MarkRead"}, New With {.id = "\d+"})
        routes.MapRoute("NotificationsMarkAll", "notifications/read-all", New With {.controller = "Notifications", .action = "MarkAll"})
        routes.MapRoute("StaffIndex", "staff-panel", New With {.controller = "Staff", .action = "Index"})
        routes.MapRoute("StaffAccess", "staff-panel/rooms/access", New With {.controller = "Staff", .action = "AccessPolicy"})
        routes.MapRoute("StaffBroadcast", "staff-panel/broadcast", New With {.controller = "Staff", .action = "Broadcast"})

        routes.MapRoute(
            name:="Default",
            url:="{controller}/{action}/{id}",
            defaults:=New With {.controller = "Home", .action = "Index", .id = UrlParameter.Optional}
        )
    End Sub
End Module