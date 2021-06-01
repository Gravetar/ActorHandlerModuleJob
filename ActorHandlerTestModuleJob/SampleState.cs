using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace ActorModule
{
    // Реализуем интерфейс компонента состояния
    public class SampleState : IState
    {
        // Голод
        public int Hunger;

        // Место работы
        public Point Job;

        // Время работы
        public TimeInterval JobTime;

        // Расположение дома
        public Point Home;

        // Интервал времени, когда актор находится дома
        public TimeInterval HomeTime;

        // Список любимых мест актора
        public readonly List<Geometry> FavoritePlaces;

        // Создает пустое состояние
        public SampleState()
        {
            FavoritePlaces = new List<Geometry>();
        }

        // Конструктор копирования
        public SampleState(SampleState state)
        {
            // Исключение, если копируемое состояние - null
            if (state == null)
                throw new ArgumentNullException("state");

            Hunger = state.Hunger; //Int неизменяемый

            // Точки на карте вроде никуда не убегут
            Job = state.Job;
            Home = state.Home;

            // Интервалы теперь readonly struct, так что копирования не требуется
            JobTime = state.JobTime;
            HomeTime = state.HomeTime;

            // Список тоже изменяемый, поэтому нужен новый список с его элементами
            FavoritePlaces = new List<Geometry>(state.FavoritePlaces);

        }

        // Выполняет копирование компонента, необходим для соответствия интерфейсу
        public IState Copy()
        {
            return new SampleState(this);
        }
    }
}
