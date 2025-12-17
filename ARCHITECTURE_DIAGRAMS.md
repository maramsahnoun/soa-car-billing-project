# Maintenance Service - Architecture Diagrams

## 1. System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CLIENT / GATEWAY LAYER                            │
└──────────────────┬──────────────────────────────────────────┬──────────────┘
                   │                                           │
           ┌───────▼──────────┐                      ┌────────▼─────────┐
           │ API Gateway      │                      │ Direct SOAP      │
           │ (Port 8080)      │                      │ Clients          │
           └───────┬──────────┘                      └────────┬─────────┘
                   │                                           │
                   │ JWT Authentication                        │
                   │ Request Routing                           │
                   │                                           │
    ┌──────────────▼──────────────────────────────────────────▼────────────┐
    │                                                                       │
    │              ┌──────────────────────────────────┐                  │
    │              │  MAINTENANCE SERVICE (8085)      │                  │
    │              │  Type: SOAP Web Service          │                  │
    │              │  Framework: .NET Core 8.0        │                  │
    │              │                                   │                  │
    │              │  Endpoint: /ws/maintenance       │                  │
    │              │  WSDL: /ws/maintenance?wsdl      │                  │
    │              └──────────────────────────────────┘                  │
    │                        │                                           │
    │        ┌───────────────┼───────────────┐                          │
    │        │               │               │                          │
    │   ┌────▼────┐    ┌─────▼─────┐  ┌────▼────┐                     │
    │   │Services │    │  Models   │  │  Data   │                     │
    │   │Layer    │    │  Layer    │  │ Access  │                     │
    │   └────┬────┘    └─────┬─────┘  └────┬────┘                     │
    │        │               │              │                          │
    │   Main            Maintenance    DbContext                       │
    │   tenanceServ     SparePart      SQL Queries                    │
    │   iceImpl                                                        │
    │        │               │              │                          │
    └────────┼───────────────┼──────────────┼──────────────────────────┘
             │               │              │
    ┌────────▼────────┐      │     ┌────────▼────────┐
    │                │      │     │                │
    │ Vehicle Service │      │     │ MySQL Database │
    │ (REST)         │      │     │ maintenance_db │
    │ Port: 8082     │      │     │                │
    │                │      │     │ Tables:       │
    │ /api/vehicles  │      │     │ - maintenance │
    │                │      │     │ - spare_parts │
    └────────────────┘      │     └────────────────┘
                            │
                    HTTP Client Factory
                    Error Handling
                    Logging
```

---

## 2. Maintenance Service Internal Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                   SOAP ENDPOINT LAYER                           │
│              /ws/maintenance (SoapCore)                         │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                   SERVICE INTERFACE                             │
│              IMaintenanceService                               │
│                                                                 │
│  ┌─ ScheduleMaintenance                                       │
│  ├─ UpdateMaintenanceStatus                                   │
│  ├─ GetMaintenanceHistory                                     │
│  ├─ GetUpcomingMaintenances                                   │
│  ├─ RecordRepair                                              │
│  ├─ UpdateVehicleCondition                                    │
│  ├─ CalculateMaintenanceCost                                  │
│  ├─ GenerateMaintenanceReport                                 │
│  ├─ GetMaintenanceById                                        │
│  ├─ CancelMaintenance                                         │
│  └─ CreateMaintenance (legacy)                                │
│     CloseMaintenance (legacy)                                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                  IMPLEMENTATION LAYER                           │
│            MaintenanceServiceImpl                               │
│                                                                 │
│  ┌─ Business Logic                                            │
│  │  • State machine transitions                               │
│  │  • Cost calculations                                       │
│  │  • Validation rules                                        │
│  │                                                             │
│  ├─ Vehicle Service Integration                               │
│  │  • VehicleExists(vehicleId)                               │
│  │  • UpdateVehicleStatus(vehicleId, status)                 │
│  │                                                             │
│  ├─ Data Mapping                                              │
│  │  • ToResponse()                                            │
│  │  • ToDto()                                                 │
│  │  • ToDtoLegacy()                                           │
│  │  • ToSparePartDto()                                        │
│  │                                                             │
│  └─ Error Handling & Logging                                  │
│     • ILogger integration                                      │
│     • Exception wrapping                                       │
│     • Graceful degradation                                     │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                  DATA ACCESS LAYER                              │
│                MaintenanceDbContext                            │
│                 (Entity Framework Core)                         │
│                                                                 │
│  DbSets:                                                        │
│  ├─ Maintenances                                              │
│  └─ SpareParts                                                │
│                                                                 │
│  Features:                                                      │
│  ├─ LINQ queries                                              │
│  ├─ Relationships (one-to-many)                               │
│  ├─ Cascade delete                                            │
│  └─ Change tracking                                           │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                               │
│              MySQL / SQL Server                                │
│                                                                 │
│  ┌──────────────┐        ┌──────────────┐                    │
│  │ maintenance  │        │ spare_parts  │                    │
│  │              │        │              │                    │
│  │ id (PK)      │        │ id (PK)      │                    │
│  │ vehicle_id   │        │ maintenance  │                    │
│  │ type         │◄───────│ _id (FK)     │                    │
│  │ scheduled_dt │        │ part_number  │                    │
│  │ completed_dt │        │ quantity     │                    │
│  │ status       │        │ unit_price   │                    │
│  │ cost fields  │        │ total_price  │                    │
│  │ mileage      │        └──────────────┘                    │
│  └──────────────┘                                              │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. Data Flow: Schedule Maintenance

```
┌─────────────┐
│ SOAP Client │
└──────┬──────┘
       │
       │ POST /ws/maintenance
       │ <MaintenanceRequest>
       │ - vehicleId: 42
       │ - maintenanceType: PREVENTIVE
       │ - scheduledDate: 2024-12-25
       │ - estimatedCost: 150.00
       │
       ▼
┌──────────────────────────────────────┐
│ SoapCore Deserialization             │
│ MaintenanceRequest → Object          │
└──────────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ ScheduleMaintenance Operation    │
    │                                  │
    │ 1. Validate input                │
    │ 2. Check vehicle exists          │◄──┐
    │                                  │   │
    │    → Call Vehicle Service        │   │
    │    → GET /api/vehicles/42        │   │ HTTP
    │    → Verify 200 OK               │   │ Request
    │                                  │   │
    └──────────────┬───────────────────┘   │
                   │                       │
                   ├──────────────────────►│
                   │                       │
                   │◄──────────────────────┤
                   │ {status: 200}         │
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Create Maintenance Entity        │
    │ - Status: SCHEDULED              │
    │ - CreatedAt: now                 │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Save to Database                 │
    │ _db.Maintenances.Add(entity)     │
    │ _db.SaveChanges()                │
    │                                  │
    │ → INSERT INTO maintenance ...    │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ├─────────────────┐
                   │                 ▼
                   │    ┌────────────────────┐
                   │    │ Update Vehicle     │
                   │    │ Status to MAINT.   │
                   │    │                    │
                   │    │ PATCH /api/        │
                   │    │ vehicles/42/status │
                   │    │ {status: MAINT.}   │
                   │    │                    │
                   │    └────────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Map to Response DTO              │
    │ Maintenance → MaintenanceResponse│
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ SoapCore Serialization           │
    │ MaintenanceResponse → XML        │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼ SOAP Response
                   │ <MaintenanceResponse>
                   │ - Id: 101
                   │ - VehicleId: 42
                   │ - Status: SCHEDULED
                   │ - CreatedAt: 2024-12-11T...
                   │
┌──────────────────┴──────────────────┐
│       SOAP Client receives           │
│       201 Created status             │
└─────────────────────────────────────┘
```

---

## 4. Data Flow: Record Repair

```
┌─────────────────────────────────┐
│ SOAP Client sends RepairRequest │
│ - MaintenanceId: 101            │
│ - MechanicNotes: "..."          │
│ - ActualCost: 175.50            │
│ - SpareParts: [...]             │
└──────────────┬──────────────────┘
               │
               ▼
    ┌──────────────────────────────────┐
    │ RecordRepair Operation           │
    │                                  │
    │ 1. Load Maintenance + SpareParts │
    │    .Include(m => m.SpareParts)   │
    │                                  │
    │ 2. Validate exists               │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Update Maintenance Record        │
    │ - Status: COMPLETED              │
    │ - CompletedDate: now             │
    │ - MechanicNotes: set             │
    │ - ActualCost: set                │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Add Spare Parts                  │
    │                                  │
    │ For each SparePartRequest:       │
    │   Create SparePart entity        │
    │   Set MaintenanceId (FK)         │
    │   Calculate TotalPrice           │
    │   Add to Maintenance.SpareParts  │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Save to Database                 │
    │                                  │
    │ UPDATE maintenance SET ...       │
    │ INSERT INTO spare_parts ...      │
    │ INSERT INTO spare_parts ...      │
    │ (cascade through one-to-many)    │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ├─────────────────┐
                   │                 ▼
                   │    ┌────────────────────┐
                   │    │ Check if vehicle   │
                   │    │ has other active   │
                   │    │ maintenance        │
                   │    │                    │
                   │    │ If NOT:            │
                   │    │ Update vehicle to  │
                   │    │ DISPONIBLE         │
                   │    │                    │
                   │    └────────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ Build Response                   │
    │ - MaintenanceId: 101             │
    │ - Status: COMPLETED              │
    │ - TotalCost: 175.50              │
    │ - SpareParts: [...] (mapped)     │
    │                                  │
    └──────────────┬───────────────────┘
                   │
                   ▼
┌──────────────────────────────────┐
│ SOAP Client receives             │
│ RepairResponse with all details  │
└──────────────────────────────────┘
```

---

## 5. Status State Machine

```
                    ┌─────────────────────┐
                    │                     │
    ┌───────────────►  COMPLETED ◄────────┤
    │               │                     │
    │               └─────────────────────┘
    │                 ▲
    │                 │
    │            fulfilled
    │                 │
┌───┴─────────────────────────────┐
│                                 │
│    ┌──────────────────────┐    │
│    │                      │    │
│    │  IN_PROGRESS         │    │
│    │                      │    │
│    └──────────┬───────────┘    │
│               │                │
│               │ repair work    │
│               │ completed      │
│               │                │
    │    ┌──────────────────────┐    │
    │    │                      │    │
    │    │  SCHEDULED           │    │
    │    │                      │    │
    │    └──────────┬───────────┘    │
    └─────────────▲──────────────────┘
                  │
              work begins
                  │
                  │    OR
                  │
                  ▼
            ┌──────────────┐
            │ CANCELLED    │
            └──────────────┘

Legend:
─────► State Transition
─ ─ ─►  Conditional Path
      Cancellable from any state (except COMPLETED)
```

---

## 6. Database Schema Relationship Diagram

```
┌─────────────────────────────────────┐
│        MAINTENANCE TABLE            │
│                                     │
│  PK  id (INT)                      │
│      vehicle_id (INT)              │◄──────┐ Foreign Key relationship
│      maintenance_type (VARCHAR)    │       │ (not enforced in code)
│      scheduled_date (DATETIME)     │       │ but validated
│      completed_date (DATETIME)     │       │
│      status (VARCHAR)              │       │
│      estimated_cost (DECIMAL)      │       │
│      actual_cost (DECIMAL)         │       │
│      current_mileage (INT)         │       │
│      description (VARCHAR)         │       │
│      mechanic_notes (VARCHAR)      │       │
│      priority (VARCHAR)            │       │
│      created_at (DATETIME)         │       │
│      updated_at (DATETIME)         │       │
│                                     │       │
│  Indexes:                          │       │
│  - idx_vehicle_id                  │       │
│  - idx_status                      │       │
│  - idx_scheduled_date              │       │
└─────────────────────────────────────┘       │
                │                            │
                │ 1 ──────── N              │
                │                            │
                ▼                            │
┌─────────────────────────────────────┐      │
│      SPARE_PARTS TABLE              │      │
│                                     │      │
│  PK  id (INT)                      │      │
│      maintenance_id (INT) ──────────┘ FK  │
│      name (VARCHAR)                │      │
│      part_number (VARCHAR)         │      │
│      quantity (INT)                │      │
│      unit_price (DECIMAL)          │      │
│      total_price (DECIMAL)         │      │
│      created_at (DATETIME)         │      │
│                                     │      │
│  Indexes:                          │      │
│  - idx_maintenance_id              │      │
│                                     │      │
│  Constraints:                      │      │
│  - FK on maintenance_id            │      │
│  - Cascade delete                  │      │
└─────────────────────────────────────┘      │
                                             │
    Vehicle Service Reference (HTTP)         │
    (Not a DB FK)                            │
    Verified at application level ───────────┘
```

---

## 7. Error Handling Flow

```
┌─────────────────────────────────────┐
│      SOAP Request Received          │
└──────────────┬──────────────────────┘
               │
               ▼
    ┌──────────────────────────────┐
    │  Validate Input              │
    │ (DataContract binding)       │
    └──────────────┬───────────────┘
                   │
            ┌──────┴──────┐
            │             │
            ▼             ▼
    ┌─────────────┐ ┌──────────────┐
    │   Valid     │ │   Invalid    │
    │             │ │              │
    └──────┬──────┘ │ ArgumentNull │
           │        │ Exception    │
           │        │              │
           │        └──────┬───────┘
           │               │
           ▼               ▼
    ┌──────────────────────────────┐
    │  SOAP Fault Response         │
    │  faultcode: Client           │
    │  faultstring: detailed msg   │
    └──────────────────────────────┘
           ▲
           │
    ┌──────┴──────┐
    │             │
Execute business   │
logic              ▼
    │    ┌──────────────────────────┐
    │    │  Check Preconditions     │
    │    │  (e.g., vehicle exists)  │
    │    └──────────────┬───────────┘
    │                   │
    │            ┌──────┴──────┐
    │            │             │
    │            ▼             ▼
    │    ┌─────────────┐ ┌──────────────┐
    │    │   Valid     │ │   Invalid    │
    │    │             │ │              │
    │    └──────┬──────┘ │ InvalidOper  │
    │           │        │ationException
    │           │        │  (Vehicle    │
    │           │        │   not found) │
    │           │        │              │
    │           │        └──────┬───────┘
    │           │               │
    │           ▼               ▼
    │    ┌──────────────────────────────┐
    │    │  SOAP Fault Response         │
    │    │  faultcode: Server           │
    │    │  faultstring: detailed msg   │
    │    └──────────────────────────────┘
    │           ▲
    │           │
    └───────────┘

    Any Unhandled Exception
           │
           ▼
    ┌──────────────────────────────┐
    │  Log Error                   │
    │  _logger.LogError(ex, ...)   │
    │                              │
    │  Return SOAP Fault:          │
    │  faultcode: Server           │
    │  faultstring: "Internal err" │
    └──────────────────────────────┘
```

---

## 8. Service Integration Points

```
┌─────────────────────────────────────────────────────────────┐
│         API GATEWAY (Port 8080)                             │
│  Request → /api/maintenance/schedule                       │
└──────────────────────┬──────────────────────────────────────┘
                       │
            ┌──────────┴──────────┐
            │                     │
    ┌───────▼─────────┐   ┌──────▼──────────┐
    │ JWT Validation  │   │ Route to SOAP   │
    │ Extract userId  │   │ Maintenance     │
    └───────┬─────────┘   └──────┬──────────┘
            │                    │
            └────────┬───────────┘
                     │
                     ▼
    ┌──────────────────────────────────┐
    │ SOAP Adapter (if needed)         │
    │ JSON → XML SOAP Envelope         │
    └──────────────┬───────────────────┘
                   │
                   ▼
    ┌──────────────────────────────────┐
    │ MAINTENANCE SERVICE              │
    │ (Port 8085)                      │
    │ /ws/maintenance                  │
    └──────┬───────────────────────────┘
           │
    ┌──────┴──────┐
    │             │
    ▼             ▼
 ┌────────────────────────────────────┐
 │  VEHICLE SERVICE CALL              │
 │  (Port 8082)                       │
 │  GET /api/vehicles/42              │
 │  PATCH /api/vehicles/42/status     │
 │                                    │
 │  Timeout: 30 seconds               │
 │  Retry: None (degradation)         │
 │  Error: Log & continue             │
 └────────────────────────────────────┘

 ┌────────────────────────────────────┐
 │  DATABASE OPERATIONS               │
 │  (via EF Core)                     │
 │  MySQL / SQL Server                │
 │  Port: 3306 / 1433                 │
 │                                    │
 │  Connection Pool: Active           │
 │  Timeout: 30 seconds               │
 │  Retry: Yes (3 times)              │
 └────────────────────────────────────┘
```

---

_Diagrams Generated: December 11, 2025_  
_Service: Maintenance Management (SOA Car Rental)_  
_Architecture: Microservices with SOAP/REST Integration_
