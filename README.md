# ASP.NET Core - Secure Database & Development Practices

This repository demonstrates the implementation of a secure ASP.NET Core Web API with strict database security, data integrity, and safe coding practices. It was built to satisfy complex requirements involving the handling of highly sensitive data, such as personal information, financial details, and authentication credentials.

## 🛡️ Key Security Features Implemented

- **Password Hashing:** Uses `BCrypt.Net` to securely hash passwords before storing them in the database.
- **Data Integrity (HMAC):** Implements HMAC (Hash-based Message Authentication Code) to verify that sensitive database rows have not been tampered with.
- **Data Protection API (Encryption at Rest):** Utilizes ASP.NET Core's Data Protection API within an EF Core `ValueConverter` to seamlessly encrypt and decrypt sensitive fields (like Credit Card numbers) at the column level.
- **Secure Transit:** Configuration strictly enforces SSL/TLS encryption (`Encrypt=True;TrustServerCertificate=False;`) for the SQL Server connection to prevent MITM attacks.
- **Database Auditing:** An EF Core Interceptor automatically tracks and logs all additions, modifications, and deletions to sensitive tables in an `AuditLog`.
- **Role-Based Access Control (RBAC):** API endpoints are protected using standard ASP.NET Core Authorization policies (`[Authorize(Roles="...")]`).
- **CSRF Protection:** State-changing requests are protected via Anti-Forgery Tokens (`[ValidateAntiForgeryToken]`), paired with secure, `HttpOnly` and `SameSite=Strict` session cookies.
- **Secure Logging:** Custom logger implementations ensure that PII and sensitive credentials are automatically scrubbed from application logs.

## 🚀 Tech Stack

- **Framework:** ASP.NET Core Web API (.NET)
- **Database:** SQL Server
- **ORM:** Entity Framework Core (Code First)
- **Security:** ASP.NET Core Data Protection, BCrypt.Net, Anti-Forgery

## ⚙️ How to Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Giridhar706/ASP.NET_Core-Secure_Database_Development_Practices.git
   cd ASP.NET_Core-Secure_Database_Development_Practices
   ```

2. **Update Connection String:**
   Ensure the connection string in `appsettings.json` points to your active SQL Server instance or LocalDB.

3. **Apply Database Migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   *(Or run `Add-Migration InitialCreate` and `Update-Database` from the Visual Studio Package Manager Console).*

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   Or hit **F5** in Visual Studio.

## 🧪 Testing

The API provides endpoints for:
*   `POST /api/Auth/register`
*   `POST /api/Auth/login`
*   `GET /api/Auth/csrf-token`
*   `POST /api/Financial` (Requires Auth & CSRF Token)
*   `GET /api/Financial` (Requires Auth)

For complete end-to-end testing, tools like **Postman** are recommended to smoothly handle the secure `HttpOnly` session cookies and the CSRF token headers.


## Author

  **Giridhar Gopal**
