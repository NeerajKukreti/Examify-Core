# 📚 Examify-Core

> A secure, flexible, and efficient way to test knowledge anytime, anywhere.

Examify is a comprehensive online examination platform built with ASP.NET Core, designed for educational institutes to conduct secure, proctored exams with advanced anti-cheat mechanisms.

---

## ✨ Features

### 🎓 For Students
- **Secure Exam Environment** - Fullscreen mode with anti-cheat protection
- **Real-time Auto-save** - Responses saved every 30 seconds
- **Smart Navigation** - Jump to any question, mark for review
- **Review Summary** - Visual overview before submission
- **Keyboard Shortcuts** - Fast navigation (N/P/M/C keys)
- **Time Warnings** - Alerts at 10, 5, and 2 minutes remaining

### 🏫 For Institutes
- **Question Bank Management** - Create, organize questions by subject/topic
- **Flexible Exam Configuration** - Custom marks, negative marking, time limits
- **Batch & Class Management** - Organize students efficiently
- **Multiple Question Types** - MCQ, Subjective, True/False, Match Pairs
- **Advanced Filtering** - Filter questions by difficulty, type, topic

### 👨💼 For Admins
- **Multi-Institute Support** - Manage multiple educational institutes
- **User Management** - Role-based access (Admin/Institute/Student)
- **Dashboard Analytics** - Exam statistics and performance metrics
- **Audit Logs** - Track all system activities

### 🔒 Security Features
- **JWT Authentication** - Secure token-based auth
- **Anti-Cheat System** - Tab switching detection, fullscreen enforcement
- **Violation Tracking** - Monitor and log suspicious activities
- **Session Management** - Secure exam sessions with timeout

---

## 🛠️ Technology Stack

- **Backend**: ASP.NET Core 8.0 (MVC + Web API)
- **Database**: SQL Server
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog
- **Frontend**: jQuery, Bootstrap, DataTables
- **Caching**: In-Memory Cache
- **Architecture**: Repository Pattern, Service Layer

---

## 📋 Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code
- Node.js (for frontend dependencies, optional)

---

## 🚀 Quick Start

### 1. Clone Repository
```bash
git clone https://github.com/yourusername/ExamiFy-Core.git
cd ExamiFy-Core
```

### 2. Database Setup
```sql
-- Create database
CREATE DATABASE db_exam;

-- Run stored procedures from Database/StoredProcedures/
-- Execute all .sql files in order
```

📖 **Detailed Guide:** See [DATABASE_SETUP.md](DATABASE_SETUP.md) for complete instructions

### 3. Configure Connection String
Update `ExamifyAPI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=db_exam;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  }
}
```

### 4. Configure JWT Settings
Update `ExamifyAPI/appsettings.json`:
```json
{
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "ExamPortalAPI",
    "Audience": "ExamPortalUsers",
    "ExpireHours": 3
  }
}
```

### 5. Update API URL
Update `Examify/appsettings.json`:
```json
{
  "ExamifyAPI": {
    "BaseUrl": "https://localhost:7271/api/"
  }
}
```

### 6. Restore & Build
```bash
dotnet restore
dotnet build
```

### 7. Run Applications
```bash
# Terminal 1 - Run API
cd ExamifyAPI
dotnet run

# Terminal 2 - Run MVC App
cd Examify
dotnet run
```

### 8. Access Application
- **MVC App**: https://localhost:7001
- **API**: https://localhost:7271
- **API Docs**: https://localhost:7271/swagger

---

## 📁 Project Structure

```
ExamiFy-Core/
├── Examify/              # MVC Web Application
│   ├── Controllers/      # MVC Controllers
│   ├── Views/           # Razor Views
│   ├── Services/        # Business Logic
│   └── wwwroot/         # Static files (JS, CSS)
├── ExamifyAPI/          # REST API
│   ├── Controllers/     # API Controllers
│   ├── Services/        # API Services
│   └── Middleware/      # Custom Middleware
├── DAL/                 # Data Access Layer
│   └── Repository/      # Repository Pattern
├── Model/               # Domain Models & DTOs
│   ├── DTO/            # Data Transfer Objects
│   └── Exam/           # Exam-related models
├── Examify.Logs/        # Logging Configuration
└── Database/            # SQL Scripts
    └── StoredProcedures/
```

---

## 👥 User Roles

### Admin
- Manage institutes
- View system-wide analytics
- Configure global settings

### Institute
- Create and manage exams
- Manage question bank
- Manage students and batches
- View institute-specific reports

### Student
- Take assigned exams
- View exam results
- Access exam history

---

## 🔑 Default Credentials

After database setup, create users via Institute/Admin registration pages.

---

## 📖 Documentation

### Setup & Installation
- [Database Setup Guide](DATABASE_SETUP.md)
- [Contributing Guidelines](CONTRIBUTING.md)

### Architecture & Design
- [System Architecture](ARCHITECTURE.md) - Complete architectural overview with diagrams

### Technical Documentation
- [Exam Question Configuration](Documentation/ExamQuestionConfiguration_Implementation.md)
- [Session Management](Documentation/SessionManagement_Implementation.md)
- [State API](Documentation/StateAPI.md)
- [User Service Architecture](Documentation/UserServiceArchitecture.md)

### Additional Resources
- [Improvements Log](IMPROVEMENTS.md)
- [Quick Reference](QUICK_REFERENCE.md)

---

## 🎯 Key Workflows

### Creating an Exam
1. Login as Institute
2. Navigate to Exam → Create Exam
3. Fill exam details (name, duration, instructions)
4. Configure Questions → Select from question bank
5. Set marks and negative marking
6. Assign to batches/students
7. Publish exam

### Taking an Exam
1. Login as Student
2. View available exams
3. Click "Start Exam"
4. Accept terms and enter fullscreen
5. Answer questions with auto-save
6. Review summary before submit
7. View results (if enabled)

---

## 🔧 Configuration Options

### Anti-Cheat Settings
Configure in exam settings:
- Tab switch warnings (default: 2)
- Fullscreen enforcement
- Right-click disable
- Copy/paste restrictions
- Developer tools detection

### Exam Settings
- Duration (minutes)
- Total marks
- Passing marks
- Negative marking
- Question randomization
- Result visibility

---

## 🐛 Troubleshooting

### API Connection Issues
- Verify API is running on correct port
- Check `ExamifyAPI.BaseUrl` in Examify/appsettings.json
- Ensure firewall allows connections

### Database Connection Failed
- Verify SQL Server is running
- Check connection string credentials
- Ensure database exists

### JWT Token Errors
- Verify JWT Key is at least 32 characters
- Check token expiration settings
- Clear browser cache/cookies

---

## 📊 Performance Tips

- Enable response caching for static data
- Use DataTables pagination for large datasets
- Optimize SQL queries with proper indexing
- Configure Serilog log levels appropriately

---

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

Quick steps:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

---

## 📝 License

This project is proprietary software. See the [LICENSE](LICENSE) file for terms and conditions.

---

## 📞 Support

For issues and questions:
- Create an issue on GitHub
- Check existing documentation
- Review logs in `Examify/Logs/` and `ExamifyAPI/Logs/`

---

## 🎉 Acknowledgments

Built with ❤️ for educational institutions worldwide.

---

**Version**: 2.0  
**Last Updated**: 2024  
**Status**: ✅ Production Ready
