using Confluent.Kafka;
using System;
using System.Threading.Tasks;
using Avro.IO;
using Avro.Specific;
using kafka_producer;
using restaurant.com;
using Avro;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.1.37:9092" // Адрес вашего Kafka-брокера
        };

        using (var producer = new ProducerBuilder<Null, byte[]>(config).Build())
        {
            using (var sender = new SendToKafka(producer))
            {
                const int maxDishCount = 5;

                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                long prevtimestamp = milliseconds;
                long currentOrderId = 0;


                string[] dishnames = new string[] { "Тобико гункан", "Кани гункан", "Кани спайси гункан", "Угорь спайси гункан", "Сякэ суси", "Филадельфия гункан", "Эби гункан с авокадо" };
                Random rndDishName = new Random();
                Random rndPrice = new Random();
                Random rndDishCount = new Random();
                var schema = (LogicalSchema)Schema.Parse("{\"type\": \"bytes\", \"logicalType\": \"decimal\", \"precision\": 16, \"scale\": 2 }");

                // Нажмите ESC для остановки цикла.
                do
                {
                    while (!Console.KeyAvailable)
                    {
                        milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        if (milliseconds % 5000 == 0 && prevtimestamp!= milliseconds)
                        {
                            Console.WriteLine("Генерация сообщения. Нажмите ESC для остановки цикла.");
                            prevtimestamp = milliseconds;
                            currentOrderId++;

                            var dishData = new DishOrder
                            {
                                CreatedAt = DateTime.UtcNow,
                                OrderID = currentOrderId,
                                Dishes = new List<Dish>(),
                                TotalCost = (AvroDecimal)0.00M,
                            };

                            
                            var dishcount = rndDishCount.Next(1, maxDishCount + 1);

                            for (int i = 0; i < dishcount; i++) 
                            {
                                decimal cost = (decimal)rndPrice.Next(10000, 99999) / 100;

                                var dish = new Dish
                                {
                                    DishName = dishnames[rndDishName.Next(dishnames.Length)],
                                    DishCost = (AvroDecimal)decimal.Parse(cost.ToString("N2")),
                                };

                                dishData.TotalCost = (AvroDecimal)decimal.Parse((cost + (decimal)dishData.TotalCost.UnscaledValue / (decimal)Math.Pow(10, dishData.TotalCost.Scale)).ToString("N2"));
                                dishData.Dishes.Add(dish);
                            }

                            byte[] avroData;
                            using (var ms = new MemoryStream())
                            {
                                var writer = new BinaryEncoder(ms);
                                var datumWriter = new SpecificDatumWriter<DishOrder>(dishData.Schema);
                                datumWriter.Write(dishData, writer);
                                avroData = ms.ToArray();
                            }

                            // kafka-topics --create --topic orders-topic --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1
                            var result = sender.SendData("orders-topic", avroData);
                        }
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
        }
    }
}
