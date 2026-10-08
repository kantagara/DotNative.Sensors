package com.dotnative.plugins

import android.app.Activity
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener
import android.hardware.SensorManager

class SensorsPlugin(activity: Activity) {
    private val manager = activity.getSystemService(SensorManager::class.java)
    private val listeners = mutableMapOf<Int, SensorEventListener>()
    private val streams = mutableListOf<NativeEventChannel>()

    init {
        register("dotnative.sensors.accelerometer", Sensor.TYPE_ACCELEROMETER)
        register("dotnative.sensors.gyroscope", Sensor.TYPE_GYROSCOPE)
    }

    private fun register(name: String, type: Int) {
        streams.add(
            NativeEventChannel(
                name,
                onListen = { _, events ->
                    val sensorManager = manager
                    val sensor = sensorManager?.getDefaultSensor(type)
                    if (sensor == null) {
                        events.failure("sensor_unavailable", "Requested sensor is not available")
                    } else {
                        val listener =
                            object : SensorEventListener {
                                override fun onSensorChanged(event: SensorEvent) {
                                    events.success(
                                        mapOf(
                                            "x" to event.values[0].toDouble(),
                                            "y" to event.values[1].toDouble(),
                                            "z" to event.values[2].toDouble(),
                                            "timestamp" to event.timestamp / 1_000_000L,
                                        ),
                                    )
                                }

                                override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) =
                                    Unit
                            }
                        listeners[type] = listener
                        if (
                            !sensorManager.registerListener(
                                listener,
                                sensor,
                                SensorManager.SENSOR_DELAY_NORMAL,
                                NativeChannels.main,
                            )
                        ) {
                            events.failure(
                                "sensor_unavailable",
                                "Could not start the requested sensor",
                            )
                        }
                    }
                },
                onCancel = { _ -> listeners.remove(type)?.let { manager?.unregisterListener(it) } },
            ),
        )
    }
}
