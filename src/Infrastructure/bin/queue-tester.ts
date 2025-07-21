#!/usr/bin/env node
import * as cdk from 'aws-cdk-lib';
import { QueueTesterStack } from '../lib/queue-tester-stack';
import {Tags} from "aws-cdk-lib";

const app = new cdk.App();
const stack = new QueueTesterStack(app, 'QueueTesterStack', {});
Tags.of(stack).add("app", "queue-tester")