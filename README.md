# Notification Service

A microservice for handling notification requests via SMS, Email, and In-App channels.

## Overview

This service receives notification requests through three separate controllers and processes them according to the channel type:
- **SMS & Email**: Records are inserted into the database, then pushed to Azure Queue Storage
- **In-App**: Records are only inserted into the database

## Architecture

The service follows Clean Architecture principles with the following layers:
- **API Layer**: Controllers for SMS, Email, and App notifications
- **Application Layer**: Business logic and DTOs
- **Domain Layer**: Entities and repository interfaces
- **Infrastructure Layer**: Database and Azure Queue implementations

## Database Schema

The service uses PostgreSQL with the following table structure:

```sql
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
```

Run the schema with:
```bash
psql -U username -d database -f database/schema.sql
```

## API Endpoints

### SMS Controller (`/api/sms`)

#### Forget Password
```
POST /api/sms/forget-password?phone={phone}&code={code}
```

#### Verification
```
POST /api/sms/verification?phone={phone}&code={code}
```

### Email Controller (`/api/email`)

#### Forget Password
```
POST /api/email/forget-password?email={email}&code={code}
```

#### Welcome
```
POST /api/email/welcome?email={email}&firstName={firstName}
```

#### Verification
```
POST /api/email/verification?email={email}&code={code}
```

### App Controller (`/api/app`)

#### New Message
```
POST /api/app/new-message?id={id}&message={message}&from={from}
```

#### Review Approved
```
POST /api/app/review-approved?id={id}&reviewId={reviewId}
```

#### Review Rejected
```
POST /api/app/review-rejected?id={id}&reviewId={reviewId}&reason={reason}
```

## Response Format

For SMS and Email channels, the response follows this format:

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "template": "forget-password",
  "channel": "email",
  "retryCount": 0,
  "recipient": "user@example.com",
  "payload": {
    "email": "user@example.com",
    "code": "123456"
  },
  "requestedAt": "2026-01-18T12:00:00Z"
}
```

For In-App notifications:
```json
{
  "message": "In-app notification created successfully"
}
```

## Configuration

Update `appsettings.json` with your database and Azure Queue Storage connection strings:

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=reviewapp;Username=username;Password=password",
    "AzureQueueStorage": "DefaultEndpointsProtocol=https;AccountName=clereviewst;AccountKey=YOUR_KEY_HERE;EndpointSuffix=core.windows.net"
  }
}
```

## Running the Service

1. Restore dependencies:
```bash
dotnet restore
```

2. Run database migrations:
```bash
psql -U username -d database -f database/schema.sql
```

3. Run the service:
```bash
dotnet run --project src/NotificationService.Api/NotificationService.Api.csproj
```

The API will be available at `https://localhost:5001` (or the configured port).

## Running Tests

```bash
dotnet test
```

## Azure Queue Storage

For SMS and Email notifications, the service pushes messages to an Azure Queue Storage at:
```
https://clereviewst.queue.core.windows.net/notifications
```

After successfully pushing to the queue, the notification status is updated to `"pushed"`.

## Development

### Adding New Endpoints

1. Add the endpoint to the appropriate controller (SmsController, EmailController, or AppController)
2. Ensure required parameters are validated
3. Call `_notificationService.ProcessNotificationAsync()` with the template name, channel, recipient, and payload
4. Add corresponding tests in the test project

### Technology Stack

- .NET 9.0
- PostgreSQL with Dapper
- Azure Storage Queues
- NUnit for testing
- Moq for mocking

## License

MIT
