using System;
using System.Text;
using Amazon.S3;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub
{
    internal class S3FeatureRepository : IFeatureRepository
    {
        // One client for every read and write: a new one each time cost a new connection (and TLS handshake).
        static readonly Lazy<AmazonS3Client> Client = new(() => new AmazonS3Client());

        public async Task Write(string key, string features)
        {
            using (var stream = new System.IO.MemoryStream(features.ToBytes(Encoding.UTF8)))
                await new Amazon.S3.Transfer.TransferUtility(Client.Value).UploadAsync(stream, Config.GetOrThrow("Blob:S3:Bucket"), key);
        }

        public async Task<string> Read(string key)
        {
            using (var response = await Client.Value.GetObjectAsync(Config.GetOrThrow("Blob:S3:Bucket"), key))
                return await response.ResponseStream.ReadAllText();
        }
    }
}