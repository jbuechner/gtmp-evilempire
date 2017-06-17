using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.entities;
using gtmp.evilempire.services;
using GrandTheftMultiplayer.Server.API;
using Quartz;
using Quartz.Impl;

namespace gtmp.evilempire.server.services
{
    class EnvironmentService : IEnvironmentService
    {

        public API _api;
        public List<Weather> WeatherList = new List<Weather>();


        public EnvironmentService(API api)
        {
            _api = api;
            WeatherList.Add(new Weather("ExtraSunny", 0));
            WeatherList.Add(new Weather("Clear", 1));
            WeatherList.Add(new Weather("Clouds", 2));
            WeatherList.Add(new Weather("Smog", 3));
            WeatherList.Add(new Weather("Foggy", 4));
            WeatherList.Add(new Weather("Overcast", 5));
            WeatherList.Add(new Weather("Rain", 6));
            WeatherList.Add(new Weather("Thunder", 7));
            WeatherList.Add(new Weather("LightRain", 8));


            // Grab the Scheduler instance from the Factory 
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

            // and start it off
            scheduler.Start();

            
            IJobDetail job = JobBuilder.Create<WeatherJob>()
                .WithIdentity("weather_job", "weatherservice")
                .Build();
            job.JobDataMap["weatherService"] = this;
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("weather_trigger", "weatherservice")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(30)
                    .RepeatForever())
                .Build();


            IJobDetail timeJob = JobBuilder.Create<WeatherJob>()
                .WithIdentity("weather_job", "weatherservice")
                .Build();
            job.JobDataMap["weatherService"] = this;
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger timeTrigger = TriggerBuilder.Create()
                .WithIdentity("time_tick", "weatherservice")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(new TimeSpan(0,0,0,0,2))
                    .RepeatForever())
                .Build();

            scheduler.ScheduleJob(job, trigger);



        }

        public Weather GetCurrentWeather()
        {

            foreach (var item in WeatherList)
            {
                if (item.WeatherId == _api.getWeather())
                {
                    return item;
                }
            }
            return null;
        }

        public void SetWeather(Weather weather)
        {
            if (WeatherList.Contains(weather))
            {
                _api.setWeather(weather.WeatherId);
                
            }
        }

        public void SetTime(TimeSpan time)
        {
            _api.setTime(time.Hours, time.Minutes);
        }
        
        

       
    }

    public class TimeJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var environmentService = (EnvironmentService)dataMap["weatherService"];

            var currTIme = environmentService._api.getTime();
            if (currTIme.Minutes == 59)
            {
                environmentService.SetTime(new TimeSpan(currTIme.Hours, 00, 00));
            }
            else
            {
                
            }

            

        }
    }

    public class WeatherJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var weatherService = (EnvironmentService)dataMap["weatherService"];
            Random rdm = new Random();
            var weather = weatherService.WeatherList[rdm.Next(0, weatherService.WeatherList.Count - 1)];
            weatherService.SetWeather(weather);
            //Console.WriteLine("Wetter wurde zu " + weather.Name + " geändert");

        }
    }

}
