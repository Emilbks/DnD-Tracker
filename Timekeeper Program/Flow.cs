using System.Text.Json.Serialization;
namespace Timekeeper_Program
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName="$flowType")]
    [JsonDerivedType(typeof(HistoricFlow), "HistoricFlow")]
    [JsonDerivedType(typeof(NoteFlow), "NoteFlow")]
    [JsonDerivedType(typeof(TransactionFlow), "TransactionFlow")]
	public abstract class Flow
	{
        [JsonInclude]
        protected string flowType { get; set; } = "";
        [JsonInclude]
        public int id { get; protected set; } = -1;
        [JsonInclude]
        public string reference { get; protected set; } = "";
        [JsonInclude]
        public long value { get; protected set; }
        [JsonInclude]
        public double tax { get; protected set; }
		[JsonInclude]
		public string recipient { get; protected set; } = "";
        [JsonInclude]
        public string sender { get; protected set; } = "";

        /*
         * This property is true if both the sender and recipient are actual entities in the system. If either the sender or recipient is not an entity, this property will be false.
         */
        [JsonInclude]
        public bool trueFlow { get; protected set; } = true;
        
        public Result<bool, string> SetId(int newId) 
        {
            if (id != -1) return Result<bool, string>.Failure("ID has already been set.");
            id = newId;
            return Result<bool, string>.Success(true);
        }
        public Result<bool, string> SetValue(long newValue) 
        { 
            value = newValue; 
            return Result<bool, string>.Success(true);
        }
        public Result<bool, string> SetRecipient(string newRecipient, bool isEntity = true) 
        { 
            if (string.IsNullOrEmpty(newRecipient)) return Result<bool, string>.Failure("Recipient cannot be null or empty.");
            if (isEntity && GlobalState.Instance.GetEntityByReference(newRecipient) == null) return Result<bool, string>.Failure($"Recipient '{newRecipient}' is not a valid entity.");
            recipient = newRecipient; 
            trueFlow = GlobalState.Instance.GetEntityByReference(sender) != null && GlobalState.Instance.GetEntityByReference(recipient) != null;
            return Result<bool, string>.Success(true);
        }
        public Result<bool, string> SetSender(string newSender, bool isEntity = true) 
        { 
            if (string.IsNullOrEmpty(newSender)) return Result<bool, string>.Failure("Sender cannot be null or empty.");
            if (isEntity && GlobalState.Instance.GetEntityByReference(newSender) == null) return Result<bool, string>.Failure($"Sender '{newSender}' is not a valid entity.");
            sender = newSender; 
            trueFlow = GlobalState.Instance.GetEntityByReference(sender) != null && GlobalState.Instance.GetEntityByReference(recipient) != null;
            return Result<bool, string>.Success(true);
        }
		public void SetTax(double newTax) { tax = newTax; }
    }
}