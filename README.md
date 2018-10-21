# Pioneer Receiver Control

[![NuGet Badge](https://buildstats.info/nuget/PioneerReceiverControl.Rx)](https://www.nuget.org/packages/PioneerReceiverControl.Rx)

[![System.Reactive](http://img.shields.io/badge/Rx-v4.1.2-ff69b4.svg)](http://reactivex.io/) 

*Please star this project if you find it useful. Thank you!*

## Credits
This project was inspired by a library by Rayscene NS [Pioneer-Receiver-Controller](https://github.com/RaysceneNS/Pioneer-Receiver-Controller)


## Why this library
There are a few other Pioneer Receiver libraries on GitHub, however none of them is for .NET Standard 2.0, none of the are for both network and serial port usage, and none of them utilizes Reactive Extensions. In particular the support for Reactive Extensions was a motivational factor for creating this library, as Rective Extensions are very suitable for managing and reacting to streams of data. 

## How to use this library
The library is more like a tool set. The most obvious us is to use the `ReceiverController` class, however it is also possible to use the library in ***raw mode***

### `ReceiverController` Constructors
There are four different options for construting the `ReceiverController`. All constructors need a list of 



## Raw mode

[MQTT](http://mqtt.org/) and [Reactive Extensions](http://reactivex.io/) (aka. ReactiveX or just Rx) are a perfect for each other! Rx is an API for asynchronous programming
with observable streams, while MQTT is a protocol that produces asynchronous streams.

https://www.pioneerelectronics.com/PUSA/Support/Home-Entertainment-Custom-Install/RS-232+&+IP+Codes/A+V+Receivers