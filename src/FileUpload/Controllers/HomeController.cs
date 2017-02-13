using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
namespace FileUpload.Controllers
{
    public class HomeController : Controller
    {
        private const string ProgressSessionKey = "FileUploadProgress";
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IList<IFormFile> files)
        {
            SetProgress(HttpContext.Session, 0);
            long totalBytes = files.Sum(f => f.Length);

            if (!IsMultipartContentType(HttpContext.Request.ContentType))
                return StatusCode(415);

            foreach (IFormFile file in files)
            {
                ContentDispositionHeaderValue contentDispositionHeaderValue =
                    ContentDispositionHeaderValue.Parse(file.ContentDisposition);

                string filename = contentDispositionHeaderValue.FileName.Trim('"');
                
                byte[] buffer = new byte[16 * 1024];

                using (FileStream output = System.IO.File.Create(GetPathAndFilename(filename)))
                {
                    using (Stream input = file.OpenReadStream())
                    {
                        long totalReadBytes = 0;
                        int readBytes;

                        while ((readBytes = input.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            await output.WriteAsync(buffer, 0, readBytes);
                            totalReadBytes += readBytes;
                            int progress = (int)((float)totalReadBytes / (float)totalBytes * 100.0);
                            SetProgress(HttpContext.Session, progress);

                            Log($"SetProgress: {progress}", @"\LogSet.txt");

                            await Task.Delay(100);
                        }
                    }
                }
            }

            return Content("success");
        }

        [HttpPost]
        public ActionResult Progress()
        {
            int progress = GetProgress(HttpContext.Session);

            Log($"GetProgress: {progress}", @"\LogGet.txt");

            return Content(progress.ToString());
        }

        private string GetPathAndFilename(string filename)
        {
            string path = _hostingEnvironment.WebRootPath + @"\upload\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + filename;
        }

        private static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType) && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int GetProgress(ISession session)
        {
            int? progress = session.GetInt32(ProgressSessionKey);

            if (progress.HasValue)
                return progress.Value;

            return 0;
        }

        private static void SetProgress(ISession session, int progress)
        {
            session.SetInt32(ProgressSessionKey, progress);
        }

        private void Log(string message, string filename)
        {
            using (StreamWriter writer = new StreamWriter(_hostingEnvironment.WebRootPath + filename, true))
            {
                writer.WriteLine($"{DateTime.Now} {message}");
            }
        }
    }
}
