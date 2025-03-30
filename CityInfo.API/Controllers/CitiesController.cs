using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

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
       [FromQuery] bool includePointsOfInterest = false)
        {
            var searchResults = await _cityInfoRepository.GetCitiesAsync(name,search, includePointsOfInterest);
            if (searchResults == null || searchResults.Count() == 0)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
                return Ok(_mapper.Map<IEnumerable<CityDto>>(searchResults));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(searchResults));

        }

    }
}
