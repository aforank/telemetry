# Open Telemetry Demo (Ride Booking Application)
## Overall Architecture
![image](https://user-images.githubusercontent.com/19813442/156512361-0183728d-fef0-4977-a0ba-63b76e0f855c.png)

## Pre-rquisites
- Visual Studio 19 or greater (Visual Studio 22 Recommended)
- NET 6
- Azure Subscription

## Local Apps/Resources
- MongoDB on local
- SQL on local
- Jaeger (Optional) For traces
- Prometheus (Optional) - For metrics
- Seq (Optional) - For logs

### Install Mongo DB
- Install Mongo DB for Windows from https://docs.mongodb.com/manual/tutorial/install-mongodb-on-windows/
- Install Mongo DB Compass (GUI) from https://www.mongodb.com/try/download/compass
- Mongo DB usually runs on mongodb://localhost:27017 locally

### Install SQL
- Install SSMS
- Create a new database "CustomerDB" on your local MS Server.
- Table and Records will be auto seeded when you run the application.

### Install Jaeger (Optional)
- Download Jaeger from https://github.com/jaegertracing/jaeger/releases/download/v1.31.0/jaeger-1.31.0-windows-amd64.tar.gz
- Unzip and open command prompt
- Run `jaeger-all-in-one --collector.zipkin.host-port=:9411`
- Navigate to http://localhost:16686 to ensure it is running

### Prometheus (Optional)
- Download Prometheus from https://github.com/prometheus/prometheus/releases/download/v2.33.4/prometheus-2.33.4.windows-amd64.zip
- Unzip and open command prompt
- Run prometheus --config.file=prometheus.yml
- Navigate to http://localhost:9090 to ensure it is running

### Seq (Optional)
- Download Seq https://datalust.co/download
- Install by clicking the exe file
- Navigate to http://localhost:5341 to ensure it is running

## Azure Resources
- Application Insights
- Service Bus
- Redis Cache
- Storage Account

### Application Insights
- Create a new Application Insights resource on your Azure subscription and keep the Instumentation key and Connection String with you.
- Update Instumentation key and Connection String in all the appsettings.json files (6 projects)

### Service Bus
- Create a new Service Bus resource and keep the connection string with you.
- Update connection string in Booking API, Payment API and Notifcation API appsettings.json files
- Create two queues with default settings
  - notifications
  - payments

### Azure Redis
- Create a new Redis cache resource and keep the connection string with you.
- Update connection string in Drivers API project (app settings.json file)

### Azure Storage
- Create a storage account and keep the connection string with you.
- Create a container "templates"
- Create a dummy file by the name of "Email.txt"
- Update connection string in Notifcation API project (app settings.json file)

## Run the application
- Download/Clone source code from this repository
- Build the solution file
- Set Startup Projects
  ![image](https://user-images.githubusercontent.com/19813442/156520048-da255e6d-e68a-4d36-ab62-01b782939624.png)
- Run the application
- Swagger UI will start up with just one API
- Try out the API by providing the CustomerId (Make sure there are no digits only alphabets)
- Hit the endpoint and see you have recieved TraceId as a response or not

## Monitoring
Wait for a few seconds and check your telemetry data
- Traces - Application Insights & Jaeger
- Logs - Seq and Console
- Metrics - Prometheus

# Give it a Star if you like it. Thanks!
