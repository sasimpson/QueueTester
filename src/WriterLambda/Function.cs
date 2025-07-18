using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Lambda.APIGatewayEvents;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WriterLambda;

public class Function(IAmazonSQS sqsClient)
{
    public Function() : this(new AmazonSQSClient())
    {
    }

    /// <summary>
    /// Lambda function that writes a message to an SQS queue
    /// </summary>
    /// <param name="request">The incoming api request</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        var queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL");
        context.Logger.LogInformation($"Queue URL: {queueUrl}");
        
        context.Logger.LogInformation($"Message: {request.Body}");
        
        if (string.IsNullOrEmpty(queueUrl))
        {
            throw new InvalidOperationException("QUEUE_URL environment variable is not set");
        }

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var message = JsonSerializer.Deserialize<Domain.Message>(request.Body, options: jsonOptions);
        var sendMessageRequest = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageGroupId = Guid.NewGuid().ToString(),
            MessageDeduplicationId = Guid.NewGuid().ToString()
        };

        var response = await sqsClient.SendMessageAsync(sendMessageRequest);
        
        context.Logger.LogInformation($"Body {message.Body}");
        context.Logger.LogInformation($"Message sent to queue. MessageId: {response.MessageId}");

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = response.MessageId,
            Headers = new Dictionary<string, string>
            {
                {"x-app-lang", "csharp .net 8"}
            }
        };
    }
}