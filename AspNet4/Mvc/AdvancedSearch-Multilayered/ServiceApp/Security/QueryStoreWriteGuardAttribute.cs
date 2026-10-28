using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EqDemo.Security
{
    /// <summary>
    /// Rejects malformed model/query ids (400) and only allows creating, saving and removing
    /// stored queries when the front app marks the caller as a query editor (403).
    /// The editor header is trusted only because <see cref="ServiceKeyAuthorizeAttribute"/>
    /// has already authenticated the front app.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class QueryStoreWriteGuardAttribute : ActionFilterAttribute
    {
        public const string QueryEditorHeaderName = "X-Eq-Query-Editor";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!IsValidArgument(actionContext, "modelId") || !IsValidArgument(actionContext, "queryId")) {
                actionContext.Response = JsonError(actionContext.Request, HttpStatusCode.BadRequest, "Invalid model or query id");
                return;
            }

            if (IsQueryStoreWrite(actionContext.Request) && !IsQueryEditor(actionContext.Request)) {
                actionContext.Response = JsonError(actionContext.Request, HttpStatusCode.Forbidden, "Forbidden");
            }
        }

        public static bool IsQueryStoreWrite(HttpRequestMessage request)
        {
            var segments = request.RequestUri.AbsolutePath.TrimEnd('/').Split('/');
            if (segments.Length < 3) {
                return false;
            }

            var method = request.Method;

            // POST .../models/{modelId}/queries
            if (method == HttpMethod.Post) {
                return IsSegment(segments[segments.Length - 1], "queries")
                    && IsSegment(segments[segments.Length - 3], "models");
            }

            // PUT|DELETE .../models/{modelId}/queries/{queryId}
            if (method == HttpMethod.Put || method == HttpMethod.Delete) {
                return segments.Length >= 4
                    && IsSegment(segments[segments.Length - 2], "queries")
                    && IsSegment(segments[segments.Length - 4], "models");
            }

            return false;
        }

        private static bool IsValidArgument(HttpActionContext actionContext, string name)
        {
            object value;
            return !actionContext.ActionArguments.TryGetValue(name, out value)
                || StoreIdValidator.IsValid(value as string);
        }

        private static bool IsQueryEditor(HttpRequestMessage request)
        {
            if (!request.Headers.Contains(QueryEditorHeaderName)) {
                return false;
            }

            var value = request.Headers.GetValues(QueryEditorHeaderName).FirstOrDefault();
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSegment(string segment, string expected)
        {
            return string.Equals(segment, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static HttpResponseMessage JsonError(HttpRequestMessage request, HttpStatusCode status, string message)
        {
            var response = request.CreateResponse(status);
            response.Content = new StringContent("{\"error\":\"" + message + "\"}", Encoding.UTF8, "application/json");
            return response;
        }
    }
}
