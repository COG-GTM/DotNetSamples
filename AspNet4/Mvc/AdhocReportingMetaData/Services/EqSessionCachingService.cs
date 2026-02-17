using Microsoft.AspNetCore.Http;

using Korzh.EasyQuery.Services;

namespace EqDemo.Services
{
    public class EqSessionCachingService : IEqCachingService
    {
        protected readonly ISession Session;

        public EqSessionCachingService(IHttpContextAccessor httpContextAccessor)
        {
            Session = httpContextAccessor.HttpContext.Session;
        }

        public string GetValue(string key)
        {
            return Session.GetString(key);
        }

        public void PutValue(string key, string value)
        {
            Session.SetString(key, value);
        }
    }
}
