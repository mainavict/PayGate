using PayGate.Models;
using Microsoft.EntityFrameworkCore;

namespace PayGate.Data;

public class AppDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; }
}