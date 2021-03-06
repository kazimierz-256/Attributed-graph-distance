﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Postgres_enron_database.Data;

namespace Postgres_enron_database.Migrations
{
    [DbContext(typeof(EnronContext))]
    partial class EnronContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Postgres_enron_database.Models.DestinationEmail", b =>
                {
                    b.Property<long>("EmailId")
                        .HasColumnType("bigint");

                    b.Property<long>("EmailAddressId")
                        .HasColumnType("bigint");

                    b.Property<int>("SendType")
                        .HasColumnType("integer");

                    b.HasKey("EmailId", "EmailAddressId", "SendType");

                    b.HasIndex("SendType");

                    b.ToTable("DestinationEmails");
                });

            modelBuilder.Entity("Postgres_enron_database.Models.EmailAddress", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("varchar");

                    b.Property<bool>("BelongsToEnron")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("BelongsToEnron");

                    b.HasIndex("Id");

                    b.ToTable("EmailAddresses");
                });

            modelBuilder.Entity("Postgres_enron_database.Models.EmailObject", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<long>("FromId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("SendDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("varchar");

                    b.HasKey("Id");

                    b.HasIndex("FromId");

                    b.HasIndex("Id");

                    b.HasIndex("SendDate");

                    b.HasIndex("URL")
                        .IsUnique();

                    b.ToTable("Emails");
                });
#pragma warning restore 612, 618
        }
    }
}
