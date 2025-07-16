using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WriterLambda;

public class Foo
{
    public string Name { get; set; } = string.Empty;
}

public class Function(IAmazonSQS sqsClient)
{
    public Function() : this(new AmazonSQSClient())
    {
    }

    /// <summary>
    /// Lambda function that writes a message to an SQS queue
    /// </summary>
    /// <param name="input">The message to send to the SQS queue.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<string> FunctionHandler(Foo input, ILambdaContext context)
    {
        var queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL");
        context.Logger.LogInformation($"Queue URL: {queueUrl}");
        context.Logger.LogInformation($"Message: {input}");
        
        if (string.IsNullOrEmpty(queueUrl))
        {
            throw new InvalidOperationException("QUEUE_URL environment variable is not set");
        }

        var json = JsonSerializer.Serialize(input);

        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = json,
            MessageGroupId = Guid.NewGuid().ToString(),
            MessageDeduplicationId = Guid.NewGuid().ToString()
        };

        var response = await sqsClient.SendMessageAsync(sendMessageRequest);
        
        context.Logger.LogInformation($"Message sent to queue. MessageId: {response.MessageId}");
        
        return response.MessageId;
    }
}