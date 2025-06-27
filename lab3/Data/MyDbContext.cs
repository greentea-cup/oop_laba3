using Microsoft.EntityFrameworkCore;
using lab3.Models;

namespace lab3.Data;

public class MyDbContext : DbContext {
	public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) {}

	public DbSet<PrinterModel> Printer { get; set; }
	public DbSet<PrintJobModel> PrintJob { get; set; }
}
