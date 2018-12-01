using Corspro.Services.CorsProServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Corspro.Services
{
    public class Services
    {
        private readonly string _sdaCloudUrl;
        private readonly string _sdaCloudLogin;
        private readonly string _sdaCloudPwd;

        /// <summary>
        /// </summary>
        /// <param name="CorsProCloudUrl"></param>
        /// <param name="CorsProCloudLogin"></param>
        /// <param name="CorsProCloudPwd"></param>
        public Services(string CorsProCloudUrl, string CorsProCloudLogin, string CorsProCloudPwd)
        {
            _sdaCloudUrl = CorsProCloudUrl;
            _sdaCloudLogin = CorsProCloudLogin;
            _sdaCloudPwd = CorsProCloudPwd;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true; // IMPORTANT!!! execute this to avoid error connection because of the certificate in the server
        }

        /// <summary>
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public string GetClientIdFromName(string clientName)
        {
            string clientId = "";
            if (!String.IsNullOrEmpty(clientName))
            {
                try
                {
                    var uri = new Uri(_sdaCloudUrl);
                    var address = new EndpointAddress(uri);
                    var cloudService = new ServiceClient("WSHttpBinding_IService", address);
                    clientId = cloudService.GetCLientIdByName(clientName);
                }
                catch (Exception e)
                {
                    clientId = "Error " + e.Message;
                }
            }
            return clientId;
        }

    }
}
