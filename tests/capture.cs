using System;
using System.Collections.Generic;

namespace ChargeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
            ulong trnid = 522;
            var data = new Scanpay.CaptureReq
            {
                total = "1 DKK",
                index = 0,
            };
            /* The following opts is to use the test environment, omit it to use the production env. */
            var opts = new Scanpay.Options
            {
                hostname = "api.scanpay.dev",
            };
            try
            {
                client.capture(trnid, data, opts);
            }
            catch (Exception e)
            {
                Console.WriteLine("Capture failed: " + e.Message);
                return;
            }
            Console.WriteLine("Capture of {0} succeded", data.total);
        }
    }
}
