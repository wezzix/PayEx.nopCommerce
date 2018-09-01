using System.Xml.Linq;
using SD.Payex2.Entities;

namespace SD.Payex2.Utilities
{
    internal static class ResultParser
    {
        #region Private Methods

        private static void ParseStatus(BaseResult result, XElement statusElement)
        {
            foreach (var node in statusElement.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                switch (name)
                {
                    case "code":
                        result.Code = value;
                        break;
                    case "errorcode":
                        result.ErrorCode = value;
                        break;
                    case "description":
                        result.Description = value;
                        break;
                    case "paramname":
                        result.ParamName = value;
                        break;
                    case "thirdpartyerror":
                        result.ThirdPartyError = value;
                        break;
                }
            }
        }

        private static BaseTransactionResult ParseTransactionResult(BaseTransactionResult result, XElement root)
        {
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                int i;
                switch (name)
                {
                    case "status":
                        ParseStatus(result, node);
                        break;
                    case "transactionstatus":
                        int.TryParse(value, out i);
                        result.TransactionStatus = (Enumerations.TransactionStatusCode)i;
                        break;
                    case "originaltransactionnumber":
                        result.OriginalTransactionNumber = value;
                        break;
                    case "transactionnumber":
                        result.TransactionNumber = value;
                        break;
                }
            }

            return result;
        }

        #endregion

        #region Public Methods

        public static BaseResult ParseBaseResult(string xmlText)
        {
            var result = new BaseResult { RawXml = xmlText };
            var doc = XDocument.Parse(xmlText);
            var root = doc.Element("payex");
            ParseStatus(result, root.Element("status"));
            return result;
        }

        public static InitializeResult ParseInitializeResult(string xmlText)
        {
            var result = new InitializeResult { RawXml = xmlText };
            var doc = XDocument.Parse(xmlText);
            var root = doc.Element("payex");
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                switch (name)
                {
                    case "status":
                        ParseStatus(result, node);
                        break;
                    case "orderref":
                        result.OrderRef = value;
                        break;
                    case "redirecturl":
                        result.RedirectUrl = value;
                        break;
                    case "sessionref":
                        result.SessionRef = value;
                        break;
                }
            }

            return result;
        }

        public static CaptureResult ParseCaptureResult(string xmlText)
        {
            var result = new CaptureResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            return result;
        }

        public static CreditResult ParseCreditResult(string xmlText)
        {
            var result = new CreditResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            return result;
        }

        public static CancelResult ParseCancelResult(string xmlText)
        {
            var result = new CancelResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            return result;
        }

        public static CompleteResult ParseCompleteResult(string xmlText)
        {
            var result = new CompleteResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                int i;
                switch (name)
                {
                    case "orderid":
                        result.OrderID = value;
                        break;
                    case "paymentmethod":
                        result.PaymentMethod = value;
                        break;
                    case "amount":
                        int.TryParse(value, out i);
                        result.Amount = i / 100.0M;
                        break;
                    case "agreementref":
                        result.AgreementRef = value;
                        break;
                    case "paymentmethodexpiredate":
                        result.PaymentMethodExpireDate = value.DateFromPayEx();
                        break;
                    case "maskednumber":
                        result.MaskedNumber = value;
                        break;
                    case "transactionerrorcode":
                        result.TransactionErrorCode = value;
                        break;
                    case "transactionerrordescription":
                        result.TransactionErrorDescription = value;
                        break;
                    case "transactionthirdpartyerror":
                        result.TransactionThirdPartyError = value;
                        break;
                    case "alreadycompleted":
                        result.AlreadyCompleted = string.Compare(value, "true", true) == 0;
                        break;
                    case "pending":
                        result.Pending = string.Compare(value, "true", true) == 0;
                        break;
                }
            }

            return result;
        }

        public static TransactionDetailsResult ParseTransactionDetailsResult(string xmlText)
        {
            var result = new TransactionDetailsResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            var isCredit = result.TransactionStatus == Enumerations.TransactionStatusCode.Credit;

            int i;
            if (int.TryParse(root.GetString("debitAmount"), out i))
                result.Amount = i / 100.0M * (isCredit ? -1 : 1);

            result.CurrencyCode = root.GetString("debitCurrency");
            result.AccountNumber = isCredit
                ? root.GetNullableInt("debitAccountNumber")
                : root.GetNullableInt("creditAccountNumber");

            result.OrderDescription = root.GetString("orderDescription");
            result.OrderId = root.GetString("orderId");

            result.PaymentMethod = root.GetString("paymentMethod");

            return result;
        }

        public static AutoPayResult ParseAutoPayResult(string xmlText)
        {
            var result = new AutoPayResult { RawXml = xmlText };
            var root = result.GetRootElement();
            ParseTransactionResult(result, root);
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                switch (name)
                {
                    case "paymentmethod":
                        result.PaymentMethod = value;
                        break;
                }
            }

            return result;
        }

        public static CreateAgreementResult ParseCreateAgreementResult(string xmlText)
        {
            var result = new CreateAgreementResult { RawXml = xmlText };
            var root = result.GetRootElement();
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                switch (name)
                {
                    case "status":
                        ParseStatus(result, node);
                        break;
                    case "agreementref":
                        result.AgreementRef = value;
                        break;
                }
            }

            return result;
        }

        public static DeleteAgreementResult ParseDeleteAgreementResult(string xmlText)
        {
            var result = new DeleteAgreementResult { RawXml = xmlText };
            var root = result.GetRootElement();
            foreach (var node in root.Elements())
            {
                var value = node.Value;
                var name = node.Name.LocalName.ToLower();
                switch (name)
                {
                    case "status":
                        ParseStatus(result, node);
                        break;
                    case "agreementref":
                        result.AgreementRef = value;
                        break;
                }
            }

            return result;
        }

        #endregion
    }
}