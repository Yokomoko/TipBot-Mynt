
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 10/25/2018 12:17:20
-- Generated from EDMX file: C:\Users\achapman\Documents\Visual Studio 2015\Projects\TipBot Mynt\TipBot_BL\FantasyPortfolio\FantasyPortfolio.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [FantasyPortfolio_Mynt_DB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_RoundPortfolio]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Portfolios] DROP CONSTRAINT [FK_RoundPortfolio];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Coins]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Coins];
GO
IF OBJECT_ID(N'[dbo].[FlipResults]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FlipResults];
GO
IF OBJECT_ID(N'[dbo].[LeftUsers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LeftUsers];
GO
IF OBJECT_ID(N'[dbo].[Portfolios]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Portfolios];
GO
IF OBJECT_ID(N'[dbo].[Rounds]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Rounds];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Coins'
CREATE TABLE [dbo].[Coins] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TickerId] int  NOT NULL,
    [TickerName] nvarchar(max)  NOT NULL,
    [PriceUSD] decimal(18,8)  NOT NULL,
    [LastUpdated] datetime  NOT NULL,
    [Volume24] decimal(28,18)  NULL
);
GO

-- Creating table 'Portfolios'
CREATE TABLE [dbo].[Portfolios] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundId] int  NOT NULL,
    [UserId] nvarchar(max)  NOT NULL,
    [TickerId] int  NOT NULL,
    [CoinAmount] decimal(18,8)  NOT NULL
);
GO

-- Creating table 'Rounds'
CREATE TABLE [dbo].[Rounds] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoundEnds] datetime  NOT NULL
);
GO

-- Creating table 'Leaderboards'
CREATE TABLE [dbo].[Leaderboards] (
    [RN] bigint  NULL,
    [RoundId] int  NOT NULL,
    [UserId] nvarchar(max)  NOT NULL,
    [totalamount] decimal(38,6)  NULL
);
GO

-- Creating table 'LeaderboardTickers'
CREATE TABLE [dbo].[LeaderboardTickers] (
    [RN] bigint  NULL,
    [RoundId] int  NOT NULL,
    [UserId] nvarchar(max)  NOT NULL,
    [TickerName] nvarchar(max)  NOT NULL,
    [DollarValue] decimal(38,6)  NULL,
    [CoinCount] decimal(38,8)  NULL
);
GO

-- Creating table 'FlipResults'
CREATE TABLE [dbo].[FlipResults] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [DateTime] datetime  NULL,
    [UserId] varchar(100)  NULL,
    [FlipResult] tinyint  NULL,
    [UserFlip] tinyint  NULL,
    [IsWin] int  NOT NULL,
    [FlipValue] decimal(18,8)  NULL
);
GO

-- Creating table 'FlipLeaderboard'
CREATE TABLE [dbo].[FlipLeaderboard] (
    [ID] bigint  NOT NULL,
    [UserId] varchar(100)  NULL,
    [TotalBet] decimal(38,8)  NULL,
    [TotalWins] int  NULL
);
GO

-- Creating table 'FlipResultStatistics'
CREATE TABLE [dbo].[FlipResultStatistics] (
    [id] int IDENTITY(1,1) NOT NULL,
    [TotalFlips] int  NULL,
    [Wins] int  NULL,
    [Losses] int  NULL,
    [WinPercentage] decimal(29,11)  NULL,
    [LossPercentage] decimal(29,11)  NULL,
    [TotalFlipped] decimal(38,8)  NULL,
    [PaidOut] decimal(38,10)  NULL,
    [PaidIn] decimal(38,8)  NULL,
    [HeadFlips] int  NULL,
    [TailFlips] int  NULL
);
GO

-- Creating table 'LeftUsers'
CREATE TABLE [dbo].[LeftUsers] (
    [Id] int  NOT NULL,
    [UserId] varchar(100)  NULL,
    [TimeLeft] datetime  NULL,
    [GuildId] varchar(100)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Coins'
ALTER TABLE [dbo].[Coins]
ADD CONSTRAINT [PK_Coins]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Portfolios'
ALTER TABLE [dbo].[Portfolios]
ADD CONSTRAINT [PK_Portfolios]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Rounds'
ALTER TABLE [dbo].[Rounds]
ADD CONSTRAINT [PK_Rounds]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [RoundId], [UserId] in table 'Leaderboards'
ALTER TABLE [dbo].[Leaderboards]
ADD CONSTRAINT [PK_Leaderboards]
    PRIMARY KEY CLUSTERED ([RoundId], [UserId] ASC);
GO

-- Creating primary key on [RoundId], [UserId], [TickerName] in table 'LeaderboardTickers'
ALTER TABLE [dbo].[LeaderboardTickers]
ADD CONSTRAINT [PK_LeaderboardTickers]
    PRIMARY KEY CLUSTERED ([RoundId], [UserId], [TickerName] ASC);
GO

-- Creating primary key on [ID] in table 'FlipResults'
ALTER TABLE [dbo].[FlipResults]
ADD CONSTRAINT [PK_FlipResults]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'FlipLeaderboard'
ALTER TABLE [dbo].[FlipLeaderboard]
ADD CONSTRAINT [PK_FlipLeaderboard]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [id] in table 'FlipResultStatistics'
ALTER TABLE [dbo].[FlipResultStatistics]
ADD CONSTRAINT [PK_FlipResultStatistics]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'LeftUsers'
ALTER TABLE [dbo].[LeftUsers]
ADD CONSTRAINT [PK_LeftUsers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [RoundId] in table 'Portfolios'
ALTER TABLE [dbo].[Portfolios]
ADD CONSTRAINT [FK_RoundPortfolio]
    FOREIGN KEY ([RoundId])
    REFERENCES [dbo].[Rounds]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_RoundPortfolio'
CREATE INDEX [IX_FK_RoundPortfolio]
ON [dbo].[Portfolios]
    ([RoundId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------