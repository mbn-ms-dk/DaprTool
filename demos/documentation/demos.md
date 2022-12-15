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
* `--readme` which is a global command (can be added to all commands and subcommands) will open edge browser and show a more detailed readme for the different demos
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

### debug command
The __debug__ command list some variables to show the tools installation location.

This command have no other options than help: `-?|-h|--help`:
![Image showing debug help](/demos/documentation/images/demos_debug_help.png)

The full command
```bash
demos debug
```
![Image showing debug command](/demos/documentation/images/demos_debug.png)