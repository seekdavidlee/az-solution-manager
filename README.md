# Introduction
Az Solution Manager streamlines the management of solutions in your Azure Subscription. You can think of a solution as a grouping of Azure Resource Groups and Resources that your application depends on. A solution can also be hosted in different environments - dev, prod, stage etc as well as in different regions - central us, west us, east us etc.

Az Solution Manager creates references to mark resource groups and resources as part of a solution you define - even if they do not exist yet. During deployment time, if you need reference marked resource groups and resources of a solution, such as for getting Resource Id, Resource Name or Resource Group Name, you can use Az Solution Manager to pull them. For example, you may need to know an Azure Continer Registry (ACR) name during deployment so you pull container image from. Rather than hardcoding the ACR name as part of your deployment variable, you can use Az Solution Manager to pull that out.

## Getting Started

Make sure you are already logged in via Azure CLI and you have selected the appropriate Azure Subscription before starting.

For the first step, you need to setup your Azure Subscription. Underneth the hoods, Az Solution Manager will create the appropriate Azure Policies that will help create references. A managed identity is used to provide access to perform this. You must access to perform role assignments.

```bash
asm init --resource-group-name asm --location centralus --managed-identity asm-identity
```

Now, we are ready to apply the manifest on your Azure Subscription. The manifest contains your solution definations.

```bash
asm manifest apply -f manifest.json
```

You can list all solutions hosted in your Azure Subscription with the solution command.

```bash
asm solution list
```

You can generate deployment parameter values based on a deployment parameters file where you can leverage built-in functions to do lookup with.

```powershell
$p = asm deployment parameters -f $file
$json = $p | ConvertTo-Json -Compress
$json = $json.Replace('"', '\"')
az deployment group create --resource-group $resourceGroupName --template-file deploy.bicep --parameters $json
```

You can lookup resource groups and resources with the lookup command.

```
asm lookup group --asm-sol $solutionId --asm-env $envName --asm-reg $region
```

```
asm lookup resource --asm-rid $resourceId --asm-sol $solutionId --asm-env $envName --asm-reg $region
```

You can perform role assignments with managed solution.

```
asm role assign --role-name $roleName --principal-id $principalId --principal-type $principalType --asm-sol $solutionId --asm-env $envName
```

To remove the solution, use the delete option.

```bash
asm solution delete --asm-sol mysolution --asm-env dev
```

## Flags

You can specify the following flags along with the command.

* t: Specifiy the tenant Id
* s: Specify the subscription id or name

## Manifest

The following are tags you can apply via your manifest.

### Tags

* asm-resource-id
* asm-solution-id
* asm-environment
* asm-region (optional)
* asm-internal-solution-id: Internal id. Do not use.

### Metadata

* Version is required. Please refer to this [documentation](https://learn.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#common-metadata-properties) for the structure used in versioning.
* Category is not required but recommended for filtering purposes. If you do not set a category, a default category will be used.
