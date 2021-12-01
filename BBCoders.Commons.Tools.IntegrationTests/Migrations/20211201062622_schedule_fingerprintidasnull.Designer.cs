﻿// <auto-generated />
using System;
using BBCoders.Commons.Tools.IntegrationTests.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BBCoders.Commons.Tools.IntegrationTests.Migrations
{
    [DbContext(typeof(TestContext))]
    [Migration("20211201062622_schedule_fingerprintidasnull")]
    partial class schedule_fingerprintidasnull
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.Action", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<byte[]>("ActionId")
                        .IsRequired()
                        .HasColumnType("varbinary(16)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)");

                    b.HasKey("Id");

                    b.HasIndex("ActionId")
                        .IsUnique();

                    b.ToTable("Actions");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.Fingerprint", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("CreatedById")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP()");

                    b.Property<DateTime?>("ExpirationDate")
                        .HasColumnType("datetime");

                    b.Property<byte[]>("FingerprintId")
                        .IsRequired()
                        .HasColumnType("varbinary(16)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<long>("LastUpdatedById")
                        .HasColumnType("bigint");

                    b.Property<long>("NmlsId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("RenewalDate")
                        .HasColumnType("datetime");

                    b.Property<long>("StateId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("UpdatedDate")
                        .HasColumnType("datetime");

                    b.HasKey("Id");

                    b.HasIndex("FingerprintId")
                        .IsUnique();

                    b.HasIndex("StateId");

                    b.ToTable("Fingerprint");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.Schedule", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long?>("ActionId")
                        .HasColumnType("bigint");

                    b.Property<long>("CreatedById")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime");

                    b.Property<long?>("FingerPrintId")
                        .HasColumnType("bigint");

                    b.Property<long>("LastUpdatedById")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("LastUpdatedDate")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("ScheduleDate")
                        .HasColumnType("datetime");

                    b.Property<byte[]>("ScheduleId")
                        .IsRequired()
                        .HasColumnType("varbinary(16)");

                    b.Property<long>("ScheduleSiteId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ActionId");

                    b.HasIndex("FingerPrintId");

                    b.HasIndex("ScheduleId")
                        .IsUnique();

                    b.HasIndex("ScheduleSiteId");

                    b.ToTable("Schedules");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.ScheduleSite", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<bool>("IsActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<byte[]>("ScheduleSiteId")
                        .IsRequired()
                        .HasColumnType("varbinary(16)");

                    b.HasKey("Id");

                    b.HasIndex("ScheduleSiteId")
                        .IsUnique();

                    b.ToTable("ScheduleSites");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.State", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<byte[]>("StateId")
                        .IsRequired()
                        .HasColumnType("varbinary(16)");

                    b.HasKey("Id");

                    b.HasIndex("StateId")
                        .IsUnique();

                    b.ToTable("States");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.Fingerprint", b =>
                {
                    b.HasOne("BBCoders.Commons.Tools.IntegrationTests.Context.State", "State")
                        .WithMany()
                        .HasForeignKey("StateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("State");
                });

            modelBuilder.Entity("BBCoders.Commons.Tools.IntegrationTests.Context.Schedule", b =>
                {
                    b.HasOne("BBCoders.Commons.Tools.IntegrationTests.Context.Action", "Action")
                        .WithMany()
                        .HasForeignKey("ActionId");

                    b.HasOne("BBCoders.Commons.Tools.IntegrationTests.Context.Fingerprint", "Fingerprint")
                        .WithMany()
                        .HasForeignKey("FingerPrintId");

                    b.HasOne("BBCoders.Commons.Tools.IntegrationTests.Context.ScheduleSite", "scheduleSite")
                        .WithMany()
                        .HasForeignKey("ScheduleSiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Action");

                    b.Navigation("Fingerprint");

                    b.Navigation("scheduleSite");
                });
#pragma warning restore 612, 618
        }
    }
}
