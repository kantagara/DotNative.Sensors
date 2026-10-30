using DotNative.Plugins;

namespace DotNative.Sensors;

public static class Sensors
{
    public static ISensors Create() => PluginImplementations.Resolve<ISensors>(() =>
    {
        var runtime = PluginRuntime.Current;
        if (runtime.Target.Platform is not (NativePlatform.Android or NativePlatform.IOS))
            throw new PlatformNotSupportedException("Built-in sensors support Android/iOS. Register an ISensors implementation for this platform with PluginImplementations.Register.");
        return new NativeSensors(runtime.Channels);
    });
    public static IAsyncEnumerable<SensorSample> WatchAsync(SensorKind sensor, CancellationToken cancellationToken = default) =>
        Create().WatchAsync(sensor, cancellationToken);
}
