using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Logging;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System.Collections.Specialized;
using Quartz;
using Quartz.Listener;

namespace QuartzConsole
{
    class Program2
    {
        static void Main(string[] args)
        {
            RunProgramRunExample().GetAwaiter().GetResult();
        }

        private static async Task RunProgramRunExample()
        {
            Alarm alarm1 = new Alarm()
            {
                NimId = "NIM1",
                Category = "operational"
            };

            Alarm alarm2 = new Alarm()
            {
                NimId = "NIM2",
                Category = "operational"
            };

            Alarm alarm3 = new Alarm()
            {
                NimId = "NIM3",
                Category = "service"
            };

            AlarmRoute alarmRoute1 = new AlarmRoute()
            {
                Category = "service",
                CustomerID = -1,
                OrgId = 3,
                Id = "No1",
                Schedule = new AlarmSchedule()
                {
                    Cron = "0 * * ? * FRI"
                },
                Rules = new List<AlarmRouteRule>()
                {
                    new AlarmRouteRule()
                    {
                        Id = 1,
                        Destination = "Mr.Keert",
                        Duration = 10,
                        Fleet = 0,
                        Repeat = 3
                    },
                    new AlarmRouteRule()
                    {
                        Id = 2,
                        Destination = "Bergen",
                        Duration = 10,
                        Fleet = 0,
                        Repeat = 1
                    },
                    new AlarmRouteRule()
                    {
                        Id = 3,
                        Destination = "RS",
                        Duration = 1,
                        Fleet = 15,
                        Repeat = 1
                    }
                }
            };

            try
            {
                // Grab the Scheduler instance from the Factory
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                    { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz"},
                    { "quartz.jobStore.tablePrefix", "QRTZ_"},
                    { "quartz.jobStore.dataSource", "quartzDatasource" },
                    { "quartz.dataSource.quartzDatasource.connectionString", @"Server=(localdb)\MSSQLLocalDB;Database=IntelinetTMSTest;Uid=quartz;Pwd=quartz123" },
                    { "quartz.dataSource.quartzDatasource.provider", "SqlServer"}

                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                int counter = 0;

                List<JobKey> jobKeys = new List<JobKey>();
                List<AlarmJobInfo> alarmJobInfoList = new List<AlarmJobInfo>();

                foreach(AlarmRouteRule rule in alarmRoute1.Rules)
                {
                    string groupname = "Route_rule_" + counter;
                    IJobDetail job = JobBuilder.Create<AlarmAction>()
                        .WithIdentity("AlarmRouteRule_"+rule.Id,groupname)
                        .Build();
                    
                    job.JobDataMap.Put("alarm", alarm1);
                    job.JobDataMap.Put("rule", rule);
                    
                    ITrigger trigger = GetTrigger(groupname, rule,job);
                    jobKeys.Add(new JobKey("AlarmRouteRule_" + rule.Id, groupname));
                    
                    alarmJobInfoList.Add(new AlarmJobInfo() { Job = job, Trigger = trigger });
                    counter++;
                }
          
                for (int i = 0; i < alarmJobInfoList.Count-1; i++)
                {
                    IJobDetail j = alarmJobInfoList[i].Job;
                    j.JobDataMap.Put("nextJob", alarmJobInfoList[i + 1].Job);
                    j.JobDataMap.Put("nextTrigger", alarmJobInfoList[i + 1].Trigger);
                }

                JobChainHandler jobChainHandler = new JobChainHandler();
                TriggerChainHandler triggerChainHandler = new TriggerChainHandler();
                
                scheduler.ListenerManager.AddJobListener(jobChainHandler, GroupMatcher<JobKey>.AnyGroup());
                scheduler.ListenerManager.AddTriggerListener(triggerChainHandler, GroupMatcher<TriggerKey>.AnyGroup());
                await scheduler.ScheduleJob(alarmJobInfoList[0].Job,alarmJobInfoList[0].Trigger);
                

                // some sleep to show what's happening
                //await Task.Delay(TimeSpan.FromSeconds(30));
                await scheduler.Start();
                // and last shut down the scheduler when you are ready to close your program
                //await scheduler.Shutdown();
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
        }

        
        public struct AlarmJobInfo
        {
            public IJobDetail Job;
            public ITrigger Trigger;
        }

        private static ITrigger GetTrigger(string group,AlarmRouteRule rule,IJobDetail job)
        {
            ITrigger trigger = null;

        
                trigger = TriggerBuilder.Create()
                        .WithIdentity("Trigger_route_rule" + rule.Id,group)
                        
                        .WithSimpleSchedule(x=>x
                            .WithIntervalInSeconds(rule.Duration)
                            .WithRepeatCount(rule.Repeat))
                            .StartAt(DateTime.Now)
                        .ForJob(job)
                        .Build();

            

            return trigger;
        }

        private class ConsoleLogProvider : ILogProvider
        {
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if (level >= LogLevel.Info && func != null)
                    {
                        Console.WriteLine("[" + DateTime.Now.ToLongTimeString() + "] [" + level + "] " + func(), parameters);
                    }
                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                throw new NotImplementedException();
            }

            public IDisposable OpenMappedContext(string key, string value)
            {
                throw new NotImplementedException();
            }
        }

        [DisallowConcurrentExecution]
        public class AlarmAction : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
                JobDataMap jobDataMap = context.MergedJobDataMap;
                AlarmRouteRule rule = (AlarmRouteRule)jobDataMap.Get("rule");
                Alarm alarm = (Alarm)jobDataMap.Get("alarm");

                Console.WriteLine(String.Format("Alarm {0} given assignment for destination {1} with duration {2} and repeat {3}",alarm.NimId,rule.Destination,rule.Duration,rule.Repeat));
                await Console.Out.WriteLineAsync(String.Format("Alarm {0} given update and sent to queue for processing (await) - time is: {1}", alarm.NimId,DateTime.Now.ToLongTimeString()));

              
            }
        }

        public class BaseJob : IJob
        {
            public virtual async Task Execute(IJobExecutionContext context)
            {
                bool hasJob = context.MergedJobDataMap.ContainsKey("nextJob");
                IJobDetail nextJob = context.MergedJobDataMap.ContainsKey("nextJob") ? (IJobDetail)context.MergedJobDataMap["nextJob"] : null;
                ITrigger nextTrigger = context.MergedJobDataMap.ContainsKey("nextTrigger") ? (ITrigger)context.MergedJobDataMap["nextTrigger"] : null;
                
                if (nextJob != null) await context.Scheduler.ScheduleJob(nextJob, nextTrigger);
            }
        }

        
        public class HelloJob : IJob
        {
            public async Task Execute(IJobExecutionContext context)
            {
            

                await Console.Out.WriteLineAsync("Greetings from HelloJob!");
            }
        }
    }
}
