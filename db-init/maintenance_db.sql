CREATE TABLE IF NOT EXISTS Maintenances (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    VehicleId INT NOT NULL,
    MaintenanceType VARCHAR(50) NOT NULL DEFAULT 'PREVENTIVE',
    ScheduledDate DATETIME NOT NULL,
    CompletedDate DATETIME NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'SCHEDULED',
    EstimatedCost DECIMAL(10,2) NOT NULL DEFAULT 0,
    ActualCost DECIMAL(10,2) NULL,
    CurrentMileage INT NOT NULL DEFAULT 0,
    Description VARCHAR(500) NULL,
    MechanicNotes VARCHAR(1000) NULL,
    Priority VARCHAR(20) NOT NULL DEFAULT 'MEDIUM',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    
    INDEX idx_vehicle (VehicleId),
    INDEX idx_status (Status),
    INDEX idx_scheduled_date (ScheduledDate)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS SpareParts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    MaintenanceId INT NOT NULL,
    Name VARCHAR(200) NOT NULL,
    PartNumber VARCHAR(100) NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalPrice DECIMAL(10,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_maintenance (MaintenanceId),
    FOREIGN KEY (MaintenanceId) REFERENCES Maintenances(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

