USE WidowedOrphansManagement;
GO

-- ���� ������� ������
CREATE TABLE [dbo].[Statuses]
(
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [StatusName] NVARCHAR(50) NOT NULL
);

-- ���� ����� (������/��)
CREATE TABLE [dbo].[Parents]
(
    [Id] INT IDENTITY(1,1),
    [IdentityNumber] NVARCHAR(50) PRIMARY KEY NOT NULL,  -- ����� ���� ����
    [LastName] NVARCHAR(50) NULL,
    [FirstName] NVARCHAR(50) NULL,
    [BirthDate] DATE NOT NULL,
    [Gender] NVARCHAR(10) NULL,
    [StatusId] INT NOT NULL,  -- FK ������ �������� �������
    [Neighborhood] NVARCHAR(50) NULL,
    [Street] NVARCHAR(50) NULL,
    [HouseNumber] NVARCHAR(10) NULL,
    [Floor] NVARCHAR(10) NULL,
    [HomePhone] NVARCHAR(20) NULL,
    [MobilePhone] NVARCHAR(20) NULL,
    [WorkPhone] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NULL,
    [Notes] NVARCHAR(255) NULL,
    [CreatedRow] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedRow] DATETIME NULL,
    CONSTRAINT FK_Parent_Status FOREIGN KEY (StatusId) REFERENCES [dbo].[Statuses](Id)
);

-- ���� ����� (������/��)
CREATE TABLE [dbo].[Orphans]
(
    [Id] INT IDENTITY(1,1),
    [ParentIdentityNumber] NVARCHAR(50) NOT NULL,  -- ����� ���� ����
    [IdentityNumber] NVARCHAR(50) PRIMARY KEY NOT NULL,  -- ����� ���� ���
    [LastName] NVARCHAR(50) NULL,
    [FirstName] NVARCHAR(50) NULL,
    [BirthDate] DATE NOT NULL,
    [Gender] NVARCHAR(10) NULL,
    [StatusId] INT NOT NULL,  -- FK �������� �������
    [Neighborhood] NVARCHAR(50) NULL,
    [Street] NVARCHAR(50) NULL,
    [HouseNumber] NVARCHAR(10) NULL,
    [Floor] NVARCHAR(10) NULL,
    [HomePhone] NVARCHAR(20) NULL,
    [MobilePhone] NVARCHAR(20) NULL,
    [WorkPhone] NVARCHAR(20) NULL,
    [Email] NVARCHAR(100) NULL,
    [Notes] NVARCHAR(255) NULL,
    [CreatedRow] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedRow] DATETIME NULL,
    CONSTRAINT FK_Orphan_Status FOREIGN KEY (StatusId) REFERENCES [dbo].[Statuses](Id),
    CONSTRAINT FK_Orphan_Parent FOREIGN KEY (ParentIdentityNumber) REFERENCES [dbo].[Parents](IdentityNumber)
);
