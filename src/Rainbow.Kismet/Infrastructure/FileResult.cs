using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Rainbow.Kismet.Infrastructure
{
    public class FileResult : ActionResult
    {
        protected const string DefaultContentType = "text/plain";
        protected const int BufferSize = 64 * 1024;

        public string FileDownloadName { get; set; }

        public Stream FileStream { get; }

        public string ContentType { get; private set; }

        public DateTimeOffset? LastModified { get; set; }

        public EntityTagHeaderValue EntityTag { get; set; }

        public bool EnableRangeProcessing { get; set; }

        protected FileResult(byte[] fileContents)
            : this(fileContents, string.Empty)
        { }

        public FileResult(byte[] fileContents, MediaTypeHeaderValue contentType)
            :this(fileContents, contentType?.ToString())
        { }

        public FileResult(byte[] fileContents, string contentType)
        {
            if(fileContents == null)
            {
                throw new ArgumentNullException(nameof(fileContents));
            }

            this.ContentType = contentType;
            this.FileStream = new MemoryStream(fileContents);
        }

        public FileResult(Stream fileStream)
            : this(fileStream, string.Empty)
        { }

        public FileResult(Stream fileStream, MediaTypeHeaderValue contentType)
            : this(fileStream, contentType?.ToString())
        { }

        public FileResult(Stream fileStream, string contentType)
        {
            this.ContentType = contentType;
            this.FileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        public FileResult(string fileName)
            : this(fileName, string.Empty)
        { }

        public FileResult(string fileName, MediaTypeHeaderValue contentType)
            : this(fileName, contentType?.ToString())
        { }

        public FileResult(string fileName, string contentType)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            FileInfo info = new FileInfo(fileName);
            if(info.Exists)
            {
                this.FileDownloadName = info.Name;
                this.LastModified = info.LastWriteTimeUtc;
            }
            else
            {
                throw new FileNotFoundException($"file '{info.Name}' not found!", info.Name);
            }

            this.ContentType = contentType;
            this.FileStream = new FileStream(fileName, 
                FileMode.Open, 
                FileAccess.Read, 
                FileShare.ReadWrite, 
                BufferSize, 
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if(string.IsNullOrWhiteSpace(this.ContentType) && !string.IsNullOrWhiteSpace(this.FileDownloadName))
            {
                string contentType;
                var provider = context.HttpContext.RequestServices.GetRequiredService<IContentTypeProvider>();
                this.ContentType = provider.TryGetContentType(this.FileDownloadName, out contentType) ? contentType : DefaultContentType;
            }

            var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<FileResult>>();
            return executor.ExecuteAsync(context, this);
        }
    }
}
