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

    public async Task InsertConsumptionsAsync(TimeSeriesResponse consumptions, CancellationToken cancellationToken = default)
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
}