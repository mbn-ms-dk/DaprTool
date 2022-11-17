# This script issues and displays the correct dapr run command for running with
# local or cloud resources. To run in the clould add -env azure parameter. If the
# script determines the infrastructure has not been deployed it will call the
# setup script first.
[CmdletBinding()]
param (
    [Parameter(
        Position = 0,
        HelpMessage = "The name of the resource group to be created. All resources will be place in the resource group and start with name."
    )]
    [string]
    $rgName = "dapr_statestore_demo",

    [Parameter(
        Position = 1,
        HelpMessage = "The location to store the meta data for the deployment."
    )]
    [string]
    $location = "westeurope",

    [Parameter(
        HelpMessage = "Set to the location of the resources to use."
    )]
    [ValidateSet("local", "azure", "aws")]
    [string]
    $env = "local",

    [Parameter(
        HelpMessage = "When provided deploys the cloud infrastructure without running the demo"
    )]
    [switch]
    $deployOnly
)

# This function will run an bicep deployment to deploy all the required
# resources into Azure. All the keys, tokens and endpoints will be
# automatically retreived and written to ./components/azure/local_secrets.json.
# PowerShell Core 7 (runs on macOS, Linux and Windows)
# Azure CLI (log in, runs on macOS, Linux and Windows)
function Deploy-AzureInfrastructure {
    [CmdletBinding()]
    param (
        [Parameter(
            Position = 0,
            HelpMessage = "The name of the resource group to be created. All resources will be place in the resource group and start with name."
        )]
        [string]
        $rgName,

        [Parameter(
            Position = 1,
            HelpMessage = "The location to store the meta data for the deployment."
        )]
        [string]
        $location
    )

    begin {
        Push-Location -Path './deploy'
    }

    process {
        Write-Output 'Deploying the Azure infrastructure'
        $deployment = $(az deployment sub create --name $rgName `
                --location $location `
                --template-file ./azure/main.bicep `
                --parameters location=$location `
                --parameters rgName=$rgName `
                --output json) | ConvertFrom-Json

        # Store the outputs from the deployment to create
        # ./components/azure/local_secrets.json
        $cosmosDbName = $deployment.properties.outputs.cosmosDbName.value
        $cosmosDbEndpoint = $deployment.properties.outputs.cosmosDbEndpoint.value
        
        $cosmosDbKey = $(az cosmosdb keys list `
                --name $cosmosDbName `
                --resource-group $rgName `
                --query primaryMasterKey `
                --output tsv)

        Write-Verbose "cosmosDbKey = $cosmosDbKey"
        Write-Verbose "cosmosDbEndpoint = $cosmosDbEndpoint"

        # Creating components/azure/local_secrets.json
        $secrets = [PSCustomObject]@{
            url = $cosmosDbEndpoint
            key = $cosmosDbKey
        }

        Write-Output 'Saving ./components/azure/local_secrets.json for local secret store'
        $secrets | ConvertTo-Json | Set-Content ../components/azure/local_secrets.json
    }
    
    end {
        Pop-Location
    }
}

# This will deploy the infrastructure without running the demo. You can use
# this flag to set everything up before you run the demos to save time. Some
# infrastucture can take some time to deploy.
if ($deployOnly.IsPresent) {
    # Deploy-AWSInfrastructure
    Deploy-AzureInfrastructure -rgName $rgName -location $location
    
    return
}

# Load the sample requests file for the demo
code ./sampleRequests.http

if ($env -eq "azure") {
    # If you don't find the ./components/azure/local_secrets.json deploy infrastucture
    if ($(Test-Path -Path './components/azure/local_secrets.json') -eq $false) {
        Write-Output "Could not find ./components/azure/local_secrets.json"
        Deploy-AzureInfrastructure -rgName $rgName -location $location
    }
}

Write-Output "Running demo with $env resources"
Write-Output "dapr run --app-id $env --dapr-http-port 3500 --components-path ./components/$env `n"
dapr run --app-id $env --dapr-http-port 3500 --components-path ./components/$env