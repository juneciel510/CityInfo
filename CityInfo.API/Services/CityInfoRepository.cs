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

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? cityName, string? searchQuery, bool includePointsOfInterest, int pageNumber, int pageSize)
        {
            IQueryable<City> cities = _context.Cities;

            // Apply include if needed
            if (includePointsOfInterest)
            {
                cities = cities.Include(c => c.PointsOfInterest);
            }

            // Apply filtering if parameters provided
            if (!string.IsNullOrWhiteSpace(cityName)) 
            {
                cityName = cityName.Trim();
                cities = cities.Where(c => c.Name.Contains(cityName));
            }

            if(!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                //could be improve by applying full text search using a Search Library (Lucene.NET) public  
                cities = cities.Where(c => c.Name.Contains(searchQuery) || (c.Description!=null &&c.Description.Contains(searchQuery)));
            }

            // Apply pagination
            var totalCities = await cities.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalCities, pageSize, pageNumber);

            //Query is deferred until ToListAsync() is called
            var citiesToReturn = await cities
                .OrderBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (citiesToReturn, paginationMetadata);

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

        //public async Task<IEnumerable<City>> SearchCitiesAsync(string search, bool includePointsOfInterest)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
