using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Olive;
using vm = ViewModel;

namespace ViewModel
{
    partial class SingleSignOn
    {
        public string Ticket = Guid.NewGuid().ToString();
        public string Errors;
    }
}

namespace Controllers
{
    partial class SSOController
    {
        string CreateToken(string ticket)
        {
            var token = new
            {
                Ticket = ticket,
                Email = User.GetEmail(),
                Secret = Config.GetOrThrow("SingleSignOn:Secret")
            };

            var tokenBytes = JsonConvert.SerializeObject(token).ToBytes(Encoding.ASCII);
            var key = Config.Get("SingleSignOn:EncryptionKey");

            return Olive.Security.Encryption.Encrypt(tokenBytes, key)
                .ToBase64String();
        }

        public async Task PrepareApps(vm.SingleSignOn info)
        {
            foreach (var item in info.Items)
            {
                try
                {
                    await new ApiClient(item.ServiceUrl + "?awaitToken=" + info.Token.UrlEncode()).Get<string>();
                }
                catch
                {
                    // No logging is needed
                    info.Errors += "<div style='background:#eba; margin:20px; width:100px; height:100px;'>" +
                        item.Item + " SSO Failed</div>";
                }
            }
        }
    }
}