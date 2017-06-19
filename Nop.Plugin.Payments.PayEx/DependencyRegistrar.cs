using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Data;

namespace Nop.Plugin.Payments.PayEx
{
    public class DependencyRegistrar : IDependencyRegistrar
    {

        #region Private Constants

        private const string CONTEXT_NAME = "nop_object_context_payex_agreement";

        #endregion

        #region Private Methods

        /// <summary>
        /// Registers the I db context.
        /// </summary>
        /// <param name="componentContext">The component context.</param>
        /// <param name="dataSettings">The data settings.</param>
        /// <returns></returns>
        private PayExAgreementObjectContext RegisterIDbContext(IComponentContext componentContext, DataSettings dataSettings)
        {
            string dataConnectionString;

            if (dataSettings != null && dataSettings.IsValid())
                dataConnectionString = dataSettings.DataConnectionString;
            else
                dataConnectionString = componentContext.Resolve<DataSettings>().DataConnectionString;

            return new PayExAgreementObjectContext(dataConnectionString);
        }

        #endregion

        #region Public Properties

        public int Order { get { return 0; } }

        #endregion

        #region Public Methods

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // Load custom data settings
            var dataSettingsManager = new DataSettingsManager();
            DataSettings dataSettings = dataSettingsManager.LoadSettings();

            // Register custom object context
            builder.Register<IDbContext>(c => RegisterIDbContext(c, dataSettings)).Named<IDbContext>(CONTEXT_NAME).InstancePerLifetimeScope();
            builder.Register(c => RegisterIDbContext(c, dataSettings)).InstancePerLifetimeScope();

            // Register services
            builder.RegisterType<PayExAgreementService>().As<IPayExAgreementService>();

            // Override the repository with our custom context
            builder.RegisterType<EfRepository<PayExAgreement>>().As<IRepository<PayExAgreement>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME)).InstancePerLifetimeScope();
        }

        #endregion
    }
}
