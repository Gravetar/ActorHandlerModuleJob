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

                foreach (TimeInterval item in actor.GetState<JobState>().JobTimes)
                {
                    if (item.Ongoing)
                    {
                        newPriority = 75;
                        JobTimeStart = item.Start;
                        JobTimeEnd = item.End;
                        break;
                    }
                    else
                    {
                        newPriority = 0;
                    }
                }

                //установлена ли уже активность у актора
                bool isActivity = actor.Activity != null;
                //Является ли активность нашей
                bool isMovementActivityJob = actor.Activity is MovementActivityJob;
                bool isWaitingActivityJob = actor.Activity is WaitingActivityJob;

               // Console.WriteLine($"Flags: IsActivity={isActivity} IsActivityMovement={isMovementActivityJob} IsActivityWaiting={isWaitingActivityJob}");

                //Если активность не установлена или приоритет Активностей работы выше, чем у текущей
                if ((!isMovementActivityJob && !isWaitingActivityJob && newPriority > (actor.Activity?.Priority ?? 0)) && newPriority > 0)
                {
                    // Назначить актору путь до работы
                    actor.Activity = new MovementActivityJob(newPriority, new TimeInterval(JobTimeStart, JobTimeEnd));
                    Console.WriteLine("Said actor go work\n");
                }
            }
        }
    }
}