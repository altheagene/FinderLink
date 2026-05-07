using Microsoft.AspNetCore.Mvc;

namespace FinderLink.Controllers
{
    public static class ControllerAuthExtensions
    {
        public static bool IsAdminLoggedIn(this Controller controller)
        {
            return controller.HttpContext.Session.GetInt32("AdminId").HasValue;
        }
    }
}
