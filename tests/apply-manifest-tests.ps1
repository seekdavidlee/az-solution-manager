$ErrorActionPreference = "Stop"

$roleName = "Contributor"
$principalId = az ad signed-in-user show --query "id" | ConvertFrom-Json
$principalType = "User"
$location = (Get-Location).Path
$manifestPath = "$location\manifest.json"
$manifestWithToken = "$location\manifestwithtoken.json"

Push-Location ..\src\AzSolutionManager
dotnet run --no-launch-profile -- profile --show --devtest
if ($LastExitCode -ne 0) {
    throw "Did not expect an error here."
}

dotnet run --no-launch-profile -- manifest apply -f $manifestPath --devtest
if ($LastExitCode -eq 0) {
    throw "Expected an error to be thrown but no error was thrown."
}
else {
    Write-Host "Applying manifest without init throws an exception passed!" -ForegroundColor Green
}

dotnet run --no-launch-profile -- init -g asmdevtest -m asm-managed-identity -l centralus --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "Unable to initalize subscription!"
}
else {
    Write-Host "Initialization passed!" -ForegroundColor Green
}

dotnet run --no-launch-profile -- manifest apply -f $manifestPath --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "Unable to apply $manifestPath!"
}

$tagKeys = @("asm-environment", "asm-internal-solution-id", "asm-solution-id", "asm-region")
$manifestObj = Get-Content $manifestPath | ConvertFrom-Json

$manifestObj.groups | ForEach-Object {

    $rgName = $_."resource-group-name"

    Write-Host "Testing $rgName"

    $exist = az group exists --name $rgName
    if ($LastExitCode -ne 0) {
        throw "Unable to check if group exist!"
    }

    if ($exist -ne "true") {
        throw "Expected group $rgName to exist but it does not!"
    }

    $group = az group show --name $rgName | ConvertFrom-Json
    
    $tagKeys | ForEach-Object {
        $tagKey = $_
        $tagValue = $group.tags."$tagKey"
        if (!$tagValue) {
            throw "Expected tag $tagKey to exist but it does not!"
        }
    }

    az group delete --name $rgName --yes
    if ($LastExitCode -eq 0) {
        throw "Expected not to be able to delete group $rgName but we did."
    }

    Write-Host -Object "$rgName passed!" -ForegroundColor Green

    $solutionId = $_."asm-solution-id"
    $envName = $_."asm-environment"
    $region = $_."asm-region"
    $component = $_."asm-component"

    Write-Host "Testing group lookup for $rgName"

    if ( $component ) {
        $json = dotnet run --no-launch-profile -- lookup group --asm-sol $solutionId --asm-env $envName --asm-reg $region --asm-com $component --devtest --logging Debug
    }
    else {
        $json = dotnet run --no-launch-profile -- lookup group --asm-sol $solutionId --asm-env $envName --asm-reg $region --devtest --logging Debug
    }
    
    if ($LastExitCode -ne 0) {
        throw "Error with group lookup."
    }

    if (!$json) {
        throw "Expected group with [$solutionId, $envName] to exist but it does not!"
    }

    try {
        $obj = $json | ConvertFrom-Json
    }
    catch [System.ArgumentException] {
        Write-Error "Unable to convert json to object! Output: $json"
        throw
    }

    $objName = $obj.Name
    if ($objName -ne $rgName) {
        throw "Expected $rgName but got $objName"
    }

    if ($solutionId -eq "shared1") {

        # Test assignments
        dotnet run --no-launch-profile -- role assign --role-name $roleName --principal-id $principalId --principal-type $principalType --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
        if ($LastExitCode -ne 0) {
            throw "Error assigning role $roleName!"
        }

        Write-Host "Waiting 15 seconds before checking for assignment."
        Start-Sleep 15

        $exist = az role assignment list -g $rgName | ConvertFrom-Json | Where-Object { $_.principalId -eq $principalId -and $_.roleDefinitionName -eq $roleName }
        if (!$exist) {
            throw "Role assignment is not found!"
        }

        $file = "$location\deploy-shared.bicep.$envName.json"
        Write-Host "Testing $file"

        dotnet run --no-launch-profile -- deployment run -f $file --template-filepath "$location\deploy-shared.bicep" --devtest --logging Debug
        if ($LastExitCode -ne 0) {
            throw "Error processing $file"
        }

        $json = dotnet run --no-launch-profile -- lookup resource --asm-rid "shared-storage" --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
        if ($LastExitCode -ne 0) {
            throw "Error with resource lookup."
        }

        if (!$json) {
            throw "Unable to look up shared-storage [$solutionId, $envName]! Output: $json"
        }
    }

    if ($solutionId -eq "someapp1") {

        az deployment group create --resource-group $objName --template-file "$location\deploy-microservice.bicep"

        if ($LastExitCode -ne 0) {
            throw "Error running deployment for $objName."
        }

        $json = dotnet run --no-launch-profile -- lookup resource-type --type-name "Microsoft.Storage/storageAccounts" --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
        if ($LastExitCode -ne 0) {
            throw "Error with resource lookup."
        }

        if (!$json) {
            throw "Unable to look up by resource type name [$solutionId, $envName]! Output: $json"
        }

        $stors = $json | ConvertFrom-Json
        $storsRes = az resource show --ids $stors.ResourceId | ConvertFrom-Json

        $str1 = $storsRes | Where-Object { $_.tags."x-used-by" -eq "foo" }
        if ($str1) {
            Write-Host "Found x-used-by=foo passed!" -ForegroundColor Green
        }
        else {
            throw "Error finding a resource x-used-by=foo."
        }

        $str2 = $storsRes | Where-Object { $_.tags."x-used-by" -eq "bar" }    
        if ($str2) {
            Write-Host "Found x-used-by=bar passed!" -ForegroundColor Green
        }
        else {
            throw "Error finding a resource x-used-by=bar."
        }
    }
}

$envName = "dev"
$solutionId = "tokenreplacementtest"
dotnet run --no-launch-profile -- manifest apply -f $manifestWithToken --asm-env $envName --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "Unable to apply $manifestWithToken!"
}

$json = dotnet run --no-launch-profile -- lookup group --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "Error with group lookup."
}

if (!$json) {
    throw "Expected group with [$solutionId, $envName] to exist but it does not!"
}

$obj = $json | ConvertFrom-Json
$group = az group show --name $obj.Name | ConvertFrom-Json

$actualSolutionId = $group.tags."asm-solution-id"
if ($actualSolutionId -ne $solutionId) {
    throw "Expected $solutionId but got $actualSolutionId"
}

$actualEnvName = $group.tags."asm-environment"
if ($actualEnvName -ne $envName) {
    throw "Expected $envName but got $actualEnvName"
}

$json = dotnet run --no-launch-profile -- solution list --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "An error has occured while listing all solutions!"
}

$allSolutions = $json | ConvertFrom-Json
if (!$allSolutions -or $allSolutions.Length -ne 5) {
    throw "Insufficent solutions listed! Solutions: $allSolutions"
}

$allSolutions | ForEach-Object {
    
    if (!$_.SolutionId) {
        throw "Expected property SolutionId!"
    }

    $solutionId = $_.SolutionId

    if ($solutionId -eq "networking" -and $_.ResourceGroups.Length -ne 4) {
        throw "Expected for $solutionId to contain exactly 4 resource groups!"
    }

    if ($solutionId -eq "someapp2" -and $_.ResourceGroups.Length -ne 2) {
        throw "Expected for $solutionId to contain exactly 2 resource groups!"
    }

    if ($solutionId -eq "tokenreplacementtest" -and $_.ResourceGroups.Length -ne 1) {
        throw "Expected for $solutionId to contain exactly 1 resource group!"
    }

    if ($solutionId -eq "someapp1" -and $_.ResourceGroups.Length -ne 1) {
        throw "Expected for $solutionId to contain exactly 1 resource group!"
    }

    if ($solutionId -eq "shared1" -and $_.ResourceGroups.Length -ne 2) {
        throw "Expected for $solutionId to contain exactly 2 resource groups!"
    }
}

dotnet run --no-launch-profile -- deployment run -f "$location\deploy-err.json" --template-filepath "$location\deploy-err.bicep" --devtest --logging Debug
if ($LastExitCode -eq 0) {
    throw "Expected error but did not get an error processing deploy-err.bicep"
}

$solutionId = "shared1"
dotnet run --no-launch-profile -- solution delete --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "An error has occured while running destroy operation!"
}

$json = dotnet run --no-launch-profile -- lookup group --asm-sol $solutionId --asm-env $envName --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "Error with group lookup [$solutionId, $envName]."
}

if ($null -ne $json) {
    throw "Expected json to be null be it was $json"
}

dotnet run --no-launch-profile -- destroy --devtest --logging Debug
if ($LastExitCode -ne 0) {
    throw "An error has occured!"
}

$manifestObj.groups | ForEach-Object {
    $rgName = $_."resource-group-name"
    $exist = az group exists --name $rgName
    if ($LastExitCode -ne 0) {
        throw "Unable to check if group exist!"
    }

    if ($exist -eq "true") {
        throw "Expected group $rgName to NOT exist but does!"
    }
}

Pop-Location
Write-Host "All tests completed successfully." -ForegroundColor Green