using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;

namespace TrialPlugin
{
    public class CreateDefectfromCase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtain the IOrganizationService instance which you will need for  
            // web service calls.  
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                //Entity object initialization - incident
                Entity incident = null;
                if (context.MessageName == "Create")
                {
                    //Retrieving target entity - case
                    incident = (context.InputParameters.Contains("Target") && context.InputParameters["Target"] != null) ?
                    context.InputParameters["Target"] as Entity : null;

                    //Read Case record
                    string CaseNumber = incident.GetAttributeValue<string>("ticketnumber");
                    EntityReference ProductLookup = incident.GetAttributeValue<EntityReference>("productid");
                    bool NeedDefect = incident.GetAttributeValue<bool>("rr_doyouwanttologaproductdefect");
                    Int32 DefectType = incident.GetAttributeValue<OptionSetValue>("rr_typeofdefect_case").Value;            

                    //If 'Do you want to log a product defect?' is Yes:
                    if (NeedDefect)  
                    {
                        //Create a new record in Product Defect entity
                        Entity newDefect = new Entity("rr_productdefect");
                        //Set field values
                        newDefect["rr_name"] = "Defect from " + CaseNumber;
                        newDefect["rr_productname"] = ProductLookup;
                        newDefect.Attributes.Add("rr_typeofdefect", new OptionSetValue(DefectType));
                        //Send create request
                        service.Create(newDefect);
                    }
                }               
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
