using System;
using OSMLSGlobalLibrary.Modules;
using NodaTime;
using NodaTime.Extensions;
using ActorModule;
using InitializeActorModule;

namespace ActorHandlerModuleJob
{
    public class ActorHandlerModuleJob : OSMLSModule
    {
        public LocalTime JobTimeStart;
        public LocalTime JobTimeEnd;
        /// <summary>
        /// Инициализация модуля. В отладочной конфигурации выводит сообщение
        /// </summary>
        protected override void Initialize()
        {
            Console.WriteLine("ActorHandlerModuleJob: Initialize");
        }
        /// <summary>
        /// Вызывает Update на всех акторах
        /// </summary>
        public override void Update(long elapsedMilliseconds)
        {
            var actors = MapObjects.GetAll<Actor>();

            foreach (var actor in actors)
            {
                //Установка приоритета
                int newPriority = 0;
                //Интервалы времени работы
                TimeInterval Interval1 = actor.GetState<JobState>().JobTimes[0];
                TimeInterval Interval2 = actor.GetState<JobState>().JobTimes[1];

                //Для проверки нужно ли идти на работу
                var zonedClock = SystemClock.Instance.InTzdbSystemDefaultZone();
                //Если время работать по первому интервалу или время работать по второму интервалу
                if (zonedClock.GetCurrentTimeOfDay() > Interval1.Start && zonedClock.GetCurrentTimeOfDay() < Interval1.End || zonedClock.GetCurrentTimeOfDay() > Interval2.Start && zonedClock.GetCurrentTimeOfDay() < Interval2.End)
                {
                    newPriority = 75;
                    if (zonedClock.GetCurrentTimeOfDay() > Interval1.Start && zonedClock.GetCurrentTimeOfDay() < Interval1.End)
                    {
                        JobTimeStart = Interval1.Start;
                        JobTimeEnd = Interval1.End;
                    }
                    else
                    {
                        JobTimeStart = Interval2.Start;
                        JobTimeEnd = Interval2.End;
                    }
                }
                else
                {
                    newPriority = 0;
                }

                //установлена ли уже активность у актора
                bool isActivity = actor.Activity != null;
                //Является ли активность нашей
                bool isMovementActivityJob = actor.Activity is MovementActivityJob;
                bool isWaitingActivityJob = actor.Activity is WaitingActivityJob;

                Console.WriteLine($"Flags: IsActivity={isActivity} IsActivityMovement={isMovementActivityJob} IsActivityWaiting={isWaitingActivityJob}");

                //Если активность не установлена или приоритет Активностей работы выше, чем у текущей
                if ((!isActivity) || (!isMovementActivityJob && !isWaitingActivityJob && newPriority > actor.Activity.Priority))
                {
                    // Назначить актору путь до работы
                    actor.Activity = new MovementActivityJob(newPriority, new TimeInterval(JobTimeStart, JobTimeEnd));
                    Console.WriteLine("Said actor go work\n");
                }
            }
        }
    }
}