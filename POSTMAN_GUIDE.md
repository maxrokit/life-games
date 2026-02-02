# Postman Collection Guide

This guide explains how to use the Life Games API Postman collection for testing.

## Files

- `Life-Games-API.postman_collection.json` - Complete API test collection
- `Life-Games-API.postman_environment.json` - Local development environment

## Import into Postman

1. Open Postman
2. Click **Import** button
3. Drag and drop both JSON files or click **Choose Files**
4. Select both files:
   - `Life-Games-API.postman_collection.json`
   - `Life-Games-API.postman_environment.json`

## Setup

1. **Select Environment**: In the top-right corner, select "Life Games - Local Development" from the environment dropdown
2. **Start API**: Make sure the API is running locally:
   ```bash
   cd src/life-games-api/Api/LifeGames.Api
   dotnet run
   ```
3. **Verify**: The API should be running at `http://localhost:5253`

## Collection Structure

### 1. Boards
Main API endpoints for board management:

- **Create Board - Glider** - Creates a glider pattern (auto-saves board ID)
- **Create Board - Blinker** - Period-2 oscillator
- **Create Board - Block** - Still life (never changes)
- **Create Board - Pulsar** - Period-3 oscillator
- **Get Board State** - Retrieves generation 0
- **Get Next Generation** - Gets N+1 generation
- **Get Specific Generation (N=5)** - Jump to generation 5
- **Get Specific Generation (N=100)** - Jump to generation 100
- **Get Final State** - Gets stable state with cycle detection

### 2. Content Negotiation
Demonstrates JSON/XML support:

- **Get Board State (XML)** - Returns XML response
- **Get Next Generation (JSON)** - Returns JSON response

### 3. Error Handling
Tests error responses:

- **Get Non-Existent Board** - Tests 404 handling
- **Create Board - Invalid Request** - Tests validation (400)
- **Get Invalid Generation Number** - Tests negative number validation

### 4. Health Checks
Container orchestration endpoints:

- **Health Check** - Basic liveness check
- **Readiness Check** - Database connectivity check

## Quick Start Workflow

1. **Create a Board**: Run "Create Board - Glider"
   - This automatically saves the board ID to the `boardId` variable
   - Check the test results to confirm successful creation

2. **Get Generations**: Use the saved board ID to:
   - Get initial state: "Get Board State"
   - Get next: "Get Next Generation"
   - Jump ahead: "Get Specific Generation (N=5)"

3. **Final State**: Run "Get Final State"
   - For Glider: Will compute until max iterations or cycle detected
   - For Block: Will detect as still life (cycle length 1)
   - For Blinker: Will detect as period-2 oscillator

## Features

### Automatic Board ID Management
The collection automatically captures and stores board IDs from create requests. No need to manually copy/paste IDs between requests.

### Built-in Tests
Each request includes automated tests that verify:
- Correct HTTP status codes
- Response structure
- HATEOAS links
- Content types
- Cycle detection fields

### Environment Variables
- `baseUrl` - HTTP endpoint (default: `http://localhost:5253`)
- `baseUrlHttps` - HTTPS endpoint (default: `https://localhost:7230`)
- `boardId` - Auto-populated from create board requests
- `apiVersion` - API version header

## API Versioning

The API uses media type versioning. All requests include:
```
Accept: application/vnd.lifegames.v1+json
```

To request XML instead of JSON:
```
Accept: application/vnd.lifegames.v1+xml
```

## Common Patterns

### Game of Life Patterns Included

1. **Glider** - Classic spaceship that moves diagonally
2. **Blinker** - Simple period-2 oscillator
3. **Block** - 2x2 still life
4. **Pulsar** - Large period-3 oscillator

### Create Your Own Pattern

Use the "Create Board - Glider" request as a template and modify the cells array:

```json
{
  "name": "My Custom Pattern",
  "cells": [
    { "x": 0, "y": 0 },
    { "x": 1, "y": 0 },
    { "x": 2, "y": 0 }
  ]
}
```

## Response Structure

### Board Response
```json
{
  "id": "guid",
  "name": "Pattern Name",
  "createdAt": "2026-02-01T...",
  "generationNumber": 0,
  "cells": [
    { "x": 1, "y": 0 },
    { "x": 2, "y": 1 }
  ],
  "links": {
    "self": { "href": "/api/boards/{id}" },
    "next": { "href": "/api/boards/{id}/next" },
    "final": { "href": "/api/boards/{id}/final" }
  }
}
```

### Final State Response
```json
{
  "boardId": "guid",
  "finalGenerationNumber": 2,
  "cells": [...],
  "isCyclic": true,
  "cycleLength": 2,
  "cycleStartGeneration": 0,
  "links": {...}
}
```

## Troubleshooting

### Connection Refused
- Ensure the API is running: `dotnet run --project src/life-games-api/Api/LifeGames.Api`
- Check port 5253 is not in use by another application

### 404 Errors
- Make sure you've created a board first
- Verify the `boardId` variable is set (check Environment quick look)
- The board ID expires if the database is reset

### Validation Errors (400)
- Check request body matches the expected format
- Cells array must not be empty
- Generation numbers must be >= 0

## Advanced Usage

### Running All Tests
Use Postman's Collection Runner:
1. Right-click on "Life Games API" collection
2. Select "Run collection"
3. Review test results for all endpoints

### CI/CD Integration
Export collection and use Newman (Postman CLI):
```bash
npm install -g newman
newman run Life-Games-API.postman_collection.json \
  -e Life-Games-API.postman_environment.json
```

## Support

For API documentation, see:
- Swagger UI: `http://localhost:5253/swagger`
- `docs/PRD.md` - Product requirements
- `PROJECT_SUMMARY.md` - Implementation details
