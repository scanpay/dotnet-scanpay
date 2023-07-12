using System;

namespace NewURLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
            var data = new Scanpay.NewURLReq
            {
                orderid     = "999",
                successurl  = "https://example.com",
                items = new Scanpay.Item[]
                {
                    new Scanpay.Item
                    {
                        name     = "Ultra Bike 7000",
                        total    = "1337.01 DKK",
                        quantity = 2,
                        sku      = "ff123",
                    },
                    new Scanpay.Item
                    {
                      name      = "巨人宏偉的帽子",
                      total     = "420 DKK",
                      quantity  = 2,
                      sku       = "124",
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
                language    = "",
                autocapture = false,
                lifetime    = "1h",
            };
            /* The following opts is to use the test environment, omit it to use the production env. */
            var opts = new Scanpay.Options
            {
                hostname = "api.scanpay.dev",
            };
            var url = client.newURL(data, opts);
            Console.WriteLine("Payment URL is: " + url);
        }
    }
}
