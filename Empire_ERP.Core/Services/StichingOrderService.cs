using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;

namespace Empire_ERP.Core.Services
{
    public class StichingOrderService : IStichingOrderService
    {
        public IStichingOrderRepository _stichingOrderRepository { get; set; }
        public StichingOrderService(IStichingOrderRepository stichingOrderRepository)
        {
            _stichingOrderRepository = stichingOrderRepository;
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

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            return _stichingOrderRepository.Delete(id, common);
        }
    }
}
