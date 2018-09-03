using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.PayEx.Data;
using Nop.Plugin.Payments.PayEx.Domain;
using Nop.Plugin.Payments.PayEx.Services;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Payments.PayEx.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string ContextName = "nop_object_context_payex_agreement";

        public int Order => 0;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // Register custom object context
            this.RegisterPluginDataContext<PayExAgreementObjectContext>(builder, ContextName);

            // Register services
            builder.RegisterType<PayExAgreementService>().As<IPayExAgreementService>();

            // Override the repository with our custom context
            builder.RegisterType<EfRepository<PayExAgreement>>()
                .As<IRepository<PayExAgreement>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(ContextName))
                .InstancePerLifetimeScope();
        }
    }
}