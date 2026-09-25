using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using System.Data;

namespace Empire_ERP.Core.Services
{
    public class StichingOrderService : IStichingOrderService
    {
        public IStichingOrderRepository _stichingOrderRepository { get; set; }
        public IMenuService _menuService { get; set; }
        public ICompanyService _companyService { get; set; }
        public IBranchService _branchService { get; set; }
        public StichingOrderService(IStichingOrderRepository stichingOrderRepository, IMenuService menuService, ICompanyService companyService, IBranchService branchService)
        {
            _stichingOrderRepository = stichingOrderRepository;
            _menuService = menuService;
            _companyService = companyService;
            _branchService = branchService;
        }

        public MyHttpResponseMessage QuickSearch(Common common)
        {
            return _stichingOrderRepository.QuickSearch(common);
        }

        public MyHttpResponseMessage Save(StichingOrder model, Common common)
        {
            return _stichingOrderRepository.Save(model, common);
        }

        public string GenerateNextId(Common common)
        {
            return _stichingOrderRepository.GenerateNextId(common);
        }

        public MyHttpResponseMessage GetCustomerById(int id, Common common)
        {
            return _stichingOrderRepository.GetCustomerById(id, common);
        }

        public MyHttpResponseMessage GetCustomerByContactNo(string contactNo, Common common)
        {
            return _stichingOrderRepository.GetCustomerByContactNo(contactNo, common);
        }

        public MyHttpResponseMessage GetOrderById(int id, Common common)
        {
            return _stichingOrderRepository.GetOrderById(id, common);
        }

        public MyHttpResponseMessage GetOrderByRef(string refNo, Common common)
        {
            return _stichingOrderRepository.GetOrderByRef(refNo, common);
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _stichingOrderRepository.Delete(id, common);
        }

        public MyHttpResponseMessage GetDataForPrintReport(RDLCReport modelRecord, DataTable dataTable, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            CustomMenuDetail menuDetail = new CustomMenuDetail();

            try
            {
                var menuResponse = _menuService.GetMenuDetails(common.MenuID);
                var currentCompanyResponse = _companyService.GetCompanyByCode(common.Company);
                if (currentCompanyResponse.msgType != 1)
                {
                    response.msgType = 2;
                    return response;
                }

                var currentBranchResponse = _branchService.GetBranchByCode(common.Branch);
                if (currentBranchResponse.msgType != 1)
                {
                    response.msgType = 2;
                    return response;
                }

                if (menuResponse.msgType == 1 && menuResponse.data != null)
                {
                    var menuData = (List<CustomMenuDetail>)menuResponse.data;
                    if (menuData != null && menuData.Count > 0)
                    {
                        if (modelRecord.MD_ID > 0)
                        {
                            menuDetail = menuData.Where(m => m.MD_ID == modelRecord.MD_ID).FirstOrDefault();
                        }
                        if (menuDetail == null || (menuDetail.MD_ID ?? 0) <= 0)
                        {
                            menuDetail = menuData.FirstOrDefault();
                        }
                    }
                }

                var currentBranch = (Branch)currentBranchResponse.data;
                var currentCompany = (Company)currentCompanyResponse.data;
                if (menuDetail == null)
                {
                    menuDetail = new CustomMenuDetail();
                }

                return _stichingOrderRepository.GetDataForPrintReport(modelRecord, dataTable, menuDetail, currentCompany, currentBranch, common);
            }
            catch (Exception ex)
            {
                string _catchMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    _catchMessage += "<br/>" + ex.InnerException.Message;
                }
                response.msgType = 2;
                response.msg = _catchMessage;
            }
            return response;
        }
    }
}
