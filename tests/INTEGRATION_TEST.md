# Integration Test

Az Solution Manager uses integration testing to ensure all features are tested. To run the integration tests, you can run the following command from `tests` directory. Make sure you are already logged in via Azure CLI and you have selected the appropriate Azure Subscription before starting.

```powershell
.\apply-manifest-tests.ps1
```

The integration tests will take several minutes to run. If the the integration test is successful, you will noticed the message `All tests completed successfully.` at the end. Otherwise, there is an error message for the test that failed. You may need to manually remove the resources that were created by running the `dotnet run -- destroy --devtest --logging Debug` command.