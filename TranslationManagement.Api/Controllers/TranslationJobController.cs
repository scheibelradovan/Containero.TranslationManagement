using External.ThirdParty.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TranslationManagement.Api.Db;
using TranslationManagement.Common.Model;

namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/TranslationJob/[action]")]
    public class TranslationJobController : ControllerBase
    {
        private AppDbContext _context;
        private readonly ILogger<TranslationJobController> _logger;

        public TranslationJobController(IServiceScopeFactory scopeFactory, ILogger<TranslationJobController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public TranslationJob[] GetJobs()
        {
            return _context.TranslationJobs.ToArray();
        }

        [HttpGet]
        public TranslationJob GetJob(int id)
        {
            return _context.TranslationJobs.FirstOrDefault(t => t.Id == id);
        }

        [HttpGet]
        public TranslationJob[] GetJobsByTranslator(int translatorId)
        {
            return _context.TranslationJobs.Where(t => t.TranslatorId == translatorId).ToArray();
        }

        const double PricePerCharacter = 0.01;
        private void SetPrice(TranslationJob job)
        {
            job.Price = job.OriginalContent.Length * PricePerCharacter;
        }

        [HttpPost]
        public ApiResult CreateJob(string customerName, string originalContent, int translatorId)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(customerName)) { return ApiResult.CreateError("customerName argument exception!"); }
                if (String.IsNullOrWhiteSpace(originalContent)) { return ApiResult.CreateError("originalContent argument exception!"); }
                if (translatorId == 0) { return ApiResult.CreateError("translatorId argument exception!"); }

                TranslationJob job = new TranslationJob()
                {
                    CustomerName = customerName,
                    Id = 0,
                    OriginalContent = originalContent,
                    Status = TranslationJobStatus.New,
                    TranslatedContent = String.Empty,
                    TranslatorId = translatorId
                };
                SetPrice(job);

                bool result = CreateJob(job);
                return new ApiResult<bool>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(CreateJob)}(customerName={customerName}, originalContent={originalContent}, translatorId={translatorId}) exception.");
                return ApiResult.CreateError($"Unexpected exception: {ex.Message}");
            }
        }

        private bool CreateJob(TranslationJob job)
        {
            _context.TranslationJobs.Add(job);
            bool success = _context.SaveChanges() > 0;
            if (success)
            {
                var notificationSvc = new UnreliableNotificationService();
                while (!notificationSvc.SendNotification("Job created: " + job.Id).Result)
                {
                }

                _logger.LogInformation("New job notification sent");
            }

            return success;
        }

        [HttpPost]
        public bool CreateJobWithFile(IFormFile file, string customer)
        {
            var reader = new StreamReader(file.OpenReadStream());
            string content;

            if (file.FileName.EndsWith(".txt"))
            {
                content = reader.ReadToEnd();
            }
            else if (file.FileName.EndsWith(".xml"))
            {
                var xdoc = XDocument.Parse(reader.ReadToEnd());
                content = xdoc.Root.Element("Content").Value;
                customer = xdoc.Root.Element("Customer").Value.Trim();
            }
            else
            {
                throw new NotSupportedException("unsupported file");
            }

            var newJob = new TranslationJob()
            {
                OriginalContent = content,
                TranslatedContent = "",
                CustomerName = customer,
            };

            SetPrice(newJob);

            return CreateJob(newJob);
        }

        [HttpPost]
        public ApiResult UpdateJobStatus(int jobId, int translatorId, TranslationJobStatus newStatus)
        {
            try
            {
                _logger.LogInformation($"Job status update request received: {newStatus} for job {jobId} by translator {translatorId}");
                if (newStatus == 0)
                {
                    return ApiResult.CreateError("Invalid status!");
                }

                var job = _context.TranslationJobs.Single(j => j.Id == jobId);
                if (job == null)
                {
                    return ApiResult.CreateError("Job was not found!");
                }

                bool isInvalidStatusChange = (job.Status == TranslationJobStatus.New && newStatus == TranslationJobStatus.Completed) ||
                                             job.Status == TranslationJobStatus.Completed || newStatus == TranslationJobStatus.New;
                if (isInvalidStatusChange)
                {
                    return ApiResult.CreateError("invalid status change");
                }

                job.Status = newStatus;
                job.TranslatorId = translatorId;
                _context.SaveChanges();
                return ApiResult.Successfull;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateJobStatus)}(jobId={jobId}, translatorId={translatorId}, newStatus={newStatus}) exception.");
                return ApiResult.CreateError($"Unexpected exception: {ex.Message}");
            }
        }
    }
}