using System;

namespace RenewSubscriberTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
            ulong subscriberid = 5;
            var data = new Scanpay.RenewReq
            {
                successurl = "https://docs.test.scanpay.dk/subscriptions/renew-subscriber",
                language   = "da",
                lifetime   = "1h",
            };
            /* The following opts is to use the test environment, omit it to use the production env. */
            var opts = new Scanpay.Options
            {
                hostname = "api.test.scanpay.dk",
            };
            var url = client.renew(subscriberid, data, opts);
            Console.WriteLine("Subscriber renew URL is: " + url);
        }
    }
}
