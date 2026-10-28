using System;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EqDemo.Security
{
    /// <summary>
    /// Restricts an action to authenticated users in the role configured by the
    /// "EqProxy:QueryEditorRole" app setting. Denies everyone while it is empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class QueryEditorAuthorizeAttribute : AuthorizeAttribute
    {
        public const string QueryEditorRoleSetting = "EqProxy:QueryEditorRole";

        public static bool IsQueryEditor(HttpContextBase httpContext)
        {
            var role = ConfigurationManager.AppSettings[QueryEditorRoleSetting];
            var user = httpContext.User;

            return !string.IsNullOrWhiteSpace(role)
                && user != null
                && user.Identity.IsAuthenticated
                && user.IsInRole(role);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return IsQueryEditor(httpContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User?.Identity.IsAuthenticated == true) {
                filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
