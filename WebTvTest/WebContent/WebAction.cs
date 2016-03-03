using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebTvTest
{
    public class WebAction
    {
        public static void Action(HttpContext Context)
        {
            var b = Encoding.UTF8.GetBytes("OK");
            if (Context.Request.Url.StartsWith("/test/"))
            {
                var task = new RecTask.RecTask(1);
                //task.SendCommand("LoadBonDriver", new Dictionary<string, object>(){
                //    {"FilePath", "BonDriver_VTPT.dll"}
                //});
                //task.SendCommand("OpenTuner");
                b = Encoding.UTF8.GetBytes(JsonUtil.Serialize(task.GetTaskInfo()));
            }
            Context.Response.OutputStream.Write(b, 0, b.Length);
            Context.Response.Send();
            Context.Close();
        }
    }
}
