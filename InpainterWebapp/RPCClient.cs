using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RpcClient : IDisposable
{
    private const string QUEUE_NAME = "celery";

    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

    public RpcClient()
    {
        var factory = new ConnectionFactory { HostName = "localhost", Port = 5672, UserName = "user", Password = "password" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        // declare a server-named queue
        replyQueueName = channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(consumer: consumer,
                             queue: replyQueueName,
                             autoAck: true);
    }

    public Task<string> CallAsync(object[] arguments, CancellationToken cancellationToken = default)
    {
        IDictionary<string, object> headers = new Dictionary<string, object>();
        headers.Add("task", "tasks.add");
        Guid guid = Guid.NewGuid();
        headers.Add("id", guid.ToString());

        IBasicProperties props = channel.CreateBasicProperties();
        props.CorrelationId = guid.ToString();
        props.Headers = headers;
        props.ContentEncoding = "utf-8";
        props.ContentType = "application/json";
        props.ReplyTo = replyQueueName;

        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(object[]));
        ser.WriteObject(stream, arguments);
        stream.Position = 0;
        StreamReader sr = new StreamReader(stream);
        string message = sr.ReadToEnd();

        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(guid.ToString(), tcs);

        channel.BasicPublish(exchange: string.Empty,
                             routingKey: QUEUE_NAME,
                             basicProperties: props,
                             body: messageBytes);

        cancellationToken.Register(() => callbackMapper.TryRemove(guid.ToString(), out _));
        return tcs.Task;
    }

    public void Dispose()
    {
        connection.Close();
    }
}