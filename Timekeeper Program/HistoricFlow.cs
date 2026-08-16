using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace Timekeeper_Program
{
	public class HistoricFlow : Flow
	{
		[JsonInclude]
        public Date date { get; private set; }
		[JsonInclude]
		public HistoricFlowType type { get; private set; }
		[JsonInclude]
        public Flow? oldFlow { get; private set; }
		[JsonInclude]
        public Flow? newFlow { get; private set; }
		[JsonInclude]
        public long oldBalance { get; private set; }
		[JsonInclude]
        public long newBalance { get; private set; }

		public HistoricFlow(Date date, HistoricFlowType type)
		{
			flowType = "HistoricFlow";
			this.date = date;
			this.type = type;
		}

		public HistoricFlow(Date date, HistoricFlowType type, Flow oldFlow)
		{
			flowType = "HistoricFlow";
			this.date = date;
			this.type = type;
			this.oldFlow = oldFlow;
		}
		

		public HistoricFlow(Date date, HistoricFlowType type, Flow oldFlow, Flow newFlow)
		{
			flowType = "HistoricFlow";
			this.date = date;
			this.type = type;
			this.oldFlow = oldFlow;
			this.newFlow = newFlow;
		}

		public HistoricFlow(Date date, HistoricFlowType type, Flow oldFlow, long oldBalance, long newBalance)
		{
			flowType = "HistoricFlow";
			this.date = date;
			this.type = type;
			this.oldFlow = oldFlow;
			this.oldBalance = oldBalance;
			this.newBalance = newBalance;
		}

		[JsonConstructor]
		private HistoricFlow(Date date, HistoricFlowType type, Flow oldFlow, Flow newFlow, long oldBalance, long newBalance)
		{
			flowType = "HistoricFlow";
			this.date = date;
			this.type = type;
			this.oldFlow = oldFlow;
			this.newFlow = newFlow;
			this.oldBalance = oldBalance;
			this.newBalance = newBalance;
		}

		public void SetDate(Date newDate) { date = newDate; }
		public void SetType(HistoricFlowType newType) { type = newType; }
		public void SetRecipient(string recipient) { this.recipient = recipient; }
		public void SetSender(string sender) { this.sender = sender; }
		public void SetReference(string reference) { this.reference = reference; }

		public void SetOldFlow(Flow oldFlow) { this.oldFlow = oldFlow ; }
        public void SetNewFlow(Flow newFlow) { this.newFlow = newFlow; }
        public void SetOldBalance(long oldBalance) { this.oldBalance = oldBalance; }
        public void SetNewBalance(long newBalance) { this.newBalance = newBalance; }

		
        public void DisplayFlow(bool debug = false)
        {
			string content;
			switch (type)
				{
					case HistoricFlowType.AddedFlow:
						content = $"{"Added Flow", -GlobalState.DESCRIPTION_LENGTH}: {oldFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH}";
						if (debug) content += $"\r\n- Flow: {oldFlow?.ToString()}";
						break;
					case HistoricFlowType.ChangedFlow:
						content = $"{"Changed Flow", -GlobalState.DESCRIPTION_LENGTH}: {oldFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH} to {newFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH}";
						if (debug) content += $"\r\n- Old Flow: {oldFlow?.ToString()}\r\n- New Flow: {newFlow?.ToString()}";
						break;
					case HistoricFlowType.RemovedFlow:
						content = $"{"Removed Flow", -GlobalState.DESCRIPTION_LENGTH}: {oldFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH}";
						if (debug) content += $"\r\n- Flow: {oldFlow?.ToString()}";
						break;
					case HistoricFlowType.AppliedFlow:
						content = $"{"Applied Flow", -GlobalState.DESCRIPTION_LENGTH}: {oldFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH} from {oldFlow?.sender, -GlobalState.NOTE_REFERENCE_LENGTH} to {oldFlow?.recipient, -GlobalState.NOTE_REFERENCE_LENGTH} with value {GlobalState.FormatNotes(value), 15} at {tax * 100, 3}% tax. | Balance {GlobalState.FormatNotes(oldBalance), 12} -> {GlobalState.FormatNotes(newBalance), 12}";
						break;
					case HistoricFlowType.TransactionFlow:
						content = $"{"Transaction", -GlobalState.DESCRIPTION_LENGTH}: {oldFlow?.reference, -GlobalState.NOTE_REFERENCE_LENGTH} from {oldFlow?.sender, -GlobalState.NOTE_REFERENCE_LENGTH} to {oldFlow?.recipient, -GlobalState.NOTE_REFERENCE_LENGTH} with value {GlobalState.FormatNotes(oldFlow?.value ?? 0), 15} at {oldFlow?.tax * 100 ?? 0, 3}% tax. | Balance {GlobalState.FormatNotes(oldBalance), 12} -> {GlobalState.FormatNotes(newBalance), 12}";
						break;
					default:
						content = $"{type, -GlobalState.DESCRIPTION_LENGTH}: {GlobalState.FormatNotes(value), 15}, tax {tax * 100}%";
						break;
				}
				Console.WriteLine($"{Date.WrittenDate(date), -10} - {content}");
        }
	}

	public enum HistoricFlowType
	{
		AddedFlow,
		ChangedFlow,
		RemovedFlow,
		AppliedFlow,
		TransactionFlow
	}
}
