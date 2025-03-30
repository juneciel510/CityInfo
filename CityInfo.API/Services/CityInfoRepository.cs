using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context=context?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c=>c.Name).ToListAsync();
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if(includePointsOfInterest)
            {
                return await _context.Cities
                    .Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId)
                    .FirstOrDefaultAsync();
            }
            return await _context.Cities
                .Where(c => c.Id == cityId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<City>> GetCitiesAsync(string? cityName, bool includePointsOfInterest)
        {
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return await GetCitiesAsync();
            }

            cityName=cityName.Trim();
            if (includePointsOfInterest)
            {
                return await _context.Cities
                    .Include(c => c.PointsOfInterest)
                    .Where(c => c.Name.Contains(cityName))
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            return await _context.Cities
                .Where(c => c.Name.Contains(cityName))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointsOfInterest.Where(p=>p.CityId==cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            //the line below is not an I/O operation, thus no need to use async
            city?.PointsOfInterest.Add(pointOfInterest);
        }

        //this approach also works for deleting a point of interest,but here we choose another one below
        //public async Task DeletePointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        //{
        //    var city = await GetCityAsync(cityId, false);
        //    //the line below is not an I/O operation, thus no need to use async
        //    city?.PointsOfInterest.Remove(pointOfInterest);
        //}

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> SaveAsync()
        {

            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
