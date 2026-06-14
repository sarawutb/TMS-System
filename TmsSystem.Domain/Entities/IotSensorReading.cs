using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class IotSensorReading : AuditableEntity
{
    public long IotSensorReadingId { get; set; }
    public long IotDeviceId { get; set; }
    public long? ShipmentId { get; set; }
    public DateTime ReadingDate { get; set; }
    public decimal? TemperatureC { get; set; }
    public decimal? HumidityPercent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool AlertFlag { get; set; }
}
