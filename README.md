# QueueTester

this is a test of how to use a simple Lambda -> SQS -> Lambda process with encryption. 

## How to use

the `src/Infrastructure` directory contains the CDK stack

    cd src/Infrastructure
    cdk deploy

you can execute the lambda function via the test console with sample input:

    { "name": "testing" }

or can use the AWS CLI like so: 

    aws lambda invoke --function-name myFunction --payload '{"key1":"value1"}' outputfile.txt

you should see the ouput in CloudWatch logs. 

## Why?

Proof of concept for .NET AWS Service interaction, thats it. 
