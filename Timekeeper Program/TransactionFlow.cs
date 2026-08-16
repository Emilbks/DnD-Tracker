using System.Text.Json.Serialization;
namespace Timekeeper_Program
{
	public class TransactionFlow : Flow
	{
        

        public TransactionFlow(string reference, long value, double tax, string sender, string recipient)
        {
            flowType = "TransactionFlow";
            this.reference = reference;
            this.value = value;
            this.tax = tax;
            this.sender = sender;
            this.recipient = recipient;
        }
    }
}