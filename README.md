# WRA.Sensors Solution
An Arduino and Universal Windows Platform project to deploy an Arduino as a telemetry device 
and a Raspberry Pi running Windows 10 IoT Core as an IoT Gateway pushing sensor data to 
Azure IoT Hub.

## WRA.Sensors.UI
This is the UWP app that we run on the Raspberry Pi.  It calls to the Arduino over bluetooth and triggers a 
custom Firmata Sysex command to retrieve the sensor data.  It then retrieves the response in the 
`Firmata_StringMessageReceived()` event handler.  This dta is then packaged up into a JSON string
and published to the IoT Hub. 

##WRA.Sensors.Arduino
This is the Arduino sketch.  It is a modified version of the StandardFirmata sketch that includes
a modified `sysecCallback()` function which reads a bunch of sensors and returns a string of readings
back to the caller (Raspberry Pi). 
  
 ---  
   
 ##Support Apps
 The following console apps were created from code listed on the [Get started with Azure IoT Hub] page.
 
 [Get started with Azure IoT Hub]: https://azure.microsoft.com/en-us/documentation/articles/iot-hub-csharp-csharp-getstarted/
 
###CreateDeviceIdentity
This console app is used to get a deviceId from Azure IoT Hub 
that is needed in the WRA.Sensors.UI app.  

###ReadDeviceToCloudMessages
This console app is used to read the messages sent to the IoT Hub from the __SimulatedDevice__ console app 
or from the __WRA.Sensors.UI__ UWP app.  This code is also from the blog post above.

###SimulatedDevice
This console app sends data to your IoT Hub to test it out before you add the telemetry code to your custom app.  The
important code from this app was ported into the __WRA.Sensors.UI__ app.
