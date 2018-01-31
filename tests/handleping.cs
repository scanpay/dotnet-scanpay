using System;

namespace NewURLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1089:bx2a4DATi8ad87Nm4uaxg5nggYA8J/Hv99CON977YiEdvYa6DmMwdoRPoYWyBJSi");
            var body = "{\"seq\": 1,\"shopid\": 1089}";
            var signature = "ANtDa0NzHihPDDgl+3zlO6u1zO3mSwQJSogIIahzpAY=";

            var pingobj = client.handlePing(body, signature);
            Console.WriteLine("Ping seq = " + pingobj.seq);
            Console.WriteLine("Ping shopid = " + pingobj.shopid);
        }
    }
}
