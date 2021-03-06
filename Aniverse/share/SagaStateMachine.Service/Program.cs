using Aniverse.MessageContracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service.Instruments.Post;
using SagaStateMachine.Service.StateMachines;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddMassTransit(configure =>
        {
            configure.AddSagaStateMachine<AppStateMachine, AppStateInstance>()
              .EntityFrameworkRepository(options =>
              {
                  options.AddDbContext<DbContext, AppStateDbContext>((provider, builder) =>
                  {
                      builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SQLServer"));
                  });
              });
            configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(config =>
            {
                config.Host(new Uri(RabbitMqConstants.URI), h =>
                {
                    h.Username(RabbitMqConstants.Username);
                    h.Password(RabbitMqConstants.Password);
                });
                
                config.ReceiveEndpoint(RabbitMqConstants.StateMachine, ep =>
                {
                    ep.UseMessageRetry(r => r.Interval(2, 100));
                    ep.UseInMemoryOutbox();
                    ep.ConfigureSaga<AppStateInstance>(provider);
                });
            }));
        });
    })
    .Build();

await host.RunAsync();
