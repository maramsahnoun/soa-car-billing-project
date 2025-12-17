using System.ServiceModel;
using MaintenanceService.Dtos;

/// <summary>
/// SOAP Web Service for Maintenance Management
/// Supports scheduling, tracking, and reporting maintenance operations
/// </summary>
[ServiceContract(Namespace = "http://maintenance-service.soa.com/2024/12")]
public interface IMaintenanceService
{
    /// <summary>
    /// Schedule a new maintenance for a vehicle
    /// </summary>
    [OperationContract]
    MaintenanceResponse ScheduleMaintenance(MaintenanceRequest request);

    /// <summary>
    /// Update the status of a maintenance record
    /// </summary>
    [OperationContract]
    bool UpdateMaintenanceStatus(int id, string status);

    /// <summary>
    /// Get complete maintenance history for a vehicle
    /// </summary>
    [OperationContract]
    List<MaintenanceResponse> GetMaintenanceHistory(int vehicleId);

    /// <summary>
    /// Get all upcoming (scheduled or in-progress) maintenances
    /// </summary>
    [OperationContract]
    List<MaintenanceResponse> GetUpcomingMaintenances();

    /// <summary>
    /// Record repair/maintenance work completion
    /// </summary>
    [OperationContract]
    RepairResponse RecordRepair(RepairRequest request);

    /// <summary>
    /// Update vehicle condition after inspection
    /// </summary>
    [OperationContract]
    bool UpdateVehicleCondition(ConditionReport report);

    /// <summary>
    /// Calculate total maintenance cost including spare parts
    /// </summary>
    [OperationContract]
    decimal CalculateMaintenanceCost(int maintenanceId);

    /// <summary>
    /// Generate maintenance report for date range
    /// </summary>
    [OperationContract]
    MaintenanceReport GenerateMaintenanceReport(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get details of a specific maintenance record
    /// </summary>
    [OperationContract]
    MaintenanceResponse GetMaintenanceById(int maintenanceId);

    /// <summary>
    /// Cancel a scheduled maintenance
    /// </summary>
    [OperationContract]
    bool CancelMaintenance(int maintenanceId, string reason);

    /// <summary>
    /// Backward compatible: Create maintenance (legacy)
    /// </summary>
    [OperationContract]
    MaintenanceDto CreateMaintenance(int vehicleId, string maintenanceType, string description, DateTime scheduledDate);

    /// <summary>
    /// Backward compatible: Close maintenance (legacy)
    /// </summary>
    [OperationContract]
    MaintenanceDto CloseMaintenance(int maintenanceId, DateTime dateFin);
}
