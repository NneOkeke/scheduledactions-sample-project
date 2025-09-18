# ComputeScheduleSampleProject

This project demonstrates how to use the Azure ComputeSchedule SDK to manage virtual machines (VMs) in Azure using scheduled actions. The sample covers creating, starting, and deleting VMs programmatically, including handling retries and operation polling.

## Prerequisites

- .NET 8 SDK
- Azure Subscription
- Properly configured Azure credentials (e.g., via `az login` or environment variables)

## Main Methods

### `Main(string[] args)`

The entry point of the application. It demonstrates the following workflow:
- Sets up Azure credentials and ARM client.
- Prepares execution parameters for scheduled actions (including retry policy).
- Creates a virtual network and subnet (required for VM creation).
- Calls `ExecuteCreateOperation` to create a VM.
- Commented code Shows how to start or delete VMs using scheduled actions.

### `ExecuteCreateOperation(...)`

Handles the creation of virtual machines using the ComputeSchedule SDK. It:
- Generates a unique correlation ID for tracking.
- Prepares resource override details (e.g., VM name, size, credentials).
- Builds the request for VM creation.
- Executes the create operation and polls for completion.
- Handles and logs errors, including retryable and blocking exceptions.

### `ExecuteStartOperation(...)`

Demonstrates how to start one or more VMs using scheduled actions. It:
- Prepares a list of VM resource IDs to start.
- Builds and sends the start request.
- Polls for operation status and handles errors.

### `ExecuteDeleteOperation(...)`

Handles deletion of VMs using scheduled actions. It:
- Prepares the delete request with force deletion enabled.
- Executes the delete operation and polls for completion.
- Handles and logs errors, including retryable and blocking exceptions.

## Utility Methods

The project references a `UtilityMethods` class (not shown here) for common tasks such as:
- Creating virtual networks and subnets.
- Generating resource override items for VM creation.
- Polling operation status.
- Excluding resources not processed due to validation or conflicts.

## Error Handling

All operations handle `RequestFailedException` and general exceptions, logging error codes and messages. Special handling is included for known blocking exceptions (e.g., due to Azure service outages).

## Usage

1. Clone the repository.
2. Restore dependencies and build the project.
3. Update the subscription ID, resource group, and other parameters as needed.
4. Run the project:
5. Review the console output for operation status and results.

## Notes

- The sample uses dummy subscription and resource group names. Replace them with your actual Azure resources.
- The VM admin password and username are hardcoded for demonstration. Use secure methods for production code.
- The project is designed for .NET 8 and C# 12.

## License

This project is for demonstration purposes and does not include a license.
