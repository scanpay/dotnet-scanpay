using System;
using System.Collections.Generic;

namespace ChargeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
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
                    new Scanpay.Item
                    {
                      name      = "巨人宏偉的帽子",
                      total     = "420 DKK",
                      quantity  = 2,
                      sku       = "124",
                    },
                },
                autocapture = true,
            };
            /* The following opts is to use the test environment, omit it to use the production env. */
            var opts = new Scanpay.Options
            {
                hostname = "api.test.scanpay.dk",
                headers = new Dictionary<string, string>
                {
                    ["Idempotency-Key"] = client.generateIdempotencyKey(),
                },
            };
            Scanpay.ChargeRes res = null;
            int i;
            for (i = 0; i < 3; i++)
            {
                Console.WriteLine("Attempting charge with idempotency key " + opts.headers["Idempotency-Key"]);
                try
                {
                    res = client.charge(subscriberid, data, opts);
                    break;
                }
                catch (Scanpay.IdempotentResponseException e)
                {
                    /* Regenerate idempotency key */
                    opts.headers["Idempotency-Key"] = client.generateIdempotencyKey();
                    Console.WriteLine("Idempotent exception: " + e.Message);
                    if (i < 2) { Console.WriteLine("Regenerating idempotency key"); }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception (not idempotent): " + e.Message);
                }
                System.Threading.Thread.Sleep(50);
            }
            if (i == 3)
            {
                throw new Exception("Attempted charging 3 times and failed");
            }
            Console.WriteLine("Charge succeded:");
            Console.WriteLine("id = {0}", res.id);
            Console.WriteLine("authorized = {0}", res.totals.authorized);
        }
    }
}
