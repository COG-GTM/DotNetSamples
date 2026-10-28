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
    /// Cross-origin requests are rejected because Windows credentials are sent
    /// automatically by the browser (CSRF).
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

        public static bool IsSameOrigin(HttpRequestBase request)
        {
            var source = request.Headers["Origin"];
            if (string.IsNullOrEmpty(source)) {
                source = request.Headers["Referer"];
            }

            // Modern browsers send Origin on every POST/PUT/DELETE, so a write with
            // neither header is treated as untrusted.
            return !string.IsNullOrEmpty(source)
                && Uri.TryCreate(source, UriKind.Absolute, out var sourceUri)
                && Uri.Compare(sourceUri, request.Url, UriComponents.SchemeAndServer,
                    UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return IsQueryEditor(httpContext) && IsSameOrigin(httpContext.Request);
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
