using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ComputeSchedule;
using Azure.ResourceManager.ComputeSchedule.Models;
using Azure.ResourceManager.Resources;
using System.ClientModel.Primitives;
using System.Resources;

namespace ComputeScheduleSampleProject
{
    internal static class Program
    {
        /// <summary>
        /// This project shows a sample use case for the ComputeSchedule use case
        /// </summary>
        private static void Main(string[] args)
        {
            // Testing the ExecuteStart operation
            var executeStartRequest = new ExecuteStartContent(new ScheduledActionExecutionParameterDetail(), new UserRequestResources(new List<ResourceIdentifier>() { new("/subscriptions/afe495ca-b99a-4e36-86c8-9e0e41697f1c/resourcegroups/Kronox_SyntheticRuns_EastAsia/providers/Microsoft.Compute/virtualMachines/nneka-computeschedule-testvm") }), Guid.NewGuid().ToString());
            var executeStartResult = TestExecuteStartAsync("eastasia", executeStartRequest, "afe495ca-b99a-4e36-86c8-9e0e41697f1c").Result;

            var executeStartProcessedData = ModelReaderWriter.Write(executeStartResult, ModelReaderWriterOptions.Json);
            Console.WriteLine(executeStartProcessedData.ToString());


            // Testing the GetOperationStatus operation
            var allOperationIds = executeStartResult.Results.Select(result => result.Operation?.OperationId).Where(operationId => !string.IsNullOrEmpty(operationId)).ToList();
            var getOpsStatusReq = new GetOperationStatusContent(allOperationIds, Guid.NewGuid().ToString());
            var getOperationStatus = TestGetOpsStatusAsync("eastasia", getOpsStatusReq, "afe495ca-b99a-4e36-86c8-9e0e41697f1c").Result;

            var getOperationStatusProcessedData = ModelReaderWriter.Write(getOperationStatus, ModelReaderWriterOptions.Json);
            Console.WriteLine(getOperationStatusProcessedData.ToString());

        }
        private static async Task<StartResourceOperationResult> TestExecuteStartAsync(string location, ExecuteStartContent executeStartRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteStartContent content = executeStartRequest;
            StartResourceOperationResult? result;
            try
            {
                result = await subscriptionResource.ExecuteVirtualMachineStartAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        private static async Task<DeallocateResourceOperationResult> TestExecuteDeallocateAsync(string location, ExecuteDeallocateContent executeDeallocateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteDeallocateContent content = executeDeallocateRequest;

            DeallocateResourceOperationResult? result;
            try
            {
                result = await subscriptionResource.ExecuteVirtualMachineDeallocateAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;

        }

        private static async Task<HibernateResourceOperationResult> TestExecuteHibernateAsync(string location, ExecuteHibernateContent executeHibernateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteHibernateContent content = executeHibernateRequest;
            HibernateResourceOperationResult? result;

            try
            {
                result = await subscriptionResource.ExecuteVirtualMachineHibernateAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        public static async Task<CancelOperationsResult> TestCancelOpsAsync(string location, CancelOperationsContent cancelOpsRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();

            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            CancelOperationsContent content = cancelOpsRequest;
            CancelOperationsResult? result;

            try
            {
                result = await subscriptionResource.CancelVirtualMachineOperationsAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        public static async Task<GetOperationStatusResult> TestGetOpsStatusAsync(string location, GetOperationStatusContent getOpsStatusRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();

            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            GetOperationStatusContent content = getOpsStatusRequest;
            GetOperationStatusResult? result;

            try
            {
                result = await subscriptionResource.GetVirtualMachineOperationStatusAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        public static async Task<GetOperationErrorsResult> TestGetOpsErrorsAsync(string location, GetOperationErrorsContent getOpsErrorsRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();

            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            GetOperationErrorsContent content = getOpsErrorsRequest;
            GetOperationErrorsResult? result;

            try
            {
                result = await subscriptionResource.GetVirtualMachineOperationErrorsAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }
    }
}