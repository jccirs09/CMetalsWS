using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMetalsWS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CityCentroid",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityCentroid", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DestinationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TagNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Snapshot = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    SnapshotUnit = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SnapshotLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemRelationship",
                columns: table => new
                {
                    ItemCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoilRelationship = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemRelationship", x => x.ItemCode);
                });

            migrationBuilder.CreateTable(
                name: "TransferItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKU = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatGroups_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EstimatedLbsPerHour = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PdfWorkOrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TagNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    MachineCategory = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentItemId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentItemDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentItemWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ParentItemLocation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Operator = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatGroupUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChatGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatGroupUsers", x => new { x.UserId, x.ChatGroupId });
                    table.ForeignKey(
                        name: "FK_ChatGroupUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatGroupUsers_ChatGroups_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "ChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RecipientId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ChatGroupId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatGroups_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "ChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DestinationRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RequiresPooling = table.Column<bool>(type: "bit", nullable: false),
                    CoordinatorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationRegions_AspNetUsers_CoordinatorId",
                        column: x => x.CoordinatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PinnedThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThreadId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinnedThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PinnedThreads_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CapacityWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CapacityVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trucks_AspNetUsers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Trucks_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageReactions_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageSeens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageSeens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageSeens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessageSeens_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullAddress = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    BusinessHours = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Street1 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Street2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    City = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DestinationRegionId = table.Column<int>(type: "int", nullable: true),
                    DestinationGroupId = table.Column<int>(type: "int", nullable: true),
                    MaxSkidCapacity = table.Column<int>(type: "int", nullable: true),
                    MaxSlitCoilWeight = table.Column<int>(type: "int", nullable: true),
                    ReceivingHourStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    ReceivingHourEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    ServiceTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessRestrictions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AppointmentRequired = table.Column<bool>(type: "bit", nullable: false),
                    PreferredTruckType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_DestinationGroups_DestinationGroupId",
                        column: x => x.DestinationGroupId,
                        principalTable: "DestinationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Customer_DestinationRegions_DestinationRegionId",
                        column: x => x.DestinationRegionId,
                        principalTable: "DestinationRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DestinationRegionBranch",
                columns: table => new
                {
                    BranchesId = table.Column<int>(type: "int", nullable: false),
                    DestinationRegionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationRegionBranch", x => new { x.BranchesId, x.DestinationRegionsId });
                    table.ForeignKey(
                        name: "FK_DestinationRegionBranch_Branches_BranchesId",
                        column: x => x.BranchesId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DestinationRegionBranch_DestinationRegions_DestinationRegionsId",
                        column: x => x.DestinationRegionsId,
                        principalTable: "DestinationRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoadNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TruckId = table.Column<int>(type: "int", nullable: true),
                    ShippingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OriginBranchId = table.Column<int>(type: "int", nullable: false),
                    DestinationBranchId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loads_Branches_DestinationBranchId",
                        column: x => x.DestinationBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loads_Branches_OriginBranchId",
                        column: x => x.OriginBranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loads_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TruckRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    RouteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegionCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TruckId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckRoutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckRoutes_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PickingLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoldTo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShipTo = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SalesRep = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Buyer = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PrintDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    DestinationRegionId = table.Column<int>(type: "int", nullable: true),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RemainingWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ScannedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScannedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedToId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingLists_AspNetUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickingLists_AspNetUsers_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickingLists_AspNetUsers_ScannedById",
                        column: x => x.ScannedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickingLists_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PickingLists_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickingLists_DestinationRegions_DestinationRegionId",
                        column: x => x.DestinationRegionId,
                        principalTable: "DestinationRegions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TruckRouteStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    PlannedStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualDepart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualArrive = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TruckRouteStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TruckRouteStops_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TruckRouteStops_TruckRoutes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TruckRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PickingListImports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourcePdfPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagesPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListImports_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PickingListImports_PickingLists_PickingListId",
                        column: x => x.PickingListId,
                        principalTable: "PickingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PickingListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListId = table.Column<int>(type: "int", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    PulledQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    PulledWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ScheduledProcessingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduledShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CoilId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PackingNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    QualityChecked = table.Column<bool>(type: "bit", nullable: false),
                    QualityCheckedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    QualityCheckedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DamageNotes = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListItems_AspNetUsers_QualityCheckedById",
                        column: x => x.QualityCheckedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PickingListItems_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PickingListItems_PickingLists_PickingListId",
                        column: x => x.PickingListId,
                        principalTable: "PickingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PickingListPageImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListImportId = table.Column<int>(type: "int", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListPageImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListPageImages_PickingListImports_PickingListImportId",
                        column: x => x.PickingListImportId,
                        principalTable: "PickingListImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoadId = table.Column<int>(type: "int", nullable: false),
                    PickingListId = table.Column<int>(type: "int", nullable: false),
                    PickingListItemId = table.Column<int>(type: "int", nullable: false),
                    StopSequence = table.Column<int>(type: "int", nullable: false),
                    ShippedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ShippedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadItems_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoadItems_PickingListItems_PickingListItemId",
                        column: x => x.PickingListItemId,
                        principalTable: "PickingListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadItems_PickingLists_PickingListId",
                        column: x => x.PickingListId,
                        principalTable: "PickingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickingListItemId = table.Column<int>(type: "int", nullable: true),
                    TaskType = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAuditEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAuditEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAuditEvents_PickingListItems_PickingListItemId",
                        column: x => x.PickingListItemId,
                        principalTable: "PickingListItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    PickingListItemId = table.Column<int>(type: "int", nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OrderQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    OrderWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Width = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Length = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ProducedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ProducedWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsStockItem = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OriginalOrderLineItemId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderItems_PickingListItems_PickingListItemId",
                        column: x => x.PickingListItemId,
                        principalTable: "PickingListItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrderItems_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName",
                table: "AspNetUsers",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FirstName_LastName",
                table: "AspNetUsers",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastName",
                table: "AspNetUsers",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MachineId",
                table: "AspNetUsers",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ShiftId",
                table: "AspNetUsers",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroups_BranchId",
                table: "ChatGroups",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatGroupUsers_ChatGroupId_UserId",
                table: "ChatGroupUsers",
                columns: new[] { "ChatGroupId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatGroupId_Timestamp",
                table: "ChatMessages",
                columns: new[] { "ChatGroupId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_RecipientId",
                table: "ChatMessages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_RecipientId_Timestamp",
                table: "ChatMessages",
                columns: new[] { "SenderId", "RecipientId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_CityCentroid_City_Province",
                table: "CityCentroid",
                columns: new[] { "City", "Province" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Active",
                table: "Customer",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_City",
                table: "Customer",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustomerCode",
                table: "Customer",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationGroupId",
                table: "Customer",
                column: "DestinationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_DestinationRegionId",
                table: "Customer",
                column: "DestinationRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PostalCode",
                table: "Customer",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_Province",
                table: "Customer",
                column: "Province");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationRegionBranch_DestinationRegionsId",
                table: "DestinationRegionBranch",
                column: "DestinationRegionsId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationRegions_CoordinatorId",
                table: "DestinationRegions",
                column: "CoordinatorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_BranchId",
                table: "InventoryItems",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemId",
                table: "InventoryItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemRelationship_CoilRelationship",
                table: "ItemRelationship",
                column: "CoilRelationship");

            migrationBuilder.CreateIndex(
                name: "IX_LoadItems_LoadId",
                table: "LoadItems",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadItems_PickingListId",
                table: "LoadItems",
                column: "PickingListId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadItems_PickingListItemId",
                table: "LoadItems",
                column: "PickingListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_DestinationBranchId",
                table: "Loads",
                column: "DestinationBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_OriginBranchId",
                table: "Loads",
                column: "OriginBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_TruckId",
                table: "Loads",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_BranchId",
                table: "Machines",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId",
                table: "MessageReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UserId",
                table: "MessageReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeens_MessageId_UserId",
                table: "MessageSeens",
                columns: new[] { "MessageId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MessageSeens_UserId",
                table: "MessageSeens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListImports_BranchId",
                table: "PickingListImports",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListImports_PickingListId",
                table: "PickingListImports",
                column: "PickingListId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_MachineId",
                table: "PickingListItems",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_PickingListId",
                table: "PickingListItems",
                column: "PickingListId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_QualityCheckedById",
                table: "PickingListItems",
                column: "QualityCheckedById");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListPageImages_PickingListImportId",
                table: "PickingListPageImages",
                column: "PickingListImportId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_AssignedToId",
                table: "PickingLists",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_BranchId_SalesOrderNumber",
                table: "PickingLists",
                columns: new[] { "BranchId", "SalesOrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_CustomerId",
                table: "PickingLists",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_DestinationRegionId",
                table: "PickingLists",
                column: "DestinationRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_ModifiedById",
                table: "PickingLists",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_SalesOrderNumber",
                table: "PickingLists",
                column: "SalesOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_ScannedById",
                table: "PickingLists",
                column: "ScannedById");

            migrationBuilder.CreateIndex(
                name: "IX_PinnedThreads_UserId_ThreadId",
                table: "PinnedThreads",
                columns: new[] { "UserId", "ThreadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAuditEvents_PickingListItemId",
                table: "TaskAuditEvents",
                column: "PickingListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAuditEvents_UserId",
                table: "TaskAuditEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckRoutes_BranchId_RouteDate_RegionCode",
                table: "TruckRoutes",
                columns: new[] { "BranchId", "RouteDate", "RegionCode" });

            migrationBuilder.CreateIndex(
                name: "IX_TruckRoutes_TruckId",
                table: "TruckRoutes",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckRouteStops_LoadId",
                table: "TruckRouteStops",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_TruckRouteStops_RouteId_StopOrder",
                table: "TruckRouteStops",
                columns: new[] { "RouteId", "StopOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_BranchId",
                table: "Trucks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_DriverId",
                table: "Trucks",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Identifier",
                table: "Trucks",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItems_PickingListItemId",
                table: "WorkOrderItems",
                column: "PickingListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItems_WorkOrderId",
                table: "WorkOrderItems",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_BranchId",
                table: "WorkOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_MachineId",
                table: "WorkOrders",
                column: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChatGroupUsers");

            migrationBuilder.DropTable(
                name: "CityCentroid");

            migrationBuilder.DropTable(
                name: "DestinationRegionBranch");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "ItemRelationship");

            migrationBuilder.DropTable(
                name: "LoadItems");

            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropTable(
                name: "MessageSeens");

            migrationBuilder.DropTable(
                name: "PickingListPageImages");

            migrationBuilder.DropTable(
                name: "PinnedThreads");

            migrationBuilder.DropTable(
                name: "TaskAuditEvents");

            migrationBuilder.DropTable(
                name: "TransferItems");

            migrationBuilder.DropTable(
                name: "TruckRouteStops");

            migrationBuilder.DropTable(
                name: "WorkOrderItems");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "PickingListImports");

            migrationBuilder.DropTable(
                name: "Loads");

            migrationBuilder.DropTable(
                name: "TruckRoutes");

            migrationBuilder.DropTable(
                name: "PickingListItems");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "ChatGroups");

            migrationBuilder.DropTable(
                name: "Trucks");

            migrationBuilder.DropTable(
                name: "PickingLists");

            migrationBuilder.DropTable(
                name: "Customer");

            migrationBuilder.DropTable(
                name: "DestinationGroups");

            migrationBuilder.DropTable(
                name: "DestinationRegions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Branches");
        }
    }
}
