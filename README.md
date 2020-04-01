# ObjectDeliverer-CSharp

## nuget
[https://www.nuget.org/packages/ObjectDeliverer/](https://www.nuget.org/packages/ObjectDeliverer/)

## Description
ObjectDeliverer is a data transmission / reception library for C#.

It is a sister library of the same name for UE4.

https://github.com/ayumax/ObjectDeliverer

It has the following features.

+ Communication protocol, data division rule, serialization method can be switched by part replacement.

## Communication protocol
The following protocols can be used with built-in.
You can also add your own protocol.
+ TCP/IP Server(Connectable to multiple clients)
+ TCP/IP Client
+ UDP(Sender)
+ UDP(Receiver)
+ Shared Memory
+ LogFile Writer
+ LogFile Reader

## Data division rule
The following rules are available for built-in split rules of transmitted and received data.
+ FixedSize  
	Example) In the case of fixed 1024 bytes
	![fixedlength](https://user-images.githubusercontent.com/8191970/56475737-7d999f00-64c7-11e9-8e9e-0182f1af8156.png)


+ Header(BodySize) + Body  
	Example) When the size area is 4 bytes  
	![sizeandbody](https://user-images.githubusercontent.com/8191970/56475796-6e672100-64c8-11e9-8cf0-6524f2899be0.png)


+ Split by terminal symbol  
	Example) When 0x00 is the end
	![terminate](https://user-images.githubusercontent.com/8191970/56475740-82f6e980-64c7-11e9-91a6-05d77cfdbd60.png)

## Serialization method
+ Byte Array
+ UTF-8 string
+ Object(Json)


# Quick Start

```cs
// Create an ObjectDelivererManager
var deliverer = new ObjectDelivererManager();

// Watching for connection events
deliverer.Connected.Subscribe(async x =>
{
    Console.WriteLine("connected");

    // Sending data to a connected party
    await deliverer.SendAsync(new byte[] { 0x00, 0x12 });
    await deliverer.SendAsync(new byte[] { 0x00, 0x12, 0x23 });
});

// Watching for incoming events
deliverer.ReceiveData.Subscribe(x => Console.WriteLine($"received length = {x.Buffer.Length}"));

// Start the ObjectDelivererManager
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });
```
