﻿using System;
using NetTopologySuite.Geometries;  // Отсюда Point и другая геометрия
using NetTopologySuite.Mathematics; // Отсюда векторы
using ActorModule;
using PathsFindingCoreModule;
using NodaTime;
using NodaTime.Extensions;
using InitializeActorModule;

namespace ActorHandlerModuleJob
{
    public class MovementActivityJob : IActivity
    {
        //Координаты пути до работы и счетчик пути
        public Coordinate[] Path;
        public int i = 0;
        //Флаг-путь построен.
        public bool IsPath = true;

        // Приоритет делаем авто-свойством, со значением по умолчанию
        // Вообще он дожен был быть полем, но интерфейсы не дают объявлять поля, так что...
        public int Priority { get; set; } = 0;
        public TimeInterval JobTime { get; set; }

        public MovementActivityJob(int priority, TimeInterval jobTime)
        {
            Priority = priority;
            JobTime = jobTime;
        }

        // Здесь происходит работа с актором
        public bool Update(Actor actor, double deltaTime)
        {

            // Расстояние, которое может пройти актор с заданной скоростью за прошедшее время
            double distance = actor.GetState<SpecState>().Speed * deltaTime;

            //Уменьшаем статы акторы
                //Проверка здоровья
            if (actor.GetState<SpecState>().Health <= 0.1) actor.GetState<SpecState>().Health = 0;
                //Голод
            if (actor.GetState<SpecState>().Hunger <= 0.1) actor.GetState<SpecState>().Health -= 0.001;
            else actor.GetState<SpecState>().Hunger -= 0.001;
                //Усталость
            if (actor.GetState<SpecState>().Fatigue <= 0.1) actor.GetState<SpecState>().Health -= 0.001;
            else actor.GetState<SpecState>().Fatigue -= 0.001;
                //Настроение(Падает медленее, чем другие)
            if (actor.GetState<SpecState>().Mood <= 0.1) actor.GetState<SpecState>().Health -= 0.001;
            else actor.GetState<SpecState>().Mood -= 0.0001;
#if DEBUG
            Console.WriteLine($"Health: {actor.GetState<SpecState>().Health}; Hunger: {actor.GetState<SpecState>().Hunger}; Fatigue: {actor.GetState<SpecState>().Fatigue}; Mood: {actor.GetState<SpecState>().Mood}");
#endif

            //Если путь еще не построен
            if (IsPath)
            {
                //Начальные координаты и координаты точки работы
                var firstCoordinate = new Coordinate(actor.X, actor.Y);
                var secondCoordinate = new Coordinate(actor.GetState<JobState>().Job.X, actor.GetState<JobState>().Job.Y);
                //Строим путь
                Path = PathsFinding.GetPath(firstCoordinate, secondCoordinate, "Walking").Result.Coordinates;
                IsPath = false;
            }

            Vector2D direction = new Vector2D(actor.Coordinate, Path[i]);
            // Проверка на перешагивание
            if (direction.Length() <= distance)
            {
                // Шагаем в точку, если она ближе, чем расстояние которое можно пройти
                actor.X = Path[i].X;
                actor.Y = Path[i].Y;
            }
            else
            {
                // Вычисляем новый вектор, с направлением к точке назначения и длинной в distance
                direction = direction.Normalize().Multiply(distance);

                // Смещаемся по вектору
                actor.X += direction.X;
                actor.Y += direction.Y;
            }
            //Если актор достиг следующей точки пути
            if (actor.X == Path[i].X && actor.Y == Path[i].Y && i < Path.Length - 1)
            {
                i++;
                //Console.WriteLine(i);//Точка пути
                //Console.WriteLine(Path.Length);//Длина пути
            }

            // Если в процессе шагания мы достигли точки назначения
            if (actor.X == Path[Path.Length - 1].X && actor.Y == Path[Path.Length - 1].Y)
            {
                Console.WriteLine("Start Waiting");
                Priority = 0;
                i = 0;
                IsPath = true;
                //Запуск активити ожидания(имитация нахождения на работе)
                actor.Activity = new WaitingActivityJob(actor, Priority, JobTime);
            }
            return false;
        }
    }
}
