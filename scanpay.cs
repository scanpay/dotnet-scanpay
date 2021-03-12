using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/* Basic calls (Arguments wrapped quare brackets are optional):
 * client.newURL(NewURLReq reqdata, [Options opts]), returns paymenturl string
 * client.seq(ulong seq, [Options opts]), returns SeqRes
 * client.handlePing(byte[] body, string signature, [Options opts[]]), returns Ping object
 * client.capture(ulong trnid, CaptureReq  reqdata, [Options opts[]]), returns CaptureRes object
 *
 * Subscription calls:
 * client.generateIdempotencyKey(), returns idempotency-key string
 * client.charge(ulong subid, ChargeReq reqdata, [Options opts]), returns ChargeRes
 * client.renew(ulong subid, RenewReq reqdata, [Options opts]), returns paymenturl string
 *
 * See the tests/ folder for usage examples.
 */

namespace Scanpay
{
    public class Client
    {
        private const string HOSTNAME = "api.scanpay.dk";
        private readonly string apikey;

        public Client(string _apikey)
        {
            apikey = _apikey;
        }

        private Tres request<Tdata, Tres>(string url, Tdata data, Options opts)
        {
            var hostname = (opts == null || opts.hostname == null) ? HOSTNAME : opts.hostname;
            var req = (HttpWebRequest)WebRequest.Create("https://" + hostname + url);
            req.Method = "GET";
            req.ContentType = "application/json";
            req.Headers["X-SDK"] = ".NET-1.0.0";
            if (opts != null && opts.headers != null)
            {
                foreach(KeyValuePair<string, string> hdr in opts.headers)
                {
                    req.Headers[hdr.Key] = hdr.Value;
                }
            }
            if (string.IsNullOrEmpty(req.Headers[HttpRequestHeader.Authorization]))
            {
                var auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apikey));
                req.Headers[HttpRequestHeader.Authorization] = auth;
            }
            req.AutomaticDecompression = DecompressionMethods.GZip;
            if (data != null)
            {
                req.Method = "POST";
                using (var sw = new StreamWriter(req.GetRequestStream(), new UTF8Encoding(false)))
                using (JsonTextWriter jtw = new JsonTextWriter(sw))
                {
                    JsonSerializer.Create().Serialize(jtw, data);
                    jtw.Flush();
                }
            }

            try
            {
                using (var res = (HttpWebResponse)req.GetResponse()) {
                    if (!string.IsNullOrEmpty(req.Headers["X-Idempotency-Key"])) {
                        string err;
                        switch (res.Headers["Idempotency-Status"]) {
                            case "OK":
                                err = null;
                                break;
                            case "ERROR":
                                err = "Server failed to provide idempotency";
                                break;
                            case "":
                                err = "Idempotency status response header missing";
                                break;
                            default:
                                err = "Server returned unknown idempotency status " +
                                    res.Headers["Idempotency-Status"];
                                break;
                        }
                        if (err != null)
                        {
                            throw new Exception(err + ". Scanpay returned " + res.StatusCode +
                                " - " + res.StatusDescription);
                        }
                    }
                    using (var stream = res.GetResponseStream())
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        try
                        {
                            return JsonSerializer.Create().Deserialize<Tres>(jtr);
                        }
                        catch (JsonException je)
                        {
                            throw new IdempotentResponseException("JSON parsing exception: " + je.Message);
                        }
                    }
                }
            }
            catch (WebException we)
            {
                if (we.Response == null)
                {
                    throw new Exception("Network error getting Scanpay res: " + we.Message);
                }
                var statusMsg = ((HttpWebResponse)we.Response).StatusDescription;
                throw new IdempotentResponseException("Scanpay responded with error: '" + statusMsg + "'");
            }
        }

        public string newURL(NewURLReq reqdata, Options opts=null)
        {
            var resobj = request<NewURLReq, NewURLRes>("/v1/new", reqdata, opts);
            return resobj.url;
        }

        public SeqRes seq(ulong seqnum, Options opts=null)
        {
            return request<NullReq, SeqRes>("/v1/seq/" + seqnum, null, opts);
        }

        private bool constTimeEquals(string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++) {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        public Ping handlePing(byte[] body, string signature, Options opts=null)
        {
            var binApikey = Encoding.UTF8.GetBytes(apikey);
            var hasher = new System.Security.Cryptography.HMACSHA256(binApikey);
            var binDigest = hasher.ComputeHash(body);
            var digest = Convert.ToBase64String(binDigest);
            if (!constTimeEquals(digest, signature))
            {
                throw new Exception("invalid ping signature");
            }
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms))
            using (var jtr = new JsonTextReader(sr))
            {
                return JsonSerializer.Create().Deserialize<Ping>(jtr);
            }
        }

        public Ping handlePing(string body, string signature, Options opts=null)
        {
            return handlePing(Encoding.UTF8.GetBytes(body), signature);
        }

        public string generateIdempotencyKey()
        {
            byte[] random = new Byte[32];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(random);
            char[] trimChars = {'='};
            return Convert.ToBase64String(random).Trim(trimChars);
        }

        public CaptureRes capture(ulong trnid, CaptureReq reqdata, Options opts=null)
        {
            var url = String.Format("/v1/transactions/{0}/capture", trnid);
            var resobj = request<CaptureReq, CaptureRes>(url, reqdata, opts);
            return resobj;
        }

        public ChargeRes charge(ulong subid, ChargeReq reqdata, Options opts=null)
        {
            var url = String.Format("/v1/subscribers/{0}/charge", subid);
            var resobj = request<ChargeReq, ChargeRes>(url, reqdata, opts);
            return resobj;
        }

        public string renew(ulong subid, RenewReq reqdata, Options opts=null)
        {
            var url = string.Format("/v1/subscribers/{0}/renew", subid);
            var resobj = request<RenewReq, RenewRes>(url, reqdata, opts);
            return resobj.url;
        }

        internal class NullReq {}
    }

    public class IdempotentResponseException : Exception {
        public IdempotentResponseException(string msg) : base(msg) {}
    }

    /* Request/Response object definitions */

    public class NewURLReq
    {
        public string     orderid;
        public string     successurl;
        public Item[]     items;
        public Subscriber subscriber;
        public Billing    billing;
        public Shipping   shipping;
        public string     language;
        public bool       autocapture;
        public string     lifetime;
    }

    public class Item
    {
        public string name;
        public uint   quantity;
        public string total;
        public string sku;
    }

    public class Subscriber
    {
        public string @ref;
    }

    public class Billing
    {
        public string   name;
        public string   company;
        public string   vatin;
        public string   gln;
        public string   email;
        public string   phone;
        public string[] address;
        public string   city;
        public string   zip;
        public string   state;
        public string   country;
    }

    public class Shipping
    {
        public string   name;
        public string   company;
        public string   email;
        public string   phone;
        public string[] address;
        public string   city;
        public string   zip;
        public string   state;
        public string   country;
    }

    public class NewURLRes
    {
        public string url;
    }

    public class SeqRes
    {
        public ulong    seq;
        public Change[] changes;
    }

    [JsonConverter(typeof(ChangeConverter))]
    public abstract class Change
    {
        public string type;
        public ulong  id;
        public string rev;
        public string error;
        public Time   time;
        public Act[]  acts;
        public class Time
        {
            public long created;
            public long authorized;
        }
        public class Act
        {
            public string act;
            public long   time;
            public string total;
        }
    }

    public class TransactionChange : Change
    {
        public string orderid;
        public Totals totals;
        public class Totals
        {
            public string authorized;
            public string captured;
            public string refunded;
            public string left;
        }
    }

    public class ChargeChange : TransactionChange
    {
        public Subscriber subscriber;
        public class Subscriber
        {
            public ulong id;
            public string @ref;
        }
    }

    public class SubscriberChange : Change
    {
        public string @ref;
        public string orderid;
        public Subscriber subscriber;
    }
    public class UnknownChange : Change{}

    /* JSON handling of change */
    public class ChangeConverter : JsonConverter
    {
        public override bool CanConvert(Type t)
        {
            return (t == typeof(Change));
        }
        public override object ReadJson(JsonReader r, Type t, object existing, JsonSerializer ser)
        {
            if (t == typeof(Change))
            {
                JObject jo = JObject.Load(r);
                if (jo["type"].Value<string>() == "transaction")
                {
                    return jo.ToObject<TransactionChange>(ser);
                }
                if (jo["type"].Value<string>() == "charge")
                {
                    return jo.ToObject<ChargeChange>(ser);
                }
                if (jo["type"].Value<string>() == "subscriber")
                {
                    return jo.ToObject<SubscriberChange>(ser);
                }
                return jo.ToObject<UnknownChange>(ser);;
            } else
            {
                ser.ContractResolver.ResolveContract(t).Converter = null;
                return ser.Deserialize(r, t);
            }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter w, object v, JsonSerializer ser)
        {
            throw new NotImplementedException();
        }
    }

    public class Ping
    {
        public ulong shopid;
        public ulong seq;
    }

    public class CaptureReq
    {
        public string total;
        public uint index;
    }

    public class CaptureRes
    {
    }

    public class ChargeReq
    {
        public string   orderid;
        public Item[]   items;
        public Billing  billing;
        public Shipping shipping;
        public bool     autocapture;
    }

    public class ChargeRes
    {
        public ulong  id;
        public Totals totals;
        public class  Totals
        {
            public string authorized;
        }
    }

    public class RenewReq
    {
        public string     successurl;
        public Billing    billing;
        public Shipping   shipping;
        public string     language;
        public string     lifetime;
    }

    public class RenewRes
    {
        public string url;
    }

    public class Options
    {
        public string                     hostname;
        public Dictionary<string, string> headers;
    }
}
