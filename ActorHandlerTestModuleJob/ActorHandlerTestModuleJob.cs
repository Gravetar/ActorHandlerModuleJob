using System;

using NodaTime; // Отсюда LocalTime
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

using OSMLSGlobalLibrary.Modules;

using ActorModule;

using OSMLSGlobalLibrary.Map;

using PathsFindingCoreModule;

using ActorHandlerModuleJob;
using CityDataExpansionModule;

namespace ActorHandlerTestModuleJob
{
    public class ActorHandlerTestModuleJob : OSMLSModule
    {
        [CustomStyle(@"new style.Style({
                stroke: new style.Stroke({
                    color: 'rgba(90, 0, 157, 1)',
                    width: 2
                })
            });
        ")]
        private class Highlighted : GeometryCollection
        {
            public Highlighted(Geometry[] geometries) : base(geometries)
            {

            }
        }

        // Координаты центра Волгограда. Сюда будем закидывать акторов
        private double x = 4173165;
        private double y = 7510997;

        // Зададим радиус, в котором будут ходить акторы
        private double radius = 100;

        //Генератор случайных чисел
        private Random random = new Random();

        // И случайное смещение от центра, которое будем использовать для создания точек интереса
        private double offset { get { return random.NextDouble() * 2 * radius - radius; } }


        // Этот метод будет вызван один раз при запуске, соответственно тут вся инициализация
        protected override void Initialize()
        {
            // Создаем состояние шаблон. Потом каждому человеку зададим свои точки интересов
            SampleState state = new SampleState()
            {
                Hunger = 100,

                // Интервал можно задать через объекты LocalTime
                HomeTime = new TimeInterval(new LocalTime(8, 2), new LocalTime(8, 0)),

                // А можно через часы и минуты (и секунды тоже, если есть такая необходимость)
                JobTime = new TimeInterval(new LocalTime(20, 0), new LocalTime(18, 26))
            };

            // Создаём акторов
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine($"Creating actor {i + 1}");

                // Делаем для каждого точку дома и точку работы в квадрате заданного радиуса от точки спавна
                state.Home = new Point(x + offset, y + offset);
                state.Job = new Point(x + offset, y + offset);

                Console.WriteLine($"Home at {state.Home.X}, {state.Home.Y}; " +
                                  $"Job at {state.Job.X}, {state.Job.Y}");

                // Создаём актора
                Actor actor = new Actor(x, y);

                // Добавляем компонент состояния. Внутри компонент копируется (тем самым методом copy), так что в принципе
                // можно всех акторов одним и тем же состоянием инициализировать
                actor.AddState(state);

                // Добавляем актора в объекты карты
                MapObjects.Add(actor);

                var firstCoordinate = new Coordinate(x, y);
                var secondCoordinate = new Coordinate(state.Job.X, state.Job.Y);

                Console.WriteLine($"Coor {firstCoordinate} and {secondCoordinate}");

                Console.WriteLine("Building path...");
                MapObjects.Add(new Highlighted(new Geometry[]
                    {
                                PathsFinding.GetPath(firstCoordinate, secondCoordinate, "Walking").Result
                    }));
                Console.WriteLine("Path was builded");
            }

            // Получаем список акторов на карте и выводим их количество
            var actors = MapObjects.GetAll<Actor>();
            Console.WriteLine($"Added {actors.Count} actors");

            foreach (var actor in actors)
                Console.WriteLine($"Actor on ({actor.Coordinate.X}, {actor.Coordinate.Y})\n" +
                                  $"\tHome at {actor.GetState<SampleState>().Home.X}, {actor.GetState<SampleState>().Home.Y}\n" +
                                  $"\tJob at {actor.GetState<SampleState>().Job.X}, {actor.GetState<SampleState>().Job.Y}");
        }

        // Этот метод вызывается регулярно, поэтому тут все действия, которые будут повторяться
        public override void Update(long elapsedMilliseconds)
        {
            Console.WriteLine("\nActorTestModule: Update");

            // Снова получаем список акторов
            var actors = MapObjects.GetAll<Actor>();
            Console.WriteLine($"Got {actors.Count} actors\n");

            // Для каждого актёра проверяем условия и назначаем новую активность если нужно
            foreach (var actor in actors)
            {
                // Достаём нужный компонент состояния
                SampleState state = actor.GetState<SampleState>();

                // Дальше идёт куча првоверок
                // Основная их цель - убедиться, что я не перезаписываю одну и ту же активность на каждой итерации
                // Это может быть полезно, если, например, создание активности затрачивает много времени

                // Не уверен, что это правильный способ решать данную проблему, но другого способа я не знаю

                // Есть ли активность
                bool isActivity = actor.Activity != null;

                // Если активность есть, то наша ли это активность
                bool goActivity = isActivity ? actor.Activity is MovementActivityJob : false;

                // Если активность наша, ведёт ли она актора домой или на работу


                Console.WriteLine($"Flags: {isActivity} {goActivity}");


                // Если активности нету, или активность не наша, или активность наша, но не ведет на работу
                if (!isActivity)
                {
                    // Назначить актору путь до работы
                    //actor.Activity = new MovementActivityJob(state.Job);
                    Console.WriteLine("Said actor go job\n");
                }




                // Здесь я не проверяю приоритет (не знаю, по каким правилам его стоит задавать), 
                // но его следует проверять до задания активности. Например:
                //
                //     int newPrioriy = ...;  // Считаем приоритет своей активности
                //
                //     if (newPriority > actor.Activity.Priority)  // Задаём активность актору, если приоритет выше текущей
                //         actor.Activity = new GoActivity(actor.GetState<State>().Job, newPriority);
            }
        }
    }
}
