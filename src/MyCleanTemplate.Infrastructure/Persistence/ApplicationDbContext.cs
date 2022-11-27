using Microsoft.EntityFrameworkCore;
using MyCleanTemplate.Application.Common.Interfaces;

namespace MyCleanTemplate.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
    }
}
