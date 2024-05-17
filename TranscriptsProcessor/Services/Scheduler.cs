using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace TranscriptsProcessor.Services
{
    public class Scheduler
    {
        public Scheduler(ILogger logger,
                         IFileManager fileGetter,
                         ISender senderService)
        {
            Logger = logger;
            PendingFileGetter = fileGetter;
            SenderService = senderService;
        }

        public void Start(string filePath)
        {
            FilePath = filePath;
            var timeToGo = GetNextMidnight() - DateTime.Now;
            if (timeToGo < TimeSpan.Zero)
            {
                timeToGo += TimeSpan.FromDays(1); // next day if it's already past midnight
            }

            timer = new Timer(timeToGo.TotalMilliseconds);
            timer.Elapsed += (sender, args) => TimerElapsed(sender, args, timer);
            timer.AutoReset = false; // Ensure the timer runs only once
            timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e, Timer timer)
        {
            PerformScheduledTask();

            // Reset the timer to fire again in 24 hours
            timer.Interval = 86400000; // 24 hours in milliseconds
            timer.Start();
        }

        private DateTime GetNextMidnight()
        {
            return DateTime.Today.AddDays(1);
        }

        private Task PerformScheduledTask()
        {
            Console.WriteLine("Performing scheduled task at " + DateTime.Now);
            var service = new Processor(Logger, PendingFileGetter, SenderService);
            return service.Run(FilePath);
        }

        private Timer timer;
        private string FilePath;
        private readonly ILogger Logger;
        private readonly IFileManager PendingFileGetter;
        private readonly ISender SenderService;
    }
}