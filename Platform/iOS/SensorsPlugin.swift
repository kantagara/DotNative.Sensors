import CoreMotion
import Foundation

@MainActor final class SensorsPlugin {
    private static var instance: SensorsPlugin?
    private let motion = CMMotionManager()
    private var streams: [NativeEventChannel] = []

    static func register() {
        if instance == nil {
            instance = SensorsPlugin()
        }
    }

    private init() {
        streams.append(
            NativeEventChannel(
                "dotnative.sensors.accelerometer",
                onListen: { [motion] _, events in
                    guard motion.isAccelerometerAvailable else {
                        events.failure("sensor_unavailable", "Accelerometer is not available")
                        return
                    }
                    motion.accelerometerUpdateInterval = 0.1
                    motion.startAccelerometerUpdates(to: .main) { data, error in
                        Task { @MainActor in
                            if let error {
                                events.failure("sensor_failed", error.localizedDescription)
                                return
                            }
                            guard let data else { return }
                            events.success(
                                .map([
                                    "x": .double(data.acceleration.x * 9.80665),
                                    "y": .double(data.acceleration.y * 9.80665),
                                    "z": .double(data.acceleration.z * 9.80665),
                                    "timestamp": .integer(Int64(data.timestamp * 1000)),
                                ]))
                        }
                    }
                },
                onCancel: { [motion] _ in motion.stopAccelerometerUpdates() }
            ))
        streams.append(
            NativeEventChannel(
                "dotnative.sensors.gyroscope",
                onListen: { [motion] _, events in
                    guard motion.isGyroAvailable else {
                        events.failure("sensor_unavailable", "Gyroscope is not available")
                        return
                    }
                    motion.gyroUpdateInterval = 0.1
                    motion.startGyroUpdates(to: .main) { data, error in
                        Task { @MainActor in
                            if let error {
                                events.failure("sensor_failed", error.localizedDescription)
                                return
                            }
                            guard let data else { return }
                            events.success(
                                .map([
                                    "x": .double(data.rotationRate.x),
                                    "y": .double(data.rotationRate.y),
                                    "z": .double(data.rotationRate.z),
                                    "timestamp": .integer(Int64(data.timestamp * 1000)),
                                ]))
                        }
                    }
                },
                onCancel: { [motion] _ in motion.stopGyroUpdates() }
            ))
    }
}
