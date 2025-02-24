using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Brayns.System
{
    public class PrintReport
    {
        public string DefinitionPath { get; set; } = "";
        public Dictionary<string, object> Datasets { get; } = new();
        public string Format { get; set; } = "";

        public string GetDefinition()
        {
            if (DefinitionPath.Length > 0)
            {
                using (FileStream fs = new FileStream(DefinitionPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buf = new byte[fs.Length];
                    fs.Read(buf, 0, buf.Length);
                    return Convert.ToBase64String(buf);
                }
            }

            return "";
        }

        public void GetDatasets(JObject jRequest)
        {
            JObject result = new();

            foreach (var ds in Datasets.Keys)
            {
                if (Datasets[ds].GetType() == typeof(DataTable))
                {
                    DataSet mds = new();
                    mds.Tables.Add((DataTable)Datasets[ds]);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        mds.WriteXml(ms);
                        result.Add(ds, Convert.ToBase64String(ms.ToArray()));
                    }
                }
            }

            jRequest["datasets"] = result;
        }
    }

    public class PrintMgmt : Codeunit
    {
        public PrintSetup Setup { get; } = new PrintSetup();

        public PrintMgmt()
        {
            Setup.Reset();
            Setup.Default.SetRange(true);
            if (!Setup.FindFirst())
                throw Setup.ErrorNotFound();
        }

        public PrintMgmt(string profile)
        {
            if (!Setup.Get(profile))
                throw Setup.ErrorNotFound();
        }

        public void RenderReport(PrintReport report, string destination)
        {
            byte[] buf = RenderReport(report);
            using (FileStream fs = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
            {
                fs.Write(buf);
            }
        }

        public byte[] RenderReport(PrintReport report)
        {
            JObject jRequest = new();
            jRequest["request"] = "render";
            jRequest["format"] = "RDL";
            jRequest["report"] = report.GetDefinition();
            report.GetDatasets(jRequest);

            return Execute(jRequest);
        }

        private byte[] Execute(JObject jRequest)
        {
            string uri = Setup.PrintServer.Value;
            if (!uri.EndsWith("/")) uri += "/";
            uri += "RPC.aspx";

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
            {
                return true;
            };

            var client = new HttpClient(handler);
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(uri);

            jRequest["token"] = Functions.DecryptString(Setup.AuthToken.Value);

            var reqContent = new StringContent(jRequest.ToString(), Encoding.UTF8, "application/json");
            request.Content = reqContent;

            var response = client.Send(request);

            StreamReader sr = new StreamReader(response.Content.ReadAsStream());
            var jRes = JObject.Parse(sr.ReadToEnd());
            sr.Close();

            if (jRes["status"]!.ToString() != "success")
                throw new Exception(jRes["message"]!.ToString());

            if (jRes.ContainsKey("result"))
                return Convert.FromBase64String(jRes["result"]!.ToString());
            else
                return new byte[0];
        }

        public void Test()
        {
            JObject jRequest = new();
            jRequest["request"] = "test";
            Execute(jRequest);
            Message.Show(Label("Token is valid"));
        }
    }
}
