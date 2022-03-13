using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Olive.Microservices.Hub
{
    internal class S3FeatureRepository : IFeatureRepository
    {
        public async Task Write(string key, string features)
        {
            using (var stream = new System.IO.MemoryStream(features.ToBytes(Encoding.UTF8)))
                await new Amazon.S3.Transfer.TransferUtility().UploadAsync(stream, Config.GetOrThrow("Blob:S3:Bucket"), key);
        }
        public async Task<string> Read(string key)
        {
            using (var stream = new Amazon.S3.Transfer.TransferUtility().OpenStream(Config.GetOrThrow("Blob:S3:Bucket"), key))
                return await stream.ReadAllText();
        }
    }
}
