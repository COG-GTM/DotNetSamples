using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace EqDemo.Security
{
    /// <summary>
    /// Only lets through requests that carry the shared service key configured in
    /// the "EqService:ApiKey" app setting (sent by the front app proxy).
    /// Fails closed when no key is configured.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ServiceKeyAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public const string HeaderName = "X-EqService-Key";
        public const string ApiKeySetting = "EqService:ApiKey";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var expectedKey = ConfigurationManager.AppSettings[ApiKeySetting];

            var providedKey = actionContext.Request.Headers.Contains(HeaderName)
                ? actionContext.Request.Headers.GetValues(HeaderName).FirstOrDefault()
                : null;

            if (!IsAuthorized(expectedKey, providedKey)) {
                var response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                response.Content = new StringContent("{\"error\":\"Unauthorized\"}", Encoding.UTF8, "application/json");
                actionContext.Response = response;
            }
        }

        public static bool IsAuthorized(string expectedKey, string providedKey)
        {
            if (string.IsNullOrWhiteSpace(expectedKey) || string.IsNullOrEmpty(providedKey)) {
                return false;
            }

            var expected = SHA256Hash(expectedKey);
            var provided = SHA256Hash(providedKey);

            var diff = 0;
            for (var i = 0; i < expected.Length; i++) {
                diff |= expected[i] ^ provided[i];
            }

            return diff == 0;
        }

        private static byte[] SHA256Hash(string value)
        {
            using (var sha = SHA256.Create()) {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}
