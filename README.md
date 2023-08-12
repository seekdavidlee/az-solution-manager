# Introduction

Based on a manifest you have defined to mark resource groups and resources, Az Solution Manager will be able to create references to mark resource groups and resources even if they do not exist yet. During deployment time, if you need reference the marked resource groups and resources, such as for getting Resource Id, Resource Name or Resource Group Name, you can use Az Solution Manager to pull them. For example, you may need to know an Azure Continer Registry (ACR) name during deployment so you pull container image from. Rather than hardcoding the ACR name as part of your deployment variable, you can use Az Solution Manager to pull that out.

## Getting Started

Make sure you are already logged in via Azure CLI and you have selected the appropriate Azure Subscription before starting.

For the first step, you need to setup your Azure Subscription. Underneth the hoods, Az Solution Manager will create the appropriate Azure Policies that will help create references. A managed identity is used to provide access to perform this. You must access to perform role assignments.

```bash
asm init --resource-group-name asm --location centralus --managed-identity asm-identity
```

Now, we are ready to apply the manifest on your Azure Subscription. The manifest contains your solution definations.

```bash
asm apply -f manifest.json
```

To remove the solution, use the destroy flag.

```bash
asm destroy --asm-sol mysolution --asm-env dev
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
