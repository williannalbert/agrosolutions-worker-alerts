namespace AgroSolutions.Alerts.Application.DTOs.Integration;

public record CreateReadingDto(
    Guid FieldId,
    Guid SensorId,
    string TypeSensor,
    DateTime TimeStamp, 
    object Data
);