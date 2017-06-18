# StudentIT Roster Summary
Managers need to verify hours against the roster before they can approve timecards. This program creates an email summary of the last roster period (2 weeks) to assist in this process.

## Prerequisites
* An AWS account with AWS Lambda access

## Development Environment

1. Install .NET Core (https://www.microsoft.com/net/core)
2. Install NodeJS (https://nodejs.org/en/download/)
3. Install Serverless Framework (https://serverless.com/framework/docs/getting-started/)
4. Set up AWS credentials

```
serverless config credentials -p aws --key $YOURKEY --secret $YOURSECRET
```

## Building
### Linux

```
./build.sh
```

### Windows

```
./build.ps1
```

## Deployment
Valid stages are: dev / test / production

### Uploading a new version
#### Infrastructure changes

```
serverless deploy --stage production -s $STAGE
```

#### Code changes

```
serverless deploy function -f summary -s $STAGE
```

### Removing all resources

```
serverless remove
```

## Usage
### Invoking

```
serverless invoke -f summary -s $STAGE -l
```

### Logs

```
serverless logs -f summary -s $STAGE -t
```


## Libraries
* Serverless (https://github.com/serverless/serverless/)
* Google API Dotnet Client (https://github.com/google/google-api-dotnet-client)
* AWS Lambda Dotnet (https://github.com/aws/aws-lambda-dotnet)
