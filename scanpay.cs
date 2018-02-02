using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

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
            req.Headers["X-SDK"] = ".NET-0.1.2";
            if (opts != null && opts.headers != null)
            {
                foreach(KeyValuePair<string, string> hdr in opts.headers)
                {
                    req.Headers[hdr.Key] = hdr.Value;
                }
            }
            if (string.IsNullOrEmpty(req.Headers[HttpRequestHeader.Authorization])) {
                var auth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(apikey));
                req.Headers[HttpRequestHeader.Authorization] = auth;
            }

            req.AutomaticDecompression = DecompressionMethods.GZip;
            if (data != null)
            {
                req.Method = "POST";
                using (var sw = new StreamWriter(req.GetRequestStream()))
                using (JsonTextWriter jtw = new JsonTextWriter(sw))
                {
                    JsonSerializer.Create().Serialize(jtw, data);
                    jtw.Flush();
                }
            }

            using (var res = (HttpWebResponse)req.GetResponse())
            using (var stream = res.GetResponseStream())
            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                return JsonSerializer.Create().Deserialize<Tres>(jtr);
            }
        }

        public string newURL(NewURLReq data, Options opts=null)
        {
            var resobj = request<NewURLReq, NewURLRes>("/v1/new", data, opts);
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
            if (!constTimeEquals(digest, signature)) {
                throw new Exception("invalid ping signature");
            }
            using (var ms = new MemoryStream(body))
            using (var sr = new StreamReader(ms))
            using (var jtr = new JsonTextReader(sr)) {
                return JsonSerializer.Create().Deserialize<Ping>(jtr);
            }
        }

        public Ping handlePing(string body, string signature, Options opts=null)
        {
            return handlePing(Encoding.UTF8.GetBytes(body), signature);
        }
        internal class NullReq{}
    }

    public class NewURLReq
    {
        public string   orderid;
        public string   successurl;
        public Item[]   items;
        public Billing  billing;
        public Shipping shipping;
        public string   language;
        public bool     autocapture;
        public string   lifetime;
    }

    public class Item
    {
        public string name;
        public uint   quantity;
        public string total;
        public string sku;
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

    public class Change
    {
        public ulong  id;
        public uint   rev;
        public string orderid;
        public string error;
        public Time time;
        public Act[] acts;
        public Totals totals;
    }

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

    public class Totals
    {
        public string authorized;
        public string captured;
        public string refunded;
        public string left;
    }

    public class Ping
    {
        public ulong shopid;
        public ulong seq;
    }

    public class Options
    {
        public string                     hostname;
        public Dictionary<string, string> headers;
    }

}
