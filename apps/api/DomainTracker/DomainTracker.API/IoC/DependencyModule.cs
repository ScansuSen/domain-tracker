using Autofac;
using DomainTracker.Business.Abstract;
using DomainTracker.Business.Concrete;
using DomainTracker.DataAccess.Concrete;
using DomainTracker.Entities.Models;
using Microsoft.AspNetCore.Identity;

namespace DomainTracker.API.IoC
{
    public class DependencyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(AuthService).Assembly)
                .Where(t => t.Name.EndsWith("Service", StringComparison.Ordinal) && t.GetInterface($"I{t.Name}") is not null)
                .As(t => t.GetInterface($"I{t.Name}")!)
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(DomainRepository).Assembly)
                .Where(t => t.Name.EndsWith("Repository", StringComparison.Ordinal) && t.GetInterface($"I{t.Name}") is not null)
                .As(t => t.GetInterface($"I{t.Name}")!)
                .InstancePerLifetimeScope();

            builder.RegisterType<JwtTokenGenerator>()
                .As<IJwtTokenGenerator>()
                .InstancePerLifetimeScope();
            builder.RegisterType<PasswordHasher<User>>()
                .As<IPasswordHasher<User>>()
                .SingleInstance();
        }
    }
}
