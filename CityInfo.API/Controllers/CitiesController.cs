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
    [Route("api/v{version:apiVersion}/cities")]
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

        /// <summary>
        /// Get a specific city by id
        /// </summary>
        /// <param name="cityId">The id of the city to get</param>
        /// <param name="includePointsOfInterest">Whether or not to include the points of interest</param>
        /// <returns>A city with or without points of interest</returns>
        /// <response code="200">Returns the requested city</response>
        [HttpGet("{cityId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCity(int cityId, bool includePointsOfInterest=false)
        {
            var cityEntity =await _cityInfoRepository.GetCityAsync(cityId, includePointsOfInterest);
            if (cityEntity == null)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
                return Ok(_mapper.Map<CityDto>(cityEntity));

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(cityEntity));
        }


        /// <summary>
        /// Get a list of cities with optional filtering, searching, and pagination.
        /// </summary>
        /// <param name="name">The name to filter cities by.</param>
        /// <param name="search">The search term to filter cities by.</param>
        /// <param name="includePointsOfInterest">Whether to include points of interest in the results.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The page size for pagination.</param>
        /// <returns>A list of cities.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCities(
            [FromQuery] string? name = null,
            [FromQuery] string? search = null,
            [FromQuery] bool includePointsOfInterest = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize > maxPageSize)
            {
                pageSize = maxPageSize;
            }
            (var searchResults, var paginationMetadata) = await _cityInfoRepository
                .GetCitiesAsync(name, search, includePointsOfInterest, pageNumber, pageSize);
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
