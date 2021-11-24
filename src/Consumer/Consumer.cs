using System;
using System.Collections.Specialized;
using System.IO;
using Apache.NMS;
using Apache.NMS.AMQP;
using Newtonsoft.Json;

namespace Consumer
{
    public class MQconfig
    {
        public MQconfig()
        {
            Account = "account";
            Password = "";
            AmqpsUrl = "";
            MessageCount = 100;
            MessageDelayTime = 100;
            Queue = "";
        }
        public string Account { get; set; }
        public string Password { get; set; }
        public string AmqpsUrl { get; set; }
        public int MessageCount { get; set; }
        public int MessageDelayTime { get; set; }
        public string Queue { get; set; }
    }
    public class MQsettings
    {
        public MQconfig MQconfig { get; set; }
    }
    class Program
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static void GetMQMessage()
        {
            StreamReader r = new StreamReader("setting.json");
            string jsonString = r.ReadToEnd();
            MQsettings MQ = JsonConvert.DeserializeObject<MQsettings>(jsonString);
            IConnection connection = null;
            try
            {
                var connectionFactory = new NmsConnectionFactory(MQ.MQconfig.Account, MQ.MQconfig.Password, MQ.MQconfig.AmqpsUrl);                

                Console.WriteLine("Creating Connection...");
                connection = connectionFactory.CreateConnection();
                //connection.ExceptionListener += (logger as Logger).LogException;
                Console.WriteLine("Created Connection.");
                Console.WriteLine("Version: {0}", connection.MetaData);
                Console.WriteLine("Creating Session...");
                ISession ses = connection.CreateSession();
                Console.WriteLine("Session Created.");
                Console.WriteLine("Starting Connection...");
                connection.Start();
                Console.WriteLine("Connection Started: {0} Resquest Timeout: {1}", connection.IsStarted, connection.RequestTimeout);
                IDestination dest = (IDestination)ses.GetQueue(MQ.MQconfig.Queue);// : (IDestination)ses.GetTopic(opts.topic);
                //get message
                IMessage rmsg = null;
                Console.WriteLine("Creating Message Consumer for : {0}...", dest);
                IMessageConsumer consumer = ses.CreateConsumer(dest);
                Console.WriteLine("Created Message Consumer.");

                long connTimeout = 15000;
                log.Info("Start Consumer....");
                while (true)
                {
                    try
                    {
                        rmsg = consumer.Receive(TimeSpan.FromMilliseconds(connTimeout));
                        if (rmsg == null)
                        {
                            Console.WriteLine("no data.....{0}", connTimeout);
                            log.Info(String.Format("no data.....{0}", connTimeout));
                        }
                        else
                        {
                            Console.WriteLine("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString());
                            log.Info(String.Format("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString()));
                        }
                    }
                    catch(Exception ex)
                    {
                        log.Error("Received Message exception: ");
                        log.Error(ex.ToString());
                    }
                    
                }
                //for (int i = 0; i < 10; i++)
                //{
                //    rmsg = consumer.Receive(TimeSpan.FromMilliseconds(connTimeout));
                //    if (rmsg == null)
                //    {
                //        Console.WriteLine("Failed to receive Message in {0}ms.", connTimeout);
                //    }
                //    else
                //    {
                //        Console.WriteLine("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString());
                //    }
                //}
                //if (connection.IsStarted)
                //{
                //    Console.WriteLine("Closing Connection...");
                //    connection.Close();
                //    Console.WriteLine("Connection Closed.");
                //}
            }
            catch (NMSException ne)
            {
                Console.WriteLine("Caught NMSException : {0} \nStack: {1}", ne.Message, ne);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught unexpected exception : {0}", e);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }
        static void Main(string[] args)
        {
            GetMQMessage();
        }
    }
}
