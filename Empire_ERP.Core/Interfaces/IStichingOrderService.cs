using Empire_ERP.Core.Entities;
using System.Data;

namespace Empire_ERP.Core.Interfaces
{
    public interface IStichingOrderService
    {
        MyHttpResponseMessage QuickSearch(Common common);
        MyHttpResponseMessage GetCustomerById(int id, Common common);
        MyHttpResponseMessage GetCustomerByContactNo(string contactNo, Common common);
        MyHttpResponseMessage GetOrderById(int id, Common common);
        MyHttpResponseMessage GetOrderByRef(string refNo, Common common);
        MyHttpResponseMessage Save(StichingOrder model, Common common);
        string GenerateNextId(Common common);
        MyHttpResponseMessage Delete(int id, Common common);
        MyHttpResponseMessage GetDataForPrintReport(RDLCReport modelRecord, DataTable details, Common common);
    }
}
