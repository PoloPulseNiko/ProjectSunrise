Imports System.Data.Entity
Imports Microsoft.AspNet.Identity.EntityFramework

Public Class ChatInitializer
    Inherits CreateDatabaseIfNotExists(Of ApplicationDbContext)

    Protected Overrides Sub Seed(context As ApplicationDbContext)
        For Each name In {"Staff", "Moderators", "Room Managers", "Administrators"}
            context.Roles.Add(New IdentityRole(name))
        Next
        context.SaveChanges()
    End Sub
End Class
