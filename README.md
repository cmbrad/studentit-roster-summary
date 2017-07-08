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
./scripts/build.sh
```

### Windows

```
powershell .\scripts\build.ps1
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

### Helpful commands

```
powershell .\scripts\build.ps1 && serverless deploy -s dev && serverless invoke -f summary
serverless logs -f summary -t
```

### Email -> Name translation
The translation depends on a file called people.db in the extra folder. It is a sqlite database of the form

```
CREATE TABLE "Employees" ( `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `name` TEXT NOT NULL, `email` TEXT NOT NULL UNIQUE )
```

Insert people into the database with

```
INSERT INTO Employees ('name', 'email') VALUES ('YOUR_NAME', 'YOUR_EMAIL')
```

## Libraries
* Serverless (https://github.com/serverless/serverless/)
* Google API Dotnet Client (https://github.com/google/google-api-dotnet-client)
* AWS Lambda Dotnet (https://github.com/aws/aws-lambda-dotnet)
