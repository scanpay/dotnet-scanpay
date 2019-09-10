using System;

namespace NewURLTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Scanpay.Client("1153:YHZIUGQw6NkCIYa3mG6CWcgShnl13xuI7ODFUYuMy0j790Q6ThwBEjxfWFXwJZ0W");
            var opts = new Scanpay.Options{ hostname = "api.test.scanpay.dk" };
            var seqRes = client.seq(360, opts);

            Console.WriteLine("Printing seq res: (" + seqRes.seq + ")");
            Console.WriteLine("Changes: (" + seqRes.changes.Length + ")");

            foreach(var change in seqRes.changes)
            {
                if (change.error != null)
                {
                    Console.WriteLine("  error = " + change.error);
                    continue;
                }
                if (change is Scanpay.TransactionChange)
                {
                    var trn = (Scanpay.TransactionChange)change;
                    Console.WriteLine("Change (transaction id=" + trn.id + ")");
                    Console.WriteLine("  rev        = " + trn.rev);
                    Console.WriteLine("  orderid    = " + trn.orderid);
                    if (trn is Scanpay.ChargeChange) {
                        var charge = (Scanpay.ChargeChange)trn;
                        Console.WriteLine("  subscriber id = " + charge.subscriber.id);
                        Console.WriteLine("  subscriber ref = " + charge.subscriber.@ref);
                    }
                    Console.WriteLine("  payid time = " + trn.time.created);
                    Console.WriteLine("  auth time  = " + trn.time.authorized);
                    Console.WriteLine("  authorized = " + trn.totals.authorized);
                    Console.WriteLine("  captured   = " + trn.totals.captured);
                    Console.WriteLine("  refunded   = " + trn.totals.refunded);
                    Console.WriteLine("  left capt. = " + trn.totals.left);
                    Console.WriteLine("  acts(" + trn.acts.Length + ")");
                    var nact = 0;
                    foreach(var act in trn.acts)
                    {
                        Console.WriteLine("    " + (nact++) + ":");
                        Console.WriteLine("    name = " + act.act);
                        Console.WriteLine("    time = " + act.time);
                        Console.WriteLine("    total= " + act.total);
                    }
                }
                else if (change is Scanpay.SubscriberChange)
                {
                    var sub = (Scanpay.SubscriberChange)change;
                    Console.WriteLine("Change (subscriber id=" + sub.id + ")");
                    Console.WriteLine("  rev        = " + sub.rev);
                    Console.WriteLine("  ref    = "     + sub.@ref);
                    Console.WriteLine("  orderid    = " + sub.orderid);
                    Console.WriteLine("  payid time = " + sub.time.created);
                    Console.WriteLine("  auth time  = " + sub.time.authorized);
                    Console.WriteLine("  acts(" + sub.acts.Length + ")");
                    var nact = 0;
                    foreach(var act in sub.acts)
                    {
                        Console.WriteLine("    " + (nact++) + ":");
                        Console.WriteLine("    name = " + act.act);
                        Console.WriteLine("    time = " + act.time);
                        Console.WriteLine("    total= " + act.total);
                    }
                }
                else
                {
                    Console.WriteLine("unknown change type");
                }
            }
            Console.WriteLine("New seq after applying all changes: seq = " + seqRes.seq);
        }
    }
}