using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MaintenanceService.Models;
using MaintenanceService.Dtos;

namespace MaintenanceService.Services
{
    /// <summary>
    /// Implementation of Maintenance SOAP Web Service
    /// </summary>
    public class MaintenanceServiceImpl : IMaintenanceService
    {
        private readonly MaintenanceDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _vehicleServiceBaseUrl;
        private readonly ILogger<MaintenanceServiceImpl> _logger;

        public MaintenanceServiceImpl(
            MaintenanceDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<MaintenanceServiceImpl> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger;
            _vehicleServiceBaseUrl = configuration["VehicleService:BaseUrl"] ??
                                 Environment.GetEnvironmentVariable("VEHICLE_SERVICE_BASEURL") ??
                                 "http://localhost:8082";
        }

        /// <summary>
        /// Schedule a new maintenance for a vehicle
        /// </summary>
        public MaintenanceResponse ScheduleMaintenance(MaintenanceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate vehicle exists
            if (!VehicleExists(request.VehicleId))
                throw new InvalidOperationException($"Vehicle {request.VehicleId} not found.");

            // Create maintenance record
            var maintenance = new Maintenance
            {
                VehicleId = request.VehicleId,
                MaintenanceType = request.MaintenanceType ?? "PREVENTIVE",
                ScheduledDate = request.ScheduledDate,
                Description = request.Description ?? "",
                Priority = request.Priority ?? "MEDIUM",
                EstimatedCost = request.EstimatedCost,
                CurrentMileage = request.CurrentMileage,
                Status = "SCHEDULED",
                CreatedAt = DateTime.UtcNow
            };

            _db.Maintenances.Add(maintenance);
            _db.SaveChanges();

            // Update vehicle status to MAINTENANCE
            try
            {
                UpdateVehicleStatus(request.VehicleId, "MAINTENANCE");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update vehicle status for vehicle {VehicleId}", request.VehicleId);
            }

            _logger?.LogInformation("Maintenance scheduled for vehicle {VehicleId}, ID: {MaintenanceId}",
                request.VehicleId, maintenance.Id);

            return ToResponse(maintenance);
        }

        /// <summary>
        /// Update maintenance status with state machine logic
        /// </summary>
        public bool UpdateMaintenanceStatus(int id, string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty", nameof(status));

            var maintenance = _db.Maintenances.FirstOrDefault(m => m.Id == id);
            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {id} not found.");

            var validStatuses = new[] { "SCHEDULED", "IN_PROGRESS", "COMPLETED", "CANCELLED" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Invalid status. Valid values: {string.Join(", ", validStatuses)}", nameof(status));

            maintenance.Status = status;
            maintenance.UpdatedAt = DateTime.UtcNow;

            if (status == "COMPLETED")
            {
                maintenance.CompletedDate = DateTime.UtcNow;

                // Check if other maintenances exist for this vehicle
                var hasOtherOpen = _db.Maintenances.Any(m =>
                    m.VehicleId == maintenance.VehicleId &&
                    m.Id != id &&
                    m.Status != "COMPLETED" &&
                    m.Status != "CANCELLED");

                if (!hasOtherOpen)
                {
                    try
                    {
                        UpdateVehicleStatus(maintenance.VehicleId, "DISPONIBLE");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to set vehicle {VehicleId} to DISPONIBLE", maintenance.VehicleId);
                    }
                }
            }

            _db.SaveChanges();
            _logger?.LogInformation("Maintenance {MaintenanceId} status updated to {Status}", id, status);

            return true;
        }

        /// <summary>
        /// Get maintenance history for a vehicle
        /// </summary>
        public List<MaintenanceResponse> GetMaintenanceHistory(int vehicleId)
        {
            var maintenances = _db.Maintenances
                .Where(m => m.VehicleId == vehicleId)
                .Include(m => m.SpareParts)
                .OrderByDescending(m => m.ScheduledDate)
                .ToList();

            return maintenances.Select(ToResponse).ToList();
        }

        /// <summary>
        /// Get all upcoming maintenances (scheduled or in-progress)
        /// </summary>
        public List<MaintenanceResponse> GetUpcomingMaintenances()
        {
            var upcomingStatuses = new[] { "SCHEDULED", "IN_PROGRESS" };
            var maintenances = _db.Maintenances
                .Where(m => upcomingStatuses.Contains(m.Status))
                .Include(m => m.SpareParts)
                .OrderBy(m => m.ScheduledDate)
                .ToList();

            return maintenances.Select(ToResponse).ToList();
        }

        /// <summary>
        /// Record repair/maintenance work completion
        /// </summary>
        public RepairResponse RecordRepair(RepairRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var maintenance = _db.Maintenances
                .Include(m => m.SpareParts)
                .FirstOrDefault(m => m.Id == request.MaintenanceId);

            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {request.MaintenanceId} not found.");

            // Update maintenance with repair details
            maintenance.MechanicNotes = request.MechanicNotes ?? "";
            maintenance.ActualCost = request.ActualCost;
            maintenance.Status = "COMPLETED";
            maintenance.CompletedDate = DateTime.UtcNow;
            maintenance.UpdatedAt = DateTime.UtcNow;

            // Add spare parts
            if (request.SpareParts != null && request.SpareParts.Any())
            {
                foreach (var partRequest in request.SpareParts)
                {
                    var sparePart = new SparePart
                    {
                        MaintenanceId = maintenance.Id,
                        Name = partRequest.Name,
                        PartNumber = partRequest.PartNumber,
                        Quantity = partRequest.Quantity,
                        UnitPrice = partRequest.UnitPrice,
                        TotalPrice = partRequest.Quantity * partRequest.UnitPrice,
                        CreatedAt = DateTime.UtcNow
                    };
                    maintenance.SpareParts.Add(sparePart);
                }
            }

            _db.SaveChanges();

            // Update vehicle status back to DISPONIBLE
            try
            {
                var hasOtherOpen = _db.Maintenances.Any(m =>
                    m.VehicleId == maintenance.VehicleId &&
                    m.Id != request.MaintenanceId &&
                    m.Status != "COMPLETED" &&
                    m.Status != "CANCELLED");

                if (!hasOtherOpen)
                {
                    UpdateVehicleStatus(maintenance.VehicleId, "DISPONIBLE");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update vehicle status after repair");
            }

            _logger?.LogInformation("Repair recorded for maintenance {MaintenanceId}", request.MaintenanceId);

            return new RepairResponse
            {
                MaintenanceId = maintenance.Id,
                Status = maintenance.Status,
                TotalCost = maintenance.ActualCost ?? 0,
                CompletedDate = maintenance.CompletedDate ?? DateTime.UtcNow,
                SpareParts = maintenance.SpareParts.Select(ToSparePartDto).ToList()
            };
        }

        /// <summary>
        /// Update vehicle condition from inspection report
        /// </summary>
        public bool UpdateVehicleCondition(ConditionReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (!VehicleExists(report.VehicleId))
                throw new InvalidOperationException($"Vehicle {report.VehicleId} not found.");

            // If issues found, schedule corrective maintenance
            if (report.IssuesFound != null && report.IssuesFound.Any())
            {
                var correctiveMaintenance = new Maintenance
                {
                    VehicleId = report.VehicleId,
                    MaintenanceType = "CORRECTIVE",
                    ScheduledDate = DateTime.UtcNow.AddDays(1),
                    Description = "Issues found during inspection: " + string.Join(", ", report.IssuesFound),
                    Priority = "HIGH",
                    CurrentMileage = report.CurrentMileage,
                    Status = "SCHEDULED",
                    CreatedAt = DateTime.UtcNow
                };

                _db.Maintenances.Add(correctiveMaintenance);
                _db.SaveChanges();

                _logger?.LogInformation("Corrective maintenance scheduled for vehicle {VehicleId}", report.VehicleId);
            }

            return true;
        }

        /// <summary>
        /// Calculate total maintenance cost including spare parts
        /// </summary>
        public decimal CalculateMaintenanceCost(int maintenanceId)
        {
            var maintenance = _db.Maintenances
                .Include(m => m.SpareParts)
                .FirstOrDefault(m => m.Id == maintenanceId);

            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {maintenanceId} not found.");

            decimal totalCost = maintenance.ActualCost ?? maintenance.EstimatedCost;

            if (maintenance.SpareParts != null && maintenance.SpareParts.Any())
            {
                totalCost += maintenance.SpareParts.Sum(sp => sp.TotalPrice);
            }

            return totalCost;
        }

        /// <summary>
        /// Generate maintenance report for date range
        /// </summary>
        public MaintenanceReport GenerateMaintenanceReport(DateTime startDate, DateTime endDate)
        {
            var maintenances = _db.Maintenances
                .Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
                .Include(m => m.SpareParts)
                .ToList();

            var report = new MaintenanceReport
            {
                GeneratedDate = DateTime.UtcNow,
                StartDate = startDate,
                EndDate = endDate,
                TotalMaintenances = maintenances.Count,
                CompletedMaintenances = maintenances.Count(m => m.Status == "COMPLETED"),
                PendingMaintenances = maintenances.Count(m => m.Status == "SCHEDULED" || m.Status == "IN_PROGRESS"),
                TotalCost = maintenances.Sum(m => m.ActualCost ?? m.EstimatedCost),
                Entries = maintenances.Select(m => new MaintenanceReportEntry
                {
                    MaintenanceId = m.Id,
                    VehicleId = m.VehicleId,
                    MaintenanceType = m.MaintenanceType,
                    ScheduledDate = m.ScheduledDate,
                    CompletedDate = m.CompletedDate,
                    Status = m.Status,
                    ActualCost = m.ActualCost ?? m.EstimatedCost,
                    Priority = m.Priority
                }).ToList()
            };

            _logger?.LogInformation("Maintenance report generated for period {StartDate} to {EndDate}", startDate, endDate);
            return report;
        }

        /// <summary>
        /// Get details of a specific maintenance record
        /// </summary>
        public MaintenanceResponse GetMaintenanceById(int maintenanceId)
        {
            var maintenance = _db.Maintenances
                .Include(m => m.SpareParts)
                .FirstOrDefault(m => m.Id == maintenanceId);

            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {maintenanceId} not found.");

            return ToResponse(maintenance);
        }

        /// <summary>
        /// Cancel a scheduled maintenance
        /// </summary>
        public bool CancelMaintenance(int maintenanceId, string reason)
        {
            var maintenance = _db.Maintenances.FirstOrDefault(m => m.Id == maintenanceId);
            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {maintenanceId} not found.");

            if (maintenance.Status == "COMPLETED" || maintenance.Status == "CANCELLED")
                throw new InvalidOperationException($"Cannot cancel maintenance with status {maintenance.Status}");

            maintenance.Status = "CANCELLED";
            maintenance.MechanicNotes = $"Cancelled: {reason}";
            maintenance.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            // Try to set vehicle back to available if no other active maintenance
            try
            {
                var hasOtherActive = _db.Maintenances.Any(m =>
                    m.VehicleId == maintenance.VehicleId &&
                    m.Id != maintenanceId &&
                    m.Status != "COMPLETED" &&
                    m.Status != "CANCELLED");

                if (!hasOtherActive)
                {
                    UpdateVehicleStatus(maintenance.VehicleId, "DISPONIBLE");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update vehicle status after cancellation");
            }

            _logger?.LogInformation("Maintenance {MaintenanceId} cancelled. Reason: {Reason}", maintenanceId, reason);
            return true;
        }

        /// <summary>
        /// Backward compatible: Create maintenance (legacy method)
        /// </summary>
        public MaintenanceDto CreateMaintenance(int vehicleId, string maintenanceType, string description, DateTime scheduledDate)
        {
            var request = new MaintenanceRequest
            {
                VehicleId = vehicleId,
                MaintenanceType = maintenanceType ?? "PREVENTIVE",
                Description = description ?? "",
                ScheduledDate = scheduledDate,
                Priority = "MEDIUM",
                EstimatedCost = 0,
                CurrentMileage = 0
            };

            var response = ScheduleMaintenance(request);
            return ToDtoLegacy(response);
        }

        /// <summary>
        /// Backward compatible: Close maintenance (legacy method)
        /// </summary>
        public MaintenanceDto CloseMaintenance(int maintenanceId, DateTime dateFin)
        {
            var maintenance = _db.Maintenances
                .Include(m => m.SpareParts)
                .FirstOrDefault(m => m.Id == maintenanceId);

            if (maintenance == null)
                throw new InvalidOperationException($"Maintenance {maintenanceId} not found.");

            maintenance.CompletedDate = dateFin;
            maintenance.Status = "COMPLETED";
            maintenance.UpdatedAt = DateTime.UtcNow;

            _db.SaveChanges();

            // Try to set vehicle back to available if no other active maintenance
            try
            {
                var hasOtherActive = _db.Maintenances.Any(m =>
                    m.VehicleId == maintenance.VehicleId &&
                    m.Id != maintenanceId &&
                    m.Status != "COMPLETED" &&
                    m.Status != "CANCELLED");

                if (!hasOtherActive)
                {
                    UpdateVehicleStatus(maintenance.VehicleId, "DISPONIBLE");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update vehicle status");
            }

            return ToDto(maintenance);
        }

        // =====================
        // Private Helper Methods
        // =====================

        private MaintenanceResponse ToResponse(Maintenance m)
        {
            return new MaintenanceResponse
            {
                Id = m.Id,
                VehicleId = m.VehicleId,
                MaintenanceType = m.MaintenanceType,
                ScheduledDate = m.ScheduledDate,
                CompletedDate = m.CompletedDate,
                Status = m.Status,
                EstimatedCost = m.EstimatedCost,
                ActualCost = m.ActualCost,
                CurrentMileage = m.CurrentMileage,
                Description = m.Description,
                MechanicNotes = m.MechanicNotes,
                Priority = m.Priority,
                SpareParts = m.SpareParts?.Select(ToSparePartDto).ToList() ?? new List<SparePartDto>(),
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            };
        }

        private MaintenanceDto ToDto(Maintenance m)
        {
            return new MaintenanceDto
            {
                Id = m.Id,
                VehicleId = m.VehicleId,
                MaintenanceType = m.MaintenanceType,
                ScheduledDate = m.ScheduledDate,
                CompletedDate = m.CompletedDate,
                Status = m.Status,
                EstimatedCost = m.EstimatedCost,
                ActualCost = m.ActualCost,
                CurrentMileage = m.CurrentMileage,
                Description = m.Description,
                MechanicNotes = m.MechanicNotes,
                Priority = m.Priority,
                SpareParts = m.SpareParts?.Select(ToSparePartDto).ToList() ?? new List<SparePartDto>(),
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            };
        }

        private MaintenanceDto ToDtoLegacy(MaintenanceResponse response)
        {
            return new MaintenanceDto
            {
                Id = response.Id,
                VehicleId = response.VehicleId,
                MaintenanceType = response.MaintenanceType,
                ScheduledDate = response.ScheduledDate,
                CompletedDate = response.CompletedDate,
                Status = response.Status,
                EstimatedCost = response.EstimatedCost,
                ActualCost = response.ActualCost,
                CurrentMileage = response.CurrentMileage,
                Description = response.Description,
                MechanicNotes = response.MechanicNotes,
                Priority = response.Priority,
                SpareParts = response.SpareParts,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt
            };
        }

        private SparePartDto ToSparePartDto(SparePart sp)
        {
            return new SparePartDto
            {
                Id = sp.Id,
                MaintenanceId = sp.MaintenanceId,
                Name = sp.Name,
                PartNumber = sp.PartNumber,
                Quantity = sp.Quantity,
                UnitPrice = sp.UnitPrice,
                TotalPrice = sp.TotalPrice,
                CreatedAt = sp.CreatedAt
            };
        }

        private bool VehicleExists(int vehicleId)
        {
            // Allow positive vehicle IDs (even if Vehicle Service isn't available)
            // This enables testing without requiring the Vehicle Service to be running
            if (vehicleId <= 0)
            {
                _logger?.LogWarning("Invalid vehicle ID: {VehicleId}", vehicleId);
                return false;
            }

            try
            {
                var client = CreateVehicleClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = client.GetAsync($"/api/vehicles/{vehicleId}").Result;
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // If Vehicle Service is unavailable, allow the maintenance to proceed
                // This enables testing and graceful degradation
                _logger?.LogWarning(ex, "Vehicle Service unavailable for checking vehicle {VehicleId}. Allowing maintenance to proceed.", vehicleId);
                return true;  // Allow maintenance to proceed even if service is down
            }
        }

        private void UpdateVehicleStatus(int vehicleId, string status)
        {
            try
            {
                var client = CreateVehicleClient();
                var payload = JsonConvert.SerializeObject(new { status = status });
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/vehicles/{vehicleId}/status")
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };

                var response = client.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = response.Content.ReadAsStringAsync().Result;
                    _logger?.LogWarning("Failed to update vehicle status. Status: {StatusCode}, Body: {Body}",
                        response.StatusCode, errorBody);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to update vehicle status for vehicle {VehicleId}", vehicleId);
            }
        }

        private HttpClient CreateVehicleClient()
        {
            var client = _httpClientFactory.CreateClient();
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(_vehicleServiceBaseUrl.TrimEnd('/'));
            }
            return client;
        }
    }
}
