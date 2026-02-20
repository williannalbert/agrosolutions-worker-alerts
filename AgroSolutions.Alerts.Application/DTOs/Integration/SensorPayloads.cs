namespace AgroSolutions.Alerts.Application.DTOs.Integration;

public record SoilDataDto(
    double SoilMoisturePercent,
    double SoilPh,
    SoilNutrientsDto Nutrients
);

public record SoilNutrientsDto(
    double NitrogenMgKg,
    double PhosphorusMgKg,
    double PotassiumMgKg
);

public record WeatherDataDto(
    double TempCelsius,
    double HumidityPercent,
    double WindSpeedKmh,
    string? WindDirection,
    double RainMmLastHour,
    double DewPoint
);

public record SiloDataDto(
    double FillLevelPercent,
    double AvgTempCelsius,
    double Co2Ppm
);