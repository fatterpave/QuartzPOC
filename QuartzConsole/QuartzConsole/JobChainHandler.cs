using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;

namespace QuartzConsole
{
    class JobChainHandler : IJobListener
    {
        public string Name { get { return "JobChainHandler"; } }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context,CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobExecutionException, CancellationToken cancellationToken)
        {
            if (jobExecutionException != null)
            {
                return Task.CompletedTask;
            }

            if (context == null)
            {
                throw new ArgumentNullException();
            }

            Console.Out.WriteAsync("JobComplete at " + DateTime.Now.ToLongTimeString());

            ISimpleTrigger s = (ISimpleTrigger)context.Trigger;

            var finishedJob = context.JobDetail;

            if (s.TimesTriggered == s.RepeatCount)
            {
                context.Scheduler.DeleteJob(finishedJob.Key);
                AlarmRouteRule rule = (AlarmRouteRule)finishedJob.JobDataMap.Get("rule");
                IJobDetail nextJob = (IJobDetail)finishedJob.JobDataMap.Get("nextJob");
                ITrigger nextTrigger = (ITrigger)finishedJob.JobDataMap.Get("nextTrigger");
                
                if (nextJob == null)
                {
                    return Task.CompletedTask;
                }
                ITrigger tr = nextTrigger.GetTriggerBuilder().StartAt(DateTime.Now.AddSeconds(rule.Duration)).Build();
                context.Scheduler.ScheduleJob(nextJob, tr);
            }

            return Task.CompletedTask;
        }
    }
}
