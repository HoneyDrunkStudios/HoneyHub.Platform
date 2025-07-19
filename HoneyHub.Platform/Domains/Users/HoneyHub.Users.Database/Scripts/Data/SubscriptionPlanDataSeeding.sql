-- =================================================================
-- Initial Data Seeding (Idempotent)
-- =================================================================

PRINT 'Starting SubscriptionPlan data seeding...';

-- Check if any subscription plans already exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlan])
BEGIN
    PRINT 'No existing subscription plans found. Inserting default plans...';
    
    -- Insert default subscription plans
    INSERT INTO [dbo].[SubscriptionPlan] (
        [Name], [DisplayName], [Description], [ShortDescription], [Price], [Currency], 
        [BillingCycle], [BillingIntervalMonths], [IsDefault], [SortOrder], [MaxProjects], 
        [MaxActiveWorkItemsPerProject], [CallToAction], [TrialDays]
    ) VALUES 
    -- Free Tier (Default)
    (
        'Free', 
        'Free Plan', 
        'Perfect for getting started with basic features and community support.', 
        'Basic features with community support',
        0.00, 
        'USD', 
        'Monthly', 
        1, 
        1, -- IsDefault
        1, -- SortOrder
        1, -- MaxProjects
        10, -- MaxActiveWorkItemsPerProject
        'Get Started Free',
        NULL
    ),
    -- Premium Tier
    (
        'Premium', 
        'Premium Plan', 
        'Advanced features for growing teams with priority support and enhanced analytics.', 
        'Advanced features with priority support',
        29.99, 
        'USD', 
        'Monthly', 
        1, 
        0, -- IsDefault
        2, -- SortOrder
        5, -- MaxProjects
        25, -- MaxActiveWorkItemsPerProject
        'Start Premium Trial',
        14
    ),
    -- Enterprise Tier
    (
        'Enterprise', 
        'Enterprise Plan', 
        'Unlimited features for large organizations with dedicated support and custom integrations.', 
        'Unlimited features with dedicated support',
        99.99, 
        'USD', 
        'Monthly', 
        1, 
        0, -- IsDefault
        3, -- SortOrder
        NULL, -- MaxProjects
        NULL, -- MaxActiveWorkItemsPerProject
        'Contact Sales',
        30
    );
    
    PRINT 'Default subscription plans inserted successfully. Total plans: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'Subscription plans already exist. Checking for missing plans...';
    
    -- Insert individual plans if they don't exist
    IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlan] WHERE [Name] = 'Free')
    BEGIN
        INSERT INTO [dbo].[SubscriptionPlan] (
            [Name], [DisplayName], [Description], [ShortDescription], [Price], [Currency], 
            [BillingCycle], [BillingIntervalMonths], [IsDefault], [SortOrder], [MaxProjects], 
            [MaxActiveWorkItemsPerProject], [CallToAction], [TrialDays]
        ) VALUES (
            'Free', 'Free Plan', 
            'Perfect for getting started with basic features and community support.', 
            'Basic features with community support', 0.00, 'USD', 'Monthly', 1, 1, 1, 1, 10,
            'Get Started Free', NULL
        );
        PRINT 'Free plan inserted.';
    END
    ELSE
    BEGIN
        PRINT 'Free plan already exists.';
    END
    
    IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlan] WHERE [Name] = 'Premium')
    BEGIN
        INSERT INTO [dbo].[SubscriptionPlan] (
            [Name], [DisplayName], [Description], [ShortDescription], [Price], [Currency], 
            [BillingCycle], [BillingIntervalMonths], [IsDefault], [SortOrder], [MaxProjects], 
            [MaxActiveWorkItemsPerProject], [CallToAction], [TrialDays]
        ) VALUES (
            'Premium', 'Premium Plan', 
            'Advanced features for growing teams with priority support and enhanced analytics.', 
            'Advanced features with priority support', 29.99, 'USD', 'Monthly', 1, 0, 2, 5, 25,
            'Start Premium Trial', 14
        );
        PRINT 'Premium plan inserted.';
    END
    ELSE
    BEGIN
        PRINT 'Premium plan already exists.';
    END
    
    IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlan] WHERE [Name] = 'Enterprise')
    BEGIN
        INSERT INTO [dbo].[SubscriptionPlan] (
            [Name], [DisplayName], [Description], [ShortDescription], [Price], [Currency], 
            [BillingCycle], [BillingIntervalMonths], [IsDefault], [SortOrder], [MaxProjects], 
            [MaxActiveWorkItemsPerProject], [CallToAction], [TrialDays]
        ) VALUES (
            'Enterprise', 'Enterprise Plan', 
            'Unlimited features for large organizations with dedicated support and custom integrations.', 
            'Unlimited features with dedicated support', 99.99, 'USD', 'Monthly', 1, 0, 3, NULL, NULL,
            'Contact Sales', 30
        );
        PRINT 'Enterprise plan inserted.';
    END
    ELSE
    BEGIN
        PRINT 'Enterprise plan already exists.';
    END
END

-- Ensure we have a default plan
IF NOT EXISTS (SELECT 1 FROM [dbo].[SubscriptionPlan] WHERE [IsDefault] = 1 AND [IsActive] = 1)
BEGIN
    PRINT 'No default plan found. Setting Free plan as default...';
    UPDATE [dbo].[SubscriptionPlan] 
    SET [IsDefault] = 1, [UpdatedAt] = SYSUTCDATETIME(), [UpdatedBy] = 'DataSeeding'
    WHERE [Name] = 'Free' AND [IsActive] = 1;
    PRINT 'Free plan set as default.';
END
ELSE
BEGIN
    PRINT 'Default plan already exists.';
END

-- Display summary
DECLARE @TotalPlans INT = (SELECT COUNT(*) FROM [dbo].[SubscriptionPlan]);
DECLARE @ActivePlans INT = (SELECT COUNT(*) FROM [dbo].[SubscriptionPlan] WHERE [IsActive] = 1);
DECLARE @DefaultPlans INT = (SELECT COUNT(*) FROM [dbo].[SubscriptionPlan] WHERE [IsDefault] = 1);

PRINT 'Data seeding completed successfully.';
PRINT 'Total plans: ' + CAST(@TotalPlans AS VARCHAR(10));
PRINT 'Active plans: ' + CAST(@ActivePlans AS VARCHAR(10));
PRINT 'Default plans: ' + CAST(@DefaultPlans AS VARCHAR(10));