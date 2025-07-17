---
applyTo: "**"
---

# ASP.NET PREPEN E-COMMERCE BACKEND INSTRUCTIONS

## OVERVIEW

This backend supports:

- Admin authentication
- Inventory management (CRUD items)
- Order management (status transitions)
- User management (suspend/delete)

## TECH STACK

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQLite

## FILE STRUCTURE

```txt

PrepenAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── AdminController.cs
│   ├── ProductController.cs
│   ├── OrderController.cs
│   └── UserController.cs
├── Models/
│   ├── Admin.cs
│   ├── Product.cs
│   ├── Order.cs
│   └── AppUser.cs
├── DTOs/
├── Data/
│   ├── AppDbContext.cs
├── Services/
│   ├── AuthService.cs
│   ├── AdminService.cs
│   ├── ProductService.cs
│   ├── OrderService.cs
│   └── UserService.cs
├── Program.cs
└── appsettings.json

````

---

## 1. ADMIN AUTHENTICATION

### Models/Admin.cs

```csharp
public class Admin {
    public int Id { get; set; }
    public string Username { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
}
````

### Endpoints

- `POST /auth/register` (internal only)
- `POST /auth/login` → returns JWT token

### Implementation

- Use `HMACSHA512` for password hashing
- Use `System.IdentityModel.Tokens.Jwt` to generate JWT
- Protect routes using `[Authorize(Roles = "Admin")]`

---

## 2. INVENTORY MANAGEMENT

### Models/Product.cs

```csharp
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsDeleted { get; set; }
}
```

### Products Endpoints (Admin only)

- `GET /products`
- `GET /products/{id}`
- `POST /products`
- `PUT /products/{id}`
- `DELETE /products/{id}` (soft delete → `IsDeleted = true`)

---

## 3. ORDER MANAGEMENT

### Models/Order.cs

```csharp
public enum OrderStatus {
    Pending,
    Shipped,
    Delivered,
    Canceled
}

public class Order {
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<OrderItem> Items { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderItem {
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
```

### Orders Endpoints (Admin only)

- `GET /orders`
- `GET /orders/{id}`
- `PATCH /orders/{id}/status` → accepts new status enum

---

## 4. USER MANAGEMENT

### Models/AppUser.cs

```csharp
public class AppUser {
    public int Id { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsDeleted { get; set; }
}
```

### User Endpoints (Admin only)

- `GET /users`
- `GET /users/{id}`
- `PATCH /users/{id}/suspend` → toggle suspend
- `DELETE /users/{id}` → set `IsDeleted = true`

---

## MIDDLEWARE

- Add authentication/authorization middleware
- Validate JWT in headers
- Apply `[Authorize]` attributes at controller or method level

---

## DATA ACCESS (EF CORE)

- `AppDbContext` should include `DbSet<T>` for all models
- Use Fluent API to configure relations if needed
- Use migrations to create schema

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## FINAL NOTES

- Use `AddScoped` in `Program.cs` for DI services
- Seed admin user at startup
- Use DTOs to prevent overposting and model leakage
