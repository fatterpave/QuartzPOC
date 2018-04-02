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

            var finishedJob = context.JobDetail;
            
            /*context.Scheduler.DeleteJob(finishedJob.Key);

            IJobDetail nextJob = (IJobDetail)finishedJob.JobDataMap.Get("nextJob");
            ITrigger nextTrigger = (ITrigger)finishedJob.JobDataMap.Get("nextTrigger");

            if (nextJob == null)
            {
                return Task.CompletedTask;
            }

            context.Scheduler.ScheduleJob(nextJob, nextTrigger);*/

            return Task.CompletedTask;
        }
    }
}
