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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [Entity] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NULL,
        [Action] nvarchar(max) NOT NULL,
        [ChangedBy] nvarchar(max) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [Details] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE TABLE [PaintItems] (
        [Id] int NOT NULL IDENTITY,
        [Barcode] nvarchar(450) NOT NULL,
        [SKU] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [ColorCode] nvarchar(max) NULL,
        [Volume] decimal(18,2) NULL,
        [Unit] nvarchar(max) NULL,
        [Batch] nvarchar(max) NULL,
        [Manufacturer] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_PaintItems] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE TABLE [LocationHistories] (
        [Id] int NOT NULL IDENTITY,
        [PaintItemId] int NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [QuantityMoved] decimal(18,2) NOT NULL,
        [Notes] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_LocationHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LocationHistories_PaintItems_PaintItemId] FOREIGN KEY ([PaintItemId]) REFERENCES [PaintItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE TABLE [ScanRecords] (
        [Id] int NOT NULL IDENTITY,
        [PaintItemId] int NULL,
        [BarcodeScanned] nvarchar(max) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Unit] nvarchar(max) NULL,
        [DeviceId] nvarchar(max) NULL,
        [Operator] nvarchar(max) NULL,
        [Location] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_ScanRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ScanRecords_PaintItems_PaintItemId] FOREIGN KEY ([PaintItemId]) REFERENCES [PaintItems] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LocationHistories_PaintItemId] ON [LocationHistories] ([PaintItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaintItems_Barcode] ON [PaintItems] ([Barcode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ScanRecords_PaintItemId] ON [ScanRecords] ([PaintItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260908012954_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260908012954_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

