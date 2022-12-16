# demos CLI Documentation

This is a [dotnet tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) showcasing a variaty of dapr demos.
## Prerequisites
The tool use [dapr](https://dapr.io/).

To setup dapr on your local machine:
1. Install dapr CLI [https://docs.dapr.io/getting-started/install-dapr-cli/]
2. Initialize dapr in your local environment [https://docs.dapr.io/getting-started/install-dapr-selfhost/]

An Azure subscription is required to demo in Azure environment.

Install the __demos__ tool:
1. Clone the repository
```bash
git clone https://github.com/mbn-ms-dk/DaprTool.git
```
2. Navigate to the `demos` folder and run the following command:

Install
```bash
dotnet tool install --global --add-source ./nupkg demos
```
Update
```bash
dotnet tool update --global --add-source ./nupkg demos
```
Verify it is installed:
```bash
demos -h
```
You should see the following:
![Image showing root command help](/demos/documentation/images/demos_main_help.png)

# demos Tool commands, subcommands and options
The root command is `demos` and will initially lauch the tool. 
Options are added to a command using hyphen(s) as in `--<option>` and subcommands are used in conjunction with the rootcommand 
(IE: `demos <subcommand>`).

## Options

To the root command there are options:
* `--readme` which is an option will open edge browser and show a more detailed readme for the different demos
* `--show` which will show a Figlet
* `--version` which will show the version number
* `-?`, `-h` or `--help` which will provide help information. __this option is available for all commands and subcommands__

Version & show options:

`--version` shows the version number.
```bash
demos --version
```
![image showing tool version](/demos/documentation/images/demos_version.png)

`--show` will show the following Figlet:
```bash
demos --show
```
![Image showing Figlet](/demos/documentation/images/demos_figlet.png)

## Subcommands
The tool have 3 subcommands:
1. __user__
2. __dapr__
3. __debug__

These will be described in the following sections.

### user command
The user command will shop the options in regard to authenticating your user against Azure. It will per default use the dault azure profile for your user using [Azure.Identity](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet).
The __user__ command does not have any subcommands, but it have some options which can be shown this way:
```bash
demos user -h
```
![Image showing demos user help](/demos/documentation/images/demos_user_help.png)

This command have 4 options:
1. `--default` which will show you your default subscription. This is the subscription that will be used, when working against Azure.
2. `--list-tenant` which will show you the tenants available for you
3. `--list-sub` which will show you subscriptions available for the tenant you're on
4. `--tenant` This option allows you to switch to a different tenant IE. `demos user --tenant <tenantid>`

#### Default Option
The full command with `--default` option:
```bash
demos user --default
```
Which will give you the following result:
![video showing the command](/demos/documentation/images/gifs/demos_user_def.gif)

### List Tenant Option
The full command with `--list-tenant` option:
```bash
demos user --list-tenant
```
Which will give you the following result as a dynamic table:
![video showing the command](/demos/documentation/images/gifs/demos_user_list_tenant.gif)

### List Subscription Option
The full command with `--list-sub` option:
```bash
demos user --list-sub
```
Which will give you the following result as a dynamic table:
![video showing the command](/demos/documentation/images/gifs/demos_user_list_sub.gif)

### Set Tenant Options
The full command with `--tenant <tenantId>` option
```bash
demos user --tenant <tenantId>
```
![video showing the command](/demos/documentation/images/gifs/demos_user_tenant.gif)

### dapr command
The __dapr__ command will show the different dapr demos available.

The following command will show the list of dapr demos available as subommands to the dapr command:
```bash
demos dapr -h
```
![Image showing dapr root command help](/demos/documentation/images/demos_dapr_help.png)

There are 5 different demos available as subcommands:
1. __state__ which is a dapr [state store](https://docs.dapr.io/developing-applications/building-blocks/state-management/) demo
2. __binding__ which is a dapr [binding](https://docs.dapr.io/developing-applications/building-blocks/bindings/) demo
3. __pubsub__ which is a dapr [Pub/sub](https://docs.dapr.io/reference/components-reference/supported-pubsub/) demo
4. __secrets__ which is a dapr [secrets](https://docs.dapr.io/developing-applications/building-blocks/secrets/) demo
5. __obs__ which is a dapr [observability](https://docs.dapr.io/developing-applications/building-blocks/observability/) demo

All these subommands have the same options available:
* `--deploy` which will deploy the required Azure environment to do the demo against Azure
* `--delete` which will delete the Azure resources created for the demo
* `-a|--azure` which will run the demo against Azure resources (creted with `--deploy` option). If subcommand is executed without options EG.  `demos dapr state` the demo will be run against local resources.
* `--describe` shows a shor description of the demo
* `--readme` as described in the first section

![Image showing demos dapr options](/demos/documentation/images/demos_dapr_options.png)

#### Statestore Demo Description
This demo showcases the option to run locally against _Redis_ and in cloud against _Azure CosmosDB_.

![Image showing service diagram](/demos/documentation/images/demos_dapr_state.png)

The component configuration is as following:

**local**

Created with the file _state.redis.yaml_

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"
```

**Azure**
When using the `--deploy` options a local file ( _local_secrets.json_ ) is created to be injected in to the yaml configuration of the component. Here the url and the key for the created _Azure CosmosDB_ will be stored.

![Image of json file](/demos/documentation/images/demos_dapr_state_azure_json.png)

The components used is a _secretsfile_ ( _secretstores.local.file.yaml_ ) component which reads the jason file and is used with _Azure CosmosDB_ setup.
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: strongbox
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "./components/state/azure/local_secrets.json"
```

and the _state.azure.cosmosdb.yaml_ file:
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
auth:
  secretStore: strongbox
spec:
  type: state.azure.cosmosdb
  version: v1
  metadata:
  - name: url
    secretKeyRef:
      key: url
      name: url
  - name: masterKey
    secretKeyRef:
      key: key
      name: key
  - name: database
    value: StateStore
  - name: collection
    value: StateStoreValues
```



#### Binding Demo Description
This demo showcases the option to run locally against a file folder and in cloud against _Azure Storage.

![Image showing service diagram](/demos/documentation/images/demos_dapr_binding.png)

**local**

Created with the _localstorage.yaml_ file, using a relative path _./tempfiles/_ 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: files
spec:
  type: bindings.localstorage
  version: v1
  metadata:
  - name: rootPath
    value: "./tempfiles/"
```

**Azure** 

When using the `--deploy` options a local file ( _local_secrets.json_ ) is created to be injected in to the yaml configuration of the component. Here the key and the storage account name for the created _Azure Storage_ will be stored.

![Image of json file](/demos/documentation/images/demos_dapr_binding_azure_json.png)

The components used is a _secretsfile_ ( _secretstores.local.file.yaml_ ) component which reads the json file and is used with [Azure Blob Storage](https://docs.dapr.io/reference/components-reference/supported-bindings/blobstorage/) setup.

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: strongbox
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "./components/binding/azure/local_secrets.json"
```

And the _azurestorage.yaml_ file
```yaml 
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: files
auth:
  secretStore: strongbox
spec:
  type: bindings.azure.blobstorage
  version: v1
  metadata:
  - name: storageAccount
    secretKeyRef:
      key: acct
      name: acct
  - name: storageAccessKey
    secretKeyRef:
      key: key
      name: key
  - name: container
    value: container1
  - name: decodeBase64
    value: false
  - name: getBlobRetryCount
    value: 3
```



#### Pubsub Demo Description

This demo showcases the option to run locally against _Redis_ both as a state store and using pubsub. In cloud the state store is  _Azure SQL_ and pubsub service 1 is using [Azure Service Bus](https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-azure-servicebus/) and pubsub service 2 is using [Azure Event Hub](https://docs.dapr.io/reference/components-reference/supported-pubsub/setup-azure-eventhubs/).

![Image showing the different services](/demos/documentation/images/demos_dapr_pubsub.png)

**local**

Running locally the state store file, _statestore.yaml_ is as follows

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
  - name: actorStateStore
    value: "true"

```

The pubsub 1 consist of the concomponent and the [subscription](https://docs.dapr.io/developing-applications/building-blocks/pubsub/subscription-methods/) files.
The component file _pubsub1.yaml_ 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub1
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: localhost:6379
  - name: redisPassword
    value: ""
```

And the subscription file _pubsub1_subscription.yaml_ 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: myevent-subscription
spec:
  topic: neworder
  route: /neworder
  pubsubname: pubsub1
scopes:
- app1
```

Same setup applies for pubsub service 2, except the pubsubname in the subscription file is _pubsub2_.

**Azure** 

When using the `--deploy` options a local file ( _local_secrets.json_ ) is created to be injected in to the yaml configuration of the component. In this file the following are added:
* The client IP address of your machine is added to create a Azure SQL firewall rule
* The database name
* The SQL connectionstring
* The eventhub endpoint
* The servicebus endpoint
* The storage account name
* The storageaccount key



![Image of json file](/demos/documentation/images/demos_dapr_pubsub_azure_json.png)

The components used is a _secretsfile_ ( _secretstores.local.file.yaml_ ) component which reads the json file 

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: strongbox
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "./components/binding/azure/local_secrets.json"
```

The state store _statestore.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
auth:
  secretStore: strongbox
spec:
  type: state.sqlserver
  version: v1
  metadata:
  - name: connectionString
    secretKeyRef:
      key: sqlConnectionString
      name: sqlConnectionString
```

The pubsub component for servicebus _servicebus.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub1
auth:
  secretStore: strongbox  
spec:
  type: pubsub.azure.servicebus
  version: v1
  metadata:
  - name: connectionString
    secretKeyRef:
      key: serviceBusEndpoint
      name: serviceBusEndpoint
```

The subscription for servicebus _servicebus_subscription.yaml_ 
```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: servicebus-subscription
spec:
  topic: neworder
  route: /neworder
  pubsubname: pubsub1
scopes:
- app1
```

The pubsub component for eventhub _eventhubs.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub2
auth:
  secretStore: strongbox    
spec:
  type: pubsub.azure.eventhubs
  version: v1
  metadata:
  - name: connectionString
    secretKeyRef:
      key: eventHubsEndpoint
      name: eventHubsEndpoint
  - name: storageAccountName
    secretKeyRef:
      key: storageAccountName
      name: storageAccountName
  - name: storageAccountKey
    secretKeyRef:
      key: storageAccountKey
      name: storageAccountKey
  - name: storageContainerName
    value: "neworder"
```
The subscription for eventhub _eventhubs_subscription.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Subscription
metadata:
  name: eventhubs-subscription
spec:
  topic: neworder
  route: /neworder
  pubsubname: pubsub2
scopes:
- app1
```

#### Secrets Demo Description
This demo is configured to use [Azure Key Vault](https://docs.dapr.io/reference/components-reference/supported-secret-stores/azure-keyvault/) in Azure and the local component is configured to use [local file](https://docs.dapr.io/reference/components-reference/supported-secret-stores/file-secret-store/).

![Image showing service diagram](/demos/documentation/images/demos_dapr_secrets.png)

**local**

Running locally there is a _local_secrets.json_ file which is the secret.
```json
{
  "my-secret": "My_Secret_From_Local_File"
}
```

The component configuration to use a local file as secret _secretstore.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: my-secrets-store
  namespace: default
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "./components/secrets/local/local_secrets.json"
```

**Azure**

When using the `--deploy` options a local file ( _local_secrets.json_ ) is created to be injected in to the yaml configuration of the component. In this process a Service principal is created to manage the access to _Azure Keyvault_.
The _local_secrets.json_ file is populated with service principal information as well as the keyvault name and tenant id.

![Image of json file](/demos/documentation/images/demos_dapr_secrets_azure_json.png)

To read the values from the json file _strongbox.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: strongbox
spec:
  type: secretstores.local.file
  version: v1
  metadata:
  - name: secretsFile
    value: "./components/secrets/azure/local_secrets.json"
```

The _Azure Keyvault_ component file _secretstore.yaml_
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: my-secrets-store
auth:
  secretStore: strongbox  
spec:
  type: secretstores.azure.keyvault
  version: v1
  metadata:
  - name: vaultName
    secretKeyRef:
      key: keyvaultName
      name: keyvaultName
  - name: azureTenantId
    secretKeyRef:
      key: tenant
      name: tenant
  - name: azureClientId
    secretKeyRef:
      key: appId
      name: appId
  - name: azureClientSecret
    secretKeyRef:
      key: password
      name: password
```

#### Observability Demo Description
This demo contains three services, Service A, B, and C. Service A subscribes to the **PubSub** component. When a new order is received (1) Service A calls Service B using service to service invocation (2). When Service A gets a response from Service B, Service A stores the processed order using the **StateStore** component (3). Finally, Service A publishes the order to the **PubSub** component where Service C reads it (4).

![Image showing service diagram](/demos/documentation/images/Services.png)

Furthermore this demo contains a _otel-local-config.yaml_ file that contains the configuration for the [Open Telemetry Collector used to send the data to Application Insights](https://docs.dapr.io/operations/monitoring/tracing/open-telemetry-collector-appinsights/). The Open Telemetry Collector is run in a local container. When the demo is running locally the telemetry is send to the _dapr_zipkin_ container this is installed during `dapr init` and can be viewed at http://localhost:9411/zipkin/

This demo also contains the option to run dapr with [Microsoft Tye](https://github.com/dotnet/tye) and the option is `----useTye`

**local**

The _tye_local.yaml_ file

```yaml
name: observability

services:
- name: serviceA
  executable: dapr
  args: run -a serviceA -p 5000 -H 3500 -- dotnet ./serviceA.dll --urls "http://localhost:5000"

- name: serviceB
  executable: dapr
  args: run -a serviceB -p 5010 -- dotnet ./serviceB.dll --urls "http://localhost:5010"

- name: serviceC
  executable: dapr
  args: run -a serviceC -p 5020 -- dotnet ./serviceC.dll --urls "http://localhost:5020"
```

**Azure**

The _tye_cloud.yaml_ file

```yaml
name: observability

services:
- name: serviceA
  executable: dapr
  args: run -a serviceA -p 5000 -H 3500 -- dotnet ./serviceA.dll --urls "http://localhost:5000"

- name: serviceB
  executable: dapr
  args: run -a serviceB -p 5010 -- dotnet ./serviceB.dll --urls "http://localhost:5010"

- name: serviceC
  executable: dapr
  args: run -a serviceC -p 5020 -- dotnet ./serviceC.dll --urls "http://localhost:5020"

- name: openTelemetry
  image: otel/opentelemetry-collector-contrib-dev
  bindings:
    - port: 9411
  volumes:
    - source: ../obs/config/azure/otel-local-config.yaml
      target: /etc/otel/config.yaml
```
The _otel-local-config.yaml_ file

```yaml
receivers:
  zipkin: 
extensions:
  health_check: 
  pprof:
    endpoint: :1888
  zpages:
    endpoint: :55679
exporters:
  logging:
    loglevel: debug
  azuremonitor:
    instrumentation_key: <AppInsights Instrumentation key>
    maxbatchinterval: 5s
    maxbatchsize: 5
service:
  extensions:
  - pprof
  - zpages
  - health_check
  pipelines:
    traces:
      receivers:
      - zipkin
      exporters:
      - azuremonitor
      - logging


```


### debug command
The __debug__ command list some variables to show the tools installation location.

This command have no other options than help: `-?|-h|--help`:
![Image showing debug help](/demos/documentation/images/demos_debug_help.png)

The full command
```bash
demos debug
```
![Image showing debug command](/demos/documentation/images/demos_debug.png)