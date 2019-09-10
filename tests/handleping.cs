using System;

namespace NewURLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
            var body = "{\"seq\": 1,\"shopid\": 1153}";
            var signature = "QvZglGHM3j20DxBE6CKKUdIA6x5pdT/GnY+9NgvWTv8=";

            var pingobj = client.handlePing(body, signature);
            Console.WriteLine("Ping seq = " + pingobj.seq);
            Console.WriteLine("Ping shopid = " + pingobj.shopid);
        }
    }
}
