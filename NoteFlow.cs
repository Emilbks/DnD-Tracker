using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace Timekeeper_Program
{
	public class NoteFlow : Flow
	{
		[JsonInclude]
		public int frequency { get; private set; }
		[JsonInclude]
		public int offset { get; private set; }
		[JsonInclude]
		public int occurance { get; private set; }
		[JsonInclude]
		public int delay { get; private set; }

		public NoteFlow(long value, int frequency, int offset, int occurance, double tax, string reference, string sender, string recipient)
		{
			this.value = value;
			this.frequency = frequency;
			this.offset = offset % frequency;
			delay = offset - GlobalState.Instance.system_date.day % frequency;
			delay = delay / frequency > 0 ? delay : 0;
			this.occurance = occurance;
			this.tax = tax;
			this.reference = reference;
			this.sender = sender;
			this.recipient = recipient;
		}
		public void SetFrequency(int newFrequency) { frequency = newFrequency; }
		public void SetOffset(int newOffset) { offset = newOffset; }
		public void SetOccurance(int newOccurance) { occurance = newOccurance; }
		
		public NoteFlow Clone()
		{
			return new NoteFlow(value, frequency, offset, occurance, tax, reference, sender, recipient);
		}

		public override string ToString()
		{
			return $"NoteFlow: {reference} | Value: {value} | Frequency: {frequency} | Offset: {offset} | Occurance: {occurance} | Tax: {tax * 100}% | Reference: {reference} | Sender: {sender} | Recipient: {recipient}";
		}

		public string DisplayFlow(bool debug = false)
		{
			return $"{reference, -20}| {GlobalState.FormatNotes(value), 12} {Entity.GetFrequencyText(this), -12} at {tax * 100, 3}% tax | From {sender, -GlobalState.ENTITY_REFERENCE_LENGTH} to {recipient, -GlobalState.ENTITY_REFERENCE_LENGTH} | Offset: {offset, 3}, Occurance: {occurance, 3} | Next Trigger in {NextTriggerText(GlobalState.Instance.system_date.day), 3} day(s){(debug ? $" | ID: {id, 3} | Ref: {reference, GlobalState.NOTE_REFERENCE_LENGTH}" : "")}";
		}

		public int NextTriggerText(int currentDay) { int val = frequency - ((currentDay + frequency - offset) % frequency); return delay > frequency ? delay : (val == frequency ? frequency : val); }

		public void CheckAndProgressDelay() { if (delay > 0) delay--; }
	}
}
