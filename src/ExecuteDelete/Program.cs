﻿using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ComputeSchedule.Models;
using UtilityMethods;

namespace ExecuteDelete
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var blockedOperationsException = new HashSet<string> { "SchedulingOperationsBlockedException", "NonSchedulingOperationsBlockedException" };

            // Location: The location of the virtual machines
            const string location = "eastus2euap";

            // SubscriptionId: The subscription id under which the virtual machines are located, in this case, we are using a dummy subscriptionId
            const string subscriptionId = "1d04e8f1-ee04-4056-b0b2-718f5bb45b04";

            // ResourceGroupName: The resource group name under which the virtual machines are located, in this case, we are using a dummy resource group name
            const string resourceGroupName = "computeschedule-azcliext-resources";

            Dictionary<string, ResourceOperationDetails> completedOperations = [];
            // Credential: The Azure credential used to authenticate the request
            TokenCredential cred = new DefaultAzureCredential();

            // Client: The Azure Resource Manager client used to interact with the Azure Resource Manager API
            ArmClient client = new(cred);
            var subscriptionResource = HelperMethods.GetSubscriptionResource(client, subscriptionId);

            // Execution parameters for the request including the retry policy used by Scheduledactions to retry the operation in case of failures
            var executionParams = new ScheduledActionExecutionParameterDetail()
            {
                RetryPolicy = new UserRequestRetryPolicy()
                {
                    // Number of times ScheduledActions should retry the operation in case of failures: Range 0-7
                    RetryCount = 3,
                    // Time window in minutes within which ScheduledActions should retry the operation in case of failures: Range in minutes 5-120
                    RetryWindowInMinutes = 45
                }
            };

            var resourceIds = new List<ResourceIdentifier>()
            {
                new($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/tv0"),
                new($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Compute/virtualMachines/tv1"),
            };

            // Create type operation: Create operation on virtual machines
            await ComputescheduleOperations.ExecuteDeleteOperation(
                resourceIds,
                executionParams,
                subscriptionResource,
                blockedOperationsException,
                location,
                isForceDeletion: true);
        }
    }
}
