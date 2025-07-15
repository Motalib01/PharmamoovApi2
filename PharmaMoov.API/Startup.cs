using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PharmaMoov.API.DataAccessLayer;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.DataAccessLayer.Repositories;
using PharmaMoov.API.Helpers;
using PharmaMoov.API.Helpers.HostedServices;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using PharmaMoov.API.Services.Abstractions;

namespace PharmaMoov.API
{
	public class Startup
	{
		APIConfigurationManager masterConfig = new APIConfigurationManager();
		public IConfiguration Configuration { get; }

        


		public Startup(IConfiguration configuration, IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			#region "Database Entities"
			services.AddDbContext<APIDBContext>(options => options.UseMySql(Configuration["Data:DataConnStr"]));
            Stripe.StripeConfiguration.ApiKey = Configuration["Stripe:SecretKey"];
            //Entities
            services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IAdminRepository, AdminRepository>();
			services.AddScoped<IAttachmentRepository, AttachmentRepository>();
			services.AddScoped<ICatalogueRepository, CatalogueRepository>();
			services.AddScoped<ICategoriesRepository, CategoriesRepository>();
			services.AddScoped<ICartRepository, CartRepository>();
			services.AddScoped<IDashboardRepository, DashboardRepository>();
			services.AddScoped<IFAQsRepository, FAQsRepository>();
			services.AddScoped<IOrderRepository, OrderRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IPromoRepository, PromoRepository>();
			services.AddScoped<IReviewRepository, ReviewsRepository>();
			services.AddScoped<IShopRepository, ShopRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IDeliveryJobRepository, DeliveryJobRepository>();
			services.AddScoped<IConfigRepository, ConfigRepository>();
			services.AddScoped<ICustomerRepository, CustomerRepository>();
			services.AddScoped<IUserHelpRequestRepository, UserHelpRequestRepository>();
			services.AddScoped<IPaymentRepository, PaymentRepository>();
			services.AddScoped<IHealthProfessionalRepository, HealthProfessionalRepository>();
			services.AddScoped<ICourierRepository, CourierRepository>();
			services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
			services.AddScoped<IPatientRepository, PatientRepository>();
			services.AddScoped<IDeliveryUserRepository, DeliveryUserRepository>();
			#endregion "Database Entities"

			services.AddScoped<IMainHttpClient, MainHttpClient>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			//medipim
            services.AddHttpClient<IMedipimService, MedipimService>();


            #region "ConfigurationBuilder"
            //build the master configuration
            masterConfig = new APIConfigurationManager
			{
				DataStrings = new DataStr
				{
					ConnStr = Configuration["Data:ConnStr"]
				},
				TokenKeys = new Token
				{
					Key = Configuration["Tokens:Key"],
					Exp = Convert.ToDouble(Configuration["Tokens:Exp"]),
					Audience = Configuration["Link:Audience"],
					Issuer = Configuration["Link:Issuer"]

				},
				PNConfig = new PushNotificationConfig
				{
					FireBLink = Configuration["PushNotification:FireBLink"],
					IOSFireBKey = Configuration["PushNotification:iOSFireBKey"],
					AndroidFireBKey = Configuration["PushNotification:androidFireBKey"],
					IOSSenderID = Configuration["PushNotification:iOSSenderID"],
					AndroidSenderID = Configuration["PushNotification:androidSenderID"]
				},
				MailConfig = new SMTPConfig
				{
					Server = Configuration["Mail:Server"],
					Password = Configuration["Mail:Password"],
					Username = Configuration["Mail:Username"],
					RegistrationLink = Configuration["Mail:RegistrationLink"],
					Port = Convert.ToInt32(Configuration["Mail:Port"]),
					EnableSSL = Convert.ToBoolean(Configuration["Mail:EnableSSL"])
				},
				SmsConfig = new SmsParameter
				{
					Endpoint = Configuration["Sms:Endpoint"],
					Action = Configuration["Sms:Action"],
					User = Configuration["Sms:User"],
					Password = Configuration["Sms:Password"],
					From = Configuration["Sms:From"],
					To = Configuration["Sms:To"],
					Text = Configuration["Sms:Text"]
				},
				MsgConfigs = new MessageConfigurations
				{
					RegisterMobileUser = Configuration["MsgConfigs:RegisterMobileUser"],
					RegisterShopUser = Configuration["MsgConfigs:RegisterShopUser"],
					ForgotPassword = Configuration["MsgConfigs:ForgotPassword"],
					ChangeNumber = Configuration["MsgConfigs:ChangeNumber"],
					ResendCode = Configuration["MsgConfigs:ResendCode"],
					InvoiceOFD = Configuration["MsgConfigs:InvoiceOFD"],
					InvoiceRFP = Configuration["MsgConfigs:InvoiceRFP"],
					CustomerForgotPassword = Configuration["MsgConfigs:CustomerForgotPassword"],
					ApproveUserAccount = Configuration["MsgConfigs:ApproveUserAccount"],
					OrderPlaced = Configuration["MsgConfigs:OrderPlaced"],
					OrderPlacedForShop = Configuration["MsgConfigs:OrderPlacedForShop"],
					ClosedShop = Configuration["MsgConfigs:ClosedShop"]
				},
				DeliveryJobConfig = new DeliveryJobConfig
				{
					ClientId = Configuration["DeliveryJobConfig:ClientId"],
					ClientSecret = Configuration["DeliveryJobConfig:ClientSecret"],
					BaseUrl = Configuration["DeliveryJobConfig:BaseUrl"],
					TokenEndpoint = Configuration["DeliveryJobConfig:TokenEndpoint"],
					AddressValidationEndpoint = Configuration["DeliveryJobConfig:AddressValidationEndpoint"],
					PricingEndpoint = Configuration["DeliveryJobConfig:PricingEndpoint"],
					JobValidationEndpoint = Configuration["DeliveryJobConfig:JobValidationEndpoint"],
					JobCreationEndpoint = Configuration["DeliveryJobConfig:JobCreationEndpoint"]
				},
				PaymentConfig = new PaymentConfig
				{
					ClientId = Configuration["PaymentConfig:ClientId"],
					ApiKey = Configuration["PaymentConfig:ApiKey"],
					CreditedId = Configuration["PaymentConfig:CreditedId"],
					WalletId = Configuration["PaymentConfig:WalletId"],
					BaseUrl = Configuration["PaymentConfig:BaseUrl"],
					ReturlUrl = Configuration["PaymentConfig:PaymentReturnUrl"],
					WebReturlUrl = Configuration["PaymentConfig:WebReturlUrl"]
				},
				PushNotifMessages = new PushNotifMessages
				{
					OrderStatus = Configuration["PushNotifMessages:OrderStatus"]
				},
				HostedServicesConfig = new HostedServicesConfig
				{
					EnableOrderAutoCancel = bool.Parse(Configuration["HostedServicesConfig:EnableOrderAutoCancel"]),
					AutoCancelRunningIntervalMins = Convert.ToInt32(Configuration["HostedServicesConfig:AutoCancelRunningIntervalMins"]),
					AutomaticFundsTransferIntervalHrs = Convert.ToInt32(Configuration["HostedServicesConfig:AutomaticFundsTransferIntervalhrs"]),
					EnabledAutomaticTransfer = bool.Parse(Configuration["HostedServicesConfig:EnabledAutomaticTransfer"]),
					HostedServiceRunningIntervalMins = Convert.ToInt32(Configuration["HostedServicesConfig:HostedServiceRunningIntervalMins"]),
					EnableAutoGenerateCommissionInvoice = bool.Parse(Configuration["HostedServicesConfig:EnableAutoGenerateCommissionInvoice"]),
					AutoCommissionInvoiceIntervalMins = Convert.ToInt32(Configuration["HostedServicesConfig:AutoCommissionInvoiceIntervalMins"]),
					EnableAutoPullDeliveryDistance = bool.Parse(Configuration["HostedServicesConfig:EnableAutoPullDeliveryDistance"]),
					AutoPullDeliveryDistanceIntervalMins = Convert.ToInt32(Configuration["HostedServicesConfig:AutoPullDeliveryDistanceInterval"])
				}
			};
			masterConfig.MapUrl = Configuration["Link:MapUrl"];
			masterConfig.HostURL = Configuration["Link:BaseUrl"];
			masterConfig.DefaultClientSite = Configuration["Link:StaticSite"];
			masterConfig.WebAPILink = Configuration["Link:WebAPILink"];
			services.AddSingleton(masterConfig);
			#endregion "ConfigurationBuilder"

			#region "JWT TOKEN Init"
			SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Tokens:Key"]));
			services.Configure<JwtIssuerOptions>(options =>
			{
				options.Issuer = Configuration["Link:Issuer"];
				options.Audience = Configuration["Link:Audience"];
				options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
			});


			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = Configuration["Link:Issuer"],

				ValidateAudience = true,
				ValidAudience = Configuration["Link:Audience"],

				ValidateIssuerSigningKey = true,
				IssuerSigningKey = _signingKey,

				RequireExpirationTime = false,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

			}).AddJwtBearer(configureOptions =>
			{
				configureOptions.ClaimsIssuer = Configuration["Link:Issuer"];
				configureOptions.TokenValidationParameters = tokenValidationParameters;
				configureOptions.SaveToken = true;
			});
			#endregion

			#region "Hosted Services"

			services.AddSingleton<IPaymentBackgroundProcess, PaymentBackgroundProcess>();
			services.AddSingleton<IPaymentDeliveryBackgroundProcess, PaymentDeliveryBackgroundProcess>();

			if (masterConfig.HostedServicesConfig.EnabledAutomaticTransfer == true &&
				masterConfig.HostedServicesConfig.HostedServiceRunningIntervalMins > 0)
			{
				services.AddHostedService<PaymentBackgroundWorker>();
			}
			if (masterConfig.HostedServicesConfig.HostedServiceRunningIntervalMins > 0)
			{
				services.AddHostedService<PaymentDeliveryBackgroundWorker>();
			}
			#endregion

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "PharmaMoov API", Version = "v1" });

				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "JWT Authorization header using the Bearer scheme."
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				 {
					 {
						   new OpenApiSecurityScheme
							 {
								 Reference = new OpenApiReference
								 {
									 Type = ReferenceType.SecurityScheme,
									 Id = "Bearer"
								 }
							 },
							 new string[] {}
					 }
				 });
			});

			#region "LOCALIZATION"
			services.AddScoped<SharedResource>();
			services.AddSingleton<LocalizationService>();
			services.AddLocalization(config => { config.ResourcesPath = "SharedLocalization"; });
            #endregion "LOCALIZATION"

            

            services.ConfigureLoggerService();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

			services.AddMvc(options => options.EnableEndpointRouting = false);
			services.AddMvc().AddNewtonsoftJson();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, APIDBContext db)
		{

			bool enablieDBRowsing = false;
			//if (env.IsDevelopment() || env.IsStaging())
			//{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
				// specifying the Swagger JSON endpoint.
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "PharmaMoov API V1");
				});
				enablieDBRowsing = true;
			//}

			#region "LOCALIZATION"
			var supportedCultures = new[]
		 {
				   new CultureInfo("en"),
				   new CultureInfo("ar"),
			 };

			app.UseRequestLocalization(new RequestLocalizationOptions
			{
				DefaultRequestCulture = new RequestCulture("en-US"),
				SupportedCultures = supportedCultures,
				SupportedUICultures = supportedCultures
			});
			#endregion "LOCALIZATION"

			db.Database.EnsureCreated();

			app.UseAuthentication();

            

//---------------------------------
            //app.UseFileServer(new FileServerOptions
            //{
            //	FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Resources")),
            //	RequestPath = new PathString("/Resources"),
            //	EnableDirectoryBrowsing = enablieDBRowsing
            //});
            var resourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
			if (!Directory.Exists(resourcePath))
			{
				Directory.CreateDirectory(resourcePath);
			}

			app.UseFileServer(new FileServerOptions
			{
				FileProvider = new PhysicalFileProvider(resourcePath),
				RequestPath = new PathString("/Resources"),
				EnableDirectoryBrowsing = enablieDBRowsing
			});
			//---------------------------------

			app.UseMvc();

		}
	}
}
