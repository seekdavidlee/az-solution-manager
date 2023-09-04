# Introduction

Make sure you are already logged in via Azure CLI and you have selected the appropriate Azure Subscription before starting.

Initialize your Azure Subscription.

```bash
asm init --resource-group-name asm --location centralus --managed-identity asm-identity
```

Now, we are ready to apply the manifest on your Azure Subscription. The manifest contains your solution definations.

```bash
asm manifest apply -f manifest.json
```

You can lookup resource groups and resources with the lookup command.

```
asm lookup group --asm-sol $solutionId --asm-env $envName --asm-reg $region
```

```
asm lookup resource --asm-rid $resourceId --asm-sol $solutionId --asm-env $envName --asm-reg $region
```

To remove the solution, use the delete option with the solution command.

```bash
asm solution delete --asm-sol mysolution --asm-env dev
```

To run deployment, use deployment command.

```bash
asm deployment run -f $file --template-filepath $bicepFile
```

## Get Help

Get version and show all available command options.

```bash
asm --help
```
