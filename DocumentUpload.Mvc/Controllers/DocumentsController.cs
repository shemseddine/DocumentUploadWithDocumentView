using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using DocumentUpload.Mvc.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DocumentUpload.Mvc.Controllers
{
    public class DocumentsController : Controller
    {
        public string[] AllowedExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx" };
        private readonly AppDbContext _db;
        private readonly CloudStorageAccount _storageAccount;

        public DocumentsController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("BlobStorage"));
        }
        public IActionResult Index()
        {
            var documents = _db.Documents.ToList();

            return View(documents);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpGet]
        public IActionResult View(Guid id)
        {
            var document = _db.Documents.FirstOrDefault(i => i.Id == id);
            
            if (document != null)
            {
                var sasPolicy = new SharedAccessBlobPolicy()
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.Now.AddMinutes(-10),
                    SharedAccessExpiryTime = DateTime.Now.AddMinutes(10)
                };

                var blobClient = _storageAccount.CreateCloudBlobClient();

                var container = blobClient.GetContainerReference("files");

                var blob = container.GetBlockBlobReference(document.DocumentName);
                var sasToken = blob.GetSharedAccessSignature(sasPolicy);

                var location = Uri.EscapeDataString($"{document.Location}{sasToken}");

                return View(model: location);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var blobClient = _storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference("files");

            await container.CreateIfNotExistsAsync();

            var id = Guid.NewGuid();
            
            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                if (AllowedExtensions.Contains(extension))
                {
                    var documentName = $"{id}{extension}";
                    var blockBlock = container.GetBlockBlobReference(documentName);
                    using (var stream = file.OpenReadStream())
                    {
                        await blockBlock.UploadFromStreamAsync(stream);
                    }

                    _db.Documents.Add(new DocumentEntity
                    {
                        Id = id,
                        DocumentName = documentName,
                        FileName = file.FileName,
                        Location = blockBlock.Uri.AbsoluteUri
                    });

                    await _db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }
    }
}