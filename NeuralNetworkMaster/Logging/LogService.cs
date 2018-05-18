using Microsoft.Extensions.Configuration;
using NeuralNetworkMaster.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NeuralNetworkMaster.Logging
{
    internal class LogService
    {
        private Object infoLogsLock;
        private Object logBuilderLock;
        public List<InfoLog>[] InfoLogs { get; set; }
        public IConfiguration Configuration { get; set; }
        public WebHelper webHelper { get; set; }
        private StringBuilder logBuilder;

        private bool stopService;

        public LogService(int numberOfSlaves)
        {
            infoLogsLock = new Object();
            logBuilderLock = new Object();
            logBuilder = new StringBuilder();
            InfoLogs = new List<InfoLog>[numberOfSlaves];

            for (int i = 0; i < numberOfSlaves; i++)
                InfoLogs[i] = new List<InfoLog>();
        }

        public void AddLog(List<Log> logs, int slaveNumber)
        {
            foreach (var log in logs)
            {
                if (log.LogType == "info")
                {
                    InfoLog infoLog = JsonConvert.DeserializeObject<InfoLog>(log.Message);
                    lock (infoLogsLock)
                    {
                        InfoLogs[slaveNumber].Add(infoLog);
                        
                    }

                    lock(logBuilderLock)
                    {
                        Console.WriteLine(string.Format("Node {0} : Iteration {1} Cost {2}", slaveNumber + 1, infoLog.Iteration, infoLog.Cost));
                        logBuilder.Append(string.Format("Node {0} : Iteration {1} Cost {2}", slaveNumber + 1, infoLog.Iteration, infoLog.Cost));
                    }
                }
                else
                {
                    lock (logBuilderLock)
                    {
                        logBuilder.Append(string.Format("Node {0} : {1}\n", slaveNumber + 1, log.Message));
                    }
                }
            }
        }

        public void StartLogService()
        {
            (new Thread(Service)).Start();
        }

        public void StopLogService()
        {
            SendLogs();
            stopService = true;
        }

        public void Service()
        {
            while (!stopService)
            {
                SendLogs();
                Thread.Sleep(int.Parse($"{Configuration["log-reporting-time-interval"]}"));
            }
        }

        public void SendLogs()
        {
            double averageCost=0;
            int averageIteration=0;
            int sumIteration=0;
            double sumCost = 0;
            int count = 0;
            lock (infoLogsLock)
            {


                for (int i = 0; i < InfoLogs.Length; i++)
                {
                    int length = InfoLogs[i].Count;
                    if ( length > 0)
                    {
                        sumCost += InfoLogs[i][length-1].Cost;
                        sumIteration += InfoLogs[i][length - 1].Iteration;
                        count++;
                        InfoLogs[i].Clear();
                    }
                }
                if (count != 0)
                {
                    averageCost = sumCost / count;
                    averageIteration = sumIteration / count;
                }
            }

            ServerLog serverLog;
            lock(logBuilderLock)
            {
                serverLog = new ServerLog()
                {
                    Cost = averageCost,
                    Iteration = averageIteration,
                    Log = logBuilder.ToString()
                };
                logBuilder.Clear();
            }
            if (count > 0)
            {
                Console.WriteLine("Average Cost {0}", averageCost);
                Console.WriteLine("Average Iteration {0}", averageIteration);
                webHelper.PostLog(JsonConvert.SerializeObject(serverLog));
            }
        }
    }
}