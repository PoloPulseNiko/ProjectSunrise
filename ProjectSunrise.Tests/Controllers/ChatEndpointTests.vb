Imports System.Reflection
Imports System.Web.Mvc
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ProjectSunrise

<TestClass>
Public Class ChatEndpointTests
    <TestMethod>
    Public Sub EveryChatPostRequiresAnAntiForgeryToken()
        Dim controllers = {GetType(RoomsController), GetType(PostsController), GetType(DirectController),
            GetType(ProfilesController), GetType(NotificationsController), GetType(StaffController), GetType(AdminController)}
        For Each controller In controllers
            Assert.IsTrue(controller.IsDefined(GetType(AuthorizeAttribute), True), controller.Name)
            For Each action In controller.GetMethods(BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.DeclaredOnly)
                If action.IsDefined(GetType(HttpPostAttribute), False) Then
                    Assert.IsTrue(action.IsDefined(GetType(ValidateAntiForgeryTokenAttribute), False), controller.Name & "." & action.Name)
                    Assert.IsFalse(action.IsDefined(GetType(AllowAnonymousAttribute), False))
                End If
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub DirectMessagesAndNotificationsRequireSignIn()
        For Each controller In {GetType(DirectController), GetType(NotificationsController)}
            Assert.IsTrue(controller.IsDefined(GetType(AuthorizeAttribute), True))
            For Each action In controller.GetMethods(BindingFlags.DeclaredOnly Or BindingFlags.Public Or BindingFlags.Instance)
                Assert.IsFalse(action.IsDefined(GetType(AllowAnonymousAttribute), False), action.Name)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub StaffAndDataAdministrationUseDistinctRoles()
        Dim staff = DirectCast(Attribute.GetCustomAttribute(GetType(StaffController), GetType(AuthorizeAttribute), False), AuthorizeAttribute)
        Dim admin = DirectCast(Attribute.GetCustomAttribute(GetType(AdminController), GetType(AuthorizeAttribute), False), AuthorizeAttribute)
        Assert.AreEqual("Staff", staff.Roles)
        Assert.AreEqual("Administrators", admin.Roles)
    End Sub

    <TestMethod>
    Public Sub AdminFormsExcludeAccountCredentials()
        ' The editable data catalogue is deliberately limited to chat records.
        For Each table In AdminTable.Tables.Values
            Assert.IsFalse(table.Fields.Contains("PasswordHash"))
            Assert.IsFalse(table.Fields.Contains("SecurityStamp"))
        Next
    End Sub
End Class
