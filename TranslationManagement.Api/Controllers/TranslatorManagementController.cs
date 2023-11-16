using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TranslationManagement.Api.Db;
using TranslationManagement.Common.Model;

namespace TranslationManagement.Api.Controllers
{
    [ApiController]
    [Route("api/TranslatorsManagement/[action]")]
    public class TranslatorManagementController : ControllerBase
    {
        private readonly ILogger<TranslatorManagementController> _logger;
        private AppDbContext _context;

        public TranslatorManagementController(IServiceScopeFactory scopeFactory, ILogger<TranslatorManagementController> logger)
        {
            _context = scopeFactory.CreateScope().ServiceProvider.GetService<AppDbContext>();
            _logger = logger;
        }

        [HttpGet]
        public Translator[] GetTranslators()
        {
            return _context.Translators.ToArray();
        }

        [HttpGet]
        public Translator[] GetTranslatorsByName(string name)
        {
            return _context.Translators.Where(t => t.Name == name).ToArray();
        }

        [HttpPost]
        public bool AddTranslator(Translator translator)
        {
            _context.Translators.Add(translator);
            return _context.SaveChanges() > 0;
        }

        [HttpPost]
        public ApiResult UpdateTranslatorStatus(int translatorId, TranslatorStatus newStatus)
        {
            try
            {
                _logger.LogInformation($"User status update request: {newStatus} for user {translatorId}");
                if (newStatus == 0)
                {
                    return ApiResult.CreateError("Unknown status!");
                }

                Translator translator = _context.Translators.Single(j => j.Id == translatorId);
                if (translator == null)
                {
                    return ApiResult.CreateError("Translator was not found!");
                }
                translator.Status = newStatus;
                _context.SaveChanges();

                return ApiResult.Successfull;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(UpdateTranslatorStatus)}(translatorId={translatorId}, newStatus={newStatus}) exception.");
                return ApiResult.CreateError($"Unexpected exception: {ex.Message}");
            }
        }
    }
}