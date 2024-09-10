using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ComputeSchedule;
using Azure.ResourceManager.ComputeSchedule.Models;
using Azure.ResourceManager.Resources;
using System.ClientModel.Primitives;

namespace ComputeScheduleSampleProject
{
    internal static class Program
    {
        /// <summary>
        /// This project shows a sample use case for the ComputeSchedule SDK
        /// </summary>
        private static void Main(string[] args)
        {
            // Enter the location of the virtual machines to be operated on
            var location = "LOCATION OF VIRTUAL MACHINE HERE";

            // Enter the subscription associated with the virtual machines to be operated on
            var subscriptionId = "SUBSCRIPTION GUID HERE";

            // Enter the fuly qualified ARM ID of the virtual machines to be operated on
            var armId = new List<string>()
            {
                "ARMIDS OF RESOURCES HERE"
            };

            // Testing the ExecuteStart operation
            var executeStartRequest = new ExecuteStartContent(new ExecutionParameters(), new Resources(armId), Guid.NewGuid().ToString());
            var executeStartResult = TestExecuteStartAsync(location, executeStartRequest, subscriptionId).Result;

            var executeStartProcessedData = ModelReaderWriter.Write(executeStartResult, ModelReaderWriterOptions.Json);
            Console.WriteLine(executeStartProcessedData.ToString());


            // Testing the GetOperationStatus operation
            var allOperationIds = executeStartResult.Results.Select(result => result.Operation?.OperationId).Where(operationId => !string.IsNullOrEmpty(operationId)).ToList();
            var getOpsStatusReq = new GetOperationStatusContent(allOperationIds, Guid.NewGuid().ToString());
            var getOperationStatus = TestGetOpsStatusAsync(location, getOpsStatusReq, subscriptionId).Result;

            var getOperationStatusProcessedData = ModelReaderWriter.Write(getOperationStatus, ModelReaderWriterOptions.Json);
            Console.WriteLine(getOperationStatusProcessedData.ToString());
        }
        private static async Task<StartResourceOperationResponse> TestExecuteStartAsync(string location, ExecuteStartContent executeStartRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteStartContent content = executeStartRequest;
            StartResourceOperationResponse? result;
            try
            {
                result = await subscriptionResource.VirtualMachinesExecuteStartScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        private static async Task<DeallocateResourceOperationResponse> TestExecuteDeallocateAsync(string location, ExecuteDeallocateContent executeDeallocateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteDeallocateContent content = executeDeallocateRequest;

            DeallocateResourceOperationResponse? result;
            try
            {
                result = await subscriptionResource.VirtualMachinesExecuteDeallocateScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;

        }

        private static async Task<HibernateResourceOperationResponse> TestExecuteHibernateAsync(string location, ExecuteHibernateContent executeHibernateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            ExecuteHibernateContent content = executeHibernateRequest;
            HibernateResourceOperationResponse? result;

            try
            {
                result = await subscriptionResource.VirtualMachinesExecuteHibernateScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        private static async Task<StartResourceOperationResponse> TestSubmitStartAsync(string location, SubmitStartContent submitStartRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            SubmitStartContent content = submitStartRequest;
            StartResourceOperationResponse? result;
            try
            {
                result = await subscriptionResource.VirtualMachinesSubmitStartScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        private static async Task<DeallocateResourceOperationResponse> TestSubmitDeallocateAsync(string location, SubmitDeallocateContent submitDeallocateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            SubmitDeallocateContent content = submitDeallocateRequest;
            DeallocateResourceOperationResponse? result;
            try
            {
                result = await subscriptionResource.VirtualMachinesSubmitDeallocateScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        private static async Task<HibernateResourceOperationResponse> TestSubmitHibernateAsync(string location, SubmitHibernateContent submitHibernateRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();
            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            SubmitHibernateContent content = submitHibernateRequest;
            HibernateResourceOperationResponse? result;
            try
            {
                result = await subscriptionResource.VirtualMachinesSubmitHibernateScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        public static async Task<CancelOperationsResponse> TestCancelOpsAsync(string location, CancelOperationsContent cancelOpsRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();

            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            CancelOperationsContent content = cancelOpsRequest;
            CancelOperationsResponse? result;

            try
            {
                result = await subscriptionResource.VirtualMachinesCancelOperationsScheduledActionAsync(locationparameter, content);
                Console.WriteLine($"Succeeded: {result}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return result;
        }

        public static async Task<GetOperationStatusResponse> TestGetOpsStatusAsync(string location, GetOperationStatusContent getOpsStatusRequest, string subid)
        {
            TokenCredential cred = new DefaultAzureCredential();

            ArmClient client = new(cred);
            string subscriptionId = subid;
            ResourceIdentifier subscriptionResourceId = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscriptionResource = client.GetSubscriptionResource(subscriptionResourceId);
            string locationparameter = location;
            GetOperationStatusContent content = getOpsStatusRequest;
            GetOperationStatusResponse? result;

            try
            {
                result = await subscriptionResource.VirtualMachinesGetOperationStatusScheduledActionAsync(locationparameter, content);
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