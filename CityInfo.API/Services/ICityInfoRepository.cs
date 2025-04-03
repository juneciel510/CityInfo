using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
        Task<bool> CityExistsAsync(int cityId);
        Task<bool> SaveAsync();
        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? cityName, string? searchQuery, bool includePointsOfInterest, int pageNumber, int pageSize);

        Task<bool> CityNameMatchesCityId(string? cityName, int cityId);
    }
}
