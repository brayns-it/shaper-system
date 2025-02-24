using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Principal;

namespace Brayns.System
{
    public class CredentialMgmt : Codeunit
    {
        public Credential Credential { get; } = new Credential();

        public CredentialMgmt(string code)
        {
            if (!Credential.Get(code))
                throw Credential.ErrorNotFound();
        }

        public void RunAs(Action action)
        {
            switch (Credential.Type.Value)
            {
                case CredentialType.WINDOWS:
                    RunAsWindows(action);
                    break;

                default:
                    throw new Error(Label("Unsupported credential type"));
            }
        }

        private void RunAsWindows(Action action)
        {
            if (!OperatingSystem.IsWindows())
                throw new Error(Label("Unsupported operating system"));

            string domain = ".";
            if (Credential.Domain.Value.Length > 0)
                domain = Credential.Domain.Value;

            var uc = new SimpleImpersonation.UserCredentials(domain, Credential.Username.Value, Functions.DecryptString(Credential.Password.Value));
            using (var handle = uc.LogonUser(SimpleImpersonation.LogonType.NewCredentials))
            {
                WindowsIdentity.RunImpersonated(handle, action);
            }
        }

        public void Test(string command)
        {
            if (command.StartsWith("\\\\"))
            {
                RunAs(() =>
                {
                    DirectoryInfo di = new DirectoryInfo(command);
                    di.GetFiles("DUMMY");
                });
            }
            else
                throw new Error(Label("Unsupported command"));

            Message.Show(Label("Command executed successfully"));
        }
    }
}
