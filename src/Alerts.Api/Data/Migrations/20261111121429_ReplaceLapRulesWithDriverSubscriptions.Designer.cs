using System;
using Alerts.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Alerts.Api.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261111121429_ReplaceLapRulesWithDriverSubscriptions")]
    partial class ReplaceLapRulesWithDriverSubscriptions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Alerts.Api.Data.Models.CompletedRace", b =>
                {
                    b.Property<string>("SessionId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("ObservedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("RecordedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("SessionId");

                    b.HasIndex("RecordedAt");

                    b.ToTable("CompletedRaces");
                });

            modelBuilder.Entity("Alerts.Api.Data.Models.DriverSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DriverId")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.HasIndex("UserId", "DriverId")
                        .IsUnique();

                    b.ToTable("DriverSubscriptions");
                });

            modelBuilder.Entity("Alerts.Api.Data.Models.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("PublishedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("OutboxMessages");
                });

            modelBuilder.Entity("Alerts.Api.Data.Models.ProcessedEvent", b =>
                {
                    b.Property<Guid>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("EventId");

                    b.ToTable("ProcessedEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
