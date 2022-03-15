# Scanpay .NET client

The official .NET client library for the Scanpay API ([docs](https://docs.scanpay.dk)). You can always e-mail us at [help@scanpay.dk](mailto:help@scanpay.dk), or chat with us on IRC at libera.chat #scanpay

## Installation
The library is uploaded to [nuget](https://www.nuget.org/packages/scanpay/). You can install it in several ways:

#### Install from the Package Manager:
```bash
PM>  Install-Package scanpay
```

#### Install from .NET CLI
```bash
dotnet add package scanpay
```

#### Install from within Visual Studio:

1. Open the Solution Explorer.
2. Right-click on a project within your solution.
3. Click on *Manage NuGet Packages...*
4. Click on the *Browse* tab and search for "scanpay".
5. Click on the scanpay package, select the appropriate version in the right-tab and click *Install*.

## Usage

Define a Scanpay client:
```csharp
var apikey = "1089:bx2a4DATi8ad87Nm4uaxg5nggYA8J/Hv99CON977YiEdvYa6DmMwdoRPoYWyBJSi";
var client = new Scanpay.Client(apikey);
```

The Scanpay API requires TLS 1.2 support. If you do not use the latest .NET version, you may have to explicitly enable TLS 1.2 by adding the following to your main function:

```csharp
ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
```

If  `SecurityProtocolType.Tls12` is undefined in your .NET version, you can attempt the following:
```csharp
ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;;
```


### New Payment Link
Create a payment link to which you can redirect customers.
```csharp
var data = new Scanpay.NewURLReq
{
    orderid = "999",
    items = new Scanpay.Item[]
    {
        new Scanpay.Item
        {
            name     = "Ultra Bike 7000",
            total    = "1337.01 DKK",
            quantity = 2,
        },
    }
};
var url = client.newURL(data);
Console.WriteLine("Payment URL is " + url);
```

### Seq Request
Get an array with a number of changes since the supplied sequence number:
```csharp
var oldSeq = 3
var seqRes = client.seq(oldSeq);
foreach(var change in seqRes.changes)
{
    // Update your database with change...
}
Console.WriteLine("New seq number is " + seqRes.seq);
```

### Handle Pings
Verify the ping signature and extract the seq number.
```csharp
var ping = client.handlePing(body, request.Headers["X-Signature"]);
Console.WriteLine("Ping seq=" + ping.seq + ", shopid=" + ping.shopid);
```

See the tests/ folder for more advanced examples.
