# Applications Aggregation View

The Applications Aggregation View allows you to group multiple health check endpoints into logical applications and view their aggregated health status.

## Configuration

To enable the Applications view, add an `Applications` section to your `appsettings.json` under `HealthChecksUI`:

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Flowmanager - Front",
        "Uri": "https://flowmanager-front.example.com/health"
      },
      {
        "Name": "Flowmanager - Back",
        "Uri": "https://flowmanager-back.example.com/health"
      },
      {
        "Name": "Flowmanager - API",
        "Uri": "https://flowmanager-api.example.com/health"
      },
      {
        "Name": "Sinbad",
        "Uri": "https://sinbad.example.com/health"
      }
    ],
    "Applications": {
      "Flowmanager": {
        "Members": [
          "Flowmanager - Front",
          "Flowmanager - Back",
          "Flowmanager - API"
        ]
      },
      "SinbadApp": {
        "Members": [
          "Sinbad"
        ]
      }
    },
    "EvaluationTimeinSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

## Features

### Applications Grid View

When applications are configured, a new "Applications" menu item appears in the UI navigation. This view shows:

- **Application tiles** with aggregated status (Healthy, Degraded, Unhealthy, or Unknown)
- **Health summary**: Number of healthy services vs. total services (e.g., "3/3 services OK")
- **Average latency**: Average response time across all member services
- **Last update timestamp**: When the application was last checked

### Application Details

Click on any application tile to view detailed information about its member services:

- **Service name and URI**: Each member service is listed with a link to open its health endpoint
- **Individual status**: Status of each member service
- **Response time**: Latency for each service
- **Payload viewer**: Click "View Payload" to see the raw health check response in JSON format

### Status Aggregation Rules

The application status is calculated based on member service statuses:

- **Unhealthy**: If any member is Unhealthy or Unreachable
- **Degraded**: If any member is Degraded (and none are Unhealthy)
- **Healthy**: If all members are Healthy
- **Unknown**: If status cannot be determined

## API Endpoints

When applications are configured, the following API endpoints are available:

- `GET /api/health/applications` - Returns all configured applications with their aggregated health status
- `GET /api/health/applications/{name}` - Returns details for a specific application

### Response Format

```json
{
  "applications": [
    {
      "name": "Flowmanager",
      "status": "Healthy",
      "healthyCount": 3,
      "totalCount": 3,
      "averageDurationMs": 120.5,
      "checkedAt": "2025-11-09T17:00:00Z",
      "members": [
        {
          "name": "Flowmanager - Front",
          "uri": "https://flowmanager-front.example.com/health",
          "status": "Healthy",
          "durationMs": 110,
          "payload": "{\"status\":\"Healthy\"}"
        },
        {
          "name": "Flowmanager - Back",
          "uri": "https://flowmanager-back.example.com/health",
          "status": "Healthy",
          "durationMs": 130,
          "payload": "{\"status\":\"Healthy\"}"
        }
      ]
    }
  ]
}
```

## Backward Compatibility

If no applications are configured, the UI automatically falls back to the standard Health Checks view showing individual endpoints. The Applications API endpoints will not be registered if the `Applications` configuration section is missing or empty.

## Polling and Auto-Refresh

The Applications view uses the same polling interval configured in `EvaluationTimeinSeconds`. You can pause/resume auto-refresh using the toggle in the header.
