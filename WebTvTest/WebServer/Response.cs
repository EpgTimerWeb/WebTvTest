using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace WebTvTest
{
    public class HttpResponse
    {
        public HttpResponse(HttpContext c)
        {
            StatusCode = 200;
            StatusText = "OK";
            OutputStream = new MemoryStream();
            Headers = new HttpHeaderArray()
            {
                {"Server", "EpgTimerWeb2/1.0"},
                {"Date", DateTime.Now.ToString("R")},
                {"Content-Language", "ja"},
                {"Connection", "close"}
            };
            context = c;
            UseGZip = false;
        }
        public void Send()
        {
            OutputStream.Flush();
            HttpResponse.SendResponse(context);
        }
        public MemoryStream OutputStream { get; set; }
        public HttpHeaderArray Headers { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        private HttpContext context;
        public bool UseGZip { set; get; }
        public void SetStatus(int Code, string Text)
        {
            StatusCode = Code;
            StatusText = Text;
        }
        public static void StatusPage(HttpContext Context, string Reason)
        {
            byte[] Page = Encoding.UTF8.GetBytes(string.Format(@"<html>
<body>
<h1>{0} - {1}</h1>
{2}
<hr />
EpgTimerWeb(v2) by YUKI
</body>
</html>", Context.Response.StatusCode, Context.Response.StatusText, Reason));
            Context.Response.OutputStream.Write(Page, 0, Page.Length);
        }
        public static bool SendResponseHeader(HttpContext Context, HttpHeaderArray Input)
        {
            var HeaderText = Encoding.UTF8.GetBytes(HttpHeaderArray.Generate(Input) + "\r\n");
            return SendResponseBody(Context, HeaderText);
        }
        public static bool SendResponseBody(HttpContext Context, Stream Input)
        {
            try
            {
                if (!Context.Client.Connected) return false;
                lock (Context.LockObject)
                {
                    Input.CopyTo(Context.HttpStream);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Response Error: {0}", e.Message);
                return false;
            }
        }
        public static bool SendResponseBody(HttpContext Context, byte[] Input)
        {
            try
            {
                if (!Context.Client.Connected) return false;
                SendResponseBody(Context, new MemoryStream(Input));
                return true;
            }
            catch (Exception e)
            {
                Debug.Print("Response Error: {0}", e.Message);
                return false;
            }
        }
        public static bool SendResponseCode(HttpContext Context)
        {
            var ResHeader = Encoding.UTF8.GetBytes(
                String.Format("HTTP/1.1 {0} {1}\r\n",
                Context.Response.StatusCode, Context.Response.StatusText));
            return SendResponseBody(Context, ResHeader);
        }
        public static bool SendResponse(HttpContext Context)
        {
            Context.Response.OutputStream.Seek(0, SeekOrigin.Begin);
            if (Context.Response.UseGZip)
            {
                Debug.Print("GZip Source {0}", Context.Response.OutputStream.Length);
                Context.Response.Headers["Content-Encoding"] = "gzip";
                MemoryStream buffer = new MemoryStream();
                GZipStream gzip = new GZipStream(buffer, CompressionMode.Compress, true);
                Context.Response.OutputStream.CopyTo(gzip);
                gzip.Flush();
                gzip.Close();
                Context.Response.OutputStream.Close();
                Context.Response.OutputStream = buffer;
                Context.Response.OutputStream.Seek(0, SeekOrigin.Begin);
                Debug.Print("GZip Dest {0}", Context.Response.OutputStream.Length);
                Context.Response.Headers["Content-Length"] = Context.Response.OutputStream.Length.ToString();
            }
            else if (!Context.Response.Headers.ContainsKey("Content-Length"))
            {
                Context.Response.Headers["Content-Length"] = Context.Response.OutputStream.Length.ToString();
            }
            SendResponseCode(Context);
            SendResponseHeader(Context, Context.Response.Headers);
            return SendResponseBody(Context, Context.Response.OutputStream);
        }
    }
}
