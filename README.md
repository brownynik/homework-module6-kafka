Учебный проект по Kafka.<br><br>
Kafka и ZooKeeper развёрнуты в Docker на удалённом хосте 192.168.1.37.<br>
Приложены файлы compose.<br><br>
Продюссер и консьюмер работают на фиксированном топике, который надо предварительно создать:<br>
kafka-topics --create --topic orders-topic --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1
<br><br>
Оба проекта работают в цикле, выход из цикла - клавиша ESC.<br>
Отправка синхронная, получение по схеме at least once.

