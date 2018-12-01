using Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.Services
{
    public class Services
    {

        public delegate void UpdateProgressDelegate(int ProgressPercentage);
        public event UpdateProgressDelegate UpdateProgress;
        public delegate void UpdateProgressTextDelegate(string UpdateProgressText);
        public event UpdateProgressTextDelegate UpdateProgressText;

        private readonly string EtilizeEndPoint = (ConfigurationManager.AppSettings["EtilizeEndPoint"].ToString(CultureInfo.InvariantCulture) + "/service/rest/catalog?appId={0}&siteId={1}");
        private string EtilizeAPPId;
        private string EtilizeSiteID;

        public bool CheckURL()
        {
            string str = string.Format(this.EtilizeEndPoint, this.EtilizeAPPId, this.EtilizeSiteID);
            WebRequest request = WebRequest.Create(str.Substring(0, str.IndexOf(".net") + 4));
            request.Timeout = 0x3e8;
            try
            {
                WebResponse response = request.GetResponse();
                response.Close();
                response.Dispose();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void DownloadImageContentFromParts(List<ProposalContentByPart> downloadedList, string downloadPath)
        {
            int counter = 0;
            int partsWithDownload = (from x in downloadedList
                                     where !string.IsNullOrEmpty(x.ProductPictureURL)
                                     select x).Count<ProposalContentByPart>();
            HttpClient httpClient = new HttpClient();
            downloadedList.AsParallel<ProposalContentByPart>().ForAll<ProposalContentByPart>(delegate(ProposalContentByPart part)
            {
                if (!string.IsNullOrEmpty(part.ProductPictureURL))
                {
                    counter++;
                    Task<HttpResponseMessage> async = httpClient.GetAsync(part.ProductPictureURL);
                    string str = Utilitary.CleanFileName(part.ProductPictureURL.Substring(part.ProductPictureURL.LastIndexOf("/") + 1, (part.ProductPictureURL.Length - part.ProductPictureURL.LastIndexOf("/")) - 1));
                    string path = downloadPath + str;
                    if (System.IO.File.Exists(path))
                    {
                        part.ProductPicturePath = path;
                    }
                    else
                    {
                        try
                        {
                            using (Stream stream = async.Result.Content.ReadAsStreamAsync().Result)
                            {
                                using (FileStream stream2 = System.IO.File.Create(path))
                                {
                                    stream.CopyTo(stream2);
                                }
                            }
                            this.UpdateProgressText("Downloading image for " + part.PartNumber);
                            int progressPercentage = (counter * 100) / partsWithDownload;
                            this.UpdateProgress(progressPercentage);
                            part.ProductPicturePath = path;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            });
        }

        public Task<string> ExecuteCall(string urlGet)
        {
            Task<string> task2;
            string requestUriString = string.Format(this.EtilizeEndPoint, this.EtilizeAPPId, this.EtilizeSiteID) + urlGet;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.ContentType = "GET";
                request.Method = "GET";
                request.Timeout = 0x4e20;
                request.Proxy = null;
                task2 = Task.Factory.FromAsync<WebResponse>(new Func<AsyncCallback, object, IAsyncResult>(request.BeginGetResponse), asyncResult => request.EndGetResponse(asyncResult), null).ContinueWith<string>(t => ReadStreamFromResponse(t.Result));
            }
            catch (WebException exception1)
            {
                throw new Exception(exception1.Message);
            }
            catch (Exception exception3)
            {
                throw new Exception(exception3.Message);
            }
            return task2;
        }

        public string ExecuteCall2(string urlGet)
        {
            string requestUriString = string.Format(this.EtilizeEndPoint, this.EtilizeAPPId, this.EtilizeSiteID) + urlGet;
            string str2 = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                using (Stream stream = ((HttpWebResponse)request.GetResponse()).GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        str2 = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return str2;
        }

        public string ExecuteSearch(string keywordFilter)
        {
            string requestUriString = string.Format(this.EtilizeEndPoint, this.EtilizeAPPId, this.EtilizeSiteID) + "&catalog=na&method=search&keywordFilter=" + keywordFilter + "&descriptions=all&manufacturer=default";
            string str2 = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                using (Stream stream = ((HttpWebResponse)request.GetResponse()).GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        str2 = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return str2;
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            string str2;
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    str2 = reader.ReadToEnd();
                }
            }
            return str2;
        }

        public void SetEtilizeAppId(string appId, string siteId)
        {
            this.EtilizeAPPId = appId;
            this.EtilizeSiteID = siteId;
        }

      
        //public delegate void UpdateProgressDelegate(int ProgressPercentage);
        //public event UpdateProgressDelegate UpdateProgress;
        //public delegate void UpdateProgressTextDelegate(string UpdateProgressText);
        //public event UpdateProgressTextDelegate UpdateProgressText;


        //private readonly string EtilizeEndPoint;
        //private readonly string EtilizeAPPId;

        //public Services()
        //{
        //    EtilizeEndPoint = ConfigurationManager.AppSettings["EtilizeEndPoint"].ToString(CultureInfo.InvariantCulture);
        //    EtilizeAPPId = ConfigurationManager.AppSettings["EtilizeAPPId"].ToString(CultureInfo.InvariantCulture);
        //}


        //public string ExecuteCall2(string urlGet)
        //{
        //    string EndpointFormated = String.Format(EtilizeEndPoint, EtilizeAPPId);
        //    EndpointFormated = EndpointFormated + urlGet;
        //    string content = string.Empty;
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndpointFormated);
        //        request.Method = "GET";
        //        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        //        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr = new StreamReader(stream))
        //            {
        //                content = sr.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message); 
        //    }
        //    return content;
        //}


        //public Task<string> ExecuteCall(string urlGet)
        //{
        //    string EndpointFormated = String.Format(EtilizeEndPoint, EtilizeAPPId);
        //    EndpointFormated = EndpointFormated + urlGet;
        //    string content = string.Empty;
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndpointFormated);
        //        request.ContentType = "GET";
        //        request.Method = WebRequestMethods.Http.Get;
        //        request.Timeout = 20000;
        //        request.Proxy = null;

        //        Task<WebResponse> task = Task.Factory.FromAsync(
        //            request.BeginGetResponse,
        //            asyncResult => request.EndGetResponse(asyncResult),
        //            (object)null);
               
        //        return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}


        ///// <summary>
        ///// </summary>
        ///// <param name="response"></param>
        ///// <returns></returns>
        //private static string ReadStreamFromResponse(WebResponse response)
        //{
        //    using (Stream responseStream = response.GetResponseStream())
        //    using (StreamReader sr = new StreamReader(responseStream))
        //    {
        //        //Need to return this response 
        //        string strContent = sr.ReadToEnd();
        //        return strContent;
        //    }
        //}

        ///// <summary>
        ///// </summary>
        ///// <param name="keywordFilter"></param>
        ///// <returns></returns>
        //public string ExecuteSearch(string keywordFilter)
        //{
        //    string EndpointFormated = String.Format(EtilizeEndPoint + "&catalog=na&method=search&keywordFilter="+ keywordFilter + "&descriptions=all&manufacturer=default", EtilizeAPPId);
        //    string content = string.Empty;
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(EndpointFormated);
        //        request.Method = "GET";
        //        request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
        //        request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            using (StreamReader sr = new StreamReader(stream))
        //            {
        //                content = sr.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //    return content;
        //}

        ///// <summary>
        ///// </summary>
        ///// <param name="downloadedList"></param>
        ///// <param name="downloadPath"></param>
        //public void DownloadImageContentFromParts(List<ProposalContentByPart> downloadedList, string downloadPath) 
        //{
        //    int counter = 0;
         
        //    int partsWithDownload = downloadedList.Where(x => !String.IsNullOrEmpty(x.ProductPictureURL)).Select(x => x).Count();
        //    HttpClient httpClient = new HttpClient();
        //    downloadedList.AsParallel().ForAll(part =>
        //    {
        //        if (!string.IsNullOrEmpty(part.ProductPictureURL))
        //        {
        //            counter++;
        //            try
        //            {
        //                var responseResult = httpClient.GetAsync(part.ProductPictureURL);
        //                string filename = part.ProductPictureURL.Substring(part.ProductPictureURL.LastIndexOf("/") + 1, (part.ProductPictureURL.Length - part.ProductPictureURL.LastIndexOf("/") - 1));
        //                filename = Utilitary.CleanFileName(filename);
        //                string filePath = downloadPath + filename;
        //                if (!File.Exists(filePath))
        //                {
        //                    using (var memStream = responseResult.Result.Content.ReadAsStreamAsync().Result)
        //                    {
        //                        using (var fileStream = File.Create(filePath))
        //                        {
        //                            memStream.CopyTo(fileStream);
        //                        }
        //                    }
        //                }
        //                UpdateProgressText("Downloading image for " + part.PartNumber);
        //                int total = (int)((counter * 100) / partsWithDownload);
        //                UpdateProgress(total);
        //                part.ProductPicturePath = filePath;
        //            }
        //            catch (Exception e) 
        //            {
        //                //check exception error
        //            }
        //        }
        //    });
        //}
    }

}
