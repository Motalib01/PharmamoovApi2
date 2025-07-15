using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers.HostedServices
{
    public class PaymentBackgroundWorker : IHostedService, IDisposable
    {
        private readonly ILoggerManager logMan;
        private Timer _timer;
        private APIConfigurationManager Config;
        private IPaymentBackgroundProcess PaymentProcess { get; }

        public PaymentBackgroundWorker(APIConfigurationManager _conf, ILoggerManager _logManager, IPaymentBackgroundProcess _iPayProcess)
        { 
            logMan = _logManager; 
            Config = _conf;
            PaymentProcess = _iPayProcess;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logMan.LogInfo("-- Start Payment Automatic Process at " + Config.HostedServicesConfig.HostedServiceRunningIntervalMins.ToString() + " intervals --");
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(Config.HostedServicesConfig.HostedServiceRunningIntervalMins));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {  
            logMan.LogInfo("-- Transfer Funds from Customer Wallets to Shop Wallets -- ");
            PaymentProcess.TransferFundsFromCustomerWalletToShopWallet();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logMan.LogInfo("-- STOP Payment Automatic Process --");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
