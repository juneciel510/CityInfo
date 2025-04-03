using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cities")]
    [ApiVersion(1)]
    [ApiVersion(2)]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest=false)
        {
            var cityEntity =await _cityInfoRepository.GetCityAsync(id,includePointsOfInterest);
            if (cityEntity == null)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
                return Ok(_mapper.Map<CityDto>(cityEntity));

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityEntity));
        }


        [HttpGet]
        public async Task<IActionResult> GetCities(
            [FromQuery] string? name = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includePointsOfInterest = false,
            [FromQuery] int pageNumber=1,
            [FromQuery] int pageSize=10)
        {
            if(pageSize > maxPageSize)
            {
                pageSize=maxPageSize;
            }
            (var searchResults, var paginationMetadata) = await _cityInfoRepository
                .GetCitiesAsync(name,search, includePointsOfInterest,pageNumber,pageSize);
            if (searchResults == null || searchResults.Count() == 0)
            {
                return NotFound();
            }

            Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            if (includePointsOfInterest)
                return Ok(_mapper.Map<IEnumerable<CityDto>>(searchResults));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(searchResults));

        }

    }
}
