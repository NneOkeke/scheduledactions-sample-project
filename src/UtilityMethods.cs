﻿using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.ComputeSchedule;
using Azure.ResourceManager.ComputeSchedule.Models;
using Azure.ResourceManager.Resources;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ComputeScheduleSampleProject
{
    public static class UtilityMethods
    {
        // Amount of time to wait between each polling request
        private static readonly int PollingIntervalInSeconds = 15;

        // Amount of time to wait before polling requests start, this is because the p50 for compute operations is approximately 2 minutes
        private static readonly int InitialWaitTimeBeforePollingInSeconds = 30;

        // Timeout for polling operation status
        private static readonly int OperationTimeoutInMinutes = 125;

        /// <summary>
        /// Generates a resource identifier for the subscriptionId
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subscriptionId"></param>
        /// <returns></returns>
        public static SubscriptionResource GetSubscriptionResource(ArmClient client, string subscriptionId)
        {
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            return client.GetSubscriptionResource(subscriptionResourceId);
        }


        /// <summary>
        /// Determine if the operation state is complete
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static bool IsOperationStateComplete(ScheduledActionOperationState? state)
        {
            return state != null &&
                (state == ScheduledActionOperationState.Succeeded |
                state == ScheduledActionOperationState.Failed ||
                state == ScheduledActionOperationState.Cancelled);
        }

        /// <summary>
        /// Determine if polling for operation status should continue based on the response from GetOperationsRequest
        /// </summary>
        /// <param name="response"> Response from GetOperationsRequest that is used to determine if polling should continue </param>
        /// <param name="totalVmsCount">Total number of virtual machines in the initial Start/Hibernate/Deallocate operation </param>
        /// <param name="completedOps"> Dictionary of completed operations, that is, operations where state is either Succeeded, Failed, Cancelled </param>
        /// <returns></returns>
        public static bool ShouldRetryPolling(GetOperationStatusResult response, int totalVmsCount, Dictionary<string, ResourceOperationDetails> completedOps)
        {
            var shouldRetry = true;
            foreach (var operationResult in response.Results)
            {
                var operation = operationResult.Operation;
                var operationId = operation.OperationId;
                var operationState = operation.State;
                var operationError = operation.ResourceOperationError;

                if (IsOperationStateComplete(operationState))
                {
                    completedOps.TryAdd(operationId, operation);
                    Console.WriteLine($"Operation {operationId} completed with state {operationState}");

                    if (operationError != null)
                    {
                        Console.WriteLine($"Operation {operationId} encountered the following error: errorCode {operationError.ErrorCode}, errorDetails: {operationError.ErrorDetails}");
                    }
                }
            }

            // CompletedOps.Count == TotalVmsCount means that all the operations have completed and there would be no need to retry polling
            if (completedOps.Count == totalVmsCount)
            {
                shouldRetry = false;
            }
            return shouldRetry;
        }


        /// <summary>
        /// Removes the operations that have completed from the list of operations to poll
        /// </summary>
        /// <param name="completedOps"> Dictionary of completed operations, that is, operations where state is either Succeeded, Failed, Cancelled </param>
        /// <param name="allOps"></param>
        /// <returns></returns>
        private static HashSet<string?> ExcludeCompletedOperations(Dictionary<string, ResourceOperationDetails> completedOps, HashSet<string?> allOps)
        {
            var incompleteOps = new HashSet<string?>(allOps);

            foreach (var op in allOps)
            {
                if (op != null && completedOps.ContainsKey(op))
                {
                    incompleteOps.Remove(op);
                }
            }
            Console.WriteLine(string.Join(", ", incompleteOps));
            return incompleteOps;
        }

        /// <summary>
        /// This method excludes resources not processed in Scheduledactions due to a number of reasons like operation conflicts,
        /// operations in a blocked state due to scenarios like outages in downstream services, internal outages etc.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public static HashSet<string?> ExcludeResourcesNotProcessed(IEnumerable<ResourceOperationResult> results)
        {
            var validOperationIds = new HashSet<string?>();
            foreach (var result in results)
            {
                if (result.ErrorCode != null)
                {
                    Console.WriteLine($"VM with resourceId: {result.ResourceId} encountered the following error: errorCode {result.ErrorCode}, errorDetails: {result.ErrorDetails}");
                }
                else if (result.Operation.State == ScheduledActionOperationState.Blocked)
                {
                    /// Operations on virtual machines are set to blocked state in Computeschedule when there is an ongoing outage internally or in downstream services.
                    /// These operations could still be processed later as long as their due time for execution is not past deadline time + retrywindowinminutes
                    Console.WriteLine($"Operation on VM with resourceId: {result.ResourceId} is currently blocked, operation may still complete");
                }
                else
                {
                    validOperationIds.Add(result.Operation.OperationId);
                }
            }
            return validOperationIds;
        }

        /// <summary>
        /// Polls the operation status for the operations that are in not yet in completed state
        /// </summary>
        /// <param name="cts"></param>
        /// <param name="opIdsFromOperationReq"> OperationIds from execute type operations </param>
        /// <param name="completedOps"> OperationIds of completed operations </param>
        /// <param name="location"> Location of the virtual machines from execute type operations </param>
        /// <param name="resource"> ARM subscription resource </param>
        /// <returns></returns>

        public static async Task PollOperationStatus(HashSet<string?> opIdsFromOperationReq, Dictionary<string, ResourceOperationDetails> completedOps, string location, SubscriptionResource resource)
        {
            await Task.Delay(InitialWaitTimeBeforePollingInSeconds);

            GetOperationStatusContent getOpsStatusRequest = new(opIdsFromOperationReq, Guid.NewGuid().ToString());
            GetOperationStatusResult? response = await resource.GetVirtualMachineOperationStatusAsync(location, getOpsStatusRequest);

            // Cancellation token source is used in this case to cancel the polling operation after a certain time
            using CancellationTokenSource cts = new(TimeSpan.FromMinutes(OperationTimeoutInMinutes));
            while (!cts.Token.IsCancellationRequested)
            {

                if (!ShouldRetryPolling(response, opIdsFromOperationReq.Count, completedOps))
                {
                    break;
                }
                else
                {
                    var incompleteOperations = ExcludeCompletedOperations(completedOps, opIdsFromOperationReq);
                    GetOperationStatusContent pendingOpIds = new(incompleteOperations, Guid.NewGuid().ToString());
                    response = await resource.GetVirtualMachineOperationStatusAsync(location, pendingOpIds);
                }

                await Task.Delay(TimeSpan.FromSeconds(PollingIntervalInSeconds), cts.Token);
            }
        }


        /// <summary>
        /// Generates a resource override item for virtual machines
        /// </summary>
        /// <param name="name">Name of the virtual machine</param>
        /// <param name="locationProperty">Location of the virtual machine</param>
        /// <param name="vmsize">Size of the virtual machine</param>
        /// <param name="password">Admin password for the virtual machine</param>
        /// <param name="adminUsername">Admin username for the virtual machine</param>
        public static Dictionary<string, BinaryData> GenerateResourceOverrideItem(
            string name,
            string locationProperty,
            string vmsize,
            string password,
            string adminUsername)
        {
            var resourceOverrideDetail = new Dictionary<string, BinaryData>
            {
                { "name", BinaryData.FromString(name) },
                { "location", BinaryData.FromString(locationProperty) },
                { "properties", BinaryData.FromObjectAsJson(new {
                    hardwareProfile = new {
                        vmSize = vmsize
                    }
                })
                },
                { "osProfile", BinaryData.FromObjectAsJson(new {
                    computerName = name,
                    adminUsername = adminUsername,
                    adminPassword = password,
                    windowsConfiguration = new {
                        provisionVmAgent = true,
                        enableAutomaticUpdates = true,
                        patchSettings = new {
                            patchMode = "AutomaticByPlatform",
                            assessmentMode = "ImageDefault"
                        }
                    }
                }) }
            };

            return resourceOverrideDetail;
        }

        /// <summary>
        /// Builds the execute create request content for virtual machines
        /// </summary>
        /// <param name="resourcePrefix">Resource prefix for the virtual machines</param>
        /// <param name="correlationId">Correlation ID for the request</param>
        /// <param name="resourceCount">Number of virtual machines to create</param>
        /// <param name="executionParameter">Execution parameters for the request</param>
        /// <param name="rgName">Resource group name</param>
        /// <param name="vnetName">Virtual network name</param>
        /// <param name="subnetName">Subnet name</param>
        /// <param name="azureLocation">Azure location</param>
        /// <param name="resourceOverrideDetail">Resource override details</param>
        public static ExecuteCreateContent BuildExecuteCreateRequest(
            string resourcePrefix,
            string correlationId,
            int resourceCount,
            ScheduledActionExecutionParameterDetail executionParameter,
            string rgName,
            string vnetName,
            string subnetName,
            string azureLocation,
            List<Dictionary<string, BinaryData>> resourceOverrideDetail,
            string subId,
            bool enableResourceOverride = false)
        {
            var payload = new ResourceProvisionPayload(resourceCount)
            {
                ResourcePrefix = resourcePrefix,
                BaseProfile =
                {
                    { "resourcegroupName", BinaryData.FromString(rgName) },
                    { "computeApiVersion", BinaryData.FromString("2023-09-01") },
                    { "properties", BinaryData.FromObjectAsJson(new {
                        vmExtensions = new[]{
                            new {
                                name = "Microsoft.Azure.Geneva.GenevaMonitoring",
                                location = azureLocation,
                                properties = new {
                                    publisher = "Microsoft.Azure.Geneva",
                                    type = "GenevaMonitoring",
                                    typeHandlerVersion = "2.0",
                                    autoUpgradeMinorVersion = true,
                                    enableAutomaticUpgrade = true,
                                    suppressFailures = true
                                }
                            }
                        },
                        hardwareProfile = new {
                            vmSize = "Standard_DS1_v2"
                        },
                        storageProfile = new {
                            imageReference = new {
                                publisher = "MicrosoftWindowsServer",
                                offer = "WindowsServer",
                                sku = "2022-datacenter-azure-edition",
                                version = "latest"
                            },
                            osDisk = new {
                                createOption = "FromImage",
                                osType = "Windows",
                                caching = "ReadWrite",
                                managedDisk = new {
                                    storageAccountType = "Standard_LRS"
                                },
                                deleteOption = "Detach",
                                diskSizeGB = 127,
                            }
                        },
                        networkProfile = new {
                            networkApiVersion = "2022-07-01",
                            networkInterfaceConfigurations = new[] {
                                new {
                                    name = "vmTest",
                                    properties = new {
                                        primary = true,
                                        enableIPForwarding = true,
                                        ipConfigurations = new[] {
                                            new {
                                                name = "vmTest",
                                                properties = new {
                                                    primary = true,
                                                    subnet = new {
                                                        id = $"/subscriptions/{subId}/resourceGroups/{rgName}/providers/Microsoft.Network/virtualNetworks/{vnetName}/subnets/{subnetName}",
                                                        properties = new {
                                                            defaultoutboundaccess = false,
                                                        }
                                                    },
                                                    privateIPAllocationMethod = "Dynamic"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }) }
                }
            };

            if (enableResourceOverride)
            {
                if (resourceOverrideDetail.Count < resourceCount)
                {
                    throw new Exception("Resource override count must == " + resourceCount);
                }

                foreach (var overrideDict in resourceOverrideDetail)
                {
                    payload.ResourceOverrides.Add(overrideDict);
                }
            }

            return new ExecuteCreateContent(payload, executionParameter)
            {
                CorrelationId = correlationId
            };
        }


        /// <summary>
        /// Builds the execute create request content from JSON body for virtual machines
        /// </summary>
        /// <param name="jsonContent">JSON content for the request</param>
        /// <param name="resourcePrefix">Resource prefix for the virtual machines</param>
        /// <param name="correlationId">Correlation ID for the request</param>
        /// <param name="resourceCount">Number of virtual machines to create</param>
        /// <param name="executionParameter">Execution parameters for the request</param>
        public static ExecuteCreateContent BuildExecuteCreateRequestFromJsonContent(
            string jsonContent,
            string resourcePrefix,
            string correlationId,
            int resourceCount,
            ScheduledActionExecutionParameterDetail executionParameter)
        {
            var root = JsonNode.Parse(jsonContent)!;

            var resourceConfig = root["resourceConfigParameters"]!;
            var baseProfileNode = resourceConfig["baseProfile"]!;
            var resourceOverridesNode = resourceConfig["resourceOverrides"]?.AsArray() ?? [];

            var payload = new ResourceProvisionPayload(resourceCount)
            {
                ResourcePrefix = resourcePrefix
            };

            foreach (var prop in baseProfileNode.AsObject())
            {
                payload.BaseProfile[prop.Key] = BinaryData.FromObjectAsJson(prop.Value!.Deserialize<object>());
            }

            foreach (var overrideNode in resourceOverridesNode)
            {
                var overrideDict = new Dictionary<string, BinaryData>();
                foreach (var prop in overrideNode!.AsObject())
                {
                    overrideDict[prop.Key] = BinaryData.FromObjectAsJson(prop.Value!.Deserialize<object>());
                }
                payload.ResourceOverrides.Add(overrideDict);
            }

            return new ExecuteCreateContent(payload, executionParameter)
            {
                CorrelationId = correlationId
            };
        }
    }
}