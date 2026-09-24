using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Data;
using System.Text;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class StichingOrderController : BaseController
    {
        public IStichingOrderService _stichingOrderService { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public StichingOrderController(IStichingOrderService stichingOrderService, IWebHostEnvironment hostingEnvironment, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _stichingOrderService = stichingOrderService;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            ViewBag.Permissions = CommonHelper.GetValues(HttpContext).RoleType == "A"
                ? "Admin"
                : CommonHelper.GetPermissionByMenueID(CommonHelper.GetValues(HttpContext).RoleID, CommonHelper.GetValues(HttpContext).MenuID);
            ViewBag.SuitTypes = DropdownService.SuitTypeDropdown();
            return View();
        }

        [HttpGet]
        public JsonResult QuickSearch()
        {
            try
            {
                var data = _stichingOrderService.QuickSearch(CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public JsonResult GetCustomerById(int id)
        {
            var data = _stichingOrderService.GetCustomerById(id, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpGet]
        public JsonResult GetCustomerByContactNo(string contactNo)
        {
            var data = _stichingOrderService.GetCustomerByContactNo(contactNo, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpGet]
        public JsonResult GetOrderById(int id)
        {
            var data = _stichingOrderService.GetOrderById(id, CommonHelper.GetValues(HttpContext));
            return Json(data);
        }

        [HttpPost]
        public JsonResult Save(StichingOrder model)
        {
            try
            {
                var data = _stichingOrderService.Save(model, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var data = _stichingOrderService.Delete(id, CommonHelper.GetValues(HttpContext));
                return Json(data);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult GetPrintReport(RDLCReport model)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var filePath = GenerateReport(model);
                if (!string.IsNullOrEmpty(filePath))
                {
                    response.data = filePath;
                    response.msg = "";
                    response.msgType = 1;
                }
                else
                {
                    response.msg = "Unable to generate report. Please try again later.";
                    response.msgType = 2;
                }
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msg = _catchMessage;
                response.msgType = 2;
            }
            return Json(response);
        }

        private string GenerateReport(RDLCReport model)
        {
            var filePath = "";
            if (model != null && model.TRAN_ID > 0)
            {
                DataTable reportDetails = CreateStichingOrderPrintTable();
                var responseMessage = _stichingOrderService.GetDataForPrintReport(model, reportDetails, CommonHelper.GetValues(HttpContext));
                if (responseMessage.msgType != 1)
                {
                    throw new Exception(string.IsNullOrWhiteSpace(responseMessage.msg)
                        ? "Unable to generate report. Please try again later."
                        : responseMessage.msg);
                }
                var reportData = (CustomStichingOrderForPrintReport)responseMessage.data;
                using (LocalReport report = new LocalReport())
                {
                    var reportName = !string.IsNullOrWhiteSpace(reportData.Master?.REPORT_NAME) ? reportData.Master.REPORT_NAME : "StichingOrderPrintReport";
                    var path = Path.Combine(_hostingEnvironment.ContentRootPath, $@"Reports\{reportName}.rdlc");
                    using (var stReader = new StreamReader(path))
                    {
                        string stringreader = stReader.ReadToEnd();
                        byte[] byteArray = Encoding.UTF8.GetBytes(stringreader);
                        using (var stream = new MemoryStream(byteArray))
                        {
                            report.EnableExternalImages = true;
                            report.LoadReportDefinition(stream);
                            report.DataSources.Clear();
                            var companyLogoPath = Path.Combine(_hostingEnvironment.WebRootPath, @$"Client\Company\{reportData.Master?.COMPANY_LOGO}");
                            bool showCompanyLogo = System.IO.File.Exists(companyLogoPath);
                            string companyLogoUri = "";
                            try
                            {
                                if (showCompanyLogo)
                                {
                                    companyLogoUri = new Uri(companyLogoPath).AbsoluteUri;
                                }
                            }
                            catch
                            {
                                showCompanyLogo = false;
                                companyLogoUri = "";
                            }
                            ReportParameter parameter1 = new ReportParameter("CompanyName", reportData.Master?.COMPANY_NAME ?? "");
                            ReportParameter parameter2 = new ReportParameter("CompanyAddress", reportData.Master?.COMPANY_ADDRESS ?? "");
                            ReportParameter parameter3 = new ReportParameter("CompanyPhone", reportData.Master?.COMPANY_PHONE ?? "");
                            ReportParameter parameter4 = new ReportParameter("Header", reportData.Master?.HEADER_NAME ?? "Stitching Order");
                            ReportParameter parameter5 = new ReportParameter("CompanyLogo", companyLogoUri);
                            ReportParameter parameter6 = new ReportParameter("ShowCompanyLogo", Convert.ToString(showCompanyLogo));

                            report.SetParameters(new ReportParameter[] { parameter1, parameter2, parameter3, parameter4, parameter5, parameter6 });
                            report.Refresh();
                            report.DataSources.Add(new ReportDataSource() { Name = "StichingOrder", Value = reportData.Detail });
                            byte[] file;
                            string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, @"Client\StichingOrderReport");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            if (!report.IsReadyForRendering)
                            {
                                throw new Exception("Report is not ready for printing. Please verify report parameters and dataset.");
                            }
                            file = report.Render("PDF");
                            filePath = $"SO - {reportData.Master?.SERIAL_ID}" + ".pdf";

                            stReader.Close();
                            stReader.Dispose();
                            stream.Flush();
                            stream.Close();
                            stream.Dispose();
                            report.Dispose();
                            string reportPath = Path.Combine(uploadsFolder, filePath);
                            System.IO.File.WriteAllBytes(reportPath, file);
                            filePath = $"/Client/StichingOrderReport/{filePath}";
                        }
                    }
                }
            }
            return filePath;
        }

        private DataTable CreateStichingOrderPrintTable()
        {
            DataTable table = new DataTable("StichingOrder");
            table.Columns.Add("SerialID", typeof(string));
            table.Columns.Add("CustomerName", typeof(string));
            table.Columns.Add("ContactNo", typeof(string));
            table.Columns.Add("SuitType", typeof(string));
            table.Columns.Add("Design", typeof(string));
            table.Columns.Add("Amount", typeof(string));
            table.Columns.Add("OrderDate", typeof(string));
            table.Columns.Add("DeliveryDate", typeof(string));
            table.Columns.Add("KurtaLength", typeof(string));
            table.Columns.Add("Shoulder", typeof(string));
            table.Columns.Add("Sleeves", typeof(string));
            table.Columns.Add("Chest", typeof(string));
            table.Columns.Add("Waist", typeof(string));
            table.Columns.Add("HipSize", typeof(string));
            table.Columns.Add("CollarSize", typeof(string));
            table.Columns.Add("Armhole", typeof(string));
            table.Columns.Add("CuffMori", typeof(string));
            table.Columns.Add("BottomType", typeof(string));
            table.Columns.Add("BottomStyle", typeof(string));
            table.Columns.Add("BottomLength", typeof(string));
            table.Columns.Add("Pancha", typeof(string));
            table.Columns.Add("AsanGhera", typeof(string));
            table.Columns.Add("DamanStyle", typeof(string));
            table.Columns.Add("GalaStyle", typeof(string));
            table.Columns.Add("PattiStyle", typeof(string));
            table.Columns.Add("FrontPocket", typeof(string));
            table.Columns.Add("SidePockets", typeof(string));
            table.Columns.Add("FittingStyle", typeof(string));
            table.Columns.Add("BottomPocket", typeof(string));
            table.Columns.Add("Logo", typeof(string));
            table.Columns.Add("ClothQty", typeof(string));
            table.Columns.Add("ClothRate", typeof(string));
            table.Columns.Add("Brand", typeof(string));
            table.Columns.Add("StichQty", typeof(string));
            table.Columns.Add("StichRate", typeof(string));
            table.Columns.Add("StichAmt", typeof(string));
            table.Columns.Add("NetAmount", typeof(string));
            return table;
        }
    }
}
