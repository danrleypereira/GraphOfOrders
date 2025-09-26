import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as apigateway from 'aws-cdk-lib/aws-apigatewayv2';
import * as apigatewayIntegrations from 'aws-cdk-lib/aws-apigatewayv2-integrations';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as ssm from 'aws-cdk-lib/aws-ssm';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import { Construct } from 'constructs';

export interface DeliveryInfraStackProps extends cdk.StackProps {
  stage: string;
}

export class DeliveryInfraStack extends cdk.Stack {
  private _vpc: ec2.Vpc;
  private _database: rds.DatabaseInstance;
  private _cluster: ecs.Cluster;
  private _loadBalancer: elbv2.ApplicationLoadBalancer;
  private _apiGateway: apigateway.HttpApi;

  public get vpc(): ec2.Vpc { return this._vpc; }
  public get database(): rds.DatabaseInstance { return this._database; }
  public get cluster(): ecs.Cluster { return this._cluster; }
  public get loadBalancer(): elbv2.ApplicationLoadBalancer { return this._loadBalancer; }
  public get apiGateway(): apigateway.HttpApi { return this._apiGateway; }

  constructor(scope: Construct, id: string, props: DeliveryInfraStackProps) {
    super(scope, id, props);

    const { stage } = props;

    // VPC with 2 AZs, private subnets, and NAT Gateway
    this._vpc = new ec2.Vpc(this, 'DeliveryVpc', {
      vpcName: `delivery-vpc-${stage}`,
      maxAzs: 2,
      subnetConfiguration: [
        {
          cidrMask: 24,
          name: 'Public',
          subnetType: ec2.SubnetType.PUBLIC,
        },
        {
          cidrMask: 24,
          name: 'Private',
          subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
        },
      ],
      natGateways: 1, // One NAT Gateway for cost optimization
      enableDnsHostnames: true,
      enableDnsSupport: true,
    });

    // Security Groups
    const rdsSecurityGroup = new ec2.SecurityGroup(this, 'RdsSecurityGroup', {
      vpc: this.vpc,
      description: 'Security group for RDS PostgreSQL',
      allowAllOutbound: false,
    });

    const ecsSecurityGroup = new ec2.SecurityGroup(this, 'EcsSecurityGroup', {
      vpc: this.vpc,
      description: 'Security group for ECS Fargate tasks',
      allowAllOutbound: true,
    });

    const albSecurityGroup = new ec2.SecurityGroup(this, 'AlbSecurityGroup', {
      vpc: this.vpc,
      description: 'Security group for Application Load Balancer',
      allowAllOutbound: true,
    });

    // Security Group Rules
    // ALB accepts HTTP traffic from anywhere (will be restricted by API Gateway)
    albSecurityGroup.addIngressRule(
      ec2.Peer.anyIpv4(),
      ec2.Port.tcp(80),
      'Allow HTTP traffic from anywhere'
    );

    // ECS accepts traffic from ALB
    ecsSecurityGroup.addIngressRule(
      albSecurityGroup,
      ec2.Port.tcp(8080),
      'Allow traffic from ALB to ECS'
    );

    // RDS accepts PostgreSQL traffic from ECS
    rdsSecurityGroup.addIngressRule(
      ecsSecurityGroup,
      ec2.Port.tcp(5432),
      'Allow PostgreSQL traffic from ECS'
    );

    this.setupDatabase(stage, rdsSecurityGroup);
    this.setupEcsInfrastructure(stage, ecsSecurityGroup, albSecurityGroup);
    this.setupApiGateway(stage);
    this.setupLoggingAndParameters(stage);
  }

  private setupDatabase(stage: string, securityGroup: ec2.SecurityGroup) {
    // Database credentials secret
    const dbSecret = new secretsmanager.Secret(this, 'DbSecret', {
      secretName: `delivery-db-credentials-${stage}`,
      description: 'Delivery service database credentials',
      generateSecretString: {
        secretStringTemplate: JSON.stringify({ username: 'delivery_admin' }),
        generateStringKey: 'password',
        excludeCharacters: '"@/\\\'',
        passwordLength: 32,
      },
    });

    // RDS Subnet Group
    const dbSubnetGroup = new rds.SubnetGroup(this, 'DbSubnetGroup', {
      vpc: this.vpc,
      description: 'Subnet group for delivery database',
      vpcSubnets: {
        subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
      },
    });

    // RDS PostgreSQL Instance
    this._database = new rds.DatabaseInstance(this, 'DeliveryDatabase', {
      instanceIdentifier: `delivery-db-${stage}`,
      engine: rds.DatabaseInstanceEngine.postgres({
        version: rds.PostgresEngineVersion.VER_16_10,
      }),
      instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO),
      credentials: rds.Credentials.fromSecret(dbSecret),
      vpc: this.vpc,
      subnetGroup: dbSubnetGroup,
      securityGroups: [securityGroup],
      databaseName: 'delivery',
      backupRetention: cdk.Duration.days(7),
      deleteAutomatedBackups: true,
      deletionProtection: false, // Set to true in production
      multiAz: false, // Set to true in production
      storageEncrypted: true,
      monitoringInterval: cdk.Duration.seconds(60),
      enablePerformanceInsights: true,
      performanceInsightRetention: rds.PerformanceInsightRetention.DEFAULT,
      removalPolicy: cdk.RemovalPolicy.DESTROY, // Change to RETAIN in production
    });

    // Output database endpoint for reference
    new cdk.CfnOutput(this, 'DatabaseEndpoint', {
      value: this._database.instanceEndpoint.hostname,
      description: 'RDS PostgreSQL endpoint',
    });

    new cdk.CfnOutput(this, 'DatabaseSecretArn', {
      value: dbSecret.secretArn,
      description: 'Database credentials secret ARN',
    });
  }

  private setupEcsInfrastructure(
    stage: string,
    ecsSecurityGroup: ec2.SecurityGroup,
    albSecurityGroup: ec2.SecurityGroup
  ) {
    // ECS Cluster
    this._cluster = new ecs.Cluster(this, 'DeliveryCluster', {
      clusterName: `delivery-cluster-${stage}`,
      vpc: this.vpc,
      containerInsights: true,
    });

    // Application Load Balancer
    this._loadBalancer = new elbv2.ApplicationLoadBalancer(this, 'DeliveryALB', {
      loadBalancerName: `delivery-alb-${stage}`,
      vpc: this.vpc,
      internetFacing: false, // Internal ALB for API Gateway integration
      securityGroup: albSecurityGroup,
      vpcSubnets: {
        subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
      },
    });

    // Target Group for ECS Service
    const targetGroup = new elbv2.ApplicationTargetGroup(this, 'DeliveryTargetGroup', {
      targetGroupName: `delivery-tg-${stage}`,
      port: 8080,
      protocol: elbv2.ApplicationProtocol.HTTP,
      vpc: this.vpc,
      targetType: elbv2.TargetType.IP,
      healthCheck: {
        enabled: true,
        path: '/health',
        healthyHttpCodes: '200',
        interval: cdk.Duration.seconds(30),
        timeout: cdk.Duration.seconds(5),
        healthyThresholdCount: 2,
        unhealthyThresholdCount: 3,
      },
    });

    // ALB Listener
    const listener = this._loadBalancer.addListener('DeliveryListener', {
      port: 80,
      protocol: elbv2.ApplicationProtocol.HTTP,
      defaultTargetGroups: [targetGroup],
    });

    // Task Definition
    const taskDefinition = new ecs.FargateTaskDefinition(this, 'DeliveryTaskDefinition', {
      family: `delivery-task-${stage}`,
      cpu: 256,
      memoryLimitMiB: 512,
    });

    // Container Definition (placeholder - will be updated when we have the Docker image)
    const container = taskDefinition.addContainer('DeliveryContainer', {
      containerName: 'delivery-api',
      image: ecs.ContainerImage.fromRegistry('nginx:latest'), // Placeholder
      portMappings: [
        {
          containerPort: 8080,
          protocol: ecs.Protocol.TCP,
        },
      ],
      logging: ecs.LogDrivers.awsLogs({
        streamPrefix: 'delivery-api',
        logGroup: new logs.LogGroup(this, 'DeliveryLogGroup', {
          logGroupName: `/ecs/delivery-api-${stage}`,
          retention: logs.RetentionDays.ONE_WEEK,
          removalPolicy: cdk.RemovalPolicy.DESTROY,
        }),
      }),
      environment: {
        ASPNETCORE_ENVIRONMENT: stage === 'prod' ? 'Production' : 'Development',
        ASPNETCORE_URLS: 'http://+:8080',
      },
      secrets: {
        DATABASE_CONNECTION_STRING: ecs.Secret.fromSecretsManager(
          secretsmanager.Secret.fromSecretCompleteArn(
            this,
            'ImportedDbSecret',
            this._database.secret!.secretArn
          )
        ),
      },
    });

    // ECS Service
    const service = new ecs.FargateService(this, 'DeliveryService', {
      serviceName: `delivery-service-${stage}`,
      cluster: this._cluster,
      taskDefinition,
      desiredCount: 1,
      assignPublicIp: false,
      securityGroups: [ecsSecurityGroup],
      vpcSubnets: {
        subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
      },
    });

    // Attach service to target group
    service.attachToApplicationTargetGroup(targetGroup);

    // Outputs
    new cdk.CfnOutput(this, 'LoadBalancerDNS', {
      value: this._loadBalancer.loadBalancerDnsName,
      description: 'Application Load Balancer DNS name',
    });

    new cdk.CfnOutput(this, 'EcsClusterName', {
      value: this._cluster.clusterName,
      description: 'ECS Cluster name',
    });
  }

  private setupApiGateway(stage: string) {
    // VPC Link for API Gateway to connect to internal ALB
    const vpcLink = new apigateway.VpcLink(this, 'DeliveryVpcLink', {
      vpc: this.vpc,
      subnets: {
        subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
      },
    });

    // HTTP API Gateway
    this._apiGateway = new apigateway.HttpApi(this, 'DeliveryApiGateway', {
      apiName: `delivery-api-${stage}`,
      description: 'Delivery Service API Gateway',
      corsPreflight: {
        allowHeaders: ['Content-Type', 'X-Amz-Date', 'Authorization', 'X-Api-Key'],
        allowMethods: [
          apigateway.CorsHttpMethod.GET,
          apigateway.CorsHttpMethod.POST,
          apigateway.CorsHttpMethod.PUT,
          apigateway.CorsHttpMethod.DELETE,
          apigateway.CorsHttpMethod.OPTIONS,
        ],
        allowOrigins: ['*'], // Restrict in production
        maxAge: cdk.Duration.days(10),
      },
    });

    // Integration with ALB
    const albIntegration = new apigatewayIntegrations.HttpAlbIntegration(
      'AlbIntegration',
      this._loadBalancer.listeners[0],
      {
        vpcLink,
      }
    );

    // Routes for delivery service
    this._apiGateway.addRoutes({
      path: '/deliveryPerson/{proxy+}',
      methods: [apigateway.HttpMethod.ANY],
      integration: albIntegration,
    });

    this._apiGateway.addRoutes({
      path: '/deliveryOrder/{proxy+}',
      methods: [apigateway.HttpMethod.ANY],
      integration: albIntegration,
    });

    this._apiGateway.addRoutes({
      path: '/health',
      methods: [apigateway.HttpMethod.GET],
      integration: albIntegration,
    });

    // Default stage
    const defaultStage = new apigateway.HttpStage(this, 'DefaultStage', {
      httpApi: this._apiGateway,
      stageName: stage,
      autoDeploy: true,
    });

    // Output API Gateway URL
    new cdk.CfnOutput(this, 'ApiGatewayUrl', {
      value: this._apiGateway.apiEndpoint,
      description: 'API Gateway endpoint URL',
    });
  }

  private setupLoggingAndParameters(stage: string) {
    // CloudWatch Log Groups
    const apiGatewayLogGroup = new logs.LogGroup(this, 'ApiGatewayLogGroup', {
      logGroupName: `/aws/apigateway/delivery-api-${stage}`,
      retention: logs.RetentionDays.ONE_WEEK,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    // SSM Parameters for service configuration
    new ssm.StringParameter(this, 'DatabaseEndpointParameter', {
      parameterName: `/delivery/${stage}/database/endpoint`,
      stringValue: this._database.instanceEndpoint.hostname,
      description: 'Database endpoint for delivery service',
    });

    new ssm.StringParameter(this, 'DatabaseSecretArnParameter', {
      parameterName: `/delivery/${stage}/database/secret-arn`,
      stringValue: this._database.secret!.secretArn,
      description: 'Database secret ARN for delivery service',
    });

    new ssm.StringParameter(this, 'ApiGatewayUrlParameter', {
      parameterName: `/delivery/${stage}/api/base-url`,
      stringValue: this._apiGateway.apiEndpoint,
      description: 'API Gateway base URL for delivery service',
    });

    new ssm.StringParameter(this, 'VpcIdParameter', {
      parameterName: `/delivery/${stage}/vpc/id`,
      stringValue: this.vpc.vpcId,
      description: 'VPC ID for delivery service',
    });

    new ssm.StringParameter(this, 'ClusterArnParameter', {
      parameterName: `/delivery/${stage}/ecs/cluster-arn`,
      stringValue: this._cluster.clusterArn,
      description: 'ECS Cluster ARN for delivery service',
    });

    // Output SSM parameter names
    new cdk.CfnOutput(this, 'SsmParameterPrefix', {
      value: `/delivery/${stage}/`,
      description: 'SSM Parameter prefix for delivery service configuration',
    });
  }
}