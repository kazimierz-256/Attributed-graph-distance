﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using database_csharp.Data;

namespace database_csharp.Migrations
{
    [DbContext(typeof(EnronContext))]
    [Migration("20191214093505_ChangedIntKeysToLong")]
    partial class ChangedIntKeysToLong
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("database_csharp.Models.DestinationEmail", b =>
                {
                    b.Property<long>("EmailId")
                        .HasColumnType("bigint");

                    b.Property<long>("EmailAddressId")
                        .HasColumnType("bigint");

                    b.Property<int>("SendType")
                        .HasColumnType("integer");

                    b.HasKey("EmailId", "EmailAddressId", "SendType");

                    b.ToTable("DestinationEmails");
                });

            modelBuilder.Entity("database_csharp.Models.EmailAddress", b =>
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

                    b.ToTable("EmailAddresses");
                });

            modelBuilder.Entity("database_csharp.Models.EmailObject", b =>
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

                    b.ToTable("Emails");
                });
#pragma warning restore 612, 618
        }
    }
}
