-- FinderLink Database Schema for Azure SQL Database
-- This script creates all tables with proper relationships and constraints
-- Updated for Admin-only system (no User table)

-- Drop existing tables if they exist (for fresh deployment)
-- Comment these out if you want to preserve data
-- DROP TABLE IF EXISTS [dbo].[Releases];
-- DROP TABLE IF EXISTS [dbo].[AdminLogs];
-- DROP TABLE IF EXISTS [dbo].[Claims];
-- DROP TABLE IF EXISTS [dbo].[Items];
-- DROP TABLE IF EXISTS [dbo].[Admins];

-- Create Admins table (only admins can log in)
CREATE TABLE [dbo].[Admins] (
    [AdminId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] VARCHAR(100) NOT NULL,
    [Email] VARCHAR(100) NOT NULL UNIQUE,
    [Password] VARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX [IX_Admins_Email] ON [dbo].[Admins]([Email]);

-- Create Items table
CREATE TABLE [dbo].[Items] (
    [ItemId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ItemName] VARCHAR(200) NOT NULL,
    [Description] VARCHAR(1000) NULL,
    [LocationFound] VARCHAR(200) NOT NULL,
    [Status] VARCHAR(50) NOT NULL DEFAULT 'unclaimed', -- 'unclaimed', 'pending', 'claimed', 'released'
    [DateFound] DATE NOT NULL,

    -- Finder information
    [FoundByAdminId] INT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE SET NULL,
    [FoundByName] VARCHAR(100) NULL,      -- For non-admin finders
    [FoundByContact] VARCHAR(100) NULL,   -- Email/phone for non-admin finders

    [CreatedBy] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE NO ACTION,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE INDEX [IX_Items_Status] ON [dbo].[Items]([Status]);
CREATE INDEX [IX_Items_DateFound] ON [dbo].[Items]([DateFound]);
CREATE INDEX [IX_Items_FoundByAdminId] ON [dbo].[Items]([FoundByAdminId]);
CREATE INDEX [IX_Items_CreatedBy] ON [dbo].[Items]([CreatedBy]);

-- Create Claims table
CREATE TABLE [dbo].[Claims] (
    [ClaimId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ItemId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Items]([ItemId]) ON DELETE CASCADE,
    [AdminId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE NO ACTION,
    [ClaimDescription] VARCHAR(500) NULL,
    [Status] VARCHAR(50) NOT NULL DEFAULT 'pending', -- 'pending', 'verified', 'rejected', 'released'
    [DateClaimed] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [VerifiedBy] INT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE SET NULL,
    [DateVerified] DATETIME2 NULL
);

CREATE INDEX [IX_Claims_Status] ON [dbo].[Claims]([Status]);
CREATE INDEX [IX_Claims_ItemId] ON [dbo].[Claims]([ItemId]);
CREATE INDEX [IX_Claims_AdminId] ON [dbo].[Claims]([AdminId]);
CREATE INDEX [IX_Claims_VerifiedBy] ON [dbo].[Claims]([VerifiedBy]);
CREATE INDEX [IX_Claims_DateClaimed] ON [dbo].[Claims]([DateClaimed]);

-- Create Admin Logs table
CREATE TABLE [dbo].[AdminLogs] (
    [LogId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AdminId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE CASCADE,
    [Action] VARCHAR(100) NOT NULL, -- 'verify_claim', 'release_item', 'add_item', 'update_item'
    [ItemId] INT NULL FOREIGN KEY REFERENCES [dbo].[Items]([ItemId]) ON DELETE SET NULL,
    [ClaimId] INT NULL FOREIGN KEY REFERENCES [dbo].[Claims]([ClaimId]) ON DELETE SET NULL,
    [LogDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Remarks] VARCHAR(500) NULL
);

CREATE INDEX [IX_AdminLogs_AdminId] ON [dbo].[AdminLogs]([AdminId]);
CREATE INDEX [IX_AdminLogs_LogDate] ON [dbo].[AdminLogs]([LogDate]);
CREATE INDEX [IX_AdminLogs_Action] ON [dbo].[AdminLogs]([Action]);

-- Create Releases table
CREATE TABLE [dbo].[Releases] (
    [ReleaseId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ItemId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Items]([ItemId]) ON DELETE CASCADE,
    [ClaimId] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Claims]([ClaimId]) ON DELETE SET NULL,
    [ReleasedTo] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE NO ACTION,
    [ReleasedBy] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[Admins]([AdminId]) ON DELETE NO ACTION,
    [ReleaseDate] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Proof] VARCHAR(255) NULL -- URL to proof document/image
);

CREATE INDEX [IX_Releases_ItemId] ON [dbo].[Releases]([ItemId]);
CREATE INDEX [IX_Releases_ClaimId] ON [dbo].[Releases]([ClaimId]);
CREATE INDEX [IX_Releases_ReleasedTo] ON [dbo].[Releases]([ReleasedTo]);
CREATE INDEX [IX_Releases_ReleasedBy] ON [dbo].[Releases]([ReleasedBy]);
CREATE INDEX [IX_Releases_ReleaseDate] ON [dbo].[Releases]([ReleaseDate]);

-- Create Views for common queries

-- View: Outstanding Items (unclaimed or pending)
CREATE VIEW [dbo].[vw_OutstandingItems] AS
SELECT 
    i.[ItemId],
    i.[ItemName],
    i.[Description],
    i.[LocationFound],
    i.[Status],
    i.[DateFound],
    a.[Name] AS [CreatedByName],
    (SELECT COUNT(*) FROM [dbo].[Claims] WHERE [ItemId] = i.[ItemId] AND [Status] = 'pending') AS [PendingClaimCount]
FROM [dbo].[Items] i
LEFT JOIN [dbo].[Admins] a ON i.[CreatedBy] = a.[AdminId]
WHERE i.[Status] IN ('unclaimed', 'pending');

-- View: Admin Claims Summary
CREATE VIEW [dbo].[vw_AdminClaimsSummary] AS
SELECT 
    a.[AdminId],
    a.[Name],
    a.[Email],
    COUNT(CASE WHEN c.[Status] = 'pending' THEN 1 END) AS [PendingClaims],
    COUNT(CASE WHEN c.[Status] = 'verified' THEN 1 END) AS [VerifiedClaims],
    COUNT(CASE WHEN c.[Status] = 'released' THEN 1 END) AS [ReleasedClaims],
    COUNT(CASE WHEN c.[Status] = 'rejected' THEN 1 END) AS [RejectedClaims]
FROM [dbo].[Admins] a
LEFT JOIN [dbo].[Claims] c ON a.[AdminId] = c.[AdminId]
GROUP BY a.[AdminId], a.[Name], a.[Email];

-- View: Items Found by Admins
CREATE VIEW [dbo].[vw_ItemsByFinder] AS
SELECT 
    a.[AdminId],
    a.[Name] AS [FinderName],
    COUNT(*) AS [ItemsFound],
    COUNT(CASE WHEN i.[Status] = 'unclaimed' THEN 1 END) AS [UnclaimedItems],
    COUNT(CASE WHEN i.[Status] = 'claimed' THEN 1 END) AS [ClaimedItems],
    COUNT(CASE WHEN i.[Status] = 'released' THEN 1 END) AS [ReleasedItems]
FROM [dbo].[Admins] a
LEFT JOIN [dbo].[Items] i ON a.[AdminId] = i.[FoundByAdminId]
GROUP BY a.[AdminId], a.[Name];
