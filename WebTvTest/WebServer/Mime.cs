using Microsoft.Win32;

namespace WebTvTest
{
    public class Mime
    {
        public static bool IsImage(string path)
        {
            if (path.IndexOf(".") < 0) return false;
            var split = path.Split(new char[] { '.' });
            var ext = split[split.Length - 1];
            switch (ext.ToLower())
            {
                case "png":
                case "jpg":
                case "bmp":
                case "ico":
                    return true;
                default:
                    return false;
            }
        }
        public static string Get(string path, string mimeProposed)
        {
            if (path.IndexOf(".") < 0) return mimeProposed;
            var split = path.Split(new char[] { '.' });
            var ext = split[split.Length - 1];
            if (ext == "css") return "text/css";
            if (ext == "html") return "text/html";
            if (ext == "js") return "text/javascript";
            var key = Registry.ClassesRoot.OpenSubKey("." + ext);
            if (key == null)
                return mimeProposed;
            var mime = key.GetValue("Content Type");
            if (mime == null)
                return mimeProposed;
            else
                return (string)mime;
        }
    }
}
