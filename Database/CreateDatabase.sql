USE [master]
GO
/****** Object:  Database [BATTLE24]    Script Date: 2015-05-10 22:19:24 ******/
CREATE DATABASE [BATTLE24]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'BATTLE24', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\BATTLE24.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'BATTLE24_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\BATTLE24_log.ldf' , SIZE = 2048KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [BATTLE24] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [BATTLE24].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [BATTLE24] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [BATTLE24] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [BATTLE24] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [BATTLE24] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [BATTLE24] SET ARITHABORT OFF 
GO
ALTER DATABASE [BATTLE24] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [BATTLE24] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [BATTLE24] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [BATTLE24] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [BATTLE24] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [BATTLE24] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [BATTLE24] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [BATTLE24] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [BATTLE24] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [BATTLE24] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [BATTLE24] SET  DISABLE_BROKER 
GO
ALTER DATABASE [BATTLE24] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [BATTLE24] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [BATTLE24] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [BATTLE24] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [BATTLE24] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [BATTLE24] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [BATTLE24] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [BATTLE24] SET RECOVERY FULL 
GO
ALTER DATABASE [BATTLE24] SET  MULTI_USER 
GO
ALTER DATABASE [BATTLE24] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [BATTLE24] SET DB_CHAINING OFF 
GO
ALTER DATABASE [BATTLE24] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [BATTLE24] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'BATTLE24', N'ON'
GO
USE [BATTLE24]
GO
/****** Object:  Table [dbo].[BATTLES]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BATTLES](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](800) NULL,
	[WarID] [int] NULL,
	[Date] [nvarchar](800) NULL,
	[Location] [nvarchar](800) NULL,
	[Result] [nvarchar](max) NULL,
	[TerritorialChanges] [nvarchar](800) NULL,
	[URL] [nvarchar](800) NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
	[Country] [nvarchar](max) NULL,
 CONSTRAINT [PK_BATTLES] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BATTLES_BELLIGERENTS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BATTLES_BELLIGERENTS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BelligerentID] [int] NOT NULL,
	[BattleID] [int] NULL,
	[Strength] [nvarchar](max) NULL,
	[CasualtiesAndLosses] [nvarchar](800) NULL,
	[ConflictSide] [int] NULL,
	[Result] [nvarchar](800) NULL,
	[InfantryStrength] [int] NULL,
	[CavalryStrength] [int] NULL,
	[ArtilleryStrength] [int] NULL,
	[NavyStrength] [int] NULL,
	[AllStrength] [int] NULL,
	[OtherStrength] [nvarchar](800) NULL,
	[Killed] [int] NULL,
	[Wounded] [int] NULL,
	[Captured] [int] NULL,
	[AllLosses] [int] NULL,
	[OtherLosses] [nvarchar](800) NULL,
	[ShipsLost] [int] NULL,
 CONSTRAINT [PK_BATTLES_BELLIGERENTS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BATTLES_LEADERS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BATTLES_LEADERS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LeaderID] [int] NULL,
	[BattleID] [int] NULL,
 CONSTRAINT [PK_BATTLES_LEADERS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[BELLIGERENTS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BELLIGERENTS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FlagURL] [nvarchar](800) NULL,
	[Name] [nvarchar](max) NULL,
	[URL] [nvarchar](800) NULL,
 CONSTRAINT [PK_BELLIGERENTS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[LEADERS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LEADERS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BelligerentID] [int] NULL,
	[Name] [nvarchar](800) NULL,
	[URL] [nvarchar](800) NULL,
 CONSTRAINT [PK_LEADERS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TREATIES]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TREATIES](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](800) NULL,
	[Date] [nvarchar](800) NULL,
	[Summary] [nvarchar](max) NULL,
	[URL] [nvarchar](800) NULL,
 CONSTRAINT [PK_TREATIES] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WARS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WARS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](800) NULL,
	[ParentWar] [int] NULL,
	[Date] [nvarchar](800) NULL,
	[TerritorialChanges] [nvarchar](800) NULL,
	[Result] [nvarchar](800) NULL,
	[URL] [nvarchar](800) NULL,
	[StartDate] [date] NULL,
	[EndDate] [date] NULL,
 CONSTRAINT [PK_WARS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WARS_BELLIGERENTS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WARS_BELLIGERENTS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[BelligerentID] [int] NOT NULL,
	[WarID] [int] NULL,
	[Result] [nvarchar](800) NULL,
 CONSTRAINT [PK_WARS_BELLIGERENTS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[WARS_LEADERS]    Script Date: 2015-05-10 22:19:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WARS_LEADERS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WarID] [int] NULL,
	[LeaderID] [int] NULL,
 CONSTRAINT [PK_WARS_LEADERS] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[BATTLES]  WITH CHECK ADD  CONSTRAINT [FK_BATTLES_WARS] FOREIGN KEY([WarID])
REFERENCES [dbo].[WARS] ([ID])
GO
ALTER TABLE [dbo].[BATTLES] CHECK CONSTRAINT [FK_BATTLES_WARS]
GO
ALTER TABLE [dbo].[BATTLES_BELLIGERENTS]  WITH CHECK ADD  CONSTRAINT [FK_BATTLES_BELLIGERENTS_BATTLES] FOREIGN KEY([BattleID])
REFERENCES [dbo].[BATTLES] ([ID])
GO
ALTER TABLE [dbo].[BATTLES_BELLIGERENTS] CHECK CONSTRAINT [FK_BATTLES_BELLIGERENTS_BATTLES]
GO
ALTER TABLE [dbo].[BATTLES_BELLIGERENTS]  WITH CHECK ADD  CONSTRAINT [FK_BATTLES_BELLIGERENTS_BELLIGERENTS] FOREIGN KEY([BelligerentID])
REFERENCES [dbo].[BELLIGERENTS] ([ID])
GO
ALTER TABLE [dbo].[BATTLES_BELLIGERENTS] CHECK CONSTRAINT [FK_BATTLES_BELLIGERENTS_BELLIGERENTS]
GO
ALTER TABLE [dbo].[BATTLES_LEADERS]  WITH CHECK ADD  CONSTRAINT [FK_BATTLES_LEADERS_BATTLES] FOREIGN KEY([BattleID])
REFERENCES [dbo].[BATTLES] ([ID])
GO
ALTER TABLE [dbo].[BATTLES_LEADERS] CHECK CONSTRAINT [FK_BATTLES_LEADERS_BATTLES]
GO
ALTER TABLE [dbo].[BATTLES_LEADERS]  WITH CHECK ADD  CONSTRAINT [FK_BATTLES_LEADERS_LEADERS] FOREIGN KEY([LeaderID])
REFERENCES [dbo].[LEADERS] ([ID])
GO
ALTER TABLE [dbo].[BATTLES_LEADERS] CHECK CONSTRAINT [FK_BATTLES_LEADERS_LEADERS]
GO
ALTER TABLE [dbo].[LEADERS]  WITH CHECK ADD  CONSTRAINT [FK_LEADERS_BELLIGERENTS] FOREIGN KEY([BelligerentID])
REFERENCES [dbo].[BELLIGERENTS] ([ID])
GO
ALTER TABLE [dbo].[LEADERS] CHECK CONSTRAINT [FK_LEADERS_BELLIGERENTS]
GO
ALTER TABLE [dbo].[WARS]  WITH CHECK ADD  CONSTRAINT [FK_WARS_WARS] FOREIGN KEY([ParentWar])
REFERENCES [dbo].[WARS] ([ID])
GO
ALTER TABLE [dbo].[WARS] CHECK CONSTRAINT [FK_WARS_WARS]
GO
ALTER TABLE [dbo].[WARS_BELLIGERENTS]  WITH CHECK ADD  CONSTRAINT [FK_WARS_BELLIGERENTS_BELLIGERENTS] FOREIGN KEY([BelligerentID])
REFERENCES [dbo].[BELLIGERENTS] ([ID])
GO
ALTER TABLE [dbo].[WARS_BELLIGERENTS] CHECK CONSTRAINT [FK_WARS_BELLIGERENTS_BELLIGERENTS]
GO
ALTER TABLE [dbo].[WARS_BELLIGERENTS]  WITH CHECK ADD  CONSTRAINT [FK_WARS_BELLIGERENTS_WARS] FOREIGN KEY([WarID])
REFERENCES [dbo].[WARS] ([ID])
GO
ALTER TABLE [dbo].[WARS_BELLIGERENTS] CHECK CONSTRAINT [FK_WARS_BELLIGERENTS_WARS]
GO
ALTER TABLE [dbo].[WARS_LEADERS]  WITH CHECK ADD  CONSTRAINT [FK_WARS_LEADERS_LEADERS] FOREIGN KEY([LeaderID])
REFERENCES [dbo].[LEADERS] ([ID])
GO
ALTER TABLE [dbo].[WARS_LEADERS] CHECK CONSTRAINT [FK_WARS_LEADERS_LEADERS]
GO
ALTER TABLE [dbo].[WARS_LEADERS]  WITH CHECK ADD  CONSTRAINT [FK_WARS_LEADERS_WARS] FOREIGN KEY([WarID])
REFERENCES [dbo].[WARS] ([ID])
GO
ALTER TABLE [dbo].[WARS_LEADERS] CHECK CONSTRAINT [FK_WARS_LEADERS_WARS]
GO
USE [master]
GO
ALTER DATABASE [BATTLE24] SET  READ_WRITE 
GO
