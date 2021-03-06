﻿using Postgres_enron_database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postgres_enron_database.Data
{
    public class EnronContext : DbContext
    {
        public DbSet<EmailObject> Emails { get; set; }
        public DbSet<EmailAddress> EmailAddresses { get; set; }
        public DbSet<DestinationEmail> DestinationEmails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DestinationEmail>()
                .HasKey(e => new { e.EmailId, e.EmailAddressId, e.SendType});
            modelBuilder.Entity<DestinationEmail>()
                .HasIndex(e => e.SendType);

            modelBuilder.Entity<EmailAddress>()
                .Property(ea => ea.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<EmailAddress>()
                .HasIndex(ea => ea.BelongsToEnron);
            modelBuilder.Entity<EmailAddress>()
                .HasIndex(ea => ea.Id);

            modelBuilder.Entity<EmailObject>()
                .HasIndex(eo => eo.URL).IsUnique();
            modelBuilder.Entity<EmailObject>()
                .HasIndex(eo => eo.Id);
            modelBuilder.Entity<EmailObject>()
                .HasIndex(eo => eo.FromId);
            modelBuilder.Entity<EmailObject>()
                .HasIndex(eo => eo.SendDate);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // basic connection usage, for true applications use Microsoft Secret Manager Tool (dotnet user-secrets)
            optionsBuilder.UseNpgsql("Server = localhost; Database = enron; Port = 5432; Username = postgres; Password = admin");
        }
    }
}
