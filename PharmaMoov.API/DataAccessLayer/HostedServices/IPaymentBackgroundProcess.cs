using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers.HostedServices
{
    public interface IPaymentBackgroundProcess
    {
        void TransferFundsFromCustomerWalletToShopWallet();
    }
}
