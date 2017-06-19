using System;

namespace SD.Payex2
{
    public class PayexAccount
    {
        /// <summary>
        /// Construct a payex account from a string on the format &lt;account number&gt;:&lt;encryption key&gt;
        /// </summary>
        /// <param name="combined"></param>
        public PayexAccount(string combined)
        {
            if (!string.IsNullOrEmpty(combined))
            {
                var parts = combined.Split(':');
                int accNo;
                if (parts.Length >= 2 && int.TryParse(parts[0], out accNo))
                {
                    AccountNumber = accNo;
                    EncryptionKey = parts[1];
                    return;
                }
            }

            throw new ArgumentException(
                $"Unexpected format for payex account '{combined}'. Expected format '<account number>:<encryption key>'.",
                nameof(combined));
        }

        public PayexAccount(int accountNumber, string encryptionKey)
        {
            AccountNumber = accountNumber;
            EncryptionKey = encryptionKey;
        }

        public int AccountNumber { get; set; }

        public string EncryptionKey { get; set; }

        public bool IsValid()
        {
            return AccountNumber > 0 && !string.IsNullOrEmpty(EncryptionKey);
        }

        public override string ToString()
        {
            return $"{AccountNumber}:{EncryptionKey}";
        }
    }
}