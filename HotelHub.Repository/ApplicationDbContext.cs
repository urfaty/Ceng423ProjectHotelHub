﻿using HotelHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelHub.Repository
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):
            base(options)
        {

        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public IQueryable<SubCategory> FindSubCategory(string title)
        {
            return this.SubCategories.Where(sc => sc.Title == title);
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SubCategory>()
                .HasOne(x => x.Category).WithMany(y => y.SubCategories).
                OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SubCategory>()
               .HasMany(x => x.Items).WithOne(y => y.SubCategory).
               OnDelete(DeleteBehavior.ClientSetNull);

            base.OnModelCreating(builder);
        }
    }
   
}
