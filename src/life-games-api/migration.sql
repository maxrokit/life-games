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
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    CREATE TABLE [Board] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Board] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    CREATE TABLE [BoardGeneration] (
        [Id] uniqueidentifier NOT NULL,
        [BoardId] uniqueidentifier NOT NULL,
        [GenerationNumber] int NOT NULL,
        [Cells] nvarchar(max) NOT NULL,
        [ComputedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BoardGeneration] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BoardGeneration_Board_BoardId] FOREIGN KEY ([BoardId]) REFERENCES [Board] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Board_CreatedAt] ON [Board] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BoardGeneration_BoardId] ON [BoardGeneration] ([BoardId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BoardGeneration_BoardId_GenerationNumber] ON [BoardGeneration] ([BoardId], [GenerationNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260202083416_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260202083416_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

