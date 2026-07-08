using Microsoft.EntityFrameworkCore;
using  PayGate.Models;
using PayGate.Data;
using Scalar.AspNetCore;

namespace PayGate.Data;

public class AppDbContext:DbContext
{
    
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    
    public DbSet<Payment> Payments { get; set; }
    
}