using System;
using System.Configuration;
using System.Web.Http;
using PhoneAliasesService.Models;
using PhoneAliasesService.Services;

namespace PhoneAliasesService.Controllers
{
    public class PhoneAliasesController : ApiController
    {
        private const string PhoneAliasesParamErr = "api/v1/PhoneAliases: parameters should include phone and optionally positive page number and positive page size.";

        private static readonly int DefaultPageSize = int.Parse(ConfigurationManager.AppSettings["defaultPageSize"]);


        [HttpPost]
        [Route("api/v1/PhoneAliases", Name = "GetPhoneAliases")]
        public IHttpActionResult GetPhoneAliases(PhoneAliasesParam phoneAliasesParam)
        {
            try
            {
                if (phoneAliasesParam == null) throw new ArgumentException(PhoneAliasesParamErr, nameof(phoneAliasesParam));
                int page = 1;
                bool correctPage = (string.IsNullOrWhiteSpace(phoneAliasesParam.Page)) || (int.TryParse(phoneAliasesParam.Page, out page) && page > 0);
                if (!correctPage) throw new ArgumentException(PhoneAliasesParamErr, nameof(phoneAliasesParam));

                int pageSize = DefaultPageSize;
                bool correctPageSize = (string.IsNullOrWhiteSpace(phoneAliasesParam.PageSize)) || (int.TryParse(phoneAliasesParam.PageSize, out pageSize) && pageSize > 0);
                if (!correctPageSize) throw new ArgumentException(PhoneAliasesParamErr, nameof(phoneAliasesParam));

                var processor = new PhoneAliasesProcessor(pageSize);

                bool isSuccess = processor.GetPhoneAliases(phoneAliasesParam.Phone, page, out var phoneAliasesResult);

                if (isSuccess)
                {
                    return Json(phoneAliasesResult);
                }
                else
                {
                    return BadRequest("Couldn't parse phone number");
                }

            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception ex)
            {
                // Add some logging here
                return BadRequest("Couldn't parse phone number because of incorrect parameters");
            }
        }

    }
}
