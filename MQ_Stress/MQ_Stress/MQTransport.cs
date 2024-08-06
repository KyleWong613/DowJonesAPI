using IBM.WMQ;
using System;
using System.Runtime.Remoting.Messaging;

namespace MQ_Stress
{
    public class MQTransport
    {
        private MQQueueManager mqQMgr;
        private MQQueue mqQueue;
        private MQMessage mqMsg;
        private MQPutMessageOptions mqPutMsgOpts;

        public MQTransport(string Hostname, string Channel, string Port)
        {
            MQEnvironment.Hostname = Hostname;
            MQEnvironment.Channel = Channel;
            MQEnvironment.Port = int.Parse(Port);
        }

        public bool testConnection(
          string QueueManagerName,
          string QueueName,
          string type,
          ref string ExceptionMsg)
        {
            MQQueueManager mqQueueManager = new MQQueueManager(QueueManagerName);
            bool flag;
            try
            {
                this.mqQueue = !(type == "put") ? mqQueueManager.AccessQueue(QueueName, 8193) : mqQueueManager.AccessQueue(QueueName, 8208);
                flag = true;
            }
            catch (MQException ex)
            {
                ExceptionMsg = ((Exception)ex).Message;
                flag = false;
            }
            return flag;
        }

        public bool openMQueue(
          string QueueManager,
          string QueueName,
          string type,
          ref string ExceptionMsg)
        {
            bool flag;
            try
            {
                this.mqQMgr = new MQQueueManager(QueueManager);
                this.mqQueue = !(type == "put") ? this.mqQMgr.AccessQueue(QueueName, 8193) : this.mqQMgr.AccessQueue(QueueName, 8208);
                flag = true;
            }
            catch (MQException ex)
            {
                ExceptionMsg = "MQQueueManager::AccessQueue ended with " + ((object)ex).ToString();
                flag = false;
            }
            catch (Exception ex)
            {
                ExceptionMsg = "MQQueueManager::AccessQueue ended with " + ex.ToString();
                flag = false;
            }
            return flag;
        }

        public bool closeMQueue(ref string ExceptionMsg)
        {
            bool flag;
            try
            {
                if (this.mqQueue != null)
                    this.mqQueue.Close();
                if (this.mqQMgr != null)
                    this.mqQMgr.Disconnect();
                flag = true;
            }
            catch (MQException ex)
            {
                ExceptionMsg = "A WebSphere MQ error occurred :Completion code " + (object)ex.CompletionCode + "Reason code " + (object)ex.ReasonCode;
                flag = false;
            }
            return flag;
        }

        public bool SendMessage(string messageText, ref string ExceptionMsg)
        {
            bool flag = true;
            if (flag)
            {
                try
                {
                    this.mqMsg = new MQMessage();
                    this.mqMsg.WriteString(messageText);
                    this.mqMsg.Format = "MQSTR   ";
                    this.mqPutMsgOpts = new MQPutMessageOptions();
                    this.mqQueue.Put(this.mqMsg, this.mqPutMsgOpts);
                }
                catch (MQException ex)
                {
                    ExceptionMsg = "MQQueue::Put ended with " + ((object)ex).ToString();
                    flag = false;
                }
                catch (Exception ex)
                {
                    ExceptionMsg = "MQQueue::Put ended with (Exception) " + ex.ToString();
                    flag = false;
                }
            }
            return flag;
        }

        public string ReceiveMessage(ref string ExceptionMsg)
        {
            bool flag = true;
            string message1 = "";
            long num = 0;
            while (flag)
            {
                if (num == 10L)
                    flag = false;
                MQMessage message2 = new MQMessage();
                MQGetMessageOptions gmo = new MQGetMessageOptions();
                gmo.WaitInterval = 15000;
                gmo.Options |= 1;
                try
                {
                    this.mqQueue.Get(message2, gmo);
                    num = 0L;
                    message1 += (string)(object)message2;
                    if (message2.Format.CompareTo("MQSTR   ") == 0)
                    {
                        message1 += message2.ReadString(message2.MessageLength);
                        flag = false;
                    }
                }
                catch (MQException ex)
                {
                    if (ex.Reason == 2033)
                    {
                        ExceptionMsg = "No More Messages";
                    }
                    else
                    {
                        ++num;
                        ExceptionMsg = "MQQueue::Get ended with " + ((object)ex).ToString();
                        if (ex.Reason == 2080)
                            flag = false;
                    }
                }
            }
            return message1;
        }
    }
}