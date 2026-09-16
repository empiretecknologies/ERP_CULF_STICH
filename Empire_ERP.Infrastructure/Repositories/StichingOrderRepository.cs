using Empire_ERP.Core.Entities;
using Empire_ERP.Core.Interfaces;
using Empire_ERP.Core.Services;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Empire_ERP.Infrastructure.Repositories
{
    public class StichingOrderRepository : IStichingOrderRepository
    {
        public MyHttpResponseMessage QuickSearch(Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                List<object> jsonDataResult = new List<object>();
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT K.ID AS ORDER_ID,C.ID,K.CUSTOMER_ID,C.FULL_NAME,C.CONTACT_NO,ISNULL(ST.GROUP_NAME, K.SUIT_TYPE) AS SUIT_TYPE,K.ADD_USER_ID," +
                                   "K.ADD_DATE,K.ADD_COMPUTER_NAME,K.ADD_IP_ADDRESS,K.EDIT_USER_ID," +
                                   "K.EDIT_DATE,K.EDIT_COMPUTER_NAME,K.ADD_POSTALCODE," +
                                   "K.EDIT_POSTALCODE " +
                                   "FROM TBL_KURTASHALWAR K " +
                                   "INNER JOIN TBL_WALKCUSTOMER C ON C.ID = K.CUSTOMER_ID AND C.DLT = 'T' " +
                                   "LEFT OUTER JOIN TBL_SUITTYPE ST ON CONVERT(VARCHAR, ST.GROUP_CODE) = K.SUIT_TYPE " +
                                   "WHERE K.MENU_ID = '" + common.MenuID + "' AND K.DLT = 'T' ORDER BY K.ID DESC";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var row = new StichingOrder
                        {
                            ORDER_ID = Convert.ToInt32(reader["ORDER_ID"]),
                            ID = Convert.ToInt32(reader["ID"]),
                            CUSTOMER_ID = reader["CUSTOMER_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["CUSTOMER_ID"]),
                            FULL_NAME = Convert.ToString(reader["FULL_NAME"]),
                            CONTACT_NO = Convert.ToString(reader["CONTACT_NO"]),
                            SUIT_TYPE = Convert.ToString(reader["SUIT_TYPE"]),
                            ADD_USER_ID = Convert.ToString(reader["ADD_USER_ID"]),
                            ADD_DATE = reader["ADD_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["ADD_DATE"]),
                            ADD_COMPUTER_NAME = Convert.ToString(reader["ADD_COMPUTER_NAME"]),
                            ADD_IP_ADDRESS = Convert.ToString(reader["ADD_IP_ADDRESS"]),
                            EDIT_USER_ID = Convert.ToString(reader["EDIT_USER_ID"]),
                            EDIT_DATE = reader["EDIT_DATE"] == DBNull.Value ? null : Convert.ToDateTime(reader["EDIT_DATE"]),
                            EDIT_COMPUTER_NAME = Convert.ToString(reader["EDIT_COMPUTER_NAME"]),
                            ADD_POSTALCODE = Convert.ToString(reader["ADD_POSTALCODE"]),
                            EDIT_POSTALCODE = Convert.ToString(reader["EDIT_POSTALCODE"]),
                        };

                        jsonDataResult.Add(row);
                    }
                    reader.Close();

                    response.data = jsonDataResult;
                    response.msg = "";
                    response.msgType = 1;
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
            return response;
        }

        public MyHttpResponseMessage Save(StichingOrder modelRecord, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                var Ip = common.IPAddress;
                var Computer = common.ComputerName;
                var Postal = common.PostalCode;
                var userid = common.Username;
                string connectionString = new SQLService().getconnstring();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;
                    try
                    {
                        string customerId = Convert.ToString(modelRecord.ID);
                        bool isNewCustomer = modelRecord.ID == null || modelRecord.ID == 0;
                        bool isNewOrder = modelRecord.ORDER_ID == null || modelRecord.ORDER_ID == 0;

                        if (isNewCustomer)
                        {
                            customerId = GenerateNextIdByTable(command, "TBL_WALKCUSTOMER");
                            modelRecord.ID = Convert.ToInt32(customerId);
                            command.CommandText = "INSERT INTO TBL_WALKCUSTOMER " +
                                        "(ID,FULL_NAME,CONTACT_NO,ADD_USER_ID,ADD_DATE," +
                                        "ADD_COMPUTER_NAME,ADD_IP_ADDRESS,EDIT_USER_ID,EDIT_DATE," +
                                        "EDIT_COMPUTER_NAME,ADD_POSTALCODE,EDIT_POSTALCODE," +
                                        "MENU_ID,DLT)" +
                                        "VALUES" +
                                        "('" + customerId + "','" + modelRecord.FULL_NAME + "','" + modelRecord.CONTACT_NO + "','" + userid + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                        "'" + Computer + "','" + Ip + "','" + userid + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                                        "'" + Computer + "','" + Postal + "','" + Postal + "'," +
                                        "'" + common.MenuID + "','T')";
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            command.CommandText = "SELECT COUNT(*) FROM TBL_WALKCUSTOMER WHERE ID = '" + customerId + "' AND DLT = 'T'";
                            int customerCount = (int)command.ExecuteScalar();
                            if (customerCount == 0)
                            {
                                transaction.Rollback();
                                response.msg = "Data not found in our records";
                                response.msgType = 2;
                                return response;
                            }

                            command.CommandText = "UPDATE TBL_WALKCUSTOMER SET FULL_NAME = '" + modelRecord.FULL_NAME + @"',
                                    CONTACT_NO = '" + modelRecord.CONTACT_NO + @"',
                                    EDIT_USER_ID = '" + userid + @"',
                                    EDIT_DATE = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
                                    EDIT_COMPUTER_NAME = '" + Computer + @"',
                                    EDIT_POSTALCODE = '" + Postal + @"'
                                    WHERE ID = '" + customerId + "'";
                            command.ExecuteNonQuery();
                        }

                        modelRecord.CUSTOMER_ID = Convert.ToInt32(customerId);

                        if (isNewOrder)
                        {
                            string orderId = GenerateNextIdByTable(command, "TBL_KURTASHALWAR");
                            modelRecord.ORDER_ID = Convert.ToInt32(orderId);
                            command.CommandText = GetKurtaInsertQuery(orderId, customerId, modelRecord, common, userid, Computer, Ip, Postal);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            response.msgType = 1;
                            response.msg = "Record Added Successfully";
                        }
                        else
                        {
                            string orderId = Convert.ToString(modelRecord.ORDER_ID);
                            command.CommandText = GetKurtaUpdateQuery(orderId, customerId, modelRecord, common, userid, Computer, Postal);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            response.msgType = 1;
                            response.msg = "Record Updated Successfully";
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string _catchMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            _catchMessage += "<br/>" + ex.InnerException.Message;
                        }
                        response.msg = _catchMessage;
                        response.msgType = 2;
                    }
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
            return response;
        }

        public string GenerateNextId(Common common)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    return GenerateNextIdByTable(command, "TBL_WALKCUSTOMER");
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        private string GenerateNextIdByTable(SqlCommand command, string tableName)
        {
            command.CommandText = "SELECT ISNULL(MAX(ID), 0) + 1 FROM " + tableName;
            object result = command.ExecuteScalar();
            return Convert.ToString(Convert.ToInt32(result));
        }

        public MyHttpResponseMessage GetCustomerById(int id, Common common)
        {
            return GetCustomerWithLatestOrder(common, "AND C.ID = '" + id + "'");
        }

        public MyHttpResponseMessage GetCustomerByContactNo(string contactNo, Common common)
        {
            return GetCustomerWithLatestOrder(common, "AND C.CONTACT_NO = '" + contactNo + "'");
        }

        private MyHttpResponseMessage GetCustomerWithLatestOrder(Common common, string extraWhere)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT TOP 1 C.ID,C.FULL_NAME,C.CONTACT_NO,K.CUSTOMER_ID," +
                                   "K.SUIT_TYPE,K.KURTA_LENGTH,K.SHOULDER,K.SLEEVES,K.CHEST,K.WAIST," +
                                   "K.COLLAR_SIZE,K.ARMHOLE,K.CUFF_MORI,K.BOTTOM_TYPE,K.BOTTOM_LENGTH," +
                                   "K.PANCHA,K.ASAN_GHERA,K.DAMAN_STYLE,K.GALA_STYLE,K.PATTI_STYLE," +
                                   "K.FRONT_POCKET,K.SIDE_POCKETS,K.FITTING_STYLE " +
                                   "FROM TBL_WALKCUSTOMER C " +
                                   "LEFT JOIN TBL_KURTASHALWAR K ON K.ID = (" +
                                   "SELECT TOP 1 K2.ID FROM TBL_KURTASHALWAR K2 " +
                                   "WHERE K2.CUSTOMER_ID = C.ID AND K2.DLT = 'T' " +
                                   "ORDER BY K2.ID DESC) " +
                                   "WHERE C.MENU_ID = '" + common.MenuID + "' AND C.DLT = 'T' " + extraWhere +
                                   " ORDER BY C.ID DESC";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var record = new StichingOrder
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            CUSTOMER_ID = reader["CUSTOMER_ID"] == DBNull.Value ? Convert.ToInt32(reader["ID"]) : Convert.ToInt32(reader["CUSTOMER_ID"]),
                            FULL_NAME = Convert.ToString(reader["FULL_NAME"]),
                            CONTACT_NO = Convert.ToString(reader["CONTACT_NO"]),
                            SUIT_TYPE = Convert.ToString(reader["SUIT_TYPE"]),
                            KURTA_LENGTH = GetDouble(reader["KURTA_LENGTH"]),
                            SHOULDER = GetDouble(reader["SHOULDER"]),
                            SLEEVES = GetDouble(reader["SLEEVES"]),
                            CHEST = GetDouble(reader["CHEST"]),
                            WAIST = GetDouble(reader["WAIST"]),
                            COLLAR_SIZE = GetDouble(reader["COLLAR_SIZE"]),
                            ARMHOLE = GetDouble(reader["ARMHOLE"]),
                            CUFF_MORI = GetDouble(reader["CUFF_MORI"]),
                            BOTTOM_TYPE = Convert.ToString(reader["BOTTOM_TYPE"]),
                            BOTTOM_LENGTH = GetDouble(reader["BOTTOM_LENGTH"]),
                            PANCHA = GetDouble(reader["PANCHA"]),
                            ASAN_GHERA = GetDouble(reader["ASAN_GHERA"]),
                            DAMAN_STYLE = Convert.ToString(reader["DAMAN_STYLE"]),
                            GALA_STYLE = Convert.ToString(reader["GALA_STYLE"]),
                            PATTI_STYLE = Convert.ToString(reader["PATTI_STYLE"]),
                            FRONT_POCKET = Convert.ToString(reader["FRONT_POCKET"]),
                            SIDE_POCKETS = Convert.ToString(reader["SIDE_POCKETS"]),
                            FITTING_STYLE = Convert.ToString(reader["FITTING_STYLE"]),
                        };

                        response.msg = "";
                        response.msgType = 1;
                        response.data = record;
                    }
                    else
                    {
                        response.msg = "Data not found in our records";
                        response.msgType = 2;
                    }
                    reader.Close();
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
            return response;
        }

        public MyHttpResponseMessage GetOrderById(int id, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            try
            {
                using (SqlConnection connection = new SqlConnection(new SQLService().getconnstring()))
                {
                    string query = "SELECT K.ID AS ORDER_ID,C.ID,K.CUSTOMER_ID,C.FULL_NAME,C.CONTACT_NO," +
                                   "K.SUIT_TYPE,K.KURTA_LENGTH,K.SHOULDER,K.SLEEVES,K.CHEST,K.WAIST," +
                                   "K.COLLAR_SIZE,K.ARMHOLE,K.CUFF_MORI,K.BOTTOM_TYPE,K.BOTTOM_LENGTH," +
                                   "K.PANCHA,K.ASAN_GHERA,K.DAMAN_STYLE,K.GALA_STYLE,K.PATTI_STYLE," +
                                   "K.FRONT_POCKET,K.SIDE_POCKETS,K.FITTING_STYLE " +
                                   "FROM TBL_KURTASHALWAR K " +
                                   "INNER JOIN TBL_WALKCUSTOMER C ON C.ID = K.CUSTOMER_ID AND C.DLT = 'T' " +
                                   "WHERE K.MENU_ID = '" + common.MenuID + "' AND K.DLT = 'T' AND K.ID = '" + id + "'";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        var record = new StichingOrder
                        {
                            ORDER_ID = Convert.ToInt32(reader["ORDER_ID"]),
                            ID = Convert.ToInt32(reader["ID"]),
                            CUSTOMER_ID = reader["CUSTOMER_ID"] == DBNull.Value ? null : Convert.ToInt32(reader["CUSTOMER_ID"]),
                            FULL_NAME = Convert.ToString(reader["FULL_NAME"]),
                            CONTACT_NO = Convert.ToString(reader["CONTACT_NO"]),
                            SUIT_TYPE = Convert.ToString(reader["SUIT_TYPE"]),
                            KURTA_LENGTH = GetDouble(reader["KURTA_LENGTH"]),
                            SHOULDER = GetDouble(reader["SHOULDER"]),
                            SLEEVES = GetDouble(reader["SLEEVES"]),
                            CHEST = GetDouble(reader["CHEST"]),
                            WAIST = GetDouble(reader["WAIST"]),
                            COLLAR_SIZE = GetDouble(reader["COLLAR_SIZE"]),
                            ARMHOLE = GetDouble(reader["ARMHOLE"]),
                            CUFF_MORI = GetDouble(reader["CUFF_MORI"]),
                            BOTTOM_TYPE = Convert.ToString(reader["BOTTOM_TYPE"]),
                            BOTTOM_LENGTH = GetDouble(reader["BOTTOM_LENGTH"]),
                            PANCHA = GetDouble(reader["PANCHA"]),
                            ASAN_GHERA = GetDouble(reader["ASAN_GHERA"]),
                            DAMAN_STYLE = Convert.ToString(reader["DAMAN_STYLE"]),
                            GALA_STYLE = Convert.ToString(reader["GALA_STYLE"]),
                            PATTI_STYLE = Convert.ToString(reader["PATTI_STYLE"]),
                            FRONT_POCKET = Convert.ToString(reader["FRONT_POCKET"]),
                            SIDE_POCKETS = Convert.ToString(reader["SIDE_POCKETS"]),
                            FITTING_STYLE = Convert.ToString(reader["FITTING_STYLE"]),
                        };

                        response.msg = "";
                        response.msgType = 1;
                        response.data = record;
                    }
                    else
                    {
                        response.msg = "Data not found in our records";
                        response.msgType = 2;
                    }
                    reader.Close();
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
            return response;
        }

        public MyHttpResponseMessage Delete(int id, Common common)
        {
            MyHttpResponseMessage response = new MyHttpResponseMessage();
            response.msg = "Data not found in our records";
            try
            {
                if (id == 0)
                {
                    response.msg = "ID is not in numeric format";
                    response.msgType = 2;
                }
                else
                {
                    string connectionString = new SQLService().getconnstring();
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = connection.CreateCommand();
                        command.CommandText = "UPDATE TBL_KURTASHALWAR SET DLT = 'F' WHERE ID = '" + id + "'";
                        command.ExecuteNonQuery();
                        response.msg = "Record Deleted Successfully";
                        response.msgType = 1;
                    }
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
            return response;
        }

        private string GetKurtaInsertQuery(string id, string customerId, StichingOrder modelRecord, Common common, string userid, string Computer, string Ip, string Postal)
        {
            return "INSERT INTO TBL_KURTASHALWAR " +
                   "(ID,CUSTOMER_ID,SUIT_TYPE,KURTA_LENGTH,SHOULDER,SLEEVES,CHEST,WAIST,COLLAR_SIZE,ARMHOLE,CUFF_MORI," +
                   "BOTTOM_TYPE,BOTTOM_LENGTH,PANCHA,ASAN_GHERA,DAMAN_STYLE,GALA_STYLE,PATTI_STYLE," +
                   "FRONT_POCKET,SIDE_POCKETS,FITTING_STYLE,ADD_USER_ID,ADD_DATE," +
                   "ADD_COMPUTER_NAME,ADD_IP_ADDRESS,EDIT_USER_ID,EDIT_DATE," +
                   "EDIT_COMPUTER_NAME,ADD_POSTALCODE,EDIT_POSTALCODE," +
                   "MENU_ID,DLT)" +
                   "VALUES" +
                   "('" + id + "','" + customerId + "','" + modelRecord.SUIT_TYPE + "'," + FormatNumber(modelRecord.KURTA_LENGTH) + "," + FormatNumber(modelRecord.SHOULDER) + "," + FormatNumber(modelRecord.SLEEVES) + "," + FormatNumber(modelRecord.CHEST) + "," + FormatNumber(modelRecord.WAIST) + "," + FormatNumber(modelRecord.COLLAR_SIZE) + "," + FormatNumber(modelRecord.ARMHOLE) + "," + FormatNumber(modelRecord.CUFF_MORI) + "," +
                   "'" + modelRecord.BOTTOM_TYPE + "'," + FormatNumber(modelRecord.BOTTOM_LENGTH) + "," + FormatNumber(modelRecord.PANCHA) + "," + FormatNumber(modelRecord.ASAN_GHERA) + ",'" + modelRecord.DAMAN_STYLE + "','" + modelRecord.GALA_STYLE + "','" + modelRecord.PATTI_STYLE + "'," +
                   "'" + modelRecord.FRONT_POCKET + "','" + modelRecord.SIDE_POCKETS + "','" + modelRecord.FITTING_STYLE + "','" + userid + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                   "'" + Computer + "','" + Ip + "','" + userid + "','" + CommonService.GetDateTime("Pakistan Standard Time") + "'," +
                   "'" + Computer + "','" + Postal + "','" + Postal + "'," +
                   "'" + common.MenuID + "','T')";
        }

        private string GetKurtaUpdateQuery(string id, string customerId, StichingOrder modelRecord, Common common, string userid, string Computer, string Postal)
        {
            return "UPDATE TBL_KURTASHALWAR SET CUSTOMER_ID = '" + customerId + @"',
                    SUIT_TYPE = '" + modelRecord.SUIT_TYPE + @"',
                    KURTA_LENGTH = " + FormatNumber(modelRecord.KURTA_LENGTH) + @",
                    SHOULDER = " + FormatNumber(modelRecord.SHOULDER) + @",
                    SLEEVES = " + FormatNumber(modelRecord.SLEEVES) + @",
                    CHEST = " + FormatNumber(modelRecord.CHEST) + @",
                    WAIST = " + FormatNumber(modelRecord.WAIST) + @",
                    COLLAR_SIZE = " + FormatNumber(modelRecord.COLLAR_SIZE) + @",
                    ARMHOLE = " + FormatNumber(modelRecord.ARMHOLE) + @",
                    CUFF_MORI = " + FormatNumber(modelRecord.CUFF_MORI) + @",
                    BOTTOM_TYPE = '" + modelRecord.BOTTOM_TYPE + @"',
                    BOTTOM_LENGTH = " + FormatNumber(modelRecord.BOTTOM_LENGTH) + @",
                    PANCHA = " + FormatNumber(modelRecord.PANCHA) + @",
                    ASAN_GHERA = " + FormatNumber(modelRecord.ASAN_GHERA) + @",
                    DAMAN_STYLE = '" + modelRecord.DAMAN_STYLE + @"',
                    GALA_STYLE = '" + modelRecord.GALA_STYLE + @"',
                    PATTI_STYLE = '" + modelRecord.PATTI_STYLE + @"',
                    FRONT_POCKET = '" + modelRecord.FRONT_POCKET + @"',
                    SIDE_POCKETS = '" + modelRecord.SIDE_POCKETS + @"',
                    FITTING_STYLE = '" + modelRecord.FITTING_STYLE + @"',
                    EDIT_USER_ID = '" + userid + @"',
                    EDIT_DATE = '" + CommonService.GetDateTime("Pakistan Standard Time") + @"',
                    EDIT_COMPUTER_NAME = '" + Computer + @"',
                    EDIT_POSTALCODE = '" + Postal + @"',
                    DLT = 'T'
                    WHERE ID = '" + id + "'";
        }

        private string FormatNumber(double? value)
        {
            return value == null ? "NULL" : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private double? GetDouble(object value)
        {
            return value == null || value == DBNull.Value ? null : Convert.ToDouble(value);
        }
    }
}
