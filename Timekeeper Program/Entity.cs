using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
namespace Timekeeper_Program
{
	public class Entity
	{
		[JsonInclude]
		public int historicFlowIdCounter { get; private set; } = 0;
		[JsonInclude]
		public int noteFlowIdCounter { get; private set; } = 0;
		[JsonInclude]
		public HashSet<string> flowReferences { get; private set; } = new HashSet<string>();

		[JsonInclude]
		public int id { get; private set; } = -1;
		[JsonInclude]
		public string reference { get; private set; }
		[JsonInclude]
		public string name { get; private set; }
		[JsonInclude]
		public long balance { get; private set; }
		[JsonInclude]
		public List<NoteFlow> flows { get; private set; }
		[JsonInclude]
		public List<HistoricFlow> history { get; private set; }
		[JsonInclude]
		public bool isBalanceRelevant { get; private set; } = true;
		public Entity(string reference, string name, long balance) 
		{
			this.reference = reference;
			this.name = name;
			this.balance = balance;
			flows = new List<NoteFlow>();
			history = new List<HistoricFlow>();
		}

		[JsonConstructor]
		public Entity(string reference, string name, long balance, bool isBalanceRelevant) 
		{
			this.reference = reference;
			this.name = name;
			this.balance = balance;
			flows = new List<NoteFlow>();
			history = new List<HistoricFlow>();
			this.isBalanceRelevant = isBalanceRelevant;
		}

		public void CalculateFlow(Date day)
		{
			foreach (var flow in flows)
			{
				flow.CheckAndProgressDelay();
				if (flow.delay > 0) continue;
				if ((day.day - flow.offset) % flow.frequency > 0) continue;
				long oldBalance = balance;
				balance += (long)Math.Round(flow.value * (1 - flow.tax), 0);
				HistoricFlow hFlow = new HistoricFlow(day, HistoricFlowType.AppliedFlow);
				hFlow.SetValue(flow.value);
				hFlow.SetTax(flow.tax);
				hFlow.SetOldFlow(flow);
				hFlow.SetOldBalance(oldBalance);
				hFlow.SetNewBalance(balance);
				hFlow.SetRecipient(flow.recipient);
				hFlow.SetSender(flow.sender);
				AddHistory(hFlow);
				if (flow.occurance < 0) continue;
				flow.SetOccurance(flow.occurance - 1);
			}

			int size = flows.Count - 1;

			for (int i = size; i >= 0; i--)
			{
				if (flows[i].occurance == 0) RemoveFlow(flows[i]);
			}
		}

		public void DisplayEntity(bool debug = false)
		{
			Console.WriteLine(DisplayEntityAsString(debug));
		}

		public string DisplayEntityAsString(bool debug = false, bool markdown = false)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Entity: {name} ({reference} | ID: {id})");
			sb.AppendLine($"Balance: {(isBalanceRelevant ? GlobalState.FormatNotes(balance) : "Not Relevant")}");
			sb.AppendLine("Flows:");
			if (markdown) sb.AppendLine();
        	sb.AppendLine($"| {"Reference", -20} | {"Value", -37} | {"Direction", -(GlobalState.ENTITY_REFERENCE_LENGTH * 2 + 9)} | {"Occurance", -27} | {"Next Trigger", -26} |");
			sb.AppendLine($"|{new string('-', 22)}|{new string('-', 39)}|{new string('-', GlobalState.ENTITY_REFERENCE_LENGTH * 2 + 11)}|{new string('-', 29)}|{new string('-', 28)}|");
			foreach (NoteFlow flow in flows)
			{
				sb.AppendLine($"{flow.DisplayFlow(debug, markdown)}");
			}
			sb.AppendLine();
			return sb.ToString();
		}

		public void DisplayHistory()
		{
			Console.WriteLine(DisplayHistoryAsString());
		}

		public string DisplayHistoryAsString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"History for {name} ({reference} | ID: {id}):");
			foreach (HistoricFlow flow in history)
			{
				sb.AppendLine(flow.DisplayFlowAsString());
			}
			sb.AppendLine();
			return sb.ToString();
		}

		public string DisplayHistoryAsMarkdown()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"| Date | FlowType | Info | Balance |");
			sb.AppendLine($"|{new string('-', 12)}|{new string('-', 12)}|{new string('-', 80)}|{new string('-', 28)}|");
			foreach (HistoricFlow flow in history)
			{
				sb.AppendLine(flow.DisplayFlowAsString(markdown: true));
			}
			sb.AppendLine();
			return sb.ToString();
		}

		public static string GetFrequencyText(NoteFlow flow)
		{
			if (flow.frequency == 7) return "Weekly";
			if (flow.frequency == 28) return "Monthly";
			if (flow.frequency == 364) return "Yearly";
			if (flow.frequency == 1) return "Daily";
			return $"Every {flow.frequency} days";
		}

		public Result<bool, string> SetId(int id)
		{
			if (this.id != -1) return Result<bool, string>.Failure("ID has already been set.");
			this.id = id;
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> SetReference(string reference)
		{
			if (this.reference != null) return Result<bool, string>.Failure("Reference has already been set.");
			this.reference = reference;
			return Result<bool, string>.Success(true);
		}

		public void SetName(string name) { this.name = name; }

		public void SetBalance(long balance) { this.balance = balance; }
		
		public NoteFlow? GetNoteFlowByReference(string reference)
		{
			return flows.FirstOrDefault(f => f.reference == reference);
		}

		public NoteFlow? GetNoteFlowById(int id)
		{
			return flows.FirstOrDefault(f => f.id == id);
		}

		public NoteFlow? GetNoteFlowByReferenceOrId(string referenceOrId)
		{
			if (int.TryParse(referenceOrId, out int id)) return GetNoteFlowById(id);
			else return GetNoteFlowByReference(referenceOrId);
		}

		public Result<bool, string> AddFlow(NoteFlow flow, bool addHistory = true)
		{
			if (flowReferences.Contains(flow.reference))
			{
				return Result<bool, string>.Failure("Flow with the same reference already exists.");
			}

			if (addHistory)
			{
				HistoricFlow hFlow = new HistoricFlow(GlobalState.Instance.system_date, HistoricFlowType.AddedFlow);
				hFlow.SetOldFlow(flow);
				AddHistory(hFlow);
			}
			flow.SetId(noteFlowIdCounter++);
			flows.Add(flow);
			flowReferences.Add(flow.reference);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> AddHistory(HistoricFlow flow)
		{
			var result = flow.SetId(historicFlowIdCounter++);
			if (!result.IsSuccess) return Result<bool, string>.Failure(result.Error ?? "Failed to set ID for HistoricFlow.");
			history.Add(flow);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> ChangeFlow(NoteFlow flow, long newValue, int newFrequency, int newOffset, int newOccurance, double newTax, string newReference, string newSender, string newRecipient)
		{
			NoteFlow? oldFlow = flow;
			flow = new NoteFlow(newValue, newFrequency, newOffset, newOccurance, newTax, newReference, newSender, newRecipient);
			RemoveFlow(oldFlow, false);
			AddFlow(flow, false);

			HistoricFlow hFlow = new HistoricFlow(GlobalState.Instance.system_date, HistoricFlowType.ChangedFlow);
			hFlow.SetOldFlow(oldFlow);
			hFlow.SetNewFlow(flow);
			AddHistory(hFlow);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> RemoveFlow(NoteFlow flow, bool addHistory = true)
		{
			if (addHistory)
			{
				HistoricFlow hFlow = new HistoricFlow(GlobalState.Instance.system_date, HistoricFlowType.RemovedFlow);
				hFlow.SetOldFlow(flow);
				AddHistory(hFlow);
			}
			if (!flows.Remove(flow)) return Result<bool, string>.Failure("Failed to remove flow.");
			flowReferences.Remove(flow.reference);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> RemoveHistory(HistoricFlow flow)
		{
			if (!history.Remove(flow)) return Result<bool, string>.Failure("Failed to remove history.");
			return Result<bool, string>.Success(true);
		}

		public void DisplayBalance()
		{
			Console.WriteLine($"Balance for {name} is {GlobalState.FormatNotes(balance)}");
		}

		public bool ReferenceInUse(string reference)
		{
			return flowReferences.Contains(reference);
		}

		public void SetBalanceRelevance(bool isRelevant)
		{
			isBalanceRelevant = isRelevant;
		}
	}
}
