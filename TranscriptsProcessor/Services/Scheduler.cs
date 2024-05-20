using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace TranscriptsProcessor.Services
{
    public sealed class Scheduler
    {
        public Scheduler(ILogger<Scheduler> logger,
                         IFileManager fileManager,
                         ISender sender)
        {
            Logger = logger;
            FileManager = fileManager;
            Sender = sender;
        }

        public void Start(string filePath)
        {
            FilePath = filePath;
            var timeToGo = GetNextMidnight() - DateTime.Now;
            if (timeToGo < TimeSpan.Zero)
            {
                timeToGo += TimeSpan.FromDays(1); // next day if it's already past midnight
            }

            Timer = new Timer(timeToGo.TotalMilliseconds);
            Timer.Elapsed += (sender, args) => TimerElapsed(Timer);
            Timer.AutoReset = false; // Ensure the timer runs only once
            Timer.Start();
        }

        private void TimerElapsed(Timer timer)
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
            Logger.LogInformation("Performing scheduled task at " + DateTime.Now);
            var service = new Processor(Logger, FileManager, Sender);
            return service.Run(FilePath);
        }

        private Timer Timer;
        private string FilePath;
        private readonly ILogger<Scheduler> Logger;
        private readonly IFileManager FileManager;
        private readonly ISender Sender;
    }
}