using System.Runtime.CompilerServices;
using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.Sensors;

public enum SensorKind
{
    Accelerometer,
    Gyroscope,
}

public sealed record SensorSample(double X, double Y, double Z, long MonotonicMilliseconds);

public interface ISensors
{
    IAsyncEnumerable<SensorSample> WatchAsync(
        SensorKind sensor,
        CancellationToken cancellationToken = default
    );
}

public static class SensorsServices
{
    public static IServiceCollection AddSensors(this IServiceCollection services)
    {
        services.TryAddSingleton<ISensors, NativeSensors>();
        return services;
    }
}

internal sealed class NativeSensors(IPlatformChannels channels) : ISensors
{
    public async IAsyncEnumerable<SensorSample> WatchAsync(
        SensorKind sensor,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var name = sensor switch
        {
            SensorKind.Accelerometer => "dotnative.sensors.accelerometer",
            SensorKind.Gyroscope => "dotnative.sensors.gyroscope",
            _ => throw new ArgumentOutOfRangeException(nameof(sensor)),
        };
        var events = new EventChannel(channels, name);
        await foreach (
            var item in events
                .ReadAllAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false)
        )
        {
            if (
                item is not Dictionary<string, object?> map
                || map.GetValueOrDefault("x") is not double x
                || map.GetValueOrDefault("y") is not double y
                || map.GetValueOrDefault("z") is not double z
                || map.GetValueOrDefault("timestamp") is not long timestamp
            )
                throw new InvalidDataException("Invalid native sensor event.");
            yield return new(x, y, z, timestamp);
        }
    }
}
