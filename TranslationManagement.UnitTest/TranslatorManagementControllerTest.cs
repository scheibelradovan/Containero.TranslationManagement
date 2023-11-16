using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Api.Controllers;
using TranslationManagement.Api.Db;
using TranslationManagement.Common.Model;

namespace TranslationManagement.UnitTest
{
    [TestClass]
    public class TranslatorManagementControllerTest
    {
        private IServiceScopeFactory _scopeFactory = null;
        private ILogger<TranslatorManagementController> _logger = null;
        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection()
                .AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=TranslationAppDatabase.db"))
                .AddLogging()
                .BuildServiceProvider();
            _scopeFactory = services.GetService<IServiceScopeFactory>();
            _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            _logger = services.GetService<ILoggerFactory>().CreateLogger<TranslatorManagementController>();
        }

        [TestMethod]
        public void Test()
        {
            int count = 0;
            TranslatorManagementController translatorManagementController = new TranslatorManagementController(_scopeFactory, _logger);
            Translator[] translators = translatorManagementController.GetTranslators();
            Assert.AreEqual(count++, translators.Length);

            translatorManagementController.AddTranslator(new Translator()
            {
                CreditCardNumber = "1234 5678 9012 3456",
                HourlyRate = "30€",
                Name = "Translator 1",
                Status = TranslatorStatus.Applicant
            });
            translators = translatorManagementController.GetTranslators();
            Assert.AreEqual(count++, translators.Length);

            translatorManagementController.AddTranslator(new Translator()
            {
                CreditCardNumber = "1234 5678 9012 3457",
                HourlyRate = "33€",
                Name = "Translator 2",
                Status = TranslatorStatus.Certified
            });
            translators = translatorManagementController.GetTranslators();
            Assert.AreEqual(count++, translators.Length);
            translators = translatorManagementController.GetTranslatorsByName("Translator 1");
            Assert.AreEqual(1, translators.Length);

            translatorManagementController.UpdateTranslatorStatus(1, TranslatorStatus.Certified);
            translators = translatorManagementController.GetTranslatorsByName("Translator 2");
            Assert.AreEqual(1, translators.Length);
            Assert.AreEqual(TranslatorStatus.Certified, translators[0].Status);
        }
    }
}
