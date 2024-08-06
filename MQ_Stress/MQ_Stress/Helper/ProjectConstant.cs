using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MQ_Stress.Models
{
    class ProjectConstant
    {
        public readonly static string CONNECTIONSTRING = ConfigurationManager.ConnectionStrings["MQConnectionString"].ConnectionString;

        public readonly static string WanderQueueManagerIPAdress = ConfigurationManager.AppSettings["WanderQueueManagerIPAdress"];
        public readonly static string WanderQueueManagerPort = ConfigurationManager.AppSettings["WanderQueueManagerPort"];
        public readonly static string WanderQueueManagerChannel = ConfigurationManager.AppSettings["WanderQueueManagerChannel"];
        public readonly static string WanderQueueManagerName = ConfigurationManager.AppSettings["WanderQueueManagerName"];
        public readonly static string WanderQueueSendName = ConfigurationManager.AppSettings["WanderQueueSendName"];
        public readonly static string WanderQueueReceiveName = ConfigurationManager.AppSettings["WanderQueueReceiveName"];

        public readonly static string WanderCoreQueueManagerPort = ConfigurationManager.AppSettings["WanderCoreQueueManagerPort"];
        public readonly static string WanderCoreQueueManagerChannel = ConfigurationManager.AppSettings["WanderCoreQueueManagerChannel"];
        public readonly static string WanderCoreQueueManagerName = ConfigurationManager.AppSettings["WanderCoreQueueManagerName"];
        public readonly static string WanderCoreQueueSendName = ConfigurationManager.AppSettings["WanderCoreQueueSendName"];
        public readonly static string WanderCoreQueueReceiveName = ConfigurationManager.AppSettings["WanderCoreQueueReceiveName"];

        public readonly static string AMPQueueManagerIPAdress = ConfigurationManager.AppSettings["AMPQueueManagerIPAdress"];
        public readonly static string AMPQueueManagerPort = ConfigurationManager.AppSettings["AMPQueueManagerPort"];
        public readonly static string AMPQueueManagerChannel = ConfigurationManager.AppSettings["AMPQueueManagerChannel"];
        public readonly static string AMPQueueManagerName = ConfigurationManager.AppSettings["AMPQueueManagerName"];
        public readonly static string AMPQueueSendName = ConfigurationManager.AppSettings["AMPQueueSendName"];
        public readonly static string AMPQueueReceiveName = ConfigurationManager.AppSettings["AMPQueueReceiveName"];
         
        public readonly static char MessageSeperator = '|';
        public readonly static int RetryCount = 20;
    }
}