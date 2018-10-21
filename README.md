# Pioneer Receiver Control

[![NuGet Badge](https://buildstats.info/nuget/PioneerReceiverControl.Rx)](https://www.nuget.org/packages/PioneerReceiverControl.Rx)

[![System.Reactive](http://img.shields.io/badge/Rx-v4.1.2-ff69b4.svg)](http://reactivex.io/) 

*Please star this project if you find it useful. Thank you!*

## Credits
This project was inspired by a library by Rayscene NS [Pioneer-Receiver-Controller](https://github.com/RaysceneNS/Pioneer-Receiver-Controller)


## Why this library
There are a few other Pioneer Receiver libraries on GitHub, however none of them is for .NET Standard 2.0, none of the are for both network and serial port usage, and none of them utilizes Reactive Extensions. In particular the support for Reactive Extensions was a motivational factor for creating this library, as Rective Extensions are very suitable for managing and reacting to streams of data. 

## How to use this library
The library is more like a tool set. The most obvious us is to use the `ReceiverController` class, however it is also possible to use the library in _raw mode_, which enables you to both listen to commands and send commands in the native Pioneer Receiver format - i.e. in a format that have not been enriched. Please see the copy of the [Pioneer Receiver specification](https://github.com/1iveowl/PioneerReceiverControl.Rx/tree/master/src/specification), which can also be found [here](https://www.pioneerelectronics.com/PUSA/Support/Home-Entertainment-Custom-Install/RS-232+&+IP+Codes/A+V+Receivers). 

### The `ReceiverController` Constructors
There are four different options for constructing the `ReceiverController`. Two for network connection usage and two for serial port connection usage. 

For each type of connection you can then choose to either a) let the library initialize the connection (recommended) or b) intialize the connection youself (advanced - please see raw mode for details) and then pass the observable stream of raw data to the `RecieverController` as the 2nd parameter for the constructor. 

In the following example I will only use the first type of constructor. The creating of a raw observable stream is described later.

In all of them expects a list of defined commands. Such a [list is included](https://github.com/1iveowl/PioneerReceiverControl.Rx/blob/master/src/main/PioneerReceiverControl.Rx/Data/DefaultReceiverCommandDefinition.cs) as part of the library, as provides a great example for how to extend the library.

### Example

In the following example a network connection is used to connect to the Receiver to listen for updates. 

After the connection is establised to commands are send to the receiver to demostrate the two send options:
1. Fire and forget
2. Send and wait for reply.

Please note that there is no way to guarantee that a response from the reciever is related to one specific command. If someone else is enteracting with the receiver, then they could theoretically be changing the same parameter. All the method does is to listen for updates from the Reciever that are related to the command just send.

```csharp
public class Program
{
    // Needed for alternative to Console.ReadLine();
    private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

    private static IDisposable _disposableResponse;
    private static ReceiverController _receiverController;

    private static IEnumerable<IReceiverCommandDefinition> _commandDefinitions;

    private static TcpClient _tcpClient;
    private static IPAddress _ipAddress;
    private static int _port;


    private static async Task Main(string[] args)
    {
        _ipAddress = IPAddress.Parse("192.168.0.24");
        _port = 23;

        _commandDefinitions = new DefaultReceiverCommandDefinition().GetDefaultDefinitions;

        // Run this when the user presses the ctrl-C key - alternative to Console.ReadLine();
        Console.CancelKeyPress += (o, e) =>
        {
            // Clean up...
            _disposableReceiverController?.Dispose(); 
            _receiverController?.Dispose();
            _tcpClient?.Dispose();
            Console.WriteLine("Exit");
            WaitHandle.Set();
        };

        // Start the TCP Listener.
        StartTcpListener();

        // Wait for connection
        await Task.Delay(TimeSpan.FromSeconds(5));

        // Let's send some commands

        // #1 Fire and forget
        
        // Create a command:
        var command1 = new ReceiverCommand
        {
            KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeControl, UpDown.Up)
        };

        // Let's send the command and forget about it.
        await _receiverController.SendReceiverCommandAndForgetAsync(command1);

        await Task.Delay(TimeSpan.FromSeconds(3));

        // #2 Send and wait for reply

        // Create another command:
        var command2 = new ReceiverCommand
        {
            KeyValue = new KeyValuePair<CommandName, object>(CommandName.VolumeStatus, null)
        };

        // Send a command and listen for the receiver to respond. 
        var result2 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command2, TimeSpan.FromSeconds(2));
        Console.WriteLine(FormateNiceStringFromResponse(result2));


        // Wait here until the user presses the ctrl-C key - just an alternative to Console.ReadLine();
        WaitHandle.WaitOne();

    }


    private static void StartTcpListener()
    {
        var tcpClient = new TcpClient();

        _receiverController = new ReceiverController(_commandDefinitions, tcpClient, _ipAddress, _port);

        // Connect and listen to all messages from the receiver.
        var disposableReceiverController = _receiverController.ListenerObservable
            .Subscribe(
                res =>
                {
                    // What is received from the Receiver
                    Console.WriteLine(FormateNiceStringFromResponse(res));
                },
                ex =>
                {
                    // If something goes wrong
                    Console.WriteLine(ex);
                },
                () =>
                {
                    // If the connection completes
                    Console.WriteLine("Completed.");
                });

    }

    // Make the reponse nice for the Console
    private static string FormateNiceStringFromResponse(IReceiverResponse response)
    {
        return $"Command: {response.ResponseToCommand}, " +
                $"Value: {response.GetValueString()}, " +
                $"Timed Out: {response.WaitingForResponseTimedOut}, " +
                $"Time: {response.ResponseTime}";
    }
}
```

### Send One Command Without Listening
You don't have to listen to the receiver you can also just send a command in any of the two ways decribed above and then close the connection. 


```csharp

using (var tcpClient = new TcpClient())
using (var receiverController = new ReceiverController(_commandDefinitions, _tcpClient, _ipAddress, _port))
{
    await Task.Delay(TimeSpan.FromSeconds(5));

    var command1 = new ReceiverCommand
    {
        KeyValue = new KeyValuePair<CommandName, object>(CommandName.Zone2InputStatus, null)
    };

    var result1 = await _receiverController.SendReceiverCommandAndTryWaitForResponseAsync(command1, TimeSpan.FromSeconds(2));

    Console.WriteLine(FormateNiceStringFromResponse(result1));
}
```

**IMPORTANT** When not listening to the stream, only ONE command can be send at a time. This is also why the `Using` is used in the example above. It is NOT possible to send a second command inside the `Using` scope.

### Using a Serial Port
Using a serial port is as easy as replacing the second parameter in the `ReceiverController` in the constructor with a `SerialPort` instead of a `TcpClient` and adding only a third parameter which should be an `int` specifiying the buffer size. Suggested buffer size value is 256 (i.e. 256 bytes).

## Raw mode
This library offers the opportunity to listen to the raw data from the Pioneer Receiver. 

The library includes a couple of Extensions Methods on a `TcpClient` or a `SerialPort`:
- `ToByteStreamObservable` listes to the bytes
- `ToResponseObservable` transforms the byte stream to native commands.

### Example
```csharp
var tcpClient = new TcpClient();

_disposableResponse = tcpClient
    .ToByteStreamObservable(_ipAddress, _port)
    .ToResponseObservable()
    .Subscribe(
        res =>
        {
            Console.WriteLine(res.Data);
        },
        ex =>
        {
            Console.WriteLine(ex);
        },
        () =>
        {
            Console.WriteLine("Completed.");
        });

```

To create an IObservable<IRawResonseData> to be used in the advanced `ReceiverController` mention above all that is needed is:
```csharp
var tcpClient = new TcpClient();

var rawDataObservable = tcpClient.ToByteStreamObservable(_ipAddress, _port).ToResponseObservable();
```
Note: this works identical with a `SerialPort`
