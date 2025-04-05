using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [Authorize(Policy = "MustBeFromAntwerp")]
    [ApiController]
    [ApiVersion(2)]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointOfInterestDto> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(
            ILogger<PointOfInterestDto> logger,
            IMailService mailService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger= logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get the points of interests for a certain city.
        /// </summary>
        /// <param name="cityId">The id of the city.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            try
            {
                var cityName=User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;
                if (!await _cityInfoRepository.CityNameMatchesCityId(cityName, cityId))
                {
                    return Forbid();
                }
               

                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }
                var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
           
        }

        /// <summary>
        /// Get a certain point of interest of a certain city.
        /// </summary>
        /// <param name="cityId">The id of the city.</param>
        /// <param name="pointOfInterestId">The id of the point of interest.</param>
        /// <returns></returns>
        [HttpGet("{pointOfInterestId}", Name = "GetPointOfInterest")] 
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId,int pointOfInterestId) ////Dto class that wrapped into the ActionResult, would be shown in the swagger UI
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }
            var pointOfInterest =await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId,pointOfInterestId);
            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
        }

        /// <summary>
        /// Create a certain point of interest of a certain city.
        /// </summary>
        /// <param name="cityId">The id of the city.</param>
        /// <param name="pointOfInterest">The id of the point of interest.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            //check if the input data pass the validation
            //but this check is implemented in the [ApiController] attribute, so can be removed
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest();
            //}
            try {
                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                    return NotFound();
                }

                var pointOfInterestEntity = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

                await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId,pointOfInterestEntity);
                await _cityInfoRepository.SaveAsync();
                var createdPointOfInterest = _mapper.Map<PointOfInterestDto>(pointOfInterestEntity);
                return CreatedAtRoute(
                    "GetPointOfInterest",
                    new {cityId= cityId, pointOfInterestId = createdPointOfInterest.Id },
                    createdPointOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while creating point of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
        }

        /// <summary>
        /// Update a existing point of interest.
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterestId"></param>
        /// <param name="pointOfInterest"></param>
        /// <returns></returns>
        [HttpPut("{pointOfInterestId}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointOfInterestEntity=await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId,pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }
            //update the target data entity(pointOfInterestEntity) with the source data(pointOfInterest)
            _mapper.Map(pointOfInterest, pointOfInterestEntity);
            await _cityInfoRepository.SaveAsync();
            return NoContent();
        }

        /// <summary>
        /// Partically update a existing point of interest.
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterestId"></param>
        /// <param name="patchDocument"></param>
        /// <returns></returns>
        [HttpPatch("{pointOfInterestId}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }



            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            //update the target data entity(pointOfInterestEntity) with the source data(pointOfInterest)
            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            await _cityInfoRepository.SaveAsync();

            return NoContent();
        }

        /// <summary>
        /// Delete a existing point of interest.
        /// </summary>
        /// <param name="cityId"></param>
        /// <param name="pointOfInterestId"></param>
        /// <returns></returns>
        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            //await _cityInfoRepository.DeletePointOfInterestForCityAsync(cityId,pointOfInterestEntity);
            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveAsync();

            _mailService.Send("Point of interest deleted.", 
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} has been deleted.");
            return NoContent();
        }
    }
}
