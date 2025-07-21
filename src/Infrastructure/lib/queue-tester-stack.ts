import * as cdk from "aws-cdk-lib";
import { Construct } from "constructs";
import * as sns from "aws-cdk-lib/aws-sns";
import * as sns_sub from "aws-cdk-lib/aws-sns-subscriptions";
import * as sqs from "aws-cdk-lib/aws-sqs";
import * as kms from "aws-cdk-lib/aws-kms";
import * as dotnet from "@aws-cdk/aws-lambda-dotnet";
import * as lambda from "aws-cdk-lib/aws-lambda";
import * as apiGatewayV2 from "aws-cdk-lib/aws-apigatewayv2";
import * as integrations from "aws-cdk-lib/aws-apigatewayv2-integrations";
import {SqsEventSource} from "aws-cdk-lib/aws-lambda-event-sources";

export class QueueTesterStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);


    const snsKey = new kms.Key(this, "EncSnsKey", {
      alias: "alias/QueueTesterStack-EncSnsKey"
    })

    const topic = new sns.Topic(this, "EncTopic", {
      displayName: "QueueTesterStack-EncSnsTopic",
      masterKey: snsKey,
      fifo: true
    })

    const queueKey = new kms.Key(this, 'EncQueueKey', {
      alias: "alias/QueueTesterStack-EncQueueKey",
    });
    const queue = new sqs.Queue(this, 'EncQueueTester', {
      fifo: true,
      enforceSSL: true,
      encryption: sqs.QueueEncryption.KMS,
      encryptionMasterKey: queueKey,
      visibilityTimeout: cdk.Duration.seconds(120)
    });

    topic.addSubscription(new sns_sub.SqsSubscription(queue));

    const readerLambda = new dotnet.DotNetFunction(this, 'ReaderLambda', {
      projectDir: '../../src/ReaderLambda',
      loggingFormat: lambda.LoggingFormat.JSON,
      timeout: cdk.Duration.seconds(60)
    })
    
    const writerLambda = new dotnet.DotNetFunction(this, 'WriterLambda', {
      projectDir: '../../src/WriterLambda',
      loggingFormat: lambda.LoggingFormat.JSON,
      environment: {["QUEUE_URL"]: queue.queueUrl},
      timeout: cdk.Duration.seconds(60)
    })

    topic.grantPublish(writerLambda);
    queue.grantConsumeMessages(readerLambda);
    
    readerLambda.addEventSource(new SqsEventSource(queue, {
      batchSize: 10,
    }))
    
    const api = new apiGatewayV2.HttpApi(this, 'QueueTesterApi', {
      apiName: 'dotnet Queue Tester Service',
      description: 'This service serves queue testing functionality.'
    });
    
    const writerIntegration = new integrations.HttpLambdaIntegration('WriterIntegration', writerLambda);
    
    api.addRoutes({
      path: '/write',
      methods: [apiGatewayV2.HttpMethod.POST],
      integration: writerIntegration
    });
    
    new cdk.CfnOutput(this, 'ApiUrl', {value: api.apiEndpoint});
  }
}
