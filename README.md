# 🚗 SOA Car Rental System

A microservices-based car rental management system demonstrating Service-Oriented Architecture (SOA) with multiple technologies and protocols.

![Architecture](https://img.shields.io/badge/Architecture-Microservices-blue)
![Docker](https://img.shields.io/badge/Docker-Containerized-2496ED)
![REST](https://img.shields.io/badge/Protocol-REST-green)
![SOAP](https://img.shields.io/badge/Protocol-SOAP-orange)

## 📋 Overview

This project implements a car rental system using a polyglot microservices architecture, showcasing:

- **Multiple Programming Languages**: Node.js, Python, Java, .NET
- **Multiple Protocols**: REST APIs and SOAP Web Services
- **Database per Service**: Each microservice has its own MySQL database
- **Containerization**: All services run in Docker containers

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Frontend Dashboard                          │
│                    (Node.js - Port 8080)                         │
└─────────────────────────────────────────────────────────────────┘
                                │
        ┌───────────────────────┼───────────────────────┐
        │                       │                       │
        ▼                       ▼                       ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  Car Service  │     │Billing Service│     │  Reservation  │
│   (Node.js)   │     │   (Python)    │     │    (Java)     │
│     REST      │     │     REST      │     │     SOAP      │
│   Port 3001   │     │   Port 8001   │     │   Port 8083   │
└───────┬───────┘     └───────┬───────┘     └───────┬───────┘
        │                     │                     │
        ▼                     ▼                     ▼
   ┌─────────┐           ┌─────────┐           ┌─────────┐
   │ car_db  │           │billing_ │           │reserva_ │
   │  :3307  │           │db :3308 │           │db :3309 │
   └─────────┘           └─────────┘           └─────────┘

┌───────────────┐
│ Maintenance   │
│    (.NET)     │
│     SOAP      │
│   Port 5000   │
└───────┬───────┘
        │
        ▼
   ┌─────────┐
   │mainten_ │
   │db :3310 │
   └─────────┘
```

## 🛠️ Services

| Service | Technology | Protocol | Port | Description |
|---------|------------|----------|------|-------------|
| **Car Service** | Node.js + Express | REST | 3001 | Manage vehicle fleet |
| **Billing Service** | Python + FastAPI | REST | 8001 | Handle payments & invoices |
| **Reservation Service** | Java JAX-WS | SOAP | 8083 | Manage reservations |
| **Maintenance Service** | .NET 8 | SOAP | 5000 | Track vehicle maintenance |
| **Frontend** | Node.js + Express | HTTP | 8080 | Web dashboard |

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running

### Run the Project

```bash
# Clone the repository
git clone https://github.com/maramsahnoun/soa-car-billing-project.git
cd soa-car-billing-project

# Start all services
docker compose up -d

# Check status
docker compose ps
```

### Access the Application

- **🖥️ Frontend Dashboard**: http://localhost:8080
- **🚗 Car Service API**: http://localhost:3001
- **💰 Billing Service API**: http://localhost:8001
- **📅 Reservation WSDL**: http://localhost:8083/reservation?wsdl
- **🔧 Maintenance WSDL**: http://localhost:5000/MaintenanceService.asmx?wsdl

## 📡 API Endpoints

### Car Service (REST)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/cars` | List all cars |
| GET | `/cars/:id` | Get car by ID |
| POST | `/cars` | Add new car |
| PUT | `/cars/:id` | Update car |
| DELETE | `/cars/:id` | Delete car |
| GET | `/health` | Health check |

### Billing Service (REST)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/payments` | Create payment |
| GET | `/payments/:id` | Get payment details |
| PATCH | `/payments/:id/status` | Update payment status |
| GET | `/payments/:id/invoice` | Get invoice |
| GET | `/health` | Health check |

### Reservation Service (SOAP)

- `getAllReservations` - List all reservations
- `getReservation` - Get reservation by ID
- `createReservation` - Create new reservation
- `cancelReservation` - Cancel reservation

### Maintenance Service (SOAP)

- `GetAllMaintenances` - List all maintenance records
- `CreateMaintenance` - Create maintenance record
- `GetMaintenancesByCarId` - Get maintenance for a car

## 🗄️ Database Schema

### Car Database (car_db)
```sql
voiture (id, immatriculation, marque, modele, categorie, tarif_journalier, etat, description)
```

### Billing Database (billing_db)
```sql
paiement (id, reservation_id, montant, mode_paiement, statut, date_paiement)
```

### Reservation Database (reservation_db)
```sql
reservation (id, client_id, car_id, start_date, end_date, status)
```

### Maintenance Database (maintenance_db)
```sql
maintenance (id, car_id, type, description, date, status)
spare_part (id, maintenance_id, name, quantity, unit_price)
```

## 🧪 Testing

### Using the Frontend Dashboard

1. Open http://localhost:8080
2. Check service status (all should be green ✅)
3. Navigate through tabs to test each service

### Using Command Line (PowerShell)

```powershell
# Test Car Service
Invoke-RestMethod -Uri http://localhost:3001/cars

# Test Billing Service
Invoke-RestMethod -Uri http://localhost:8001/health

# Check all services health
Invoke-RestMethod -Uri http://localhost:8080/api/health
```

## 🛑 Stop Services

```bash
# Stop all services
docker compose down

# Stop and remove volumes (reset databases)
docker compose down -v
```

## 📁 Project Structure

```
soa-car-billing-project/
├── car-service/          # Node.js REST API
│   ├── index.js
│   ├── routes/
│   └── Dockerfile
├── billing-service/      # Python FastAPI
│   ├── main.py
│   └── Dockerfile
├── reservation/          # Java SOAP Service
│   ├── src/
│   ├── pom.xml
│   └── Dockerfile
├── MaintenanceService/   # .NET SOAP Service
│   ├── Program.cs
│   ├── Services/
│   └── Dockerfile
├── frontend/             # Web Dashboard
│   ├── server.js
│   ├── public/
│   └── Dockerfile
├── db-init/              # Database init scripts
├── docker-compose.yml
└── README.md
```

## 👥 Authors

- **Maram Sahnoun** - *Initial work*

## 📄 License

This project is for educational purposes.

---

Made with ❤️ for SOA Course
