﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TTX.Infrastructure.Data;

#nullable disable

namespace TTX.Infrastructure.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TTX.Core.Models.Creator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("avatar_url");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)")
                        .HasColumnName("slug");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("ticker");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<long>("Value")
                        .HasColumnType("bigint")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.HasIndex("Ticker")
                        .IsUnique();

                    b.ToTable("creators", "public");
                });

            modelBuilder.Entity("TTX.Core.Models.LootBox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<int?>("ResultId")
                        .HasColumnType("integer")
                        .HasColumnName("result_id");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("ResultId");

                    b.HasIndex("UserId");

                    b.ToTable("loot_boxes", "public");
                });

            modelBuilder.Entity("TTX.Core.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Action")
                        .HasColumnType("integer")
                        .HasColumnName("action");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<int>("CreatorId")
                        .HasColumnType("integer")
                        .HasColumnName("creator_id");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<long>("Value")
                        .HasColumnType("bigint")
                        .HasColumnName("value");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("UserId");

                    b.ToTable("transactions", "public");
                });

            modelBuilder.Entity("TTX.Core.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("avatar_url");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<long>("Credits")
                        .HasColumnType("bigint")
                        .HasColumnName("credits");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)")
                        .HasColumnName("name");

                    b.Property<string>("TwitchId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("twitch_id");

                    b.Property<int>("Type")
                        .HasColumnType("integer")
                        .HasColumnName("type");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("TwitchId")
                        .IsUnique();

                    b.HasIndex("Type");

                    b.ToTable("users", "public");
                });

            modelBuilder.Entity("TTX.Core.Models.Vote", b =>
                {
                    b.Property<int>("CreatorId")
                        .HasColumnType("integer")
                        .HasColumnName("creator_id");

                    b.Property<DateTimeOffset>("Time")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("time");

                    b.Property<long>("Value")
                        .HasColumnType("bigint")
                        .HasColumnName("value");

                    b.HasIndex("CreatorId", "Time");

                    b.ToTable("votes", "public");
                });

            modelBuilder.Entity("TTX.Core.Models.Creator", b =>
                {
                    b.OwnsOne("TTX.Core.Models.StreamStatus", "StreamStatus", b1 =>
                        {
                            b1.Property<int>("CreatorId")
                                .HasColumnType("integer");

                            b1.Property<DateTime?>("EndedAt")
                                .HasColumnType("timestamp with time zone")
                                .HasColumnName("stream_ended_at");

                            b1.Property<bool>("IsLive")
                                .HasColumnType("boolean")
                                .HasColumnName("stream_is_live");

                            b1.Property<DateTime?>("StartedAt")
                                .HasColumnType("timestamp with time zone")
                                .HasColumnName("stream_started_at");

                            b1.HasKey("CreatorId");

                            b1.ToTable("creators", "public");

                            b1.WithOwner()
                                .HasForeignKey("CreatorId");
                        });

                    b.Navigation("StreamStatus")
                        .IsRequired();
                });

            modelBuilder.Entity("TTX.Core.Models.LootBox", b =>
                {
                    b.HasOne("TTX.Core.Models.Creator", "Result")
                        .WithMany()
                        .HasForeignKey("ResultId");

                    b.HasOne("TTX.Core.Models.User", "User")
                        .WithMany("LootBoxes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Result");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TTX.Core.Models.Transaction", b =>
                {
                    b.HasOne("TTX.Core.Models.Creator", "Creator")
                        .WithMany("Transactions")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TTX.Core.Models.User", "User")
                        .WithMany("Transactions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TTX.Core.Models.Vote", b =>
                {
                    b.HasOne("TTX.Core.Models.Creator", null)
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TTX.Core.Models.Creator", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("TTX.Core.Models.User", b =>
                {
                    b.Navigation("LootBoxes");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
