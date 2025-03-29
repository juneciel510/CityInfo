using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();
            var results = cityEntities.Select(c => new CityWithoutPointsOfInterestDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                //PointsOfInterest = c.PointsOfInterest.Select(p => new PointOfInterestDto
                //{
                //    Id = p.Id,
                //    Name = p.Name,
                //    Description = p.Description
                //}).ToList()
            });

            return Ok(results);
        }

        //[HttpGet("{id}")]
        //public ActionResult<CityDto> GetCity(int id)
        //{
        //    var cityToReturn = _citiesDataStore.Cities.FirstOrDefault(c => c.Id == id);

        //    if (cityToReturn == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(cityToReturn);
        //}
    }
}
