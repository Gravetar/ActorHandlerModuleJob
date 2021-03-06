using System;
using ActorModule;
using NodaTime;
using NodaTime.Extensions;
using InitializeActorModule;

namespace ActorHandlerModuleJob
{
    public class WaitingActivityJob : IActivity
    {
        // Приоритет делаем авто-свойством, со значением по умолчанию
        public int Priority { get; set; } = 1;
        //Интервал времени на работу
        public TimeInterval JobTime { get; set; }
        double workseconds;

        public WaitingActivityJob(double _workseconds, int priority, TimeInterval jobtime)
        {
            Priority = priority;
            JobTime = jobtime;
            workseconds = _workseconds;
        }
        //Update-работает постоянно, пока не return true
        public bool Update(Actor actor, double deltaTime)
        {
            workseconds += deltaTime;
            // Уменьшаем статы акторы
            //Проверка здоровья
            if (workseconds >= 1)
            {
                workseconds -= 1;
                //Голод
                if (actor.GetState<SpecState>().Satiety <= 0.1) actor.GetState<SpecState>().Satiety = 0;
                else actor.GetState<SpecState>().Satiety -= 0.01 * 100;
                //Усталость
                if (actor.GetState<SpecState>().Stamina <= 0.1) actor.GetState<SpecState>().Stamina = 0;
                else actor.GetState<SpecState>().Stamina -= 0.01 * 100;
                //Настроение(Падает медленее, чем другие)
                if (actor.GetState<SpecState>().Mood <= 0.1) actor.GetState<SpecState>().Mood = 0;
                else actor.GetState<SpecState>().Mood -= 0.001 * 100;
            }
        Console.WriteLine($"Health: {actor.GetState<SpecState>().Health}; Hunger: {actor.GetState<SpecState>().Satiety}; Fatigue: {actor.GetState<SpecState>().Stamina}; Mood: {actor.GetState<SpecState>().Mood}");


            //Текущее время, переведенное в строку
            string NowTime = DateTime.Now.ToString("HH:mm:ss");

            //Console.WriteLine("NOW:  " + NowTime);
            //Console.WriteLine("NEED:  " + JobTime.End.ToString());

            //Если текущее время равно времени окончания работы
            if (NowTime == JobTime.End.ToString())
            {
                Priority = 0;
                actor.GetState<SpecState>().Money += new Random().Next(1000, 2000);
                return true;//Выходим из активити
            }
            return false;
        }
    }
}
