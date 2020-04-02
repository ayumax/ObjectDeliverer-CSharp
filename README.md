# ObjectDeliverer-CSharp

## NuGet packages
[https://www.nuget.org/packages/ObjectDeliverer/](https://www.nuget.org/packages/ObjectDeliverer/)

To install with NuGet, just install the ObjectDeliverer package:

```
Install-Package ObjectDeliverer
```

## Description
ObjectDeliverer is a data transmission / reception library for C#.

It is a sister library of the same name for UE4.

https://github.com/ayumax/ObjectDeliverer

It has the following features.

+ Communication protocol, data division rule, serialization method can be switched by part replacement.
+ It is also possible to apply your own object serialization method

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
Create ObjectDelivererManager and create various communication paths by passing "Communication Protocol", "Packet Split Rule" and "Serialization Method" to the arguments of StartAsync method.

```cs
// Create an ObjectDelivererManager
var deliverer = new ObjectDelivererManager<string>();

// Watching for connection events
deliverer.Connected.Subscribe(async x =>
{
    Console.WriteLine("connected");

    // Sending data to a connected party
    await deliverer.SendAsync(new byte[] { 0x00, 0x12 });
    await deliverer.SendAsync(new byte[] { 0x00, 0x12, 0x23 });
});

// Watching for disconnection events
deliverer.Disconnected.Subscribe(x => Console.WriteLine("disconnected"));
	    
// Watching for incoming events
deliverer.ReceiveData.Subscribe(x => 
{
    Console.WriteLine($"received buffer length = {x.Buffer.Length}");
    Console.WriteLine($"received message = {x.Message}");
});

// Start the ObjectDelivererManager
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 },
    new Utf8StringDeliveryBox());

```

# Change communication protocol
You can switch to various communication protocols by changing the Protocol passed to the StartAsync method.

```cs
// TCP/IP Client
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// TCP/IP Server
await deliverer.StartAsync(
    new ProtocolTcpIpServer() { ListenPort = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// UDP Sender
await deliverer.StartAsync(
    new ProtocolUdpSocketSender() { DestinationIpAddress = "127.0.0.1", DestinationPort = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// UDP Receiver
await deliverer.StartAsync(
    new ProtocolUdpSocketReceiver() { BoundPort = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// SharedMemory
await deliverer.StartAsync(
    new ProtocolSharedMemory() { SharedMemoryName = "SharedMemory", SharedMemorySize = 1024 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// Log File Writer
await deliverer.StartAsync(
    new ProtocolLogWriter() { FilePath = @"C:\log\comlog.txt" },
    new PacketRuleFixedLength() { FixedSize = 10 });

// Log File Reader
await deliverer.StartAsync(
    new ProtocolLogReader() { FilePath = @"C:\log\comlog.txt" },
    new PacketRuleFixedLength() { FixedSize = 10 });

```

# Change of data division rule
You can easily change the packet splitting rule.

```cs
// FixedSize
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 });

// Header(BodySize) + Body
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleSizeBody() { SizeLength = 4, SizeBufferEndian = PacketRuleBase.ECNBufferEndian.Big });

// Split by terminal symbol
await deliverer.StartAsync(
        new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
        new PacketRuleTerminate() { Terminate = new byte[] { 0xFE, 0xFF } });

// Nodivision
await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleNodivision());

```

# Change of Serialization method
Using DeliveryBox enables sending and receiving of non-binary data (character strings and objects).

```cs
// UTF-8 string
var deliverer = new ObjectDelivererManager<string>();

await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleFixedLength() { FixedSize = 10 },
    new Utf8StringDeliveryBox());

deliverer.ReceiveData.Subscribe(x => Console.WriteLine(x.Message));

await deliverer.SendMessageAsync("ABCDEFG");

```

```cs
// Object
public class SampleObj
{
    public int Prop { get; set; }
    public string StringProp { get; set; }

    public string Hoge() => $"{Prop}_{StringProp}";
}

var deliverer = new ObjectDelivererManager<SampleObj>();

await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleSizeBody() { SizeLength = 4, SizeBufferEndian = PacketRuleBase.ECNBufferEndian.Big },
    new ObjectDeliveryBoxUsingJson<SampleObj>());

deliverer.ReceiveData.Subscribe(x => Console.WriteLine(x.Message.Hoge()));

var sampleObj = new SampleObj() { Prop = 1, StringProp = "abc" };
await deliverer.SendMessageAsync(sampleObj);

```

# Creating a custom DeliveryBox
You can create a DeliveryBox with your own serialization method.

Ex) When applying a serializer using MessagePack(https://github.com/neuecc/MessagePack-CSharp)

```cs
[MessagePackObject]
public class SampleObj2
{
    [Key(0)]
    public int Prop { get; set; }

    [Key(1)]
    public string StringProp { get; set; }

    public string Hoge() => $"{Prop}_{StringProp}";
}

// Definition of DeliveryBox using MessagePack
public class ObjectDeliveryBoxUsingMessagePack<T> : DeliveryBoxBase<T>
{
    public override ReadOnlyMemory<byte> MakeSendBuffer(T message) => MessagePackSerializer.Serialize(message);

    public override T BufferToMessage(ReadOnlyMemory<byte> buffer) => MessagePackSerializer.Deserialize<T>(buffer);
}

var deliverer = new ObjectDelivererManager<SampleObj2>();

await deliverer.StartAsync(
    new ProtocolTcpIpClient() { IpAddress = "127.0.0.1", Port = 9013 },
    new PacketRuleSizeBody() { SizeLength = 4, SizeBufferEndian = PacketRuleBase.ECNBufferEndian.Big },
    new ObjectDeliveryBoxUsingMessagePack<SampleObj2>());

deliverer.ReceiveData.Subscribe(x => Console.WriteLine(x.Message.Hoge()));

var sampleObj = new SampleObj2() { Prop = 1, StringProp = "abc" };
await deliverer.SendMessageAsync(sampleObj);

```


