# 🧽 BRM Cleaning Service API

### My First Complete Backend Project - A Fresh Graduate's Learning Journey

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![MySQL](https://img.shields.io/badge/MySQL-005C84?style=for-the-badge&logo=mysql&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

> 🇹🇭 **Thai comments throughout the code represent my learning journey**  
> 📚 **Every line written to understand concepts deeply, not just to work**

---

## 🌱 About This Project

This is my first comprehensive backend API built as a fresh graduate to learn modern .NET development. Every line of code was written to understand core concepts, not just to make it work. The Thai comments throughout the codebase show my learning process and ensure I truly understand what each part does.

## 🚀 Quick Start

### Prerequisites

- .NET 9.0 SDK
- MySQL Server
- Git

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   ```

2. **Install dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure Database Connection**

   Update your database connection in `BackendAPI/appsettings.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=localhost; database=YOUR_DATABASE_NAME; user=YOUR_USERNAME; password=YOUR_PASSWORD"
   }
   ```

4. **Create and Apply Database Migration**

   ```bash
   cd BackendAPI
   dotnet ef migrations add -c DataContext Initial
   dotnet ef database update
   ```

5. **Run the Application**

   ```bash
   dotnet run
   ```

6. **Access API Documentation**

   Open: http://localhost:5156/swagger/index.html

> 💡 **Note:** This is a learning project - if you encounter issues, check the error messages carefully. I've tried to make them as helpful as possible!

### Default Users

The system automatically creates these default accounts:

| Role      | Email               | Password | Permissions                  |
| --------- | ------------------- | -------- | ---------------------------- |
| **Admin** | `admin@example.com` | `1234`   | Full access to all resources |
| **User**  | _Register via API_  | _Custom_ | Manage own orders only       |
| **Guest** | _Register via API_  | _Custom_ | View products only           |

### Authentication Flow

1. **Register** a new account or use default admin
2. **Login** to receive JWT token
3. **Include token** in `Authorization: Bearer <token>` header
4. **Execute operations** based on your role permissions

---

## 📚 API Endpoints

### 🔑 Authentication

```http
POST /api/Auth/login          # Login with email/password
POST /api/Auth/register       # Register new user
POST /api/Auth/register-guest # Register guest user
```

### 🛍️ Products _(Admin Only)_

```http
GET    /api/Product           # Get all products
GET    /api/Product/{id}      # Get product by ID
POST   /api/Product           # Create new product
PUT    /api/Product/{id}      # Update product
DELETE /api/Product/{id}      # Delete product
```

### 👥 Roles _(Admin Only)_

```http
GET    /api/Role              # Get all roles
GET    /api/Role/{id}         # Get role by ID
POST   /api/Role              # Create new role
PUT    /api/Role/{id}         # Update role
DELETE /api/Role/{id}         # Delete role
```

### 📋 Orders _(Context-Aware Permissions)_

```http
GET    /api/Order             # Get orders (Admin: all, User: own)
GET    /api/Order/{id}        # Get order by ID
POST   /api/Order             # Create new order
PUT    /api/Order/{id}        # Update order (own orders only)
DELETE /api/Order/{id}        # Delete order (own orders only)
```

---

## ⚡ Key Features

### 🔒 **Security Features**

- ✅ JWT Token Authentication
- ✅ Role-Based Authorization (Admin/User/Guest)
- ✅ Password Hashing with ASP.NET Core Identity
- ✅ User session management
- ✅ Protected endpoints with proper error messages

### 📊 **Business Features**

- ✅ Complete Product Catalog Management
- ✅ User Registration (Regular Users & Guests)
- ✅ Order Management with Date Scheduling
- ✅ Role Administration
- ✅ Data Relationships (User ↔ Orders ↔ Products)

### 🛠️ **Technical Features**

- ✅ RESTful API Design
- ✅ Entity Framework Core with MySQL
- ✅ Automated Database Seeding
- ✅ Swagger/OpenAPI Documentation
- ✅ CORS Configuration
- ✅ Comprehensive Error Handling
- ✅ DTO Pattern for Data Transfer

---

**Development Philosophy:**

> "I'd rather build something I fully understand than something that just looks impressive. Every comment in Thai represents a concept I've learned and can explain."

**Contact for Learning/Collaboration:**

- Looking for junior backend developer opportunities
- Open to code reviews and learning feedback
- Interested in contributing to open-source projects

---

**Happy Coding! 🧽✨**

_This project represents genuine learning and growth. Built with curiosity, improved through practice, and shared with complete honesty about my journey as a fresh developer._
