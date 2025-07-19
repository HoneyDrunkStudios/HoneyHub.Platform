-- =================================================================
-- SubscriptionPlan Table Creation Script
-- Domain: HoneyHub.Users.Database
-- Purpose: Subscription plan management for billing and feature control
-- =================================================================

CREATE TABLE [dbo].[SubscriptionPlan] (
    -- Primary Identity
    [Id]                    INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_SubscriptionPlan PRIMARY KEY CLUSTERED,
    
    -- Plan Details
    [Name]                  NVARCHAR(100) NOT NULL
        CONSTRAINT UK_SubscriptionPlan_Name UNIQUE NONCLUSTERED,
    
    [DisplayName]           NVARCHAR(100) NOT NULL,        -- User-friendly display name
    [Description]           NVARCHAR(500) NULL,            -- Marketing description
    [ShortDescription]      NVARCHAR(150) NULL,            -- Brief summary for cards/lists
    
    -- Pricing Information
    [Price]                 DECIMAL(10,2) NOT NULL
        CONSTRAINT CK_SubscriptionPlan_Price_NonNegative CHECK ([Price] >= 0),
    
    [Currency]              NCHAR(3) NOT NULL               -- ISO 4217 currency code
        CONSTRAINT DF_SubscriptionPlan_Currency DEFAULT 'USD',
    
    [BillingCycle]          NVARCHAR(20) NOT NULL           -- Monthly, Yearly, Lifetime, etc.
        CONSTRAINT CK_SubscriptionPlan_BillingCycle 
        CHECK ([BillingCycle] IN ('Monthly', 'Yearly', 'Lifetime', 'Custom')),
    
    [BillingIntervalMonths] INT NOT NULL                    -- Number of months per billing cycle
        CONSTRAINT DF_SubscriptionPlan_BillingIntervalMonths DEFAULT 1
        CONSTRAINT CK_SubscriptionPlan_BillingIntervalMonths_Positive CHECK ([BillingIntervalMonths] > 0),
    
    -- Plan Status and Availability
    [IsActive]              BIT NOT NULL
        CONSTRAINT DF_SubscriptionPlan_IsActive DEFAULT 1,
    
    [IsPublic]              BIT NOT NULL                    -- Whether plan is publicly available
        CONSTRAINT DF_SubscriptionPlan_IsPublic DEFAULT 1,
    
    [IsDefault]             BIT NOT NULL                    -- Default plan for new users
        CONSTRAINT DF_SubscriptionPlan_IsDefault DEFAULT 0,
    
    [SortOrder]             INT NOT NULL                    -- Display order in UI
        CONSTRAINT DF_SubscriptionPlan_SortOrder DEFAULT 100,
    
    -- Feature Configuration (JSON)
    [Features]              NVARCHAR(MAX) NULL,             -- JSON feature configuration
    [Limitations]           NVARCHAR(MAX) NULL,             -- JSON usage limitations
    
    -- Marketing and Display
    [HighlightFeatures]     NVARCHAR(MAX) NULL,             -- JSON key selling points
    [CallToAction]          NVARCHAR(50) NULL,              -- Button text (e.g., "Get Started", "Contact Sales")
    [PopularBadge]          BIT NOT NULL                    -- Show "Most Popular" badge
        CONSTRAINT DF_SubscriptionPlan_PopularBadge DEFAULT 0,
    
    -- Trial Configuration
    [TrialDays]             INT NULL                        -- Number of trial days (NULL = no trial)
        CONSTRAINT CK_SubscriptionPlan_TrialDays_NonNegative CHECK ([TrialDays] IS NULL OR [TrialDays] >= 0),
    
    -- Audit Fields
    [CreatedAt]             DATETIME2 NOT NULL
        CONSTRAINT DF_SubscriptionPlan_CreatedAt DEFAULT SYSUTCDATETIME(),
    
    [CreatedBy]             NVARCHAR(100) NOT NULL
        CONSTRAINT DF_SubscriptionPlan_CreatedBy DEFAULT 'System',
    
    [UpdatedAt]             DATETIME2 NULL,
    [UpdatedBy]             NVARCHAR(100) NULL,
    
    -- Version Control for Price Changes
    [Version]               INT NOT NULL
        CONSTRAINT DF_SubscriptionPlan_Version DEFAULT 1,
    
    [EffectiveFrom]         DATETIME2 NOT NULL
        CONSTRAINT DF_SubscriptionPlan_EffectiveFrom DEFAULT SYSUTCDATETIME(),
    
    [EffectiveTo]           DATETIME2 NULL                  -- NULL means current version
);