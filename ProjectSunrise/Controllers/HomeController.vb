Public Class HomeController
    Inherits System.Web.Mvc.Controller

    Function Index() As ActionResult
        Return View()
    End Function

    Function About() As ActionResult
        Return View()
    End Function

    Function Contact() As ActionResult
        Return View("About")
    End Function
    Function Faq() As ActionResult
        Return View()
    End Function

    Function Guidelines() As ActionResult
        Return View()
    End Function
End Class
