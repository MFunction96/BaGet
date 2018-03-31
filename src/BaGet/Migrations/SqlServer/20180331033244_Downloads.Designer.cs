﻿// <auto-generated />
using BaGet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace BaGet.Migrations.SqlServer
{
    [DbContext(typeof(SqlServerContext))]
    [Migration("20180331033244_Downloads")]
    partial class Downloads
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("BaGet.Core.Entities.Package", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Authors");

                    b.Property<string>("Description");

                    b.Property<long>("Downloads");

                    b.Property<string>("IconUrlString")
                        .HasColumnName("IconUrl");

                    b.Property<string>("Id");

                    b.Property<string>("Language");

                    b.Property<string>("LicenseUrlString")
                        .HasColumnName("LicenseUrl");

                    b.Property<bool>("Listed");

                    b.Property<string>("MinClientVersion");

                    b.Property<string>("ProjectUrlString")
                        .HasColumnName("ProjectUrl");

                    b.Property<DateTime>("Published");

                    b.Property<bool>("RequireLicenseAcceptance");

                    b.Property<string>("Summary");

                    b.Property<string>("TagsString")
                        .HasColumnName("Tags");

                    b.Property<string>("Title");

                    b.Property<string>("VersionString")
                        .HasColumnName("Version");

                    b.HasKey("Key");

                    b.HasIndex("Id");

                    b.HasIndex("Id", "VersionString")
                        .IsUnique()
                        .HasFilter("[Id] IS NOT NULL AND [Version] IS NOT NULL");

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("BaGet.Core.Entities.PackageDependency", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Id");

                    b.Property<int?>("PackageDependencyGroupKey");

                    b.Property<string>("VersionRange");

                    b.HasKey("Key");

                    b.HasIndex("PackageDependencyGroupKey");

                    b.ToTable("PackageDependency");
                });

            modelBuilder.Entity("BaGet.Core.Entities.PackageDependencyGroup", b =>
                {
                    b.Property<int>("Key")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("PackageKey");

                    b.Property<string>("TargetFramework");

                    b.HasKey("Key");

                    b.HasIndex("PackageKey");

                    b.ToTable("PackageDependencyGroup");
                });

            modelBuilder.Entity("BaGet.Core.Entities.PackageDependency", b =>
                {
                    b.HasOne("BaGet.Core.Entities.PackageDependencyGroup")
                        .WithMany("Dependencies")
                        .HasForeignKey("PackageDependencyGroupKey");
                });

            modelBuilder.Entity("BaGet.Core.Entities.PackageDependencyGroup", b =>
                {
                    b.HasOne("BaGet.Core.Entities.Package")
                        .WithMany("Dependencies")
                        .HasForeignKey("PackageKey");
                });
#pragma warning restore 612, 618
        }
    }
}
