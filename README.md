# WatermarkRemover
<br>
InpainterModelZoo - питоновский проект, заготовка для зоопарка моделей, тут установлен Celery, на который будут ставится задачи из C#.
Сейчас есть только тестовая задача add в файлике tasks, которая складывает два числа и возвращает ответ в C#
<br><br>
InpainterWebapp - .NET проект, котоорый ставит задачи в rabbitMQ, файл RPCClient содержит контекст подключения к очереди Rabbit. Переменная QUEUE_NAME = 'çelery'
содержит название очереди входных задач (на картинке Queue input), которые будут ставиться на обработку в питон. Очереди на ответ задается автогенерируемое имя
(с этим надо будет разобраться подробнее, я так понял из документации по RPC, что авто имя удобнее для самого RabbitMQ).
<br>
Из контроллера происходит вызов RPC и задача летит в очередь, обрабатывается питоном и летит обратно, пока всё просто печаетается в отладочную консольку и запускается 
на обновление страницы в браузере, потому что в контроллере вызов стоит в Index методе:)