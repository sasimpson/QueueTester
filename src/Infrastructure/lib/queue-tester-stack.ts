import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';
import * as sqs from 'aws-cdk-lib/aws-sqs';
import * as kms from 'aws-cdk-lib/aws-kms';
import * as dotnet from '@aws-cdk/aws-lambda-dotnet';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import {SqsEventSource} from "aws-cdk-lib/aws-lambda-event-sources";

export class QueueTesterStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);
    
    const queueKey = new kms.Key(this, 'EncQueueKey', {
      alias: 'EncQueueKey',
    });
    const queue = new sqs.Queue(this, 'EncQueueTester', {
      fifo: true,
      enforceSSL: true,
      encryption: sqs.QueueEncryption.KMS,
      encryptionMasterKey: queueKey,
    });
    
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
    
    queue.grantSendMessages(writerLambda);
    queue.grantConsumeMessages(readerLambda);
    
    readerLambda.addEventSource(new SqsEventSource(queue, {
      batchSize: 10,
    }))
  }
}
