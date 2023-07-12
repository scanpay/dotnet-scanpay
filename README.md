# Scanpay .NET client

The official .NET client library for the Scanpay API ([docs](https://docs.scanpay.dev)). You can always e-mail us at [support@scanpay.dk](mailto:support@scanpay.dk), or chat with us on our IRC server: irc.scanpay.dev:6697 ([webchat](https://chat.scanpay.dev)).

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

### Payment Link

#### string newURL(NewURLReq reqdata, Options opts=null)
Create a payment link to which you can redirect customers.
```csharp
var client = new Scanpay.Client(" APIKEY ");
var data = new Scanpay.NewURLReq
{
    orderid = "999",
    language    = "",
    autocapture = false,
    lifetime    = "1h",
    items = new Scanpay.Item[]
    {
        new Scanpay.Item
        {
            name     = "Ultra Bike 7000",
            total    = "1337.01 DKK",
            quantity = 2,
        },
    },
    billing = new Scanpay.Billing
    {
        name    = "Hans Jensen",
        company = "HJ Planteskole ApS",
        vatin   = "DK12345678",
        gln     = "",
        email   = "hans@hjplanter.dk",
        phone   = "+45 12345678",
        address = new string[]
        {
            "Grønnegade 5, st. th",
            "C/O Hans Jensen",
        },
        city    = "Børum",
        zip     = "1234",
        country = "DK",
    },
    shipping = new Scanpay.Shipping
    {
        name    = "John Hanson",
        company = "HJ Planteskole ApS",
        email   = "john@hjplanter.dk",
        phone   = "+45 12345679",
        address = new string[]
        {
            "Gryngade 90",
            "C/O John Hanson",
        },
        city    = "Ørum",
        zip     = "1235",
        country = "DK",
    },
};
var url = client.newURL(data);
Console.WriteLine("Payment URL is " + url);
```

### Synchronization
To know when transactions, charges, subscribers and subscriber renewal succeeds, you need to use the synchronization API. It consists of pings which notify you of changes, and the seq request which allows you to pull changes.

#### Ping handlePing(byte[] body, string signature, Options opts=null)
When changes happen, a **ping** request will be sent to the **ping URL** specified in the Scanpay dashboard.
Use HandlePing to parse the ping request:
```csharp
var client = new Scanpay.Client(" APIKEY ");
var ping = client.handlePing(body, request.Headers["X-Signature"]);
Console.WriteLine("Ping seq=" + ping.seq + ", shopid=" + ping.shopid);
```

#### SeqRes seq(ulong seqnum, Options opts=null)
To pull changes since last update, use the Seq() call after receiving a ping.
Store the returned seq-value in a database and use it for the next Seq() call.
```csharp
var client = new Scanpay.Client(" APIKEY ");
var pingSeq = (ulong)100;
var mySeq = (ulong)3;
while (mySeq < pingSeq) {
    Scanpay.SeqRes seqRes = null;
    try {
        seqRes = client.seq(mySeq);
    }
    catch (Exception e)
    {
        Console.WriteLine("Seq exception:" + e.ToString());
        break;
    }
    foreach(var change in seqRes.changes)
    {
        // Update your database with change...
    }
    mySeq = seqRes.seq;
    if (seqRes.changes.Length == 0) {
        break;
    }
}
Console.WriteLine("New seq is " + mySeq);
```

### Transaction Actions

#### CaptureRes capture(ulong trnid, CaptureReq reqdata, Options opts=null)
Use Capture to capture a transaction.
```csharp
var client = new Scanpay.Client(" APIKEY ");
ulong transactionId = 522;
var data = new Scanpay.CaptureReq
{
    total = "1 DKK",
    index = 0,
};
client.capture(transactionId, data);
```

### Subscriptions
Create a subscriber by using NewURL with a Subscriber parameter.
```csharp
var client = new Scanpay.Client(" APIKEY ");
var data = new Scanpay.NewURLReq
{
    successurl  = "https://example.com",
    subscriber = new Scanpay.Subscriber
    {
        @ref = "sub1234",
    },
}
var url = client.newURL(data, opts);
Console.WriteLine("Subscription URL is: " + url);
```
#### ChargeRes charge(ulong subid, ChargeReq reqdata, Options opts=null)
Use Charge to charge a subscriber. The subscriber id is obtained with seq.
```csharp
var client = new Scanpay.Client(" APIKEY ");
ulong subscriberid = 5;
var data = new Scanpay.ChargeReq
{
    orderid     = "999",
    items = new Scanpay.Item[]
    {
        new Scanpay.Item
        {
            name     = "Ultra Bike 7000",
            total    = "1337.01 DKK",
            quantity = 2,
            sku      = "ff123",
        },
    },
};
res = client.charge(subscriberid, data);
Console.WriteLine("Charge succeded:");
Console.WriteLine("id = {0}", res.id);
Console.WriteLine("authorized = {0}", res.totals.authorized);
```
#### string renew(ulong subid, RenewReq reqdata, Options opts=null)
Use Renew to renew a subscriber, i.e. to attach a new card, if it has expired.
```csharp
var client = new Scanpay.Client(" APIKEY ");
ulong subscriberid = 5;
var data = new Scanpay.RenewReq
{
    successurl = "https://docs.scanpay.dev/subscriptions/renew-subscriber",
    language   = "da",
    lifetime   = "1h",
};
var url = client.renew(subscriberid, data);
Console.WriteLine("Subscriber renew URL is: " + url);
```
## Testing
See the tests/ folder for more examples against the test environment.

If you want an account on the [test](tests/). environment, please do not hesitate to contact us at kontakt@scanpay.dk
