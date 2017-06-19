using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Data;
using Nop.Plugin.Payments.PayEx.Domain;

namespace Nop.Plugin.Payments.PayEx.Services
{
    public class PayExAgreementService : IPayExAgreementService
    {
        
        #region Private Member Variables
        
        private readonly IRepository<PayExAgreement> _payExAgreementRepository;

        #endregion

        #region Constructor

        public PayExAgreementService(IRepository<PayExAgreement> payExAgreementRepository)
        {
            this._payExAgreementRepository = payExAgreementRepository;
        }

        #endregion

        #region Public Methods

        public virtual IEnumerable<PayExAgreement> GetAll()
        {
            var query = from gp in _payExAgreementRepository.Table
                        orderby gp.Id
                        select gp;
            return query;
        }

        public virtual PayExAgreement GetById(int payExAgreementId)
        {
            if (payExAgreementId == 0)
                return null;

            var record = _payExAgreementRepository.GetById(payExAgreementId);
            return record;
        }

        public PayExAgreement GetByAgreementRef(string agreementRef)
        {
            var query = from o in _payExAgreementRepository.Table
                        where o.AgreementRef == agreementRef
                        orderby o.Id
                        select o;
            var record = query.FirstOrDefault();
            return record;
        }

        public virtual IEnumerable<PayExAgreement> GetByCustomerId(int customerId)
        {
            if (customerId == 0)
                return null;

            var query = from o in _payExAgreementRepository.Table
                        where o.CustomerId == customerId
                        orderby o.Id
                        select o;
            return query;
        }

        public virtual IEnumerable<PayExAgreement> GetValidAgreements(int customerId, string paymentMethodSystemName)
        {
            if (customerId == 0)
                return null;

            DateTime expireDate = DateTime.Now.AddDays(-14);
            var query = from o in _payExAgreementRepository.Table
                        where o.CustomerId == customerId && o.PaymentMethodSystemName == paymentMethodSystemName &&
                            o.PaymentMethodExpireDate.HasValue && o.PaymentMethodExpireDate.Value > expireDate
                        orderby o.Id
                        select o;
            return query;
        }

        public virtual void InsertPayExAgreement(PayExAgreement payExAgreement)
        {
            if (payExAgreement == null)
                throw new ArgumentNullException("payExAgreement");

            _payExAgreementRepository.Insert(payExAgreement);
        }

        public virtual void UpdatePayExAgreement(PayExAgreement payExAgreement)
        {
            if (payExAgreement == null)
                throw new ArgumentNullException("payExAgreement");

            _payExAgreementRepository.Update(payExAgreement);
        }

        public virtual void DeletePayExAgreement(PayExAgreement record)
        {
            if (record == null)
                throw new ArgumentNullException("record");

            _payExAgreementRepository.Delete(record);
        }

        #endregion

    }
}
