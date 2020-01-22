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
    [Migration("20191213092348_RemovedCollectionFromEmailObjectAndChangedFromFieldToKey")]
    partial class RemovedCollectionFromEmailObjectAndChangedFromFieldToKey
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
                    b.Property<int>("EmailId")
                        .HasColumnType("integer");

                    b.Property<int>("EmailAddressId")
                        .HasColumnType("integer");

                    b.Property<int>("SendType")
                        .HasColumnType("integer");

                    b.HasKey("EmailId", "EmailAddressId", "SendType");

                    b.ToTable("DestinationEmails");
                });

            modelBuilder.Entity("database_csharp.Models.EmailAddress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("BelongsToEnron")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("EmailAddresses");
                });

            modelBuilder.Entity("database_csharp.Models.EmailObject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("FromId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("SendDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Emails");
                });
#pragma warning restore 612, 618
        }
    }
}
