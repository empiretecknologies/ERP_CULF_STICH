using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Empire_ERP.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Empire_ERP.Controllers
{
    [CheckSession]
    [ExtractMenuCode]
    public class StichingOrderController : BaseController
    {
        public IStichingOrderService _stichingOrderService { get; set; }
        public StichingOrderController(IStichingOrderService stichingOrderService, IMenuService menuService, IBaseService baseService) : base(menuService, baseService)
        {
            _stichingOrderService = stichingOrderService;
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
    }
}
