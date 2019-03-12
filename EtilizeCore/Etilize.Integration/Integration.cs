using DocumentManager;
using Etilize.Data;
using Etilize.Services;
using EtilizeDocument;
using Etilize.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Etilize.Integration
{
    public class Integration
    {
        //update the UI
        public delegate void UpdateProgressDelegate(int ProgressPercentage);
        public event UpdateProgressDelegate UpdateProgress;
        public delegate void UpdateProgressTextDelegate(string UpdateProgressText);
        public event UpdateProgressTextDelegate UpdateProgressText;
        public delegate void UpdateProgressSubTitleDelegate(string UpdateProgressSubTitle);
        public event UpdateProgressSubTitleDelegate UpdateProgressSubTitle;
        public delegate void UpdateStepDelegate(int UpdateStepDelegate);
        public event UpdateStepDelegate UpdateStep;

        private readonly Services.Services EtilizeServices;
        private readonly ExcelManager ExcelManager;
        private readonly EtilizeDocumentIntegration EtilizeDocumentIntegration;
        private EtilizeDocumentConfiguration documentConfiguration;
        private readonly SetupDL setupDL;

        private readonly string EtilizeConnectionPath;
        private readonly string DIRECTORY_ROOT;
        private readonly string DIRECTORY_COMPANY;
        private readonly string DownloadXMLInfo;
        private string DbPassword;
        

        public Integration(EtilizeDocumentConfiguration documentConfiguration)
        {
            string RegKey = ConfigurationManager.AppSettings["RegKey"].ToString(CultureInfo.InvariantCulture);
            string DefaultSubKeyDir = ConfigurationManager.AppSettings["DefaultSubKeyDir"].ToString(CultureInfo.InvariantCulture);
            ConfigurationManager.AppSettings["EtilizeEndPoint"].ToString(CultureInfo.InvariantCulture);
            this.DownloadXMLInfo = ConfigurationManager.AppSettings["DownloadXMLInfo"].ToString(CultureInfo.InvariantCulture);
            string str6 = ConfigurationManager.AppSettings["DefaultSubKeyLocalDB"].ToString(CultureInfo.InvariantCulture);
            this.DbPassword = Utilitary.Decrypt(ConfigurationManager.AppSettings["dwpbd"].ToString(CultureInfo.InvariantCulture));
            this.DIRECTORY_ROOT = Utilitary.ReadValueFromRegistry(RegKey, ConfigurationManager.AppSettings["SubKeyDir"].ToString(CultureInfo.InvariantCulture));
            this.DIRECTORY_COMPANY = Utilitary.ReadValueFromRegistry(RegKey, ConfigurationManager.AppSettings["Directory_Company"].ToString(CultureInfo.InvariantCulture));
            this.EtilizeConnectionPath = DefaultSubKeyDir + ConfigurationManager.AppSettings["ProposalContentDB"].ToString(CultureInfo.InvariantCulture);
            this.documentConfiguration = documentConfiguration;
            string connectionValue = this.DIRECTORY_ROOT + str6;
            this.setupDL = new SetupDL(connectionValue, this.DbPassword);
            this.EtilizeServices = new Services.Services();
            
            this.ExcelManager = new ExcelManager();
            this.EtilizeDocumentIntegration = new EtilizeDocumentIntegration(documentConfiguration);
            this.EtilizeDocumentIntegration.UpdateProgress += new EtilizeDocumentIntegration.UpdateProgressDelegate(this.UpdateProgressIntegration);
            this.EtilizeDocumentIntegration.UpdateProgressText += new EtilizeDocumentIntegration.UpdateProgressTextDelegate(this.UpdateProgressTextIntegration);
            this.EtilizeServices.UpdateProgress += new Services.Services.UpdateProgressDelegate(this.UpdateProgressIntegration);
            this.EtilizeServices.UpdateProgressText += new Services.Services.UpdateProgressTextDelegate(this.UpdateProgressTextIntegration);
        }
        
        public void StartProcess()
        {
            try
            {
                string DOCSetupFile = ConfigurationManager.AppSettings["DOCSetupFile"].ToString(CultureInfo.InvariantCulture);
                string UserName = Environment.UserName;
                string CSVSetupFile = ConfigurationManager.AppSettings["CSVSetupFile"].ToString(CultureInfo.InvariantCulture).Replace("[USERNAME]", UserName);

                //update modal content
                this.UpdateProgressText("Preparing parts");
                this.GetEtilizeCloudID();
                this.UpdateStep(25);

                List<ExcelPartRequest> excelDataByColumnNameDataReader = this.ExcelManager.GetExcelDataByColumnNameDataReader(this.DIRECTORY_ROOT + CSVSetupFile);
                VendorDL vendordl = new VendorDL(this.EtilizeConnectionPath) { DbPwd = this.DbPassword  };
                excelDataByColumnNameDataReader = this.FormatVendorIDInRequest(excelDataByColumnNameDataReader, vendordl.GetAllVendors());
                excelDataByColumnNameDataReader = this.GetProposalDocumentsSaved(excelDataByColumnNameDataReader);

                //update modal content
                this.UpdateStep(50);

                List<ProposalContentByPart> proposalContentByParts = new List<ProposalContentByPart>();
                if (excelDataByColumnNameDataReader.Count > 0)
                {
                    proposalContentByParts = this.ProcessExcelPartsRequestRTF(excelDataByColumnNameDataReader);
                }

                //saving the founded vendors
                List<Vendor> distinctVendor = this.GetDistinctVendor(proposalContentByParts);
                vendordl.Save(distinctVendor);

                //update modal content
                DOCSetupFile = DOCSetupFile.Replace("[USERNAME]", UserName);
                string savePath = this.DIRECTORY_ROOT + DOCSetupFile;
                
                this.UpdateProgressSubTitle("Assembling proposal content (please wait)…");
                this.UpdateProgressText("Processing document, this could take some minutes");
                this.UpdateStep(75);

                if (ConfigurationManager.AppSettings["UseWordDoc"].ToString(CultureInfo.InvariantCulture).ToString().ToUpper().Equals("TRUE"))
                {
                    this.EtilizeDocumentIntegration.StarEtilizeDocAssebly(proposalContentByParts, savePath, 0, 0);
                }

                //update modal content
                this.UpdateStep(100);
                this.UpdateProgressText("Saving final changes");

                //calling exe
                this.ExcelManager.UpdateEtilizeStatusExcelDoc(this.DIRECTORY_ROOT + CSVSetupFile, excelDataByColumnNameDataReader);
                this.UpdateProgressText("Calling PopGen.exe");
                Process.Start(this.DIRECTORY_ROOT + @"PropGen\PropGen.exe");
            }
            catch (Exception ex)
            {
                SendErrorMessageToCloud(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        private void SendErrorMessageToCloud(string errorMessage) 
        {
            if (Utilitary.CheckForInternetConnection())
            {
                int ClientIdInt = 0;
                int UserIdInt = 0;
                string RegKey = ConfigurationManager.AppSettings["RegKey"].ToString(CultureInfo.InvariantCulture);
                
                Int32.TryParse(Utilitary.ReadValueFromRegistry(RegKey, "ClientId"), out ClientIdInt);
                Int32.TryParse(Utilitary.ReadValueFromRegistry(RegKey, "UserId"), out UserIdInt);
                if (errorMessage.Length > 375)
                {
                    errorMessage = errorMessage.Substring(0, 375) + "...";
                }
                new CorsProServices(Utilitary.GetSetupDLProperty(this.setupDL.GetSetupList(), "SDACloudUpdatesURL"))
                .SendErroLogMessageToCloud(UserIdInt, ClientIdInt, "Etilize error: " + errorMessage);
            }
        }

        public List<ProposalContentByPart> ProcessExcelPartsRequest(List<ExcelPartRequest> excelPartsRequest)
        {
            int ContentReloadDays = Int32.Parse(ConfigurationManager.AppSettings["ContentReloadDays"].ToString(CultureInfo.InvariantCulture));
            string downloadPath = DIRECTORY_ROOT + ConfigurationManager.AppSettings["SaveFilesPath"].ToString(CultureInfo.InvariantCulture);
            ProposalContentByPartDL contentByPartDL = new ProposalContentByPartDL(EtilizeConnectionPath);
            List<ProposalContentByPart> downloadedList = new List<ProposalContentByPart>();

            try
            {
                string partsQueryFormat = GetPartNumberInQueryFormat(excelPartsRequest);
                List<ProposalContentByPart> proposalContentByPartsSaved = contentByPartDL.GetByPartNumber(partsQueryFormat);
                Dictionary<string, Task<string>> serverResponses = ExecuteServerCalls(excelPartsRequest, proposalContentByPartsSaved);

                UpdateProgressText("Processing data");
                int counter = 0;
                foreach (var excelPart in excelPartsRequest)
                {
                    counter++;
                    UpdateProgressText("Processing information for " + excelPart.PartNumber);
                    int total = (int)((counter * 100) / excelPartsRequest.Count);
                    UpdateProgress(total);
                    if (string.IsNullOrEmpty(excelPart.SDADocName))
                    {
                        ProposalContentByPart contentExist = proposalContentByPartsSaved.Where(x => x.PartNumber.Equals(excelPart.PartNumber)).FirstOrDefault();
                        if (contentExist == null)
                        {
                            string XMLServerResponse = serverResponses.Where(x => x.Key.Equals(excelPart.PartNumber)).Select(x => x.Value.Result).FirstOrDefault();
                            ProposalContentByPart proposal = GetProposalContentByPartFromXML(XMLServerResponse, excelPart);

                            if (proposal != null)
                            {
                                proposal.Optional = excelPart.Optional;
                                if (proposal != null)
                                {
                                    excelPart.Found = true;
                                    proposal.IsNew = true;
                                    downloadedList.Add(proposal);
                                }
                            }
                            else
                            {
                                excelPart.Found = false;
                            }
                        }
                        else
                        {
                            excelPart.Found = true;
                           
                            int daysLastUpdate = (DateTime.Now - contentExist.DownloadDT).Days;
                            if (daysLastUpdate > ContentReloadDays)
                            {
                                string XMLServerResponse = serverResponses.Where(x => x.Key.Equals(excelPart.PartNumber)).Select(x => x.Value.Result).FirstOrDefault();
                                ProposalContentByPart proposal = GetProposalContentByPartFromXML(XMLServerResponse, excelPart);

                                string imagepath = "";
                                if (proposal != null)
                                {
                                    contentExist.ProductName = proposal.ProductName;
                                    contentExist.FeatureBullets = proposal.FeatureBullets;
                                    contentExist.MarketingInfo = proposal.MarketingInfo;
                                    contentExist.TechnicalInfo = proposal.TechnicalInfo;
                                    contentExist.ProductPictureURL = proposal.ProductPictureURL;
                                    contentExist.ProductPicturePath = imagepath;
                                    contentExist.MfgName = proposal.MfgName;
                                    contentExist.MfgPartNumber = proposal.MfgPartNumber;
                                    contentExist.Optional = excelPart.Optional;
                                    contentExist.VendorID = excelPart.VendorId;
                                    contentExist.IsUpdate = true;
                                }
                            }
                            else if (!String.IsNullOrEmpty(contentExist.ProductPictureURL))
                            {
                                contentExist.VendorID = excelPart.VendorId;
                                contentExist.Optional = excelPart.Optional;
                            }

                            contentExist.Optional = excelPart.Optional;
                            downloadedList.Add(contentExist);
                        }
                    }
                    else
                    {
                        excelPart.Found = false;
                        ProposalContentByPart proposal = new ProposalContentByPart() 
                        {
                            PartNumber = excelPart.PartNumber,
                            VendorID = excelPart.VendorId,
                            VendorName = excelPart.VendorName,
                            Document = excelPart.Word_Doc, 
                            Optional = excelPart.Optional
                        };
                        downloadedList.Add(proposal);
                    }
                }

                //download all images
                EtilizeServices.DownloadImageContentFromParts(downloadedList, downloadPath);

                //save content
                contentByPartDL.Save(downloadedList);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return downloadedList;
        }

        private Dictionary<string, Task<string>> ExecuteServerCalls(List<ExcelPartRequest> excelPartsRequest, List<ProposalContentByPart> proposalContentByPartsSaved) 
        {
            try
            {
                Dictionary<string, Task<string>> listContent = new Dictionary<string, Task<string>>();
                if (Utilitary.CheckForInternetConnection())
                {
                    int counter = 0;
                    foreach (var excelPart in excelPartsRequest)
                    {
                        counter++;
                        //if the component not exist in the local database download it
                        ProposalContentByPart contentExist = proposalContentByPartsSaved.Where(x => x.PartNumber.Equals(excelPart.PartNumber)).FirstOrDefault();
                        if (contentExist == null && String.IsNullOrEmpty(excelPart.SDADocName))
                        {
                            UpdateProgressText("Downloading information for " + excelPart.PartNumber);
                            int total = (int)((counter * 100) / excelPartsRequest.Count);
                            UpdateProgress(total);
                            Task<string> task = this.EtilizeServices.ExecuteCall("&catalog=na&method=getProduct&partNumber=" + excelPart.PartNumber + "&mfgName=" + excelPart.VendorName + "&descriptionTypes=0,3&skuType=all&manufacturer=default&featureBulletsMax=10&displayTemplate=0&resourceTypes=all");
                            listContent.Add(excelPart.PartNumber, task);
                        }
                    }
                    UpdateProgressText("Processing server response");
                    Task.WaitAll(listContent.Values.ToArray());
                    UpdateProgressText("Server response done");
                }
                return listContent;
            }
            catch (Exception ex) 
            {
                if (ex.InnerException != null)
                {
                    throw new Exception(ex.Message + " " + ex.InnerException);
                }
                else
                {
                    throw new Exception(ex.Message);
                }
            }
        }

        private Dictionary<string, Task<string>> ExecuteServerSingleCall(ExcelPartRequest excelPartRequest, ProposalContentByPart proposalContentByPartsSaved)
        {
            Dictionary<string, Task<string>> listTest = new Dictionary<string, Task<string>>();
            if (Utilitary.CheckForInternetConnection())
            {
                Task<string> task = EtilizeServices.ExecuteCall("&catalog=na&method=getProduct&partNumber=" + excelPartRequest.PartNumber + "&mfgName=" + excelPartRequest.VendorName + "&descriptionTypes=0,3&skuType=all&manufacturer=default&featureBulletsMax=10&displayTemplate=0&resourceTypes=all");
                listTest.Add(excelPartRequest.PartNumber, task);
                Task.WaitAll(listTest.Values.ToArray());
            }
            return listTest;
        }


        private string FormatListInRTF(IEnumerable<XElement> rootElement, string subElementName, string format = "", string specificElementName = "")
        {
            RichTextBox box = new RichTextBox();
            StringBuilder builder = new StringBuilder();
            box.SelectionBullet = true;
            string rtf = "";
            foreach (XElement element in rootElement.Elements<XElement>())
            {
                string str5;
                string text1 = element.Attribute("type").Value;
                string str2 = element.Attribute("number").Value;
                string str3 = element.Value;
                string str4 = string.IsNullOrEmpty(str3) ? str2 : str3;
                if (((str5 = format) != null) && (str5 == "bullet"))
                {
                    box.Text = box.Text + str4;
                    builder.Append(str4);
                    builder.Append(Environment.NewLine);
                }
            }
            box.Text = builder.ToString();
            rtf = box.Rtf;
            Console.WriteLine(rtf);
            return rtf;
        }

       
           private List<ExcelPartRequest> GetProposalDocumentsSaved(List<ExcelPartRequest> excelPartRequests)
        {
            List<ExcelPartRequest> list;
            try
            {
                string str = ConfigurationManager.AppSettings["PQDB_Route"].ToString(CultureInfo.InvariantCulture);
                SectionTblDL ldl = new SectionTblDL(this.DIRECTORY_COMPANY + str) {
                    DbPwd = Etilize.Integration.Utilitary.Decrypt(ConfigurationManager.AppSettings["PWD"].ToString(CultureInfo.InvariantCulture))
                };
                foreach (ExcelPartRequest request in excelPartRequests)
                {
                    if (string.IsNullOrEmpty(request.SDADocName))
                    {
                        continue;
                    }
                    byte[] wordDocBySectionName = ldl.GetWordDocBySectionName(request.SDADocName);
                    if (wordDocBySectionName != null)
                    {
                        request.Word_Doc = wordDocBySectionName;
                    }
                }
                list = excelPartRequests;
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return list;
        }


        private List<string> GetQuerablePartList(List<ExcelPartRequest> excelPartRequests)
        {
            List<string> listParts = new List<string>();
            int chunkSize = 50;
            int counter = 1;
            string partText = "";
            foreach (var partRequest in excelPartRequests)
            {
                if (counter % chunkSize == 0)
                {
                    listParts.Add(partText);
                    partText = "";
                }
                partText += "task={id:" + counter + ",mfgId:" + partRequest.VendorId + ",partNumber:" + partRequest.PartNumber + "}&";
                counter++;
            }
            partText = partText.Substring(0, partText.Length - 1);
            listParts.Add(partText);
            return listParts;
        }

        
        private List<ExcelPartRequest> CleanExcelPartRequestList(List<ExcelPartRequest> excelPartRequests)
        {
            List<ExcelPartRequest> cleanedList = new List<ExcelPartRequest>();
            try
            {
                foreach (var part in excelPartRequests)
                {
                    if (string.IsNullOrEmpty(part.SDADocName))
                    {
                        cleanedList.Add(part);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return cleanedList;
        }


        private List<ExcelPartRequest> GetVendorIdByProductPartList(List<ExcelPartRequest> excelPartRequests, List<Vendor> vendors)
        {
            try
            {
                using (List<Vendor>.Enumerator enumerator = vendors.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Vendor vendor = enumerator.Current;
                        Func<ExcelPartRequest, bool> predicate = null;
                        if (predicate == null)
                        {
                            predicate = x => x.VendorName.Equals(vendor.VendorName);
                        }
                        List<ExcelPartRequest> list = excelPartRequests.Where<ExcelPartRequest>(predicate).ToList<ExcelPartRequest>();
                        int vendorId = 0;
                        foreach (ExcelPartRequest request in list)
                        {
                            if (request.VendorId != 0)
                            {
                                continue;
                            }
                            string xml = "";
                                
                            xml = this.EtilizeServices.ExecuteSearch(request.PartNumber);
                            if (!xml.Contains("Error"))
                            {
                                xml = this.RemoveXmlDefinition(xml);
                                foreach (KeyValuePair<int, string> pair in this.GetVendorListFromXML(xml))
                                {
                                    //if (pair.Value.ToUpper().Contains(request.VendorName.ToUpper()))
                                    //{
                                        request.VendorId = pair.Key;
                                        vendor.VendorID = pair.Key;
                                        vendorId = request.VendorId;
                                        break;
                                    //}
                                }
                                if (vendorId != 0)
                                {
                                    break;
                                }
                            }
                        }
                        foreach (ExcelPartRequest request2 in list)
                        {
                            if (request2.VendorId == 0)
                            {
                                request2.VendorId = vendorId;
                            }
                        }
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return excelPartRequests;
        }

     
        private List<KeyValuePair<int, string>> GetVendorListFromXML(string xml)
        {
            XElement element = this.LoadXMLFromString(xml, null);
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
          
                try
                {
                    IEnumerable<XElement> VendorList = from t in element.Elements()
                                                      where t.Name.LocalName.Contains("manufacturer")
                                                      select t;

                    foreach (XElement vendor in VendorList) 
                    {
                        list.Add(new KeyValuePair<int, string>(int.Parse(vendor.Attribute("id").Value), vendor.Attribute("name").Value));
                    }
                }
                catch (Exception exception1)
                {
                    throw new Exception(exception1.Message);
                }
            return list;
        }

        private int GetVendorIdFromXML(string xml) 
        {
            List<KeyValuePair<int, string>> VendorsList = GetVendorListFromXML(xml);
            if (VendorsList.Count > 0) 
            {
                return VendorsList.First().Key;
            }
            return 0;
        }

        private List<Vendor> GetDistinctVendor(List<ProposalContentByPart> proposalContent)
        {
            List<Vendor> list = new List<Vendor>();
            foreach (ProposalContentByPart proposal in proposalContent)
            {
                Vendor item = new Vendor
                {
                    VendorID = proposal.VendorID,
                    VendorName = proposal.VendorName
                };
                list.Add(item);
            }
            return (from x in list
                    group x by x.VendorName into x
                    select x.First<Vendor>()).ToList<Vendor>();
        }

        
        private List<Vendor> GetDistinctVendor(List<ExcelPartRequest> partsRequest)
        {
             List<Vendor> list = new List<Vendor>();
            foreach (ExcelPartRequest request in partsRequest)
            {
                Vendor item = new Vendor {
                    VendorID = request.VendorId,
                    VendorName = request.VendorName
                };
                list.Add(item);
            }
            return (from x in list
                group x by x.VendorName into x
                select x.First<Vendor>()).ToList<Vendor>();
        }

        private void GetEtilizeCloudID()
        {
            try
            {
                List<string[]> lastEtilizeRetrievedKeys = new CorsProServices(Utilitary.GetSetupDLProperty(this.setupDL.GetSetupList(), "SDACloudUpdatesURL")).GetLastEtilizeRetrievedKeys();
                if (lastEtilizeRetrievedKeys.Count > 1)
                {
                    string appId = Utilitary.DecryptCorsProServerMessage(lastEtilizeRetrievedKeys[0][0], lastEtilizeRetrievedKeys[0][1]);
                    this.EtilizeServices.SetEtilizeAppId(appId, Etilize.Integration.Utilitary.DecryptCorsProServerMessage(lastEtilizeRetrievedKeys[1][0], lastEtilizeRetrievedKeys[1][1]));
                }
                else if (lastEtilizeRetrievedKeys.Count == 1)
                {
                    throw new Exception(lastEtilizeRetrievedKeys[0].ToString());
                }
            }
            catch (Exception exception)
            {
                if (Utilitary.CheckForInternetConnection())
                {
                    throw new Exception("You are not authorized to retrieve proposal content from the cloud." + exception.Message);
                }
            }
        }

     
        private List<ExcelPartRequest> FormatVendorIDInRequest(List<ExcelPartRequest> excelPartRequests, List<Vendor> vendors)
        {
            foreach (ExcelPartRequest request in excelPartRequests)
            {
                foreach (Vendor vendor in vendors)
                {
                    if (vendor.VendorName.ToUpper().Contains(request.VendorName.ToUpper()))
                    {
                        request.VendorId = vendor.VendorID;
                        break;
                    }
                }
            }
            return excelPartRequests;
        }

        private ProposalContentByPart GetProposalContentByPartFromXML(string etilizeResult, ExcelPartRequest componentPart)
        {
            if (!etilizeResult.Contains("Error"))
            {
                XElement load = LoadXMLFromString(etilizeResult, componentPart);
                IEnumerable<XElement> resourcesList = load.Elements().Where(t => t.Name.LocalName.Contains("resources"));

                string imageUrl = GetRecomendedImage(resourcesList);
                string filename = imageUrl.Substring(imageUrl.LastIndexOf("/") + 1, (imageUrl.Length - imageUrl.LastIndexOf("/") - 1));

                if (DownloadXMLInfo.Equals("True"))
                    DownloadXML(etilizeResult, componentPart.PartNumber);

                //getting all the sections of the XML 
                XElement MainInfo = load.Elements().Where(t => t.Name.LocalName.Contains("manufacturer id")).FirstOrDefault();
                IEnumerable<XElement> SkusList = load.Elements().Where(t => t.Name.LocalName.Contains("skus"));
                IEnumerable<XElement> DescriptionsList = load.Elements().Where(t => t.Name.LocalName.Contains("descriptions"));
                IEnumerable<XElement> FeatureBulletsList = load.Elements().Where(t => t.Name.LocalName.Contains("featureBullets"));
                IEnumerable<XElement> DatasheetList = load.Elements().Where(t => t.Name.LocalName.Contains("datasheet"));

                string skuElement = GetInfoFromXElement(SkusList, "sku", "bullet");
                string ManufacurerName = GetMarketingInformationFromXElement(DatasheetList, "General Information", "Brand Name");
                string ProductName = GetMarketingInformationFromXElement(DatasheetList, "General Information", "Product Name");
                string ProductType = GetMarketingInformationFromXElement(DatasheetList, "General Information", "Product Type");
                if (string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(ManufacurerName))
                {
                    ProductName = GetDescriptionFromXElement(DescriptionsList, "description", "0");
                }
                else
                {
                    ProductName = ManufacurerName + " " + ProductName;
                }

                string ManufacturerId = (MainInfo != null) ? MainInfo.Value : "";
                string ProductLittleDesc = GetDescriptionFromXElement(DescriptionsList, "description", "3");
                string FeatureBullets = GetFeatureBulletFromXElement(FeatureBulletsList, "featureBullet");
                string MarketingInfo = GetMarketingInformationFromXElement(DatasheetList, "General Information", "Marketing Information");
                string TechnicalInfo = GetTechnicalInformationFromXElement(DatasheetList, "General Information");
                string MfgPartNumber = GetMfgPartNumberFromXElement(load.Elements(), "manufacturer", "number");
                string MfgPartName = GetMfgPartNumberFromXElement(load.Elements(), "manufacturer", "name");

                ProposalContentByPart proposal = new ProposalContentByPart
                {
                    ProductPicturePath = null,
                    ProductPictureURL = imageUrl,
                    PartNumber = componentPart.PartNumber,
                    VendorName = componentPart.VendorName,
                    VendorID = componentPart.VendorId,
                    ProductName = ProductName,
                    FeatureBullets = FeatureBullets,
                    MarketingInfo = MarketingInfo,
                    TechnicalInfo = TechnicalInfo,
                    MfgPartNumber = MfgPartNumber,
                    MfgName = MfgPartName,
                    ProductType = ProductType
                };
                return proposal;
            }
            return null;
        }

        private XElement LoadXMLFromString(string etilizeResult, ExcelPartRequest componentPart = null)
        {
            string downloadPath = DIRECTORY_ROOT + ConfigurationManager.AppSettings["SaveFilesPath"].ToString(CultureInfo.InvariantCulture);
            string documentName = "Etilize";
            
            string document = RemoveXmlDefinition(etilizeResult);
            string subPath = downloadPath + "\\"; // your code goes here
            bool exists = System.IO.Directory.Exists(subPath);
            if (!exists)
                System.IO.Directory.CreateDirectory(subPath);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(document);
            XmlNode myNode = doc.DocumentElement;
            doc.Save(subPath + documentName + ".xml");
            File.WriteAllText(subPath + documentName + ".xml", document);
            XElement element = XElement.Load(subPath + documentName + ".xml");
            if (File.Exists(subPath + documentName + ".xml"))
            {
                File.Delete(subPath + documentName + ".xml");
            }
            return element;
        }

        public List<ProposalContentByPart> ProcessExcelPartsRequestRTF(List<ExcelPartRequest> excelPartsRequest)
        {
            string downloadPath = this.DIRECTORY_ROOT + ConfigurationManager.AppSettings["SaveFilesPath"].ToString(CultureInfo.InvariantCulture);
            ProposalContentByPartDL proposalContentByPartDL = new ProposalContentByPartDL(this.EtilizeConnectionPath) { DbPwd = this.DbPassword };
            List<ProposalContentByPart> downloadedList = new List<ProposalContentByPart>();

            try
            {
                List<ProposalContentByPart> byPartNumber = proposalContentByPartDL.GetByPartNumber(this.GetPartNumberInQueryFormat(excelPartsRequest));

                Dictionary<string, Task<string>> source = this.ExecuteServerCalls(excelPartsRequest, byPartNumber);
                this.UpdateProgressText("Processing data");

                int num = 0;
                using (List<ExcelPartRequest>.Enumerator enumerator = excelPartsRequest.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ExcelPartRequest excelPart = enumerator.Current;
                        num++;
                        this.UpdateProgressText("Processing information for " + excelPart.PartNumber);
                        int progressPercentage = (num * 100) / excelPartsRequest.Count;
                        this.UpdateProgress(progressPercentage);
                        if (!string.IsNullOrEmpty(excelPart.SDADocName))
                        {
                            excelPart.Found = false;
                            ProposalContentByPart part = new ProposalContentByPart {
                                PartNumber = excelPart.PartNumber,
                                VendorID = excelPart.VendorId,
                                VendorName = excelPart.VendorName,
                                Document = excelPart.Word_Doc,
                                Optional = excelPart.Optional
                            };
                            downloadedList.Add(part);
                            continue;
                        }
                       
                        ProposalContentByPart item = byPartNumber.Where(x => x.PartNumber.Equals(excelPart.PartNumber)).FirstOrDefault<ProposalContentByPart>();
                        if (item != null)
                        {
                            excelPart.Found = true;
                            item.VendorID = excelPart.VendorId;
                            item.Optional = (excelPart.Optional == null) ? item.Optional : excelPart.Optional;
                            //if the ID is equal to 0, update the ID from the Etilize XML
                            if (item.VendorID == 0) 
                            {
                                int currentVendorId = downloadedList.Where(x => x.VendorName.Equals(excelPart.VendorName)).Select(x => x.VendorID).FirstOrDefault();
                                if (currentVendorId == 0)
                                {
                                    Dictionary<string, Task<string>> singleSource = this.ExecuteServerSingleCall(excelPart, item);
                                    Func<KeyValuePair<string, Task<string>>, bool> func2 = x => x.Key.Equals(excelPart.PartNumber);
                                    string etilizeResult = (from x in singleSource.Where<KeyValuePair<string, Task<string>>>(func2) select x.Value.Result).FirstOrDefault<string>();
                                    item.VendorID = this.GetVendorIdFromXML(etilizeResult);
                                }
                                else 
                                {
                                    item.VendorID = currentVendorId;
                                }
                            }
                            downloadedList.Add(item);
                        }
                        else
                        {
                            if (Utilitary.CheckForInternetConnection())
                            {
                                
                                Func<KeyValuePair<string, Task<string>>, bool> func2 = x => x.Key.Equals(excelPart.PartNumber);
                                string etilizeResult = (from x in source.Where<KeyValuePair<string, Task<string>>>(func2) select x.Value.Result).FirstOrDefault<string>();
                                ProposalContentByPart proposalContentByPartFromXMLInRTF = this.GetProposalContentByPartFromXMLInRTF(etilizeResult, excelPart);
                                if (proposalContentByPartFromXMLInRTF == null)
                                {
                                    excelPart.Found = false;
                                }
                                else
                                {
                                    proposalContentByPartFromXMLInRTF.Optional = excelPart.Optional;
                                    if (proposalContentByPartFromXMLInRTF != null)
                                    {
                                        excelPart.Found = true;
                                        proposalContentByPartFromXMLInRTF.IsNew = true;
                                        downloadedList.Add(proposalContentByPartFromXMLInRTF);
                                    }
                                }
                            }
                        }
                    }
                }
                this.EtilizeServices.DownloadImageContentFromParts(downloadedList, downloadPath);
                proposalContentByPartDL.Save(downloadedList);
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return downloadedList;
        }

       
        private string GetMfgPartNumberFromXElement(IEnumerable<XElement> rootElement, string subElementName, string atribute)
        {
            string str = "";
            foreach (XElement element in rootElement)
            {
                if (element.Name.LocalName.ToString().Equals(subElementName))
                {
                    str = element.Attribute(atribute).Value;
                    break;
                }
            }
            return str;
        }

        private string GetDescriptionFromXElement(IEnumerable<XElement> rootElement, string subElementName, string typeNumber)
        {
            string str = "";
            foreach (XElement element in rootElement.Elements<XElement>())
            {
                if (element.Name.LocalName.ToString().Equals(subElementName))
                {
                    string str2 = element.Attribute("type").Value;
                    if (typeNumber.Equals(str2))
                    {
                        str = element.Value;
                        break;
                    }
                }
            }
            return str;
        }

        private string GetTechnicalInformationFromXElement(IEnumerable<XElement> rootElement, string subElementName)
        {
            string data = "";
            foreach (var element in rootElement.Elements())
            {
                var name = element.Attribute("name").Value;
                if (!name.Equals(subElementName))
                {
                    foreach (var atribute in element.Elements())
                    {
                        data += "<li>" + atribute.Attribute("name").Value + ": " + atribute.Value + "</li>";
                    }
                }
            }
            if(!String.IsNullOrEmpty(data))
                data = "<ul>" + data + "</ul>";

            return data;
        }

        private string GetMarketingInformationFromXElement(IEnumerable<XElement> rootElement, string subElementName, string atributeName)
        {
            string data = "";
            foreach (var element in rootElement.Elements())
            {
                var name = element.Attribute("name").Value;
                if (name.Equals(subElementName))
                {
                    foreach (var atribute in element.Elements())
                    {
                        if (atribute.Attribute("name").Value.Equals(atributeName))
                        {
                            data = atribute.Value;
                            return data;
                        }
                    }
                }
            }
            return data;
        }

        
        private string GetFeatureBulletFromXElement(IEnumerable<XElement> rootElement, string subElementName)
        {
            string data = "";
            foreach (var element in rootElement.Elements())
            {
                if (element.Name.LocalName.ToString().Equals(subElementName))
                {
                    data +=  "<li>" + element.Value +"</li>";
                }
            }
            if (!String.IsNullOrEmpty(data))
                data = "<ul>" + data + "</ul>";
            return data;
        }

        private string GetInfoFromXElement(IEnumerable<XElement> rootElement, string subElementName, string format ="", string specificElementName = "")
        {
            string data = "";
            foreach (var element in rootElement.Elements())
            {
                if (element.Name.LocalName.ToString().Equals(subElementName))
                {
                    if (!string.IsNullOrEmpty(specificElementName))
                    {
                        var name = element.Attribute("name").Value;
                        if (name.Equals(specificElementName))
                        {
                            data = element.Value;
                            break;
                        }
                    }

                    var type = element.Attribute("type").Value; // type = resolution or company logo
                    var number = element.Attribute("number").Value; // type = resolution or company logo
                    var value = element.Value;

                    string text = type + ": " + (string.IsNullOrEmpty(value) ? number : value);

                    switch (format)
                    {
                        case "bullet":
                            data += "<li>"+ text + "</li>";
                            break;
                        case "return":
                            data += text + "/n";
                            break;
                        default:
                            data += text + " ";
                            break;
                    }
                }
            }
            if (format.Equals("bullet"))
            {
                if (!String.IsNullOrEmpty(data))
                    data = "<ul>" + data + "</ul>";
            }
            return data;
        }

        private string GetRecomendedImage(IEnumerable<XElement> imageList)
        {
            Dictionary<int, string> resourcesList = new Dictionary<int, string>();
            foreach (var node in imageList.Elements())
            {
                if (node.Name.LocalName.ToString().Contains("resource"))
                {
                    var type = node.Attribute("type").Value; // type = resolution or company logo
                    var url = node.Attribute("url").Value;
                    int formatedId = Utilitary.ConvertToInt(type);
                    if (formatedId != 0)
                    {
                        resourcesList.Add(formatedId, url);
                    }
                }
            }
            var sortedDiccionary = resourcesList.OrderBy(x => x.Key);

            string fileUrl = "";
            int imageSize = 0;
            foreach (var resource in sortedDiccionary)
            {
                fileUrl = resource.Value;
                imageSize = resource.Key;
                if (resource.Key > 1000)
                {
                    break;
                }
            }
        
            return fileUrl;
        }

      
        private string DownloadImage(string imageURL)
        {
            string fileName = "";
            try
            {
                string str2 = this.DIRECTORY_ROOT + ConfigurationManager.AppSettings["SaveFilesPath"].ToString(CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(imageURL))
                {
                    string str3 = "";
                    str3 = Etilize.Integration.Utilitary.CleanFileName(imageURL.Substring(imageURL.LastIndexOf("/") + 1, (imageURL.Length - imageURL.LastIndexOf("/")) - 1));
                    string path = str2;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (WebClient client = new WebClient())
                    {
                        if (System.IO.File.Exists(path + @"\" + str3))
                        {
                            fileName = path + @"\" + str3;
                        }
                        else
                        {
                            fileName = path + @"\" + str3;
                            client.DownloadFile(imageURL, fileName);
                        }
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return fileName;
        }

        public void DownloadXML(string xml, string partNumber)
        {
            string str = this.DIRECTORY_ROOT + ConfigurationManager.AppSettings["SaveFilesPath"].ToString(CultureInfo.InvariantCulture);
            string str2 = Etilize.Integration.Utilitary.CleanFileName(string.IsNullOrEmpty(partNumber) ? "Etilize" : partNumber);
            string path = str;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            XmlElement documentElement = document.DocumentElement;
            document.Save(path + str2 + ".xml");
            System.IO.File.WriteAllText(path + "/" + str2 + ".xml", xml);
        }

       
        private string RemoveXmlDefinition(string xml)
        {
            XDocument xdoc = XDocument.Parse(xml);
            xdoc.Declaration = null;

            return xdoc.ToString();
        }

        private string GetPartNumberInQueryFormat(List<ExcelPartRequest> excelParts)
        {
            string str = "";
            foreach (ExcelPartRequest request in excelParts)
            {
                if (!string.IsNullOrEmpty(request.PartNumber))
                {
                    str = str + "'" + request.PartNumber + "',";
                }
            }
            return str.Remove(str.LastIndexOf(','), 1);
        }


        private ProposalContentByPart GetProposalContentByPartFromXMLInRTF(string etilizeResult, ExcelPartRequest componentPart)
        {
            if ((etilizeResult == null) || etilizeResult.Contains("Error"))
            {
                return null;
            }
            if (this.DownloadXMLInfo.ToLower().Equals("true"))
            {
                this.DownloadXML(etilizeResult, componentPart.PartNumber);
            }
            XElement element = this.LoadXMLFromString(etilizeResult, componentPart);
            IEnumerable<XElement> imageList = from t in element.Elements()
                where t.Name.LocalName.Contains("resources")
                select t;
            string recomendedImage = this.GetRecomendedImage(imageList);
            recomendedImage.Substring(recomendedImage.LastIndexOf("/") + 1, (recomendedImage.Length - recomendedImage.LastIndexOf("/")) - 1);

            var elementList = element.Elements().Where(subelement => subelement.Name.LocalName.Contains("skus"));
            IEnumerable<XElement> rootElement = element.Elements().Where(rootSubElement => rootSubElement.Name.LocalName.Contains("datasheet"));
            XElement MainInfo = element.Elements().Where(subelement => subelement.Name.LocalName.Contains("manufacturer")).FirstOrDefault();
            int MfgId = 0;
            Int32.TryParse(((MainInfo == null) ? "0" : MainInfo.Attribute("id").Value.ToString()), out MfgId);

            return new ProposalContentByPart { 
                ProductPicturePath = null,
                ProductPictureURL = recomendedImage,
                PartNumber = componentPart.PartNumber,
                VendorName = componentPart.VendorName,
                VendorID = this.GetVendorIdFromXML(etilizeResult),
                ProductName = this.GetMarketingInformationFromXElement(rootElement, "General Information", "Product Name"),
                FeatureBullets = RTFParser.ConvertXMLIntoBullets(from t in element.Elements()
                    where t.Name.LocalName.Contains("featureBullets")
                    select t, "featureBullet"),
                MarketingInfo = RTFParser.ConvertIntoParragraph(rootElement, "General Information", "Marketing Information"),
                TechnicalInfo = RTFParser.ConvertIntoParragraph(rootElement, "General Information"),
                MfgPartNumber = this.GetMfgPartNumberFromXElement(element.Elements(), "manufacturer", "number"),
                MfgName = this.GetMfgPartNumberFromXElement(element.Elements(), "manufacturer", "name"),
                ProductType = GetMarketingInformationFromXElement(rootElement, "General Information", "Product Type"),
                MfgID = MfgId
            };
        }

        private List<ExcelPartRequest> CleanUpComponetListToRequest(List<ComponentPart> componentsRequest, List<ExcelPartRequest> excelPartRequests )
        {
            foreach (ExcelPartRequest excelPart in excelPartRequests.ToList())
            {
                foreach (ComponentPart part in componentsRequest)
                {
                    if (excelPart.PartNumber.Equals(part.PartNumber))
                    {
                        excelPartRequests.Remove(excelPart);
                        break;
                    }
                }
            }
            return excelPartRequests;
        }

        public void UpdateProgressTextIntegration(string text)
        {
            UpdateProgressText(text);
        }

        private void UpdateProgressIntegration(int value)
        {
            UpdateProgress(value);
        }
    }

}
