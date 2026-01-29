-- Drop the old table if it exists
DROP TABLE IF EXISTS public.notification;

-- Create the new notifications table
CREATE TABLE IF NOT EXISTS public.notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template TEXT DEFAULT null,
    channel TEXT DEFAULT null,
    retry_count INT DEFAULT 0,
    recipient TEXT DEFAULT null,
    payload JSONB DEFAULT null,
    requested_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    delivered_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(100) DEFAULT 'sent'
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_notifications_status ON public.notifications(status);
CREATE INDEX IF NOT EXISTS idx_notifications_channel ON public.notifications(channel);
CREATE INDEX IF NOT EXISTS idx_notifications_requested_at ON public.notifications(requested_at);

-- Create the otp table for one-time passwords
CREATE TABLE IF NOT EXISTS public.otp (
    id TEXT NOT NULL,
    code VARCHAR(6) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    PRIMARY KEY (id, code)
);

-- Create index for faster OTP lookups
CREATE INDEX IF NOT EXISTS idx_otp_id ON public.otp(id);
CREATE INDEX IF NOT EXISTS idx_otp_expires_at ON public.otp(expires_at);

-- Create the password_reset_requests table
CREATE TABLE IF NOT EXISTS public.password_reset_requests (
    id TEXT PRIMARY KEY,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL
);

-- Create index for password reset requests
CREATE INDEX IF NOT EXISTS idx_password_reset_requests_expires_at ON public.password_reset_requests(expires_at);
