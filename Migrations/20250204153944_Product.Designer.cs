﻿// <auto-generated />
using System;
using ASP_P22.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ASP_P22.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250204153944_Product")]
    partial class Product
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("site")
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ASP_P22.Data.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImagesCsv")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Slug");

                    b.ToTable("Categories", "site");

                    b.HasData(
                        new
                        {
                            Id = new Guid("706c9d0d-d766-48b2-8615-3dfe795b048e"),
                            Description = "Товари та вироби зі скла",
                            ImagesCsv = "glass.jpg",
                            Name = "Скло",
                            Slug = "glass"
                        },
                        new
                        {
                            Id = new Guid("cc51b8ca-ad48-456d-b83f-023f17a7cec8"),
                            Description = "Офісні та настільні товари",
                            ImagesCsv = "office.jpg",
                            Name = "Офіс",
                            Slug = "office"
                        },
                        new
                        {
                            Id = new Guid("3cf44c28-9b0b-4314-a7bd-410864432f7a"),
                            Description = "Вироби з натурального та штучного каміння",
                            ImagesCsv = "stone.jpg",
                            Name = "Каміння",
                            Slug = "stone"
                        },
                        new
                        {
                            Id = new Guid("1e7b62ed-1810-441b-a781-622f2bf86d66"),
                            Description = "Товари та вироби з деревини",
                            ImagesCsv = "wood.jpg",
                            Name = "Дерево",
                            Slug = "wood"
                        });
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImagesCsv")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<string>("Slug")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Stock")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("Slug");

                    b.ToTable("Products", "site");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("WorkPosition")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Slug");

                    b.ToTable("Users", "site");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.UserAccess", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Dk")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("UsersAccess", "site");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.Product", b =>
                {
                    b.HasOne("ASP_P22.Data.Entities.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.UserAccess", b =>
                {
                    b.HasOne("ASP_P22.Data.Entities.User", "User")
                        .WithMany("Accesses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("ASP_P22.Data.Entities.User", b =>
                {
                    b.Navigation("Accesses");
                });
#pragma warning restore 612, 618
        }
    }
}
