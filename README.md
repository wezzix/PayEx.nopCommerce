# PayEx payment provider for nopCommerce with AutoPay (Direct Debit / Debit Card / Credit Card / Swish)

This is a free extension for the [PayEx payment provider](http://www.payex.com/), including Credit/Debit Card as well as Direct Debit and Swish. They are included as separate plugins so you can choose which payment methods you wish to use.

**No account is needed for testing, as a default test accound is built in. Just install, enable and go!**

## Features:

*   Credit Card and Direct Debit as separate payment methods.
*   Autopay/Manual Recurring Payments for Debit/Credit Card, meaning the customer can save his card for future purchases without having to enter the card details again.
*   Transaction mode: Authorize or Authorize and Capture. (Should the card be debited manually before the order is shipped, or immediately).
*   Transaction Callback ensures that all successful payments are properly registered.
*   Credit, Partial Credit and Cancel.
*   Test mode with built-in test account. When enabled, connects to PayEx test servers instead of production servers.
*   Localized: English and Swedish is included. The plugin is installed with english language resources. An additional resource file with swedish is included. To apply it, simply import it into the appropriate language in nop Admin.

# Instructions
You can either choose to (1) download the pre built plugin or (2) integrate the source with your Visual Studio solution.

## Alternative 1. Download the pre built plugin
The plugin is available in compiled form at the [nopCommerce marketplace](http://www.nopcommerce.com/p/839/payex-payment-module-for-with-autopay-direct-debit-debit-card-credit-card.aspx).

## Alternative 2. Integrate the source with your Visual Studio solution

The source code is available on Git under LGPLv3 license: https://github.com/wezzix/PayEx.nopCommerce/

The easiest way to get started using the source would be to clone/submodule or download the source to the src folder in your nopCommerce project folder, for instance: nopCommerce\src\PayEx.nopCommerce. This way the relative paths remain valid when you include the project files into your solution. Alternatively copy the project folders into your nopCommerce\src\Plugins folder.

If you have an existing Git project, I would suggest adding this repository as a **submodule** to your Git project in the **src** folder.

## Installation:

1.  Ensure the compiled plugins end up in the 'src\Presentation\Nop.Web\Plugins' folder. Please note that 'Payments.PayEx' is the core module and must always be installed in nop Admin, but it does not have to be active.
2.  Reload the list of plugins in nop Admin.
3.  Click 'Install' on 'Payments.PayEx' first, then on any other PayEx payment methods you wish to include as well.
4.  Optional: Load language resources for Swedish, or any translated resource file of your choise.

## Testing (optional):

1.  Click 'Configure' on 'Payments.PayEx' to edit settings.
2.  Make sure 'Use Test Mode' is checked. Account number is optional. If not specified, a default test account for nopCommerce users will be used.
3.  Optional: Enter your Transaction Callback URL in PayEx Merchant Admin for your TEST account.
4.  Activate PayEx in 'Configuration', 'Payment Methods'
5.  Make a test transaction using [fake card/direct debit details from PayEx](http://www.payexpim.com/test-data/test-purchase-data/). Please note that Swish is not enabled in the test account and will give an error message.

## Production mode:

1.  Click 'Configure' on 'Payments.PayEx' to edit settings.
2.  To the payment provider, enter your Account number and Encryption key for your PayEx production account and make sure 'Use Test Mode' is NOT checked.
3.  If you are using the feature 'Allow save credit card', then make sure that PxAgreement is enabled in PayEx Merchant Admin.
4.  Enter your Transaction Callback URL in PayEx Merchant Admin for your Production account.
5.  Activate PayEx in 'Configuration', 'Payment Methods' if you haven't done so already.
6.  Make a real transaction to ensure everything is set up correctly. You may cancel/credit this payment afterwards.

## How to obtain a PayEx Account:

*   If you don't already have an account with PayEx, please contact a sales person at [www.payex.com](http://www.payex.com).
*   If you need any help from PayEx, please contact [PayEx support](http://www.payex.com/company/support)

PayEx for nopCommerce was originally developed by Markus Kvist, markus.kvist@sdist.se
