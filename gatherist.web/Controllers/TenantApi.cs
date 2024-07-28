using gatherist.web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gatherist.web.Controllers
{
    [Route("api/tenants")]
    [ApiController]
    public class TenantApi(ITenantRepository tenantRepository) 
        : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository =
            tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        
        private async Task<IResult> GetAsyncInternal<T>(T key)
            where T : struct
        {
            var tenant = key switch
            {
                int intKey => await _tenantRepository.GetTenantAsync(intKey),
                Guid guidKey => await _tenantRepository.GetTenantAsync(guidKey),
                _ => null
            };

            return tenant == null ? Results.NotFound() : Results.Ok(tenant);
        }

        [HttpGet("{key:Guid}")]
        public Task<IResult> GetAsync(Guid key)
            => GetAsyncInternal(key);
        
        [HttpGet("{id:int}")]
        public Task<IResult> GetAsync(int id)
            => GetAsyncInternal(id);
    }
}
