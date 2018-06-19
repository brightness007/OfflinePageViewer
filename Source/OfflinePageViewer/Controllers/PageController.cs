using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Wiry.Base32;

namespace OfflinePageViewer.Controllers
{
    public class PageController : Controller
    {
        public PageController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IActionResult Index(string id, string path)
        {
            String sFilename = GetFilename(id);

            if (!String.IsNullOrWhiteSpace(sFilename))
            {
                try
                {
                    string sLocalFilename = Path.Combine(Configuration["RootFolder"], sFilename);

                    ZipArchive _zipFile = ZipFile.OpenRead(sLocalFilename);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return Redirect(this.Request.Path.Value.TrimEnd('/') + "/index.html");
                    }

                    var entry = _zipFile.GetEntry(path);
                    if (entry != null)
                    {
                        var inputStream = entry.Open();
                        {
                            var provider = new FileExtensionContentTypeProvider();
                            string contentType;
                            if (!provider.TryGetContentType(path, out contentType))
                            {
                                contentType = "application/octet-stream";
                            }
                            return new FileStreamResult(inputStream, contentType);
                        }
                    }
                }
                catch (Exception)
                {
                    // Do Nothing
                }
            }
            return NotFound();
        }

        private String GetFilename(String sHash)
        {
            try
            {
                byte[] checksum = Base32Encoding.Standard.ToBytes(sHash);
                List<String> subfolders = new List<String>();
                string subfolder = BitConverter.ToString(checksum).Replace("-", string.Empty);
                int index = 0;
                int total = subfolder.Length;
                while (index < total)
                {
                    subfolders.Add(subfolder.Substring(index, Math.Min(4, total - index)));
                    index += 4;
                }
                return String.Format(@"{0}\{1}.zip",
                    String.Join(@"\", subfolders.ToArray()), sHash);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
