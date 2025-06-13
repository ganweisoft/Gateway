using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace GWDataCenter.Database
{
    interface IDatabase
    {
        ServiceProvider Initialize<T>(string csPWD) where T : DbContext;
    }
}
