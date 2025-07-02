# User Authentication System with Account Locking

This system implements user authentication with automatic account locking after 3 failed password attempts.

## Features

- **Account Locking**: Users are locked out after 3 consecutive failed login attempts
- **Automatic Unlock**: Accounts automatically unlock after 30 minutes
- **Manual Unlock**: Administrators can manually unlock accounts
- **Account Status Check**: Check the current status of any user account
- **Comprehensive Logging**: All authentication events are logged

## Database Changes

### New User Model Fields

The following fields have been added to the `User` model:

```csharp
public int FailedLoginAttempts { get; set; } = 0;
public bool IsLockedOut { get; set; } = false;
public DateTime? LockoutEnd { get; set; }
public DateTime? LastLoginAttempt { get; set; }
```

### Database Migration

1. **Using Entity Framework Migration**:
   ```bash
   dotnet ef database update
   ```

2. **Using SQL Script** (if EF migration fails):
   Run the SQL script located at: `Server/SingularExpress.Models/Migrations/AddAuthenticationFields.sql`

## API Endpoints

### Authentication Controller (`/api/auth`)

#### 1. Login
- **Endpoint**: `POST /api/auth/login`
- **Body**:
  ```json
  {
    "email": "user@example.com",
    "password": "userpassword"
  }
  ```
- **Response**:
  ```json
  {
    "isSuccess": true,
    "message": "Login successful.",
    "user": { /* user object without password */ },
    "isLockedOut": false,
    "lockoutEnd": null,
    "remainingAttempts": 3
  }
  ```

#### 2. Unlock Account
- **Endpoint**: `POST /api/auth/unlock-account`
- **Body**: `"user@example.com"` (JSON string)
- **Response**: Success message

#### 3. Account Status
- **Endpoint**: `GET /api/auth/account-status/{email}`
- **Response**:
  ```json
  {
    "email": "user@example.com",
    "isLockedOut": false,
    "failedAttempts": 0,
    "maxAttempts": 3,
    "remainingAttempts": 3,
    "lockoutEnd": null,
    "lastLoginAttempt": "2024-01-27T10:30:00Z"
  }
  ```

### User Test Controller (`/api/usertest`)

#### 1. Create Test User
- **Endpoint**: `POST /api/usertest/create-test-user`
- **Body**:
  ```json
  {
    "userName": "testuser",
    "email": "test@example.com",
    "password": "testpassword123",
    "firstName": "Test",
    "lastName": "User"
  }
  ```

#### 2. Get All Users
- **Endpoint**: `GET /api/usertest/all-users`

#### 3. Delete User
- **Endpoint**: `DELETE /api/usertest/delete-user/{email}`

## How the Locking System Works

### Login Process
1. User submits email and password
2. System checks if account exists
3. System checks if account is currently locked
4. If locked and lockout period expired → auto-unlock
5. If locked and lockout period active → return lockout message
6. Verify password:
   - **Success**: Reset failed attempts counter and allow login
   - **Failure**: Increment failed attempts counter

### Locking Logic
- **Failed Attempts Tracking**: Each failed login increments the counter
- **Lockout Trigger**: After 3 failed attempts, account is locked
- **Lockout Duration**: 30 minutes from the time of lockout
- **Auto-unlock**: System automatically unlocks expired lockouts
- **Reset on Success**: Successful login resets the failed attempts counter

### Security Features
- Passwords are hashed using SHA256 (upgrade to BCrypt recommended for production)
- Failed attempts are tracked per user
- Lockout times are stored in UTC
- All authentication events are logged
- Email lookup is case-insensitive

## Testing the System

### 1. Create a Test User
```bash
curl -X POST https://your-api-url/api/usertest/create-test-user \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "email": "test@example.com", 
    "password": "correctpassword",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Test Failed Login Attempts
```bash
# First failed attempt
curl -X POST https://your-api-url/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "wrongpassword"
  }'

# Repeat 2 more times to trigger lockout
```

### 3. Check Account Status
```bash
curl -X GET https://your-api-url/api/auth/account-status/test@example.com
```

### 4. Test Manual Unlock
```bash
curl -X POST https://your-api-url/api/auth/unlock-account \
  -H "Content-Type: application/json" \
  -d '"test@example.com"'
```

## Configuration

### Constants (in AuthController.cs)
```csharp
private const int MaxFailedAttempts = 3;           // Number of attempts before lockout
private const int LockoutDurationMinutes = 30;     // Lockout duration in minutes
```

### Recommended Production Changes
1. **Password Hashing**: Replace SHA256 with BCrypt or Argon2
2. **Rate Limiting**: Add IP-based rate limiting
3. **Captcha**: Add captcha after first failed attempt
4. **Audit Trail**: Enhanced logging with IP addresses and user agents
5. **Email Notifications**: Notify users when their account is locked
6. **Admin Dashboard**: Create UI for managing locked accounts

## Error Handling

The system includes comprehensive error handling:
- Database connection errors
- Invalid input validation
- Logging of all authentication events
- Graceful degradation on errors

## Security Considerations

### Current Implementation
- Basic password hashing (SHA256)
- Account lockout protection
- Case-insensitive email lookup
- UTC time handling

### Production Recommendations
- Implement stronger password hashing (BCrypt/Argon2)
- Add two-factor authentication
- Implement IP-based rate limiting
- Add email verification
- Use secure session management
- Implement password complexity requirements
- Add audit logging with IP addresses

## Monitoring

The system logs the following events:
- Successful logins
- Failed login attempts
- Account lockouts
- Manual unlocks
- Database errors

Monitor these logs for security incidents and system health.