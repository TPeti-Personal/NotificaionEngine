-- 1. FELHASZNÁLÓK
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- INTEGRÁCIÓK
CREATE TABLE user_integrations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL,
    credentials JSONB NOT NULL, 
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_user_provider UNIQUE (user_id, provider)
);

-- 2. ALERT SZABÁLYOK ÉS BEÁLLÍTÁSOK
CREATE TABLE alert_rules (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(150) NOT NULL, -- pl. "BTC Árfigyelő 5% felett"
    topic_pattern VARCHAR(100) NOT NULL, 
    conditions JSONB DEFAULT '{}'::jsonb, 
    is_enabled BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Egy szabályhoz több csatorna is tartozhat (pl. Ugyanaz a riasztás menjen Emailre ÉS Slackre is)
CREATE TABLE alert_channels (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    alert_rule_id UUID NOT NULL REFERENCES alert_rules(id) ON DELETE CASCADE,
    channel_type VARCHAR(50) NOT NULL, 
    channel_config JSONB DEFAULT '{}'::jsonb, 
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_rule_channel UNIQUE (alert_rule_id, channel_type)
);

-- 3. TELJESÍTMÉNY-OPTIMALIZÁLÓ INDEXEK
CREATE INDEX idx_alert_rules_topic ON alert_rules(topic_pattern) WHERE is_enabled = TRUE;
CREATE INDEX idx_user_integrations_provider ON user_integrations(user_id, provider) WHERE is_active = TRUE;
