using client.EnergyMeter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace client.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            
        }


    }
}
