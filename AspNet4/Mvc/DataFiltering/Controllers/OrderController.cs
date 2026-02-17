using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Korzh.EasyQuery.Services;
using Korzh.EasyQuery.Linq;

using EqDemo.Models;

namespace EqDemo.Controllers
{
    [Route("data-filtering")]
    public class OrderController : Controller
    {
        EasyQueryManagerLinq<Order> _eqManager;
        ApplicationDbContext _dbContext;

        public OrderController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            var options = new EasyQueryOptions();
            options.UseEntity((_) => _dbContext.Orders);

            _eqManager = new EasyQueryManagerLinq<Order>(options);
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View("Orders");
        }

        [HttpGet]
        [Route("models/{modelId}")]
        public async Task<IActionResult> GetModelAsync(string modelId)
        {
            var model = await _eqManager.GetModelAsync(modelId);
            return Ok(new { model });
        }

        [HttpGet]
        [Route("models/{modelId}/valuelists/{editorId}")]
        public async Task<IActionResult> GetList(string modelId, string editorId)
        {
            var list = await _eqManager.GetValueListAsync(modelId, editorId);
            return Ok(new { values = list });
        }

        [HttpPost]
        [Route("models/{modelId}/fetch")]
        public async Task<IActionResult> ApplyQueryFilter(string modelId)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            await _eqManager.ReadRequestContentFromStringAsync(modelId, body);

            var orders = _dbContext.Orders
                      .Include(o => o.Customer)
                      .Include(o => o.Employee)
                      .AsQueryable();

            var clientData = (IDictionary<string, object>)_eqManager.ClientData;
            var text = clientData != null && clientData.TryGetValue("text", out object value)
                ? (string)value : "";

            if (!string.IsNullOrEmpty(text)) {
                var options = new FullTextSearchOptions
                {
                    Depth = 2,
                    Filter = (propInfo) =>
                    {
                        if (propInfo.DeclaringType == typeof(Order)) {
                            return propInfo.PropertyType == typeof(Customer) || propInfo.PropertyType == typeof(Employee);
                        }
                        else if (propInfo.PropertyType == typeof(string)) {
                            if (propInfo.DeclaringType == typeof(Customer)) {
                                return propInfo.Name == "CompanyName" || propInfo.Name == "Country";
                            }
                            else if (propInfo.DeclaringType == typeof(Employee)) {
                                return propInfo.Name == "FirstName" || propInfo.Name == "LastName";
                            }
                        }
                        return false;
                    }
                };

                orders = orders.FullTextSearchQuery<Order>(text, options);
            }

            orders = orders
             .DynamicQuery<Order>(_eqManager.Query);

            var list = orders.ToPagedList(_eqManager.Chunk.Page, 15);
            ViewData["Text"] = text;

            return PartialView("_OrderListPartial", list);
        }
    }
}
