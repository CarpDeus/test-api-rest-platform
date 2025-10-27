# test-api-rest-platform

A .NET Core Web API application that returns specific HTTP status codes. This application includes Serilog.Sinks.File for logging all requests to file and Swagger UI for interactive API documentation.

## Features

- Controllers for each HTTP method (GET, POST, PUT, DELETE, PATCH)
- Support for returning any HTTP status code via `api/{method}/{status}` routes
- Authentication endpoint that validates Authorization headers
- Validation endpoint that validates request bodies against a predefined model
- Comprehensive logging using Serilog with file output
- **Swagger UI for interactive API documentation**

## Swagger UI

The API includes Swagger UI for easy exploration and testing of all endpoints. Once the application is running, navigate to the root URL to access the interactive documentation:

**URL:** `http://localhost:5180/` (or your configured port)

The Swagger UI provides:
- Complete API documentation
- Interactive testing of all endpoints
- Request/response examples
- Schema definitions

## API Endpoints

### Status Code Endpoints

Returns the specified HTTP status code:

- `GET /api/get/{status}` - Returns specified status code
- `POST /api/post/{status}` - Returns specified status code  
- `PUT /api/put/{status}` - Returns specified status code
- `DELETE /api/delete/{status}` - Returns specified status code
- `PATCH /api/patch/{status}` - Returns specified status code

**Example:**
```bash
curl http://localhost:5555/api/get/200
curl -X POST http://localhost:5555/api/post/201
```

### Authentication Endpoints

Validates the Authorization header against a predefined value in configuration:

- `GET /api/get/authenticate`
- `POST /api/post/authenticate`
- `PUT /api/put/authenticate`
- `DELETE /api/delete/authenticate`
- `PATCH /api/patch/authenticate`

**Responses:**
- 401 - No Authorization header provided
- 200 - Authorization header matches expected value
- 403 - Authorization header does not match expected value

**Example:**
```bash
# No auth header - returns 401
curl http://localhost:5555/api/get/authenticate

# Correct auth header - returns 200
curl -H "Authorization: Bearer test-secret-key" http://localhost:5555/api/get/authenticate

# Wrong auth header - returns 403
curl -H "Authorization: Bearer wrong-key" http://localhost:5555/api/get/authenticate
```

### Validation Endpoints

Validates request data against a predefined model (requires `name` and `email` fields):

- `GET /api/get/validate` - Uses query parameters
- `POST /api/post/validate` - Uses request body
- `PUT /api/put/validate` - Uses request body
- `DELETE /api/delete/validate` - Uses query parameters
- `PATCH /api/patch/validate` - Uses request body

**Responses:**
- 200 - Validation successful
- 400 - Validation failed (missing or invalid fields)

**Example:**
```bash
# GET with query parameters - returns 200
curl "http://localhost:5555/api/get/validate?name=John%20Doe&email=john@example.com"

# POST with request body - returns 200
curl -X POST -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}' \
  http://localhost:5555/api/post/validate

# Invalid data - returns 400
curl -X POST -H "Content-Type: application/json" \
  -d '{"name":"John Doe"}' \
  http://localhost:5555/api/post/validate
```

## Configuration

Edit `appsettings.json` to configure:

- **Authentication.ExpectedAuthHeader**: The expected Authorization header value (default: "Bearer test-secret-key")
- **Serilog**: Logging configuration including file output location and format

## Running the Application

```bash
cd TestApiRestPlatform
dotnet run
```

The application will start on http://localhost:5180 by default (or as configured in launchSettings.json). The Swagger UI will automatically open in your browser.

## Logging

All API requests are logged using structured logging with Serilog. Logs are written to both:
- **Console** - Real-time output for development
- **File** - `logs/api-{date}.txt` for persistent storage

Each log entry includes comprehensive request details:
- **Endpoint** - The API path being accessed
- **HTTP Method** - GET, POST, PUT, DELETE, PATCH
- **Source IP Address** - The client's IP address
- **Headers** - All request headers (Authorization headers are sanitized)
- **Query Parameters** - All URL query string parameters
- **Request Body** - The complete request body (if present)
- **Status Code** - HTTP response status code

Example log entry:
```
2025-10-27 20:54:19.500 +00:00 [INF] API Request: /api/post/validate POST from ::1 - Status: 200. 
Headers: {"Accept":"*/*","Host":"localhost:5180","Authorization":"Bearer test-secret-key"}, 
QueryParams: {}, 
Body: {"name":"John","email":"john@example.com"}
```
