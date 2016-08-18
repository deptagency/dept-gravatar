using D.Models.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace D.Services.ImageDownloader
{
    public class ImageDownloader : IDownloadService
    {
        public ImageDownloader()
        {
        }

        public Task<byte[]> Download(string imageUrl)
        {
            if(string.IsNullOrWhiteSpace(imageUrl))
            {
                throw new ArgumentException($"parameter {nameof(imageUrl)} cannot be null, empty or white space");
            }
            Uri uri = new Uri(imageUrl);

            if(!uri.IsAbsoluteUri)
            {
                throw new ArgumentException($"parameter {nameof(imageUrl)} must be an absolute URI");
            }

            HttpClient client = new HttpClient();
            return client.GetByteArrayAsync(uri);
        }
    }
}
