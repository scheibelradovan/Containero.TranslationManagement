using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TranslationManagement.Api.Controllers;
using TranslationManagement.Api.Db;
using TranslationManagement.Common.Model;

namespace TranslationManagement.UnitTest
{
    [TestClass]
    public class TranslationJobControllerTest
    {
        private IServiceScopeFactory _scopeFactory = null;
        private ILogger<TranslationJobController> _logger = null;
        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection()
                .AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=TranslationAppDatabase.db"))
                .AddLogging()
                .BuildServiceProvider();
            _scopeFactory = services.GetService<IServiceScopeFactory>();
            _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            _logger = services.GetService<ILoggerFactory>().CreateLogger<TranslationJobController>();
        }

        [TestMethod]
        public void Test()
        {
            int count = 1;
            TranslationJobController translatorManagementController = new TranslationJobController(_scopeFactory, _logger);
            TranslationJob[] jobs = translatorManagementController.GetJobs();
            Assert.AreEqual(count++, jobs.Length);

            ApiResult apiResult = translatorManagementController.CreateJob("Customer 1", "Sobotný večer.", 1);
            Assert.IsTrue(apiResult.Success);
            jobs = translatorManagementController.GetJobs();
            Assert.AreEqual(count++, jobs.Length);

            apiResult = translatorManagementController.CreateJob("Customer 2", "Nedeľné ráno.", 1);
            Assert.IsTrue(apiResult.Success);
            jobs = translatorManagementController.GetJobs();
            Assert.AreEqual(count++, jobs.Length);

            translatorManagementController.UpdateJobStatus(1, 1, TranslationJobStatus.Completed);
            TranslationJob job = translatorManagementController.GetJob(1);
            Assert.AreEqual(TranslationJobStatus.New, job.Status);
        }
    }
}
