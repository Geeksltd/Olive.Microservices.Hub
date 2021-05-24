using Domain;
using Microsoft.AspNetCore.Mvc;
using Olive.Microservices.Hub;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    [Route("features-menu")]
    public class FeaturesMenuController : Controller
    {
        IFeatureFileProvider FeatureFileProvider;

        public FeaturesMenuController(IFeatureFileProvider featureFileProvider)
        {
            FeatureFileProvider = featureFileProvider;
        }
        public class FeaturesUploadArgs
        {
            public string ServiceName, Xml;
        }
        [Route("upload")]
        [HttpPost]
        public async Task<IActionResult> UploadMenu([FromBody] FeaturesUploadArgs args)
        {
            try
            {

                var service = Service.FindByName(args.ServiceName) ?? throw new Exception($"Could not find a service with {args.ServiceName} name.");

                await FeatureFileProvider.Save(service, args.Xml);

                StructureDeserializer.Load();

                //await Database.Update(Domain.Settings.Current, s => s.FeaturesMenuUpdated = Olive.LocalTime.Now);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

    }
}