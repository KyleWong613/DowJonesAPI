using CRM_API;
using CRM_API.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Threading;
using CRM_API.EventObject;
using CRM_API.MessageObject;
using System.Data;
using System.Configuration;

namespace MQ_Stress.Models
{
    public class DBSimulationHelper
    {
        string ConnectionString = ProjectConstant.CONNECTIONSTRING;
        string WanderQueueManagerIPAdress = ProjectConstant.WanderQueueManagerIPAdress;
        string WanderQueueManagerPort = ProjectConstant.WanderQueueManagerPort;
        string WanderQueueManagerChannel = ProjectConstant.WanderQueueManagerChannel;
        string WanderQueueManagerName = ProjectConstant.WanderQueueManagerName;
        string WanderQueueSendName = ProjectConstant.WanderQueueSendName;
        string WanderQueueReceiveName = ProjectConstant.WanderQueueReceiveName;

        string WanderCoreQueueManagerPort = ProjectConstant.WanderCoreQueueManagerPort;
        string WanderCoreQueueManagerChannel = ProjectConstant.WanderCoreQueueManagerChannel;
        string WanderCoreQueueManagerName = ProjectConstant.WanderCoreQueueManagerName;
        string WanderCoreQueueSendName = ProjectConstant.WanderCoreQueueSendName;
        string WanderCoreQueueReceiveName = ProjectConstant.WanderCoreQueueReceiveName;

        string AMPQueueManagerIPAdress = ProjectConstant.AMPQueueManagerIPAdress;
        string AMPQueueManagerPort = ProjectConstant.AMPQueueManagerPort;
        string AMPQueueManagerChannel = ProjectConstant.AMPQueueManagerChannel;
        string AMPQueueManagerName = ProjectConstant.AMPQueueManagerName;
        string AMPQueueSendName = ProjectConstant.AMPQueueSendName;
        string AMPQueueReceiveName = ProjectConstant.AMPQueueReceiveName; 
        public string InsertNewAPIRequest(string requestType, string requestBody, string MQType)
        {
            string requestId = string.Empty;
            string projectType = string.Empty;
            try
            {
                if (MQType == "EwalletnWebMQ")
                {
                    projectType = ConnectionString;
                }
                
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(projectType))
                {
                    string sqlInsert = "INSERT INTO tblPutQueue (Request_Type, Request_Body, Request_Time, Status) " +
                        "OUTPUT INSERTED.RowId " +
                        "values(@requestType, @requestBody, GETDATE(), @status); ";

                    //add records           
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sqlInsert))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Connection = connection;

                        cmd.Parameters.AddWithValue("@requestType", requestType);
                        cmd.Parameters.AddWithValue("@requestBody", requestBody);
                        cmd.Parameters.AddWithValue("@status", 0);
                        connection.Open();
                        requestId = cmd.ExecuteScalar().ToString();//.ExecuteNonQuery();
                    }
                }



            }
            catch (Exception ex)
            {
            }

            return requestId;
        }
        public string GetRequestTime(string RequestId, string MQType)
        {
            string projectType = string.Empty;
            projectType = ConnectionString;
            string reqtime = "";
            DataTable dtblTask = new DataTable();
            try
            {
                //init drop down
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(projectType))
                {
                    string sql = "select top 1 Request_Time from tblPutQueue where RowId = @requestId " +
                        "order by RowId desc";

                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@requestId", RequestId);//pending status
                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(command))
                        {
                            da.Fill(dtblTask);
                        }
                    }
                }

                if (dtblTask.Rows.Count > 0)
                {
                    reqtime = dtblTask.Rows[0]["Request_Time"].ToString();
                }
            }
            catch (Exception ex)
            {
            }

            dtblTask.Dispose();
            return reqtime;
        }
        public void GetResponse(MsgObjResponse responseObj)
        {
            DataTable dtblTask = new DataTable();
            try
            {
                //init drop down
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    string sql = "select top 1 * from tblGetQueue where MQ_Put_RowId = @requestId " +
                        "order by RowId desc";

                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@requestId", responseObj.RequestId);//pending status
                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(command))
                        {
                            da.Fill(dtblTask);
                        }
                    }
                }

                if (dtblTask.Rows.Count > 0)
                {
                    responseObj.RequestResult = dtblTask.Rows[0]["Response_Result"].ToString();
                    responseObj.Content = dtblTask.Rows[0]["Response_Body"].ToString();
                }
            }
            catch (Exception ex)
            {
            }

            dtblTask.Dispose();
        }
        public string GetResponseTime(string RequestId, string MQType)
        {
            string projectType = string.Empty;
            projectType = ConnectionString;
            string resptime = "";
            DataTable dtblTask = new DataTable();
            try
            {
                //init drop down
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(projectType))
                {
                    string sql = "select top 1 Response_Time from tblGetQueue where MQ_Put_RowId = @requestId " +
                        "order by RowId desc";

                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@requestId", RequestId);//pending status
                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(command))
                        {
                            da.Fill(dtblTask);
                        }
                    }
                }

                if (dtblTask.Rows.Count > 0)
                {
                    resptime = dtblTask.Rows[0]["Response_Time"].ToString();
                }
            }
            catch (Exception ex)
            {
            }

            dtblTask.Dispose();
            return resptime;
        }
        public string GetResponseBody(string RequestId, string MQType)
        {
            string projectType = string.Empty;
            projectType = ConnectionString;
            string respbody = "";
            DataTable dtblTask = new DataTable();
            try
            {
                //init drop down
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(projectType))
                {
                    string sql = "select top 1 Response_Body from tblGetQueue where MQ_Put_RowId = @requestId " +
                        "order by RowId desc";

                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        command.Parameters.AddWithValue("@requestId", RequestId);//pending status
                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(command))
                        {
                            da.Fill(dtblTask);
                        }
                    }
                }

                if (dtblTask.Rows.Count > 0)
                {
                    respbody = dtblTask.Rows[0]["Response_Body"].ToString();
                }
            }
            catch (Exception ex)
            {
            }

            dtblTask.Dispose();
            return respbody;
        }
//        public void GetPendingEventRequest(EventMsgObj eventObj)
//        {
//            DataTable dtblTask = new DataTable();
//            try
//            {
//                //init drop down
//                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["WanderMQConnectionString"].ConnectionString;
//))
//                {
//                    //string sql = "UPDATE TOP (1) tblEventReqQueue SET Status = 1, Process_Time = GETDATE() " +
//                    //    "OUTPUT INSERTED.Event_Id, INSERTED.Row_Id, INSERTED.Event_Body " +
//                    //    "where status = @status AND Event_Type = @Event_Type";


//                    string sql = "UPDATE tblEventReqQueue SET Status = 1, Process_Time = GETDATE() " +
//                        "OUTPUT INSERTED.Event_Id, INSERTED.Row_Id, INSERTED.Event_Body " +
//                        "where row_id = (select top (1) Row_Id from tblEventReqQueue where status = @status AND Event_Type = @Event_Type order by row_id asc)";

//                    using (System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(sql, connection))
//                    {
//                        command.CommandType = CommandType.Text;
//                        command.Parameters.AddWithValue("@status", 0);//pending status
//                        command.Parameters.AddWithValue("@Event_Type", eventObj.GetEventType());//request type
//                        using (System.Data.SqlClient.SqlDataAdapter da = new System.Data.SqlClient.SqlDataAdapter(command))
//                        {
//                            da.Fill(dtblTask);
//                        }
//                    }
//                }

//                if (dtblTask.Rows.Count > 0)
//                {
//                    eventObj.ID = dtblTask.Rows[0]["Row_Id"].ToString();
//                    eventObj.EventID = dtblTask.Rows[0]["Event_Id"].ToString().Trim();
//                    eventObj.Content = dtblTask.Rows[0]["Event_Body"].ToString();
//                }
//            }
//            catch (Exception ex)
//            {
//            }

//            dtblTask.Dispose();
//        }

        public void InsertEventResponse(string eventId, string result, string body)
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(ConnectionString))
                {
                    string sqlInsert = "INSERT INTO tblEventResQueue (Event_Id, Response_Result, Response_Body, Response_Time, Status) " +
                        "values(@Event_Id, @Response_Result, @Response_Body, GETDATE(), @status); ";

                    //add records           
                    using (System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(sqlInsert))
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.Connection = connection;

                        cmd.Parameters.AddWithValue("@Event_Id", eventId);
                        cmd.Parameters.AddWithValue("@Response_Result", result);
                        cmd.Parameters.AddWithValue("@Response_Body", body);
                        cmd.Parameters.AddWithValue("@status", 0);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}