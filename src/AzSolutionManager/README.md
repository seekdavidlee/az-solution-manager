# Introduction

Make sure you are already logged in via Azure CLI and you have selected the appropriate Azure Subscription before starting.

Initialize your Azure Subscription.

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

## Get Help

Get version and show all available command options.

```bash
asm --help
```
