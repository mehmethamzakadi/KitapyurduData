using KitapyurduData.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace KitapYurduData.Data;

public class BookLibraryContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=BIDBMHKADI;Initial Catalog=BookLibrary;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Publisher> Publishers { get; set; }
    public DbSet<TransferInfo> TransferInfos { get; set; }


}
