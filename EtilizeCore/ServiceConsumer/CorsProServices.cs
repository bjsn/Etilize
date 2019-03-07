using Corspro.Services.CorsProServiceReference;
using System;
using System.Collections.Generic;
using System.ServiceModel;


namespace Etilize.Services
{
    public class CorsProServices
    {
        private readonly string _sdaCloudUrl;

        public CorsProServices(string sdaCloudUrl)
        {
            this._sdaCloudUrl = sdaCloudUrl;
            // IMPORTANT!!! execute this to avoid error connection because of the certificate in the server
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true; 
        }

        public List<string[]> GetLastEtilizeRetrievedKeys()
        {
            List<string[]> list = new List<string[]>();
            try
            {
                EndpointAddress remoteAddress = new EndpointAddress(new Uri(this._sdaCloudUrl));
                list.AddRange(new ServiceClient("WSHttpBinding_IService", remoteAddress).GetLastRetrievedKeys());
            }
            catch (Exception exception)
            {
                string[] item = new string[] { "Error " + exception.Message };
                list.Add(item);
            }
            return list;
        }


        public void SendErroLogMessageToCloud(int userId, int clientId, string errorMessage)
        {
          
            try
            {
                EndpointAddress remoteAddress = new EndpointAddress(new Uri(this._sdaCloudUrl));
                new ServiceClient("WSHttpBinding_IService", remoteAddress).AddErrorLogMessage(userId, clientId, errorMessage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }


    }
}

