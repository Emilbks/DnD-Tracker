using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Timekeeper_Program
{
	public class GlobalState
	{
		public const double TAX = 0.25;
		public const int ENTITY_REFERENCE_LENGTH = 12;
		public const int NOTE_REFERENCE_LENGTH = 15;
		public const int DESCRIPTION_LENGTH = 20;
		public const bool DEBUG = true;
		public static GlobalState Instance { get; private set; } = new ();

		[JsonInclude]
		public int entityIdCounter { get; private set; } = 0;
		[JsonInclude]
		public Date system_date { get; private set; } = new Date(0);
		[JsonInclude]
		public List<Entity> entities { get; private set; } = new List<Entity>();

		public GlobalState() {}

		public void ProgressDay(int day = 1)
		{
			for (int i = system_date.day; system_date.day + day > i; i++)
			{
				ProgressEntityDay(new Date(i));
			}
			system_date = new Date(system_date.day + day);
		}

		private void ProgressEntityDay(Date day) 
		{
			foreach (Entity entity in entities)
			{
				if (entity == null) continue;
				entity.CalculateFlow(day);
			}
		}

		public void DisplayState(bool debug = false)
		{
			Console.WriteLine($"Date: {Date.WrittenDate(system_date)} (Day {system_date.day})");
			foreach (Entity entity in entities)
			{
				entity.DisplayEntity(debug);
			}
		}

		public static string FormatNotes(long amount)
		{
			bool negative = amount < 0;
			amount = Math.Abs(amount);
			long ones = amount % 1000;
			long thousands = amount / 1000 % 1000;
			long millions = amount / 1000000 % 1000;
			long billions = amount / 1000000000 % 1000;

			StringBuilder sb = new StringBuilder();
			if (negative) sb.Append("-");
			sb.Append("ɴ");
			if (amount >= 1e9) sb.Append($"{string.Format("{0:000}", billions)},");
			if (amount >= 1e6) sb.Append($"{(billions > 0 ? string.Format("{0:000}", millions) : millions)},");
			if (amount >= 1e3) sb.Append($"{(millions > 0 ? string.Format("{0:000}", thousands) : thousands)},");
			sb.Append($"{(thousands > 0 ? string.Format("{0:000}", ones) : ones)}");
			return sb.ToString();
		}

		public Result<bool, string> SetDate(int newDate, bool withYEARDAYS = true, bool withProgress = true)
		{
			if (withYEARDAYS && newDate < Date.YEARDAYS) newDate += Date.YEARDAYS * system_date.year;

			if (withProgress && newDate < system_date.day) return Result<bool, string>.Failure("Cannot set date to a past value.");
			if (withProgress) ProgressDay(newDate - system_date.day);
			else system_date.SetDate(newDate);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> AddEntity(Entity entity)
		{
			var result = entity.SetId(entityIdCounter++);
			if (!result.IsSuccess) return Result<bool, string>.Failure(result.Error ?? "Failed to set ID for Entity.");
			entities.Add(entity);
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> RemoveEntity(Entity entity)
		{
			if (!entities.Remove(entity)) return Result<bool, string>.Failure("Failed to remove entity.");
			return Result<bool, string>.Success(true);
		}

		public Result<bool, string> RemoveEntityById(int id)
		{
			Entity? entity = GetEntityById(id);
			if (entity != null) return RemoveEntity(entity);
			return Result<bool, string>.Failure("Entity not found.");
		}

		public Result<bool, string> RemoveEntityByReference(string reference)
		{
			Entity? entity = GetEntityByReference(reference);
			if (entity != null) return RemoveEntity(entity);
			return Result<bool, string>.Failure("Entity not found.");
		}

		public Result<bool, string> MakeTransaction(Entity? senderEntity, Entity? recipientEntity, long value, double tax, string reference)
		{
			if (senderEntity == null) return Result<bool, string>.Failure("Sender entity not found.");
			if (recipientEntity == null) return Result<bool, string>.Failure("Recipient entity not found.");

			TransactionFlow senderFlow = new TransactionFlow(reference, value, tax, senderEntity.reference, recipientEntity.reference);
			TransactionFlow recipientFlow = new TransactionFlow(reference, value, tax, senderEntity.reference, recipientEntity.reference);

			long senderBalanceBefore = senderEntity.balance;
			long recipientBalanceBefore = recipientEntity.balance;
			long senderBalanceAfter = senderEntity.balance - value;
			long recipientBalanceAfter = recipientEntity.balance + value - (long)(value * tax);
			
			senderEntity.SetBalance(senderBalanceAfter);
			var senderResult = senderEntity.AddHistory(new HistoricFlow(system_date, HistoricFlowType.TransactionFlow, senderFlow, senderBalanceBefore, senderBalanceAfter));
			if (!senderResult.IsSuccess) return Result<bool, string>.Failure(senderResult.Error ?? "Failed to add transaction flow to sender entity history.");

			recipientEntity.SetBalance(recipientBalanceAfter);
			var recipientResult = recipientEntity.AddHistory(new HistoricFlow(system_date, HistoricFlowType.TransactionFlow, recipientFlow, recipientBalanceBefore, recipientBalanceAfter));
			if (!recipientResult.IsSuccess) return Result<bool, string>.Failure(recipientResult.Error ?? "Failed to add transaction flow to recipient entity history.");

			return Result<bool, string>.Success(true);
		}

		public Entity? GetEntityById(int id)
		{
			foreach (Entity entity in entities)
			{
				if (entity.id == id) return entity;
			}
			return null;
		}

		public Entity? GetEntityByReference(string reference)
		{
			foreach (Entity entity in entities)
			{
				if (entity.reference == reference) return entity;
			}
			return null;
		}

		public static void SetInstance(GlobalState newInstance)
		{
			Instance = newInstance;
		}

		public List<Entity> GetEntities()
		{
			return entities;
		}
	}
}
