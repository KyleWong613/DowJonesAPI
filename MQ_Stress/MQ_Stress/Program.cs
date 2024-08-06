using MQ_Stress.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MQ_Stress
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string MQType = "MQ Type: ";
            string respbody = string.Empty;
            //wander get customerinfo
            //string txtPut = "|000000000A16971722480806|CustomerInfo|001|960613085069|";
            string txtPut = "710110015542|P|U4|0|0||0||";
            string mqtype = "GET_CUSTOMER_PROFILE";
            MQType += mqtype;

            DBSimulationHelper dbSimulationHelper = new DBSimulationHelper();
            string requestId = string.Empty;
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine("Request: " + txtPut);
                requestId = dbSimulationHelper.InsertNewAPIRequest(mqtype, txtPut, mqtype);
                //respbody = dbSimulationHelper.GetResponseBody(requestId, MQType);
                //respbody = getResp(requestId, mqWander);
                //Console.WriteLine("Response: " + respbody);
            }
        }

        private static string getResp(string requestId, string mqType)
        {
            DBSimulationHelper dbSimulationHelper = new DBSimulationHelper();
            string respbody = dbSimulationHelper.GetResponseBody(requestId, mqType);
            return respbody;
        }
    }
}
