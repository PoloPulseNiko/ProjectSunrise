Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ProjectSunrise

<TestClass>
Public Class RoomAccessTests
    <TestMethod>
    Public Sub GuestsSeeOnlyPublicRoomMetadata()
        For Each visibility In {"public", "private", "staff"}
            Dim rights As New RoomAccess(New Room With {.Visibility = visibility}, Nothing, False, Nothing)
            Assert.AreEqual(visibility = "public", rights.Visible)
            Assert.IsFalse(rights.Join OrElse rights.ReadPosts OrElse rights.WritePosts OrElse rights.Manage)
        Next
    End Sub

    <TestMethod>
    Public Sub PrivateInvitationAllowsAcceptanceBeforeReading()
        Dim room As New Room With {.OwnerId = "owner", .Visibility = "private", .Joining = "invite"}
        Dim rights As New RoomAccess(room, "guest", False, New RoomSeat With {.Pending = True})
        Assert.IsTrue(rights.Visible AndAlso rights.Join)
        Assert.IsFalse(rights.ReadPosts OrElse rights.WritePosts OrElse rights.Member OrElse rights.Manage)
        Dim accepted As New RoomAccess(room, "guest", False, New RoomSeat())
        Assert.IsTrue(accepted.ReadPosts AndAlso accepted.WritePosts AndAlso accepted.Member)
        Assert.IsFalse(accepted.Join OrElse accepted.Manage)
    End Sub

    <TestMethod>
    Public Sub PrivateUninvitedVisitorCannotJoinAnOpenRoom()
        Dim rights As New RoomAccess(New Room With {.Visibility = "private", .Joining = "open"}, "visitor", False, Nothing)
        Assert.IsFalse(rights.Visible OrElse rights.Join OrElse rights.ReadPosts OrElse rights.WritePosts)
    End Sub

    <TestMethod>
    Public Sub InviteOnlyPublicRoomsExposeMetadata()
        Dim rights As New RoomAccess(New Room With {.Joining = "invite"}, "visitor", False, Nothing)
        Assert.IsTrue(rights.Visible)
        Assert.IsFalse(rights.Join OrElse rights.ReadPosts)
    End Sub

    <TestMethod>
    Public Sub MembershipDoesNotExposeAStaffRoom()
        Dim rights As New RoomAccess(New Room With {.Visibility = "staff"}, "member", False, New RoomSeat())
        Assert.IsFalse(rights.Visible OrElse rights.ReadPosts OrElse rights.WritePosts OrElse rights.Join)
    End Sub

    <TestMethod>
    Public Sub ModeratedRoomsSeparateReadingAndWriting()
        Dim room As New Room With {.OwnerId = "owner", .Posting = "moderators"}
        Dim member As New RoomAccess(room, "reader", False, New RoomSeat())
        Assert.IsTrue(member.ReadPosts)
        Assert.IsFalse(member.WritePosts OrElse member.Manage)
        Dim moderator As New RoomAccess(room, "mod", False, New RoomSeat With {.Moderator = True})
        Assert.IsTrue(moderator.WritePosts)
        Assert.IsFalse(moderator.Manage)
        Assert.IsTrue(New RoomAccess(room, "owner", False, New RoomSeat()).Manage)
    End Sub

    <TestMethod>
    Public Sub StaffOnlyPostingAppliesToOwnersAndModerators()
        Dim room As New Room With {.OwnerId = "owner", .Posting = "staff"}
        Assert.IsFalse(New RoomAccess(room, "owner", False, New RoomSeat With {.Moderator = True}).WritePosts)
        Dim rights As New RoomAccess(room, "staff", True, Nothing)
        Assert.IsTrue(rights.Visible AndAlso rights.ReadPosts AndAlso rights.WritePosts AndAlso rights.Manage)
        Assert.IsFalse(rights.Join)
    End Sub
End Class
