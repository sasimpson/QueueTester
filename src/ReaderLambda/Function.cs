using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReaderLambda;

public class Foo
{
    public string Name { get; set; } = string.Empty;
}

public class Function
{
    /// <summary>
    /// Lambda function that processes SQS messages
    /// </summary>
    /// <param name="sqsEvent">The SQS event containing messages to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public void FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            context.Logger.LogInformation($"Message received from queue ID: {record.MessageId}");
            var messageBody = JsonSerializer.Deserialize<Foo>(record.Body);
            context.Logger.LogInformation($"Processing message: {messageBody?.Name}");
        }
    }
}