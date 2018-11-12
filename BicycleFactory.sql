/*
 * File          : BicycleFactory.sql
 * Project       : BikeFactory
 * Programmer    : Dustin Brown
 * First Version : November 2018
 * Description   : Script to initialize the database for BicycleFactory
 */
CREATE DATABASE BicycleFactory;
GO
USE BicycleFactory;

--Tables
CREATE TABLE PartType (
	ID INT IDENTITY(1,1),
	[Name] VARCHAR (30) NOT NULL,
	PRIMARY KEY (ID)
);

CREATE TABLE WorkerType (
	ID INT IDENTITY(1,1),
	[Name] VARCHAR (24) NOT NULL,
	PRIMARY KEY (ID)
);

CREATE TABLE WorkStationType (
	ID INT IDENTITY (1,1),
	[Name] VARCHAR (24) NOT NULL,
	PRIMARY KEY (ID)
);

CREATE TABLE Part (
	ID INT IDENTITY(1,1),
	PartType INT NOT NULL,
	PRIMARY KEY (ID),
	FOREIGN KEY (PartType) REFERENCES PartType (ID)
);

CREATE TABLE Worker (
	ID INT IDENTITY(1,1),
	FirstName VARCHAR(24),
	LastName VARCHAR(24) NOT NULL,
	[Type] INT NOT NULL,
	PRIMARY KEY (ID),
	FOREIGN KEY ([Type]) REFERENCES WorkerType (ID)
);

CREATE TABLE Workstation (
	ID INT IDENTITY(1,1),
	[Type] INT NOT NULL,
	Worker INT,
	LastAction DATETIME,
	PRIMARY KEY (ID),
	FOREIGN KEY ([Type]) REFERENCES WorkStationType (ID),
	FOREIGN KEY (Worker) REFERENCES Worker (ID)
); 

CREATE TABLE Bin (
	ID INT IDENTITY (1,1), 
	InOut BIT NOT NULL, /* TRUE for INCOMING, FALSE FOR OUTGOING */
	Junk BIT NOT NULL, /*TRUE for JUNK, FALSE for GOOD PART */
	Workstation INT NOT NULL,
	PartType INT,
	PRIMARY KEY (ID),
	FOREIGN KEY (Workstation) REFERENCES Workstation (ID),
);

CREATE TABLE PartMap (
	Part INT NOT NULL,
	Bin INT NOT NULL,
	FOREIGN KEY (Part) REFERENCES Part (ID),
	FOREIGN KEY (Bin) REFERENCES Bin (ID)
);

--Procedures
GO
CREATE PROCEDURE spAssignWorkstation
@workerID INT,
@workerType INT
AS
BEGIN
	DECLARE @WorkStationID INT
	SELECT  @WorkstationID = ID
	FROM Workstation
	WHERE (LastAction IS NULL OR LastAction < DATEADD(MINUTE, -10, GETDATE())) AND [Type] = @workerType
	ORDER BY LastAction;
	IF (@WorkStationID IS NOT NULL)
		BEGIN
		UPDATE Workstation SET [Type] = @workerType, Worker = @workerID, LastAction = GETDATE() WHERE ID = @WorkstationID;
		SELECT @WorkstationID
		END
	ELSE
		BEGIN
		SELECT 'No workstations available'
		END
END

GO
CREATE PROCEDURE spMovePart
@sourceBin INT,
@destBin INT
AS
BEGIN
	DECLARE @part INT
	SELECT TOP 1 @part = Part
	FROM PartMap
	WHERE Bin = @sourceBin;
	UPDATE PartMap SET Bin = @destBin WHERE Part = @Part;
END

GO
CREATE PROCEDURE spTakePart
@bin INT
AS
BEGIN
	DECLARE @part INT;
	SELECT TOP 1 @part = Part FROM PartMap WHERE BIN = @bin;
	DELETE FROM PartMap WHERE Part = @part;
	DELETE FROM Part WHERE ID = @part;
END

GO
CREATE PROCEDURE spGivePart
@bin INT,
@partType INT
AS
BEGIN
	DECLARE @part INT;
	INSERT INTO Part(PartType) VALUES (@partType);
	SELECT TOP 1 @part = ID FROM Part ORDER BY ID DESC;
	INSERT INTO PartMap(Part, Bin) VALUES (@part, @bin);
END

GO
CREATE PROCEDURE spMakeBin
@workstationID INT,
@inbin BIT,
@PartType INT,
@isJunk BIT
AS
BEGIN
	INSERT INTO Bin (Workstation, InOut, PartType, Junk)
	OUTPUT inserted.ID
	VALUES (@workstationID, @inbin, @PartType, @isJunk);
END
GO

CREATE PROCEDURE spBinCount
@binID INT
AS
BEGIN
	SELECT COUNT(*) FROM PartMap WHERE Bin = @binID;
END

--inserts
GO
INSERT INTO PartType ([Name]) VALUES ('Steel Tube');
INSERT INTO PartType ([Name]) VALUES ('Steel Sheet');
INSERT INTO PartType ([Name]) VALUES ('Paint');
INSERT INTO PartType ([Name]) VALUES ('Bracket');
INSERT INTO PartType ([Name]) VALUES ('Wheel');
INSERT INTO PartType ([Name]) VALUES ('Drivetrain');
INSERT INTO PartType ([Name]) VALUES ('Seat');
INSERT INTO PartType ([Name]) VALUES ('Chain');
INSERT INTO PartType ([Name]) VALUES ('Chain Guard');
INSERT INTO PartType ([Name]) VALUES ('Frame Part A');
INSERT INTO PartType ([Name]) VALUES ('Frame Part B');
INSERT INTO PartType ([Name]) VALUES ('Frame Part C');
INSERT INTO PartType ([Name]) VALUES ('Frame Part D');
INSERT INTO PartType ([Name]) VALUES ('Frame Part E');
INSERT INTO PartType ([Name]) VALUES ('Frame Part F');
INSERT INTO PartType ([Name]) VALUES ('Frame Part G');
INSERT INTO PartType ([Name]) VALUES ('Handlebar Part A');
INSERT INTO PartType ([Name]) VALUES ('Handlebar Part B');
INSERT INTO PartType ([Name]) VALUES ('Unpainted Handlebar');
INSERT INTO PartType ([Name]) VALUES ('Unpainted Frame');
INSERT INTO PartType ([Name]) VALUES ('Painted Frame');
INSERT INTO PartType ([Name]) VALUES ('Painted Handlebar');
INSERT INTO PartType ([Name]) VALUES ('Unpainted Fender');
INSERT INTO PartType ([Name]) VALUES ('Painted Fender');
INSERT INTO PartType ([Name]) VALUES ('Bicycle Complete');

INSERT INTO WorkStationType ([Name]) VALUES ('Metal Work Station');
INSERT INTO WorkStationType ([Name]) VALUES ('Paint Station');
INSERT INTO WorkStationType ([Name]) VALUES ('Assembly Station');

INSERT INTO WorkerType ([Name]) VALUES ('Metal Worker');
INSERT INTO WorkerType ([Name]) VALUES ('Paint Worker');
INSERT INTO WorkerType ([Name]) VALUES ('Assembler');
INSERT INTO WorkerType ([Name]) VALUES ('Transporter');


