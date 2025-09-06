IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [AddressLine] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [Province] nvarchar(max) NULL,
        [Country] nvarchar(max) NULL,
        [PostalCode] nvarchar(max) NULL,
        [StartTime] time NULL,
        [EndTime] time NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [Customer] (
        [Id] int NOT NULL IDENTITY,
        [CustomerCode] nvarchar(16) NOT NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [LocationCode] nvarchar(32) NULL,
        [Address] nvarchar(256) NULL,
        [IsActive] bit NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [ModifiedUtc] datetime2 NULL,
        [DestinationRegionCategory] int NOT NULL,
        CONSTRAINT [PK_Customer] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [InventoryItems] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] int NOT NULL,
        [ItemId] nvarchar(64) NOT NULL,
        [Description] nvarchar(256) NULL,
        [TagNumber] nvarchar(64) NULL,
        [Width] decimal(18,3) NULL,
        [Length] decimal(18,3) NULL,
        [Snapshot] decimal(18,3) NULL,
        [SnapshotUnit] nvarchar(8) NULL,
        [Location] nvarchar(64) NULL,
        [Status] nvarchar(64) NULL,
        [SnapshotLabel] nvarchar(32) NULL,
        CONSTRAINT [PK_InventoryItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [ItemRelationship] (
        [Id] int NOT NULL IDENTITY,
        [ParentItemId] nvarchar(128) NOT NULL,
        [ChildItemId] nvarchar(128) NOT NULL,
        [ParentItemDescription] nvarchar(max) NOT NULL,
        [ChildItemDescription] nvarchar(max) NOT NULL,
        [Relation] nvarchar(32) NOT NULL DEFAULT N'CoilToSheet',
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ItemRelationship] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [TransferItems] (
        [Id] int NOT NULL IDENTITY,
        [SKU] nvarchar(64) NOT NULL,
        [Description] nvarchar(256) NOT NULL,
        [Weight] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_TransferItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NULL,
        [LastName] nvarchar(max) NULL,
        [BranchId] int NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUsers_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatGroups] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [BranchId] int NULL,
        CONSTRAINT [PK_ChatGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatGroups_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [Machines] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(max) NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Model] nvarchar(max) NULL,
        [Description] nvarchar(max) NULL,
        [BranchId] int NOT NULL,
        [Category] nvarchar(32) NOT NULL,
        CONSTRAINT [PK_Machines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Machines_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [PickingLists] (
        [Id] int NOT NULL IDENTITY,
        [SalesOrderNumber] nvarchar(64) NOT NULL,
        [OrderDate] datetime2 NULL,
        [ShipDate] datetime2 NULL,
        [SoldTo] nvarchar(256) NULL,
        [ShipTo] nvarchar(256) NULL,
        [SalesRep] nvarchar(128) NULL,
        [ShippingVia] nvarchar(128) NULL,
        [FOB] nvarchar(128) NULL,
        [BranchId] int NOT NULL,
        [CustomerId] int NULL,
        [TotalWeight] decimal(18,3) NOT NULL,
        [RemainingWeight] decimal(18,3) NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_PickingLists] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PickingLists_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PickingLists_Customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [TaskAuditEvents] (
        [Id] int NOT NULL IDENTITY,
        [TaskId] int NOT NULL,
        [TaskType] int NOT NULL,
        [EventType] int NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_TaskAuditEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TaskAuditEvents_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [Trucks] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(256) NULL,
        [Identifier] nvarchar(64) NOT NULL,
        [CapacityWeight] decimal(18,2) NOT NULL,
        [CapacityVolume] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        [BranchId] int NOT NULL,
        [DriverId] nvarchar(450) NULL,
        CONSTRAINT [PK_Trucks] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Trucks_AspNetUsers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_Trucks_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatGroupUsers] (
        [UserId] nvarchar(450) NOT NULL,
        [ChatGroupId] int NOT NULL,
        CONSTRAINT [PK_ChatGroupUsers] PRIMARY KEY ([UserId], [ChatGroupId]),
        CONSTRAINT [FK_ChatGroupUsers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatGroupUsers_ChatGroups_ChatGroupId] FOREIGN KEY ([ChatGroupId]) REFERENCES [ChatGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [ChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [Content] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [SenderId] nvarchar(450) NULL,
        [RecipientId] nvarchar(450) NULL,
        [ChatGroupId] int NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ChatMessages_AspNetUsers_RecipientId] FOREIGN KEY ([RecipientId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ChatMessages_ChatGroups_ChatGroupId] FOREIGN KEY ([ChatGroupId]) REFERENCES [ChatGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkOrders] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderNumber] nvarchar(64) NOT NULL,
        [PdfWorkOrderNumber] nvarchar(64) NULL,
        [TagNumber] nvarchar(64) NOT NULL,
        [BranchId] int NOT NULL,
        [MachineId] int NULL,
        [MachineCategory] int NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [ParentItemId] nvarchar(max) NULL,
        [Instructions] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastUpdatedBy] nvarchar(max) NULL,
        [LastUpdatedDate] datetime2 NOT NULL,
        [ScheduledStartDate] datetime2 NOT NULL,
        [ScheduledEndDate] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [Shift] nvarchar(32) NULL,
        CONSTRAINT [PK_WorkOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkOrders_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WorkOrders_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [PickingListItems] (
        [Id] int NOT NULL IDENTITY,
        [PickingListId] int NOT NULL,
        [LineNumber] int NOT NULL,
        [ItemId] nvarchar(64) NOT NULL,
        [ItemDescription] nvarchar(256) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [Unit] nvarchar(16) NOT NULL,
        [Width] decimal(18,3) NULL,
        [Length] decimal(18,3) NULL,
        [Weight] decimal(18,3) NULL,
        [PulledQuantity] decimal(18,3) NULL,
        [PulledWeight] decimal(18,3) NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        [ScheduledShipDate] datetime2 NULL,
        [MachineId] int NULL,
        CONSTRAINT [PK_PickingListItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PickingListItems_Machines_MachineId] FOREIGN KEY ([MachineId]) REFERENCES [Machines] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_PickingListItems_PickingLists_PickingListId] FOREIGN KEY ([PickingListId]) REFERENCES [PickingLists] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [Loads] (
        [Id] int NOT NULL IDENTITY,
        [LoadNumber] nvarchar(64) NOT NULL,
        [TruckId] int NULL,
        [ShippingDate] datetime2 NULL,
        [TotalWeight] decimal(18,3) NOT NULL,
        [Status] int NOT NULL,
        [OriginBranchId] int NOT NULL,
        [DestinationBranchId] int NULL,
        [Notes] nvarchar(512) NULL,
        CONSTRAINT [PK_Loads] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Loads_Branches_DestinationBranchId] FOREIGN KEY ([DestinationBranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Loads_Branches_OriginBranchId] FOREIGN KEY ([OriginBranchId]) REFERENCES [Branches] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Loads_Trucks_TruckId] FOREIGN KEY ([TruckId]) REFERENCES [Trucks] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [TruckRoutes] (
        [Id] int NOT NULL IDENTITY,
        [BranchId] int NOT NULL,
        [RouteDate] datetime2 NOT NULL,
        [RegionCode] nvarchar(32) NOT NULL,
        [TruckId] int NULL,
        [Status] int NOT NULL,
        [CreatedUtc] datetime2 NOT NULL,
        [ModifiedUtc] datetime2 NULL,
        CONSTRAINT [PK_TruckRoutes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TruckRoutes_Trucks_TruckId] FOREIGN KEY ([TruckId]) REFERENCES [Trucks] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [WorkOrderItems] (
        [Id] int NOT NULL IDENTITY,
        [WorkOrderId] int NOT NULL,
        [PickingListItemId] int NULL,
        [ItemCode] nvarchar(64) NOT NULL,
        [Description] nvarchar(256) NOT NULL,
        [SalesOrderNumber] nvarchar(64) NULL,
        [CustomerName] nvarchar(128) NULL,
        [OrderQuantity] decimal(18,3) NULL,
        [OrderWeight] decimal(18,3) NULL,
        [Width] decimal(18,3) NULL,
        [Length] decimal(18,3) NULL,
        [ProducedQuantity] decimal(18,3) NULL,
        [ProducedWeight] decimal(18,3) NULL,
        [Unit] nvarchar(64) NULL,
        [Location] nvarchar(64) NULL,
        [IsStockItem] bit NOT NULL,
        CONSTRAINT [PK_WorkOrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkOrderItems_PickingListItems_PickingListItemId] FOREIGN KEY ([PickingListItemId]) REFERENCES [PickingListItems] ([Id]),
        CONSTRAINT [FK_WorkOrderItems_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [LoadItems] (
        [Id] int NOT NULL IDENTITY,
        [LoadId] int NOT NULL,
        [PickingListId] int NOT NULL,
        [PickingListItemId] int NOT NULL,
        [StopSequence] int NOT NULL,
        [ShippedWeight] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_LoadItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LoadItems_Loads_LoadId] FOREIGN KEY ([LoadId]) REFERENCES [Loads] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LoadItems_PickingListItems_PickingListItemId] FOREIGN KEY ([PickingListItemId]) REFERENCES [PickingListItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LoadItems_PickingLists_PickingListId] FOREIGN KEY ([PickingListId]) REFERENCES [PickingLists] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE TABLE [TruckRouteStops] (
        [Id] int NOT NULL IDENTITY,
        [RouteId] int NOT NULL,
        [LoadId] int NOT NULL,
        [StopOrder] int NOT NULL,
        [PlannedStart] datetime2 NULL,
        [PlannedEnd] datetime2 NULL,
        [ActualDepart] datetime2 NULL,
        [ActualArrive] datetime2 NULL,
        [Notes] nvarchar(256) NULL,
        CONSTRAINT [PK_TruckRouteStops] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TruckRouteStops_Loads_LoadId] FOREIGN KEY ([LoadId]) REFERENCES [Loads] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TruckRouteStops_TruckRoutes_RouteId] FOREIGN KEY ([RouteId]) REFERENCES [TruckRoutes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUsers_BranchId] ON [AspNetUsers] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatGroups_BranchId] ON [ChatGroups] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatGroupUsers_ChatGroupId] ON [ChatGroupUsers] ([ChatGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_ChatGroupId] ON [ChatMessages] ([ChatGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_RecipientId] ON [ChatMessages] ([RecipientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ChatMessages_SenderId] ON [ChatMessages] ([SenderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customer_CustomerCode] ON [Customer] ([CustomerCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customer_LocationCode] ON [Customer] ([LocationCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_BranchId] ON [InventoryItems] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_InventoryItems_ItemId] ON [InventoryItems] ([ItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItemRelationship_ParentItemId_ChildItemId_Relation] ON [ItemRelationship] ([ParentItemId], [ChildItemId], [Relation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LoadItems_LoadId] ON [LoadItems] ([LoadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LoadItems_PickingListId] ON [LoadItems] ([PickingListId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LoadItems_PickingListItemId] ON [LoadItems] ([PickingListItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loads_DestinationBranchId] ON [Loads] ([DestinationBranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loads_OriginBranchId] ON [Loads] ([OriginBranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Loads_TruckId] ON [Loads] ([TruckId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Machines_BranchId] ON [Machines] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PickingListItems_MachineId] ON [PickingListItems] ([MachineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PickingListItems_PickingListId] ON [PickingListItems] ([PickingListId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PickingLists_BranchId_SalesOrderNumber] ON [PickingLists] ([BranchId], [SalesOrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PickingLists_CustomerId] ON [PickingLists] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PickingLists_SalesOrderNumber] ON [PickingLists] ([SalesOrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TaskAuditEvents_UserId] ON [TaskAuditEvents] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TruckRoutes_BranchId_RouteDate_RegionCode] ON [TruckRoutes] ([BranchId], [RouteDate], [RegionCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TruckRoutes_TruckId] ON [TruckRoutes] ([TruckId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TruckRouteStops_LoadId] ON [TruckRouteStops] ([LoadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TruckRouteStops_RouteId_StopOrder] ON [TruckRouteStops] ([RouteId], [StopOrder]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Trucks_BranchId] ON [Trucks] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Trucks_DriverId] ON [Trucks] ([DriverId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Trucks_Identifier] ON [Trucks] ([Identifier]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkOrderItems_PickingListItemId] ON [WorkOrderItems] ([PickingListItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkOrderItems_WorkOrderId] ON [WorkOrderItems] ([WorkOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_BranchId] ON [WorkOrders] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_MachineId] ON [WorkOrders] ([MachineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250905225411_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250905225411_InitialCreate', N'9.0.8');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250906070832_AddAvatarToApplicationUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Avatar] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250906070832_AddAvatarToApplicationUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250906070832_AddAvatarToApplicationUser', N'9.0.8');
END;

COMMIT;
GO
