using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Listener;

namespace QuartzConsole
{
    class TriggerChainHandler : TriggerListenerSupport
    {
        public override string Name => "PISSSSS";

        public override Task TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default(CancellationToken))
        {
            Console.Out.WriteAsync("TriggerComplete at " + DateTime.Now.ToLongTimeString());
            return base.TriggerComplete(trigger, context, triggerInstructionCode, cancellationToken);
        }

        public override Task TriggerFired(ITrigger trigger, IJobExecutionContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            Console.Out.WriteAsync("TriggerFired at " + DateTime.Now.ToLongTimeString());
            return base.TriggerFired(trigger, context, cancellationToken);
        }
    }
}
