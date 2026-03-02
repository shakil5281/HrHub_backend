CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginIp] nvarchar(max) NULL,
    [Country] nvarchar(max) NULL,
    [City] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
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
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Companies] (
    [Id] int NOT NULL IDENTITY,
    [Branch] int NOT NULL,
    [CompanyNameEn] nvarchar(200) NOT NULL,
    [CompanyNameBn] nvarchar(200) NOT NULL,
    [AddressEn] nvarchar(500) NOT NULL,
    [AddressBn] nvarchar(500) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [RegistrationNo] nvarchar(50) NOT NULL,
    [Industry] nvarchar(100) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [LogoPath] nvarchar(max) NULL,
    [AuthorizeSignaturePath] nvarchar(max) NULL,
    [Status] nvarchar(max) NOT NULL,
    [Founded] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Countries] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    CONSTRAINT [PK_Countries] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LeaveTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(10) NOT NULL,
    [YearlyLimit] int NOT NULL,
    [IsCarryForward] bit NOT NULL,
    [Description] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_LeaveTypes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ApplicationUserCompany] (
    [AssignedCompaniesId] int NOT NULL,
    [UsersId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_ApplicationUserCompany] PRIMARY KEY ([AssignedCompaniesId], [UsersId]),
    CONSTRAINT [FK_ApplicationUserCompany_AspNetUsers_UsersId] FOREIGN KEY ([UsersId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ApplicationUserCompany_Companies_AssignedCompaniesId] FOREIGN KEY ([AssignedCompaniesId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CompanyId] int NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Departments_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Floors] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CompanyName] nvarchar(200) NULL,
    [CompanyId] int NULL,
    CONSTRAINT [PK_Floors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Floors_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Groups] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CompanyName] nvarchar(200) NULL,
    [CompanyId] int NULL,
    CONSTRAINT [PK_Groups] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Groups_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Shifts] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [InTime] nvarchar(10) NOT NULL,
    [OutTime] nvarchar(10) NOT NULL,
    [ActualInTime] nvarchar(10) NULL,
    [ActualOutTime] nvarchar(10) NULL,
    [LateInTime] nvarchar(10) NULL,
    [LunchTimeStart] nvarchar(10) NULL,
    [LunchHour] decimal(18,2) NOT NULL,
    [Weekends] nvarchar(200) NULL,
    [CompanyName] nvarchar(200) NULL,
    [CompanyId] int NULL,
    [Status] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Shifts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shifts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Divisions] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CountryId] int NOT NULL,
    CONSTRAINT [PK_Divisions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Divisions_Countries_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Countries] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Sections] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CompanyId] int NULL,
    [DepartmentId] int NOT NULL,
    CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sections_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Sections_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Districts] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [DivisionId] int NOT NULL,
    CONSTRAINT [PK_Districts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Districts_Divisions_DivisionId] FOREIGN KEY ([DivisionId]) REFERENCES [Divisions] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Designations] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [NightBill] decimal(18,2) NOT NULL,
    [HolidayBill] decimal(18,2) NOT NULL,
    [AttendanceBonus] decimal(18,2) NOT NULL,
    [CompanyId] int NULL,
    [DepartmentId] int NULL,
    [SectionId] int NOT NULL,
    CONSTRAINT [PK_Designations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Designations_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Designations_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Designations_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Lines] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [CompanyId] int NULL,
    [DepartmentId] int NULL,
    [SectionId] int NOT NULL,
    CONSTRAINT [PK_Lines] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lines_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lines_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lines_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PostOffices] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [Code] nvarchar(20) NOT NULL,
    [DistrictId] int NOT NULL,
    CONSTRAINT [PK_PostOffices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PostOffices_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Thanas] (
    [Id] int NOT NULL IDENTITY,
    [NameEn] nvarchar(100) NOT NULL,
    [NameBn] nvarchar(100) NULL,
    [DistrictId] int NOT NULL,
    CONSTRAINT [PK_Thanas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Thanas_Districts_DistrictId] FOREIGN KEY ([DistrictId]) REFERENCES [Districts] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ManpowerRequirements] (
    [Id] int NOT NULL IDENTITY,
    [DepartmentId] int NOT NULL,
    [DesignationId] int NOT NULL,
    [RequiredCount] int NOT NULL,
    [Note] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ManpowerRequirements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ManpowerRequirements_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ManpowerRequirements_Designations_DesignationId] FOREIGN KEY ([DesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [EmployeeId] nvarchar(20) NOT NULL,
    [FullNameEn] nvarchar(200) NOT NULL,
    [FullNameBn] nvarchar(200) NULL,
    [Nid] nvarchar(50) NULL,
    [Proximity] nvarchar(50) NULL,
    [DateOfBirth] datetime2 NULL,
    [Gender] nvarchar(20) NULL,
    [Religion] nvarchar(50) NULL,
    [DepartmentId] int NOT NULL,
    [SectionId] int NULL,
    [DesignationId] int NOT NULL,
    [LineId] int NULL,
    [ShiftId] int NULL,
    [GroupId] int NULL,
    [FloorId] int NULL,
    [Status] nvarchar(50) NOT NULL,
    [JoinDate] datetime2 NOT NULL,
    [ProfileImageUrl] nvarchar(500) NULL,
    [SignatureImageUrl] nvarchar(500) NULL,
    [Email] nvarchar(100) NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [PresentAddress] nvarchar(500) NULL,
    [PresentAddressBn] nvarchar(500) NULL,
    [PresentDivisionId] int NULL,
    [PresentDistrictId] int NULL,
    [PresentThanaId] int NULL,
    [PresentPostOfficeId] int NULL,
    [PresentPostalCode] nvarchar(20) NULL,
    [PermanentAddress] nvarchar(500) NULL,
    [PermanentAddressBn] nvarchar(500) NULL,
    [PermanentDivisionId] int NULL,
    [PermanentDistrictId] int NULL,
    [PermanentThanaId] int NULL,
    [PermanentPostOfficeId] int NULL,
    [PermanentPostalCode] nvarchar(20) NULL,
    [FatherNameEn] nvarchar(200) NULL,
    [FatherNameBn] nvarchar(200) NULL,
    [MotherNameEn] nvarchar(200) NULL,
    [MotherNameBn] nvarchar(200) NULL,
    [MaritalStatus] nvarchar(50) NULL,
    [SpouseNameEn] nvarchar(200) NULL,
    [SpouseNameBn] nvarchar(200) NULL,
    [SpouseOccupation] nvarchar(100) NULL,
    [SpouseContact] nvarchar(20) NULL,
    [BasicSalary] decimal(18,2) NULL,
    [HouseRent] decimal(18,2) NULL,
    [MedicalAllowance] decimal(18,2) NULL,
    [Conveyance] decimal(18,2) NULL,
    [FoodAllowance] decimal(18,2) NULL,
    [OtherAllowance] decimal(18,2) NULL,
    [GrossSalary] decimal(18,2) NULL,
    [BankName] nvarchar(100) NULL,
    [BankBranchName] nvarchar(100) NULL,
    [BankAccountNo] nvarchar(50) NULL,
    [BankRoutingNo] nvarchar(50) NULL,
    [BankAccountType] nvarchar(50) NULL,
    [EmergencyContactName] nvarchar(200) NULL,
    [EmergencyContactRelation] nvarchar(100) NULL,
    [EmergencyContactPhone] nvarchar(20) NULL,
    [EmergencyContactAddress] nvarchar(500) NULL,
    [CompanyId] int NULL,
    [CompanyName] nvarchar(200) NULL,
    [BloodGroup] nvarchar(20) NULL,
    [IsActive] bit NOT NULL,
    [IsOtEnabled] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Designations_DesignationId] FOREIGN KEY ([DesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Floors_FloorId] FOREIGN KEY ([FloorId]) REFERENCES [Floors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Groups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Groups] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Lines_LineId] FOREIGN KEY ([LineId]) REFERENCES [Lines] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Employees_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AdvanceSalaries] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CompanyId] int NULL,
    [Amount] decimal(18,2) NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    [ApprovalDate] datetime2 NULL,
    [RepaymentMonth] int NOT NULL,
    [RepaymentYear] int NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AdvanceSalaries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AdvanceSalaries_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
    CONSTRAINT [FK_AdvanceSalaries_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AttendanceLogs] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeCard] int NOT NULL,
    [CompanyId] int NULL,
    [EmployeeId] nvarchar(50) NULL,
    [LogTime] datetime2 NOT NULL,
    [DeviceId] nvarchar(50) NULL,
    [VerificationMode] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AttendanceLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AttendanceLogs_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AttendanceLogs_Employees_EmployeeCard] FOREIGN KEY ([EmployeeCard]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Attendances] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeCard] int NOT NULL,
    [CompanyId] int NULL,
    [EmployeeId] nvarchar(50) NULL,
    [Date] datetime2 NOT NULL,
    [InTime] datetime2 NULL,
    [OutTime] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL,
    [OTHours] decimal(18,2) NOT NULL,
    [ShiftId] int NULL,
    [IsOffDay] bit NOT NULL,
    [Reason] nvarchar(100) NULL,
    [Remarks] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsManual] bit NOT NULL,
    CONSTRAINT [PK_Attendances] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Attendances_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Attendances_Employees_EmployeeCard] FOREIGN KEY ([EmployeeCard]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Attendances_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts] ([Id])
);
GO


CREATE TABLE [Bonuses] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CompanyId] int NULL,
    [BonusType] nvarchar(100) NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [PaymentDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Bonuses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bonuses_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
    CONSTRAINT [FK_Bonuses_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CounselingRecords] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CounselingDate] datetime2 NOT NULL,
    [IssueType] nvarchar(100) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [ActionTaken] nvarchar(1000) NULL,
    [FollowUpNotes] nvarchar(500) NULL,
    [Status] nvarchar(50) NOT NULL,
    [Severity] nvarchar(50) NOT NULL,
    [FollowUpDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_CounselingRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CounselingRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DailySalarySheets] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CompanyId] int NULL,
    [Date] datetime2 NOT NULL,
    [GrossSalary] decimal(18,2) NOT NULL,
    [PerDaySalary] decimal(18,2) NOT NULL,
    [AttendanceStatus] nvarchar(20) NOT NULL,
    [OTHours] decimal(18,2) NOT NULL,
    [OTAmount] decimal(18,2) NOT NULL,
    [TotalEarning] decimal(18,2) NOT NULL,
    [Deduction] decimal(18,2) NOT NULL,
    [NetPayable] decimal(18,2) NOT NULL,
    [ProcessedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DailySalarySheets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DailySalarySheets_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
    CONSTRAINT [FK_DailySalarySheets_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [EmployeeShiftRosters] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [ShiftId] int NOT NULL,
    [IsOffDay] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_EmployeeShiftRosters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_EmployeeShiftRosters_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EmployeeShiftRosters_Shifts_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shifts] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [LeaveApplications] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [LeaveTypeId] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [TotalDays] decimal(18,1) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [AppliedDate] datetime2 NOT NULL,
    [ApprovedById] int NULL,
    [ActionDate] datetime2 NULL,
    [Remarks] nvarchar(500) NULL,
    [AttachmentUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_LeaveApplications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LeaveApplications_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LeaveApplications_LeaveTypes_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [LeaveTypes] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [MonthlySalarySheets] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CompanyId] int NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [GrossSalary] decimal(18,2) NOT NULL,
    [BasicSalary] decimal(18,2) NOT NULL,
    [TotalDays] int NOT NULL,
    [PresentDays] int NOT NULL,
    [AbsentDays] int NOT NULL,
    [LeaveDays] int NOT NULL,
    [Holidays] int NOT NULL,
    [WeekendDays] int NOT NULL,
    [OTHours] decimal(18,2) NOT NULL,
    [OTRate] decimal(18,2) NOT NULL,
    [OTAmount] decimal(18,2) NOT NULL,
    [AttendanceBonus] decimal(18,2) NOT NULL,
    [OtherAllowances] decimal(18,2) NOT NULL,
    [TotalEarning] decimal(18,2) NOT NULL,
    [AbsentDeduction] decimal(18,2) NOT NULL,
    [AdvanceDeduction] decimal(18,2) NOT NULL,
    [OTDeduction] decimal(18,2) NOT NULL,
    [TotalDeduction] decimal(18,2) NOT NULL,
    [NetPayable] decimal(18,2) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [ProcessedAt] datetime2 NOT NULL,
    [ProcessedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_MonthlySalarySheets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MonthlySalarySheets_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
    CONSTRAINT [FK_MonthlySalarySheets_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [OtDeductions] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [DeductionHours] decimal(18,2) NOT NULL,
    [Reason] nvarchar(200) NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_OtDeductions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OtDeductions_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [SalaryIncrements] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [CompanyId] int NULL,
    [PreviousGrossSalary] decimal(18,2) NOT NULL,
    [IncrementAmount] decimal(18,2) NOT NULL,
    [NewGrossSalary] decimal(18,2) NOT NULL,
    [EffectiveDate] datetime2 NOT NULL,
    [IncrementType] nvarchar(100) NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [IsApplied] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SalaryIncrements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SalaryIncrements_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
    CONSTRAINT [FK_SalaryIncrements_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Separations] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [LastWorkingDate] datetime2 NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [AdminRemark] nvarchar(500) NULL,
    [IsSettled] bit NOT NULL,
    [SettledAt] datetime2 NULL,
    [ApprovedBy] nvarchar(max) NULL,
    [ApprovedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Separations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Separations_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Transfers] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeId] int NOT NULL,
    [FromDepartmentId] int NULL,
    [FromDesignationId] int NULL,
    [ToDepartmentId] int NOT NULL,
    [ToDesignationId] int NOT NULL,
    [TransferDate] datetime2 NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [AdminRemark] nvarchar(500) NULL,
    [ApprovedBy] nvarchar(max) NULL,
    [ApprovedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Transfers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transfers_Departments_FromDepartmentId] FOREIGN KEY ([FromDepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transfers_Departments_ToDepartmentId] FOREIGN KEY ([ToDepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transfers_Designations_FromDesignationId] FOREIGN KEY ([FromDesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transfers_Designations_ToDesignationId] FOREIGN KEY ([ToDesignationId]) REFERENCES [Designations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transfers_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AdvanceSalaries_CompanyId] ON [AdvanceSalaries] ([CompanyId]);
GO


CREATE INDEX [IX_AdvanceSalaries_EmployeeId] ON [AdvanceSalaries] ([EmployeeId]);
GO


CREATE INDEX [IX_ApplicationUserCompany_UsersId] ON [ApplicationUserCompany] ([UsersId]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_AttendanceLogs_CompanyId] ON [AttendanceLogs] ([CompanyId]);
GO


CREATE INDEX [IX_AttendanceLogs_EmployeeCard] ON [AttendanceLogs] ([EmployeeCard]);
GO


CREATE INDEX [IX_Attendances_CompanyId] ON [Attendances] ([CompanyId]);
GO


CREATE INDEX [IX_Attendances_EmployeeCard] ON [Attendances] ([EmployeeCard]);
GO


CREATE INDEX [IX_Attendances_ShiftId] ON [Attendances] ([ShiftId]);
GO


CREATE INDEX [IX_Bonuses_CompanyId] ON [Bonuses] ([CompanyId]);
GO


CREATE INDEX [IX_Bonuses_EmployeeId] ON [Bonuses] ([EmployeeId]);
GO


CREATE UNIQUE INDEX [IX_Companies_RegistrationNo] ON [Companies] ([RegistrationNo]);
GO


CREATE INDEX [IX_CounselingRecords_EmployeeId] ON [CounselingRecords] ([EmployeeId]);
GO


CREATE INDEX [IX_DailySalarySheets_CompanyId] ON [DailySalarySheets] ([CompanyId]);
GO


CREATE INDEX [IX_DailySalarySheets_EmployeeId] ON [DailySalarySheets] ([EmployeeId]);
GO


CREATE INDEX [IX_Departments_CompanyId] ON [Departments] ([CompanyId]);
GO


CREATE INDEX [IX_Designations_CompanyId] ON [Designations] ([CompanyId]);
GO


CREATE INDEX [IX_Designations_DepartmentId] ON [Designations] ([DepartmentId]);
GO


CREATE INDEX [IX_Designations_SectionId] ON [Designations] ([SectionId]);
GO


CREATE INDEX [IX_Districts_DivisionId] ON [Districts] ([DivisionId]);
GO


CREATE INDEX [IX_Divisions_CountryId] ON [Divisions] ([CountryId]);
GO


CREATE INDEX [IX_Employees_CompanyId] ON [Employees] ([CompanyId]);
GO


CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
GO


CREATE INDEX [IX_Employees_DesignationId] ON [Employees] ([DesignationId]);
GO


CREATE UNIQUE INDEX [IX_Employees_EmployeeId_CompanyName] ON [Employees] ([EmployeeId], [CompanyName]) WHERE [CompanyName] IS NOT NULL;
GO


CREATE INDEX [IX_Employees_FloorId] ON [Employees] ([FloorId]);
GO


CREATE INDEX [IX_Employees_GroupId] ON [Employees] ([GroupId]);
GO


CREATE INDEX [IX_Employees_LineId] ON [Employees] ([LineId]);
GO


CREATE UNIQUE INDEX [IX_Employees_Proximity_CompanyName] ON [Employees] ([Proximity], [CompanyName]) WHERE [Proximity] IS NOT NULL AND [CompanyName] IS NOT NULL;
GO


CREATE INDEX [IX_Employees_SectionId] ON [Employees] ([SectionId]);
GO


CREATE INDEX [IX_Employees_ShiftId] ON [Employees] ([ShiftId]);
GO


CREATE INDEX [IX_Employees_UserId] ON [Employees] ([UserId]);
GO


CREATE INDEX [IX_EmployeeShiftRosters_EmployeeId] ON [EmployeeShiftRosters] ([EmployeeId]);
GO


CREATE INDEX [IX_EmployeeShiftRosters_ShiftId] ON [EmployeeShiftRosters] ([ShiftId]);
GO


CREATE INDEX [IX_Floors_CompanyId] ON [Floors] ([CompanyId]);
GO


CREATE UNIQUE INDEX [IX_Floors_NameEn_CompanyId] ON [Floors] ([NameEn], [CompanyId]) WHERE [CompanyId] IS NOT NULL;
GO


CREATE INDEX [IX_Groups_CompanyId] ON [Groups] ([CompanyId]);
GO


CREATE UNIQUE INDEX [IX_Groups_NameEn_CompanyId] ON [Groups] ([NameEn], [CompanyId]) WHERE [CompanyId] IS NOT NULL;
GO


CREATE INDEX [IX_LeaveApplications_EmployeeId] ON [LeaveApplications] ([EmployeeId]);
GO


CREATE INDEX [IX_LeaveApplications_LeaveTypeId] ON [LeaveApplications] ([LeaveTypeId]);
GO


CREATE INDEX [IX_Lines_CompanyId] ON [Lines] ([CompanyId]);
GO


CREATE INDEX [IX_Lines_DepartmentId] ON [Lines] ([DepartmentId]);
GO


CREATE INDEX [IX_Lines_SectionId] ON [Lines] ([SectionId]);
GO


CREATE INDEX [IX_ManpowerRequirements_DepartmentId] ON [ManpowerRequirements] ([DepartmentId]);
GO


CREATE INDEX [IX_ManpowerRequirements_DesignationId] ON [ManpowerRequirements] ([DesignationId]);
GO


CREATE INDEX [IX_MonthlySalarySheets_CompanyId] ON [MonthlySalarySheets] ([CompanyId]);
GO


CREATE INDEX [IX_MonthlySalarySheets_EmployeeId] ON [MonthlySalarySheets] ([EmployeeId]);
GO


CREATE INDEX [IX_OtDeductions_EmployeeId] ON [OtDeductions] ([EmployeeId]);
GO


CREATE INDEX [IX_PostOffices_DistrictId] ON [PostOffices] ([DistrictId]);
GO


CREATE INDEX [IX_SalaryIncrements_CompanyId] ON [SalaryIncrements] ([CompanyId]);
GO


CREATE INDEX [IX_SalaryIncrements_EmployeeId] ON [SalaryIncrements] ([EmployeeId]);
GO


CREATE INDEX [IX_Sections_CompanyId] ON [Sections] ([CompanyId]);
GO


CREATE INDEX [IX_Sections_DepartmentId] ON [Sections] ([DepartmentId]);
GO


CREATE INDEX [IX_Separations_EmployeeId] ON [Separations] ([EmployeeId]);
GO


CREATE INDEX [IX_Shifts_CompanyId] ON [Shifts] ([CompanyId]);
GO


CREATE UNIQUE INDEX [IX_Shifts_NameEn_CompanyId] ON [Shifts] ([NameEn], [CompanyId]) WHERE [CompanyId] IS NOT NULL;
GO


CREATE INDEX [IX_Thanas_DistrictId] ON [Thanas] ([DistrictId]);
GO


CREATE INDEX [IX_Transfers_EmployeeId] ON [Transfers] ([EmployeeId]);
GO


CREATE INDEX [IX_Transfers_FromDepartmentId] ON [Transfers] ([FromDepartmentId]);
GO


CREATE INDEX [IX_Transfers_FromDesignationId] ON [Transfers] ([FromDesignationId]);
GO


CREATE INDEX [IX_Transfers_ToDepartmentId] ON [Transfers] ([ToDepartmentId]);
GO


CREATE INDEX [IX_Transfers_ToDesignationId] ON [Transfers] ([ToDesignationId]);
GO


