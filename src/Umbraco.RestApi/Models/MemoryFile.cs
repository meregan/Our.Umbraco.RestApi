using System.IO;
using System.Web;

namespace Umbraco.RestApi.Models
{
    class MemoryFile : HttpPostedFileBase
    {
        Stream stream;
        string contentType;
        string fileName;

        public MemoryFile(Stream stream, string contentType, string fileName)
        {
            this.stream = stream;
            this.contentType = contentType;
            this.fileName = fileName;
        }

        public override int ContentLength => (int)stream.Length;

        public override string ContentType => contentType;

        public override string FileName => fileName;

        public override Stream InputStream => stream;

        public override void SaveAs(string filename)
        {
            using (var file = File.Open(filename, FileMode.CreateNew))
                stream.CopyTo(file);
        }
    }
}
