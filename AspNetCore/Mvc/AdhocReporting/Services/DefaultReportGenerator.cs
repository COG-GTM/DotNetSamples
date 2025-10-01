using Microsoft.AspNetCore.Identity;
using EqDemo.Data;

namespace EqDemo.Services
{
    public class DefaultReportGenerator
    {
        private readonly AppDbContext _dbContext;

        public DefaultReportGenerator(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Generate(IdentityUser user)
        {
        }
    }
}
