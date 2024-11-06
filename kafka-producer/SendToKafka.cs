using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

namespace kafka_producer
{
    internal class SendToKafka : IDisposable
    {
        private readonly IProducer<Null, byte[]> _producer;

        public SendToKafka(IProducer<Null, byte[]> producer)
        {
            if (producer == null)
            {
                throw new ArgumentNullException(nameof(producer));
            }

            _producer = producer;
        }


        public DeliveryResult<Null, byte[]>? SendData(string topic, byte[] container)
        {
            try
            {
                var result = _producer.ProduceAsync(topic,
                new Message<Null, byte[]> { Value = container })
                                              .GetAwaiter()
                                              .GetResult();

                Console.WriteLine($"Message '{result.Value}' sent to '{result.TopicPartitionOffset}'");
                return result;
            }
            catch (ProduceException<Null, byte[]> e)
            {
                Console.WriteLine($"Failed to send message: {e.Error.Reason}");
                return null;
            }
        }

        void IDisposable.Dispose()
        {
        }
    }
}
