using System;
using System.Collections.Specialized;
using System.IO;
using Apache.NMS;
using Apache.NMS.AMQP;
using Newtonsoft.Json;

namespace Producer
{
   
    public class MQconfig
    {
        public MQconfig() {
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
    public class MQsettings{
        public MQconfig MQconfig { get; set; }
    }
    class Program
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        private static void SendMQMessage()
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
                Console.WriteLine("Created Connection.");
                Console.WriteLine("Version: {0}", connection.MetaData);
                Console.WriteLine("Creating Session...");
                ISession ses = connection.CreateSession();
                Console.WriteLine("Session Created.");
                Console.WriteLine("Starting Connection...");
                connection.Start();
                Console.WriteLine("Connection Started: {0} Resquest Timeout: {1}", connection.IsStarted, connection.RequestTimeout);
                
                IDestination dest = (IDestination)ses.GetQueue(MQ.MQconfig.Queue);// : (IDestination)ses.GetTopic(opts.topic);
                Console.WriteLine("Creating Message Producer for : {0}...", dest);
                IMessageProducer prod = ses.CreateProducer(dest);
                //IMessageConsumer consumer = ses.CreateConsumer(dest);
                Console.WriteLine("Created Message Producer.");
                prod.DeliveryMode = MsgDeliveryMode.Persistent; //MsgDeliveryMode.NonPersistent : 
                prod.TimeToLive = TimeSpan.FromDays(1);//.FromSeconds(20);
                
                int NUM_MSG = MQ.MQconfig.MessageCount;
                Console.WriteLine("Sending {0} Messages...", NUM_MSG);
                DateTime StartTime = DateTime.Now;
                Console.WriteLine("Start time:{0}", StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff"));
                log.Info(string.Format("Start time:{0}", StartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff")));

                for (int i = 0; i < NUM_MSG; i++)
                {
                    ITextMessage msg = prod.CreateTextMessage(i.ToString().PadLeft(6,'0')); //UUID System.Guid.NewGuid().ToString()
                    Console.WriteLine("Sending Msg: {0}", msg.ToString());

                    //prod.DeliveryDelay = new TimeSpan(0, 0, 0, 0, 0);
                    //TimeSpan interval = new TimeSpan(0, 0, 0, 0, 1 00 * (i + 1));
                    //prod.DeliveryDelay = interval.Add(interval);
                    long delaytime = MQ.MQconfig.MessageDelayTime * (i + 1);
                    msg.Properties["AMQ_SCHEDULED_DELAY"] = delaytime;

                    Tracer.InfoFormat("Sending Msg {0}", i + 1);
                    // Text Msg Body 
                    //msg.Text = "Hello World! n: " + i;
                    prod.Send(msg);
                    msg.ClearBody();
                }
                DateTime EndTime = DateTime.Now;
                TimeSpan ts = EndTime - StartTime;
                Console.WriteLine("End time:{0}", EndTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff"));
                log.Info(string.Format("End time:{0}", EndTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff")));

                Console.WriteLine("Total time:{0}",ts.TotalSeconds.ToString());
                log.Info(string.Format("Total time:{0}", ts.TotalSeconds.ToString()));

                Console.ReadLine();
                //IMessage rmsg = null;
                //for (int i = 0; i < opts.NUM_MSG; i++)
                //{
                //    Tracer.InfoFormat("Waiting to receive message {0} from consumer.", i);
                //    rmsg = consumer.Receive(TimeSpan.FromMilliseconds(opts.connTimeout));
                //    if(rmsg == null)
                //    {
                //        Console.WriteLine("Failed to receive Message in {0}ms.", opts.connTimeout);
                //    }
                //    else
                //    {
                //        Console.WriteLine("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString());
                //    }
                //}
                if (connection.IsStarted)
                {
                    Console.WriteLine("Closing Connection...");
                    connection.Close();
                    Console.WriteLine("Connection Closed.");
                }
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
            SendMQMessage();
        }
    }

    #region Logging

    class Logger : ITrace
    {
        public enum LogLevel
        {
            OFF = -1,
            FATAL,
            ERROR,
            WARN,
            INFO,
            DEBUG
        }

        public static LogLevel ToLogLevel(string logString)
        {
            if(logString == null || logString.Length == 0)
            {
                return LogLevel.OFF;
            }
            if ("FATAL".StartsWith(logString, StringComparison.CurrentCultureIgnoreCase))
            {
                return LogLevel.FATAL;
            }
            else if ("ERROR".StartsWith(logString, StringComparison.CurrentCultureIgnoreCase))
            {
                return LogLevel.ERROR;
            }
            else if ("WARN".StartsWith(logString, StringComparison.CurrentCultureIgnoreCase))
            {
                return LogLevel.WARN;
            }
            else if ("INFO".StartsWith(logString, StringComparison.CurrentCultureIgnoreCase))
            {
                return LogLevel.INFO;
            }
            else if ("DEBUG".StartsWith(logString, StringComparison.CurrentCultureIgnoreCase))
            {
                return LogLevel.DEBUG;
            }
            else 
            {
                return LogLevel.OFF;
            }
        }

        private LogLevel lv;

        public void LogException(Exception ex)
        {
            this.Warn("Exception: "+ex.Message);
        }

        public Logger() : this(LogLevel.WARN)
        {
        }

        public Logger(LogLevel lvl)
        {
            lv = lvl;
        }

        public bool IsDebugEnabled
        {
            get
            {
                return lv >= LogLevel.DEBUG;
            }
        }

        public bool IsErrorEnabled
        {
            get
            {
                
                return lv >= LogLevel.ERROR;
            }
        }

        public bool IsFatalEnabled
        {
            get
            {
                return lv >= LogLevel.FATAL;
            }
        }

        public bool IsInfoEnabled
        {
            get
            {
                return lv >= LogLevel.INFO;
            }
        }

        public bool IsWarnEnabled
        {
            get
            {
                return lv >= LogLevel.WARN;
            }
        }

        public void Debug(string message)
        {
            if(IsDebugEnabled)
                Console.WriteLine("Debug: {0}", message);
        }

        public void Error(string message)
        {
            if (IsErrorEnabled)
                Console.WriteLine("Error: {0}", message);
        }

        public void Fatal(string message)
        {
            if (IsFatalEnabled)
                Console.WriteLine("Fatal: {0}", message);
        }

        public void Info(string message)
        {
            if (IsInfoEnabled)
                Console.WriteLine("Info: {0}", message);
        }

        public void Warn(string message)
        {
            if (IsWarnEnabled)
                Console.WriteLine("Warn: {0}", message);
        }
    }
    #endregion 
}
