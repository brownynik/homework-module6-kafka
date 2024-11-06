using Avro;
using Avro.IO;
using Avro.Specific;
using Confluent.Kafka;
using restaurant.com;

class Program
{
    static void Main(string[] args)
    {
        // Конфигурация Consumer для чтения сообщений из Kafka
        var consumerConfig = new ConsumerConfig
        {
            GroupId = "at-least-once-group",  // Уникальная группа потребителей
            BootstrapServers = "192.168.1.37:9092",  // Адрес Kafka брокера
            AutoOffsetReset = AutoOffsetReset.Earliest,  // Начало чтения с самого раннего сообщения
            EnableAutoCommit = false  // Отключаем автоматическое коммитирование смещений
        };

        // Создание Consumer для получения сообщений
        using var consumer = new ConsumerBuilder<Ignore, byte[]>(consumerConfig).Build();
        consumer.Subscribe("orders-topic");  // Подписка на топик

        long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        long prevtimestamp = milliseconds;

        // Нажмите ESC для остановки цикла.
        do
        {
            while (!Console.KeyAvailable)
            {
                milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                if (milliseconds % 1000 == 0 && prevtimestamp != milliseconds)
                {
                    Console.WriteLine("Генерация сообщения. Нажмите ESC для остановки цикла.");
                    prevtimestamp = milliseconds;

                    try
                    {
                        var consumeResult = consumer.Consume(CancellationToken.None);
                        var order = DeserializeAvroMessage(consumeResult.Message.Value);

                        Console.WriteLine($"Получен заказ OrderID = {order.OrderID}, полная стоимость = {order.TotalCost}, метка времени {order.CreatedAt}.");
                        foreach (var item in order.Dishes)
                        {
                            Console.WriteLine($"Блюдо {item.DishName}, стоимость = {item.DishCost}.");
                        }

                        consumer.Commit(consumeResult);
                        Console.WriteLine(new string('-', 40));
                        Console.WriteLine();
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error.Reason}");
                    }
                }
            }
        } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
    }

    // Метод для десериализации Avro-сообщения
    static DishOrder DeserializeAvroMessage(byte[] avroData)
    {
        // Определяем схему User
        var dishorderSchema = DishOrder._SCHEMA;
        var dishSchema = Dish._SCHEMA;

        // Десериализация Avro-сообщения из двоичных данных
        using (var stream = new MemoryStream(avroData))
        {
            var reader = new BinaryDecoder(stream);
            var datumReader = new SpecificDatumReader<DishOrder>(dishorderSchema, dishorderSchema);

            // Чтение и десериализация данных
            return datumReader.Read(null, reader);
        }
    }
}
