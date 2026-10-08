# DotNative.Sensors

Streams accelerometer and gyroscope samples on Android and iOS.

```csharp
builder.Services.AddSensors();
var sensors = services.Sensors;
await foreach (var sample in sensors.WatchAsync(SensorKind.Accelerometer, cancellationToken))
{
    Console.WriteLine($"{sample.X}, {sample.Y}, {sample.Z}");
}
```

Each event includes X/Y/Z and monotonic milliseconds since device boot. The
accelerometer reports meters per second squared. The gyroscope reports radians
per second. Dispose the async enumerator or cancel its token to stop the native
sensor. One consumer per sensor channel is allowed. The native event queue is
bounded and raises a plugin error if the consumer cannot keep up.

| Platform | Status |
| --- | --- |
| Android | Accelerometer and gyroscope when present |
| iOS | Core Motion accelerometer and gyroscope |
| macOS | Not implemented |
| Windows | Not implemented |
| Linux | Not implemented |

No runtime permission is required. Sensor availability depends on the device.
This release does not expose magnetometer, barometer, orientation, or sensor
sampling-rate controls. The independent DotNative implementation is MIT licensed.

## Service access

Import `DotNative.Sensors` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.Sensors;

var plugin = services.Sensors;
```

The getter calls `GetRequiredService<ISensors>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddSensors(...)` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.Sensors();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
