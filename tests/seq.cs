using System;

namespace NewURLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1089:bx2a4DATi8ad87Nm4uaxg5nggYA8J/Hv99CON977YiEdvYa6DmMwdoRPoYWyBJSi");
            var opts = new Scanpay.Options{ hostname = "api.test.scanpay.dk" };
            var seqRes = client.seq(0, opts);

            Console.WriteLine("Printing seq res: (" + seqRes.seq + ")");
            Console.WriteLine("Changes: (" + seqRes.changes.Length + ")");

            foreach(var change in seqRes.changes)
            {
                Console.WriteLine("Change (transaction id=" + change.id + ")");
                if (change.error != null)
                {
                    Console.WriteLine("  error = " + change.error);
                    continue;
                }
                Console.WriteLine("  rev        = " + change.rev);
                Console.WriteLine("  orderid    = " + change.orderid);
                Console.WriteLine("  payid time = " + change.time.created);
                Console.WriteLine("  auth time  = " + change.time.authorized);
                Console.WriteLine("  authorized = " + change.totals.authorized);
                Console.WriteLine("  captured   = " + change.totals.captured);
                Console.WriteLine("  refunded   = " + change.totals.refunded);
                Console.WriteLine("  left capt. = " + change.totals.left);
                Console.WriteLine("  acts(" + change.acts.Length + ")");
                var nact = 0;
                foreach(var act in change.acts)
                {
                    Console.WriteLine("    " + (nact++) + ":");
                    Console.WriteLine("    name = " + act.act);
                    Console.WriteLine("    time = " + act.time);
                    Console.WriteLine("    total= " + act.total);
                }
            }
            Console.WriteLine("New seq after applying all changes: seq = " + seqRes.seq);
        }
    }
}