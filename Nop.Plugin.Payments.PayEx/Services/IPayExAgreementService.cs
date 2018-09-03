using System.Collections.Generic;
using Nop.Plugin.Payments.PayEx.Domain;

namespace Nop.Plugin.Payments.PayEx.Services
{
    public interface IPayExAgreementService
    {
        IEnumerable<PayExAgreement> GetAll();
        IEnumerable<PayExAgreement> GetByCustomerId(int customerId);
        IEnumerable<PayExAgreement> GetValidAgreements(int customerId, string paymentMethodSystemName);
        PayExAgreement GetByAgreementRef(string agreementRef);
        PayExAgreement GetById(int payExAgreementId);
        void InsertPayExAgreement(PayExAgreement payExAgreement);
        void UpdatePayExAgreement(PayExAgreement payExAgreement);
        void DeletePayExAgreement(PayExAgreement record);
    }
}