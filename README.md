# Currency Converter API

## Overview

This project implements a robust, scalable, and maintainable Currency Converter API using **.NET 8**. It allows users to retrieve exchange rates, perform currency conversions, and access historical exchange data. The API integrates with the [Frankfurter API](https://www.frankfurter.app/docs/) and is designed with extensibility and production-readiness in mind.

---

## Project Structure

```
- CurrencyConverter
  ├── CurrencyConverter.API             # API layer - Controllers and Middleware
  ├── CurrencyConverter.Dto             # DTOs - Request, Response, Error Models
  ├── CurrencyConverter.Infrastructure  # External providers & Factory pattern
  ├── CurrencyConverter.Service         # Business logic
  └── CurrencyConverter.Test            # Integration and unit tests
```

---

## Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   ```

2. **Update appsettings.json and appsettings.Development.json**
   - Configure API keys, JWT secrets, and Frankfurter endpoint if needed.
   - Define rate-limiting, logging, and environment-specific configs.

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   dotnet run --project CurrencyConverter.API
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

6. **API Documentation**
   - Swagger UI available at `/swagger` when running the API.

---

## Authentication

- JWT-based authentication is implemented.
- Role-based access control (RBAC) is enforced at the endpoint level.
- Include `Authorization: Bearer <token>` in request headers for protected routes.

---

## Key Features

- ✅ Retrieve latest exchange rates
- ✅ Convert currency amounts (restricted currencies: TRY, PLN, THB, MXN)
- ✅ Historical rates with pagination
- ✅ Caching (to reduce load on Frankfurter API)
- ✅ Retry policies with exponential backoff
- ✅ Circuit breaker for fault tolerance
- ✅ Pluggable provider architecture (Factory Pattern)
- ✅ JWT authentication + RBAC
- ✅ Structured logging with correlation support (Serilog with Seq)
- ✅ Dependency Injection for service abstractions
- ✅ API Versioning
- ✅ Rate Limiting Support
- ✅ 90%+ test coverage with integration tests

---

## ⚙️ Assumptions Made

- Only one exchange rate provider (Frankfurter API) is used initially; designed to support more.
- Caching is implemented in-memory for simplicity; can be extended to Redis.
- Throttling is currently performed based on the username extracted from the JWT. This can be improved by incorporating the UserId for more precise control.
- Test environments assume mock data or mock provider wrappers.
- Each request log includes HTTP Method, Target Endpont, Correlation Id, client IP, client ID, response code, and duration.

---

## 🌱 Possible Future Enhancements

- Add Redis or distributed cache for shared deployments.
- Implement UI client or Postman collection for demo/testing.
- Add database for storing historical request logs and user audit trails.
- Setup CI/CD pipeline.
- Dockerize the application.

---

## 📎 Reference

- **Frankfurter API Docs**: [https://www.frankfurter.app/docs](https://www.frankfurter.app/docs)
- **Base URL**: `https://api.frankfurter.app`
