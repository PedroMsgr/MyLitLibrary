using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
using MylitLibrary.Models;
using MylitLibrary.Entities;

namespace MylitLibrary.Data
{
    public class LibraryDatacontexContext : DbContext
    {
        public DbSet<UserList> UserList { get; set; }
        public DbSet<UserBooks> UserBooks { get; set; }
        public LibraryDatacontexContext(DbContextOptions<LibraryDatacontexContext> options)
        : base(options)
        {
        }

        public static DbContextOptions<LibraryDatacontexContext> BuildDbContextOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDatacontexContext>();
            var connectionString = ConfigurationManager.ConnectionStrings["MyDatabaseLibrary"].ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);

            return optionsBuilder.Options;
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConfigurationManager.ConnectionStrings["MyDatabaseLibrary"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}

