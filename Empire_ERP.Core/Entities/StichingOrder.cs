using System.Data;

namespace Empire_ERP.Core.Entities
{
    public class StichingOrder
    {
        public int? ID { get; set; }
        public int? ORDER_ID { get; set; }
        public int? CUSTOMER_ID { get; set; }
        public string? FULL_NAME { get; set; }
        public string? CONTACT_NO { get; set; }
        public string? SUIT_TYPE { get; set; }
        public double? KURTA_LENGTH { get; set; }
        public double? SHOULDER { get; set; }
        public double? SLEEVES { get; set; }
        public double? CHEST { get; set; }
        public double? WAIST { get; set; }
        public double? HIP_SIZE { get; set; }
        public double? COLLAR_SIZE { get; set; }
        public double? ARMHOLE { get; set; }
        public double? CUFF_MORI { get; set; }
        public string? BOTTOM_TYPE { get; set; }
        public double? BOTTOM_LENGTH { get; set; }
        public double? PANCHA { get; set; }
        public double? ASAN_GHERA { get; set; }
        public string? DAMAN_STYLE { get; set; }
        public string? GALA_STYLE { get; set; }
        public string? PATTI_STYLE { get; set; }
        public string? FRONT_POCKET { get; set; }
        public string? SIDE_POCKETS { get; set; }
        public string? FITTING_STYLE { get; set; }
        public string? BOTTOM_POCKET { get; set; }
        public string? LOGO { get; set; }
        public double? QTY { get; set; }
        public double? RATE { get; set; }
        public string? BRAND { get; set; }
        public double? AMOUNT { get; set; }
        public DateTime? DEL_DATE { get; set; }
        public string? ADD_USER_ID { get; set; }
        public DateTime? ADD_DATE { get; set; }
        public string? ADD_COMPUTER_NAME { get; set; }
        public string? ADD_IP_ADDRESS { get; set; }
        public string? EDIT_USER_ID { get; set; }
        public DateTime? EDIT_DATE { get; set; }
        public string? EDIT_COMPUTER_NAME { get; set; }
        public string? ADD_POSTALCODE { get; set; }
        public string? EDIT_POSTALCODE { get; set; }
        public int? MENU_ID { get; set; }
        public string? DLT { get; set; }
    }

    public class StichingOrderForPrint
    {
        public string? COMPANY_NAME { get; set; }
        public string? COMPANY_ADDRESS { get; set; }
        public string? COMPANY_PHONE { get; set; }
        public string? COMPANY_LOGO { get; set; }
        public string? HEADER_NAME { get; set; }
        public string? REPORT_NAME { get; set; }
        public bool? MENU_SIG1 { get; set; }
        public bool? MENU_SIG2 { get; set; }
        public bool? MENU_SIG3 { get; set; }
        public bool? MENU_SIG4 { get; set; }
        public string? SERIAL_ID { get; set; }
    }

    public class CustomStichingOrderForPrintReport
    {
        public StichingOrderForPrint? Master { get; set; }
        public DataTable Detail { get; set; }
    }
}
