using System.Text;
using System.Text.Json;
using FingridDatahubLogger.Converters;
using FingridDatahubLogger.Services.DatabaseModels;
using FingridDatahubLogger.Services.DatahubModels;
using FingridDatahubLogger.Settings;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FingridDatahubLogger.Services;

public class TimescaleClient : ITimescaleClient
{
    private readonly ILogger<TimescaleClient> _logger;
    private readonly TimescaleDbSettings _timescaleDbSettings;

    public TimescaleClient(ILogger<TimescaleClient> logger, IOptions<TimescaleDbSettings> timescaleDbSettings)
    {
        _logger = logger;
        _timescaleDbSettings = timescaleDbSettings.Value;
    }

    public async Task InsertConsumptionsAsync(TimeSeriesResponse consumptions,
        CancellationToken cancellationToken = default)
    {
        if (_timescaleDbSettings.Enabled && consumptions.TimeSeries.Count > 0)
        {
            // Count amount of observations
            var observationCount = consumptions.TimeSeries.Sum(series => series.Observations.Count);

            if (observationCount > 1000)
            {
                await InsertConsumptionsBulkAsync(consumptions, cancellationToken);
            }
            else
            {
                await InsertConsumptionsMinimalAsync(consumptions, cancellationToken);
            }
        }
    }
    
    private async Task InsertConsumptionsBulkAsync(TimeSeriesResponse consumptions, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_timescaleDbSettings.Enabled && consumptions.TimeSeries.Count > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_timescaleDbSettings.ConnectionString);
                    await conn.OpenAsync(cancellationToken);
    
                    await using var tran = await conn.BeginTransactionAsync(cancellationToken);
    
                    // Create a temporary table
                    const string createTempTableSql = $@"
                        CREATE TEMP TABLE temp_consumptions (
                            period_start TIMESTAMPTZ,
                            metering_point_ean VARCHAR,
                            resolution_duration VARCHAR,
                            product_type VARCHAR,
                            unit_type VARCHAR,
                            reading_type VARCHAR,
                            measurement_source VARCHAR,
                            quantity DOUBLE PRECISION,
                            quality VARCHAR
                        )";
                    await using (var cmd = new NpgsqlCommand(createTempTableSql, conn, tran))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
    
                    // Use COPY to insert data into the temporary table
                    var copyCommand = new StringBuilder();
                    copyCommand.AppendLine("COPY temp_consumptions (period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type, measurement_source, quantity, quality) FROM STDIN (FORMAT BINARY)");
    
                    await using (var writer = await conn.BeginBinaryImportAsync(copyCommand.ToString(), cancellationToken))
                    {
                        foreach (var series in consumptions.TimeSeries)
                        {
                            var observations = series.Observations;
                            foreach (var observation in observations)
                            {
                                await writer.StartRowAsync(cancellationToken);
                                await writer.WriteAsync(DateTimeOffset.FromUnixTimeMilliseconds(observation.Epoch), NpgsqlTypes.NpgsqlDbType.TimestampTz, cancellationToken);
                                await writer.WriteAsync(series.MeteringPointEan, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync(series.ResolutionDuration.ToString(), NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync(JsonSerializer.Serialize(series.ProductType, new JsonSerializerOptions { Converters = { new ProductTypeConverter() } }).Trim('"'), NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync(series.UnitType, NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync(series.ReadingType.ToString(), NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync("FingridDatahub", NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                                await writer.WriteAsync(observation.Quantity, NpgsqlTypes.NpgsqlDbType.Double, cancellationToken);
                                await writer.WriteAsync(observation.Quality.ToString(), NpgsqlTypes.NpgsqlDbType.Text, cancellationToken);
                            }
                        }
    
                        await writer.CompleteAsync(cancellationToken);
                    }
    
                    // Merge data from the temporary table into the target table
                    var mergeSql = $@"
                        INSERT INTO {_timescaleDbSettings.TableName} (period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type, measurement_source, quantity, quality)
                        SELECT period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type, measurement_source, quantity, quality
                        FROM temp_consumptions
                        ON CONFLICT (period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type)
                        DO UPDATE SET
                            measurement_source = EXCLUDED.measurement_source,
                            quantity = EXCLUDED.quantity,
                            quality = EXCLUDED.quality";
                    await using (var cmd = new NpgsqlCommand(mergeSql, conn, tran))
                    {
                        await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
    
                    await tran.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to TimescaleDB");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unknown error while saving data to TimescaleDB");
        }
    }

    private async Task InsertConsumptionsMinimalAsync(TimeSeriesResponse consumptions, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_timescaleDbSettings.Enabled && consumptions.TimeSeries.Count > 0)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_timescaleDbSettings.ConnectionString);
                    await conn.OpenAsync(cancellationToken);
                    
                    await using var tran = await conn.BeginTransactionAsync(cancellationToken);
    
                    foreach (var series in consumptions.TimeSeries)
                    {
                        var observations = series.Observations;
                        foreach (var observation in observations)
                        {
                            var sql = $@"
                                INSERT INTO {_timescaleDbSettings.TableName} (period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type, measurement_source, quantity, quality)
                                VALUES (@period_start, @metering_point_ean, @resolution_duration, @product_type, @unit_type, @reading_type, @measurement_source, @quantity, @quality)
                                ON CONFLICT (period_start, metering_point_ean, resolution_duration, product_type, unit_type, reading_type)
                                DO UPDATE SET
                                    measurement_source = @measurement_source, quantity = @quantity, quality = @quality
                            ";
    
                            await using var cmd = new NpgsqlCommand(sql, conn);
                            series.AddMetrics(cmd.Parameters);
                            observation.AddMetrics(cmd.Parameters);
    
                            await cmd.ExecuteNonQueryAsync(cancellationToken);
                        }
                    }
    
                    await tran.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing to TimescaleDB");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unknown error while saving data to TimescaleDB");
        }
    }
    
    public async Task<List<DbMeteringPoint>> GetMeteringPointsAsync()
    {
        var meteringPoints = new List<DbMeteringPoint>();

        await using var conn = new NpgsqlConnection(_timescaleDbSettings.ConnectionString);
        await conn.OpenAsync();

        var sql = @"
            SELECT
                ""metering_point_ean"",
                ""type"",
                ""street_name"",
                ""building_number"",
                ""postal_code"",
                ""post_office"",
                ""start_date"",
                ""createdAt"",
                ""updatedAt""
            FROM
                ""meteringPoint""";

        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var meteringPoint = new DbMeteringPoint
            {
                MeteringPointEan = reader.GetString(0),
                Type = reader.GetString(1),
                StreetName = reader.IsDBNull(2) ? null : reader.GetString(2),
                BuildingNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                PostalCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                PostOffice = reader.IsDBNull(5) ? null : reader.GetString(5),
                StartDate = reader.GetDateTime(6),
                CreatedAt = reader.GetDateTime(7),
                UpdatedAt = reader.GetDateTime(8)
            };

            meteringPoints.Add(meteringPoint);
        }

        return meteringPoints;
    }
}