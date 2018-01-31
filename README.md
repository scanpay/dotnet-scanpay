# Scanpay .NET library

## Documentation
See the [docs](https://docs.scanpay.dk/).

## Installation
From the command line:
```bash
$ nuget install scanpay
```

## Usage

Define a Scanpay client:
```csharp
var apikey = "1089:bx2a4DATi8ad87Nm4uaxg5nggYA8J/Hv99CON977YiEdvYa6DmMwdoRPoYWyBJSi";
var client = new Scanpay.Client(apikey);
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
            price    = "1337.01 DKK",
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
var seqRes = client.seq(oldSeq, opts);
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
Console.WriteLine("Ping seq=" + ping.seq + ", shopid=" + ping.shopid);;
```

See the tests/ folder for more advanced examples.