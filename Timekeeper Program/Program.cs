using Timekeeper_Program;
using System.Text.Json;

string[] debug_params = ["debug", "-debug", "--debug", "d", "-d", "--d", "true", "-true", "--true", "t", "-t", "--t", "1"];
string[] force_params = ["force", "-force", "--force", "f", "-f", "--f", "true", "-true", "--true", "t", "-t", "--t", "1"];
string[] bool_params = ["true", "-true", "--true", "t", "-t", "--t", "1", "false", "-false", "--false", "f", "-f", "--f", "0", "-0", "--0"];
string[] withEntities_params = ["withentities", "-withentities", "--withentities", "we", "-we", "--we", "true", "-true", "--true", "t", "-t", "--t", "1"];
int format;

GlobalState state = GlobalState.Instance;
string? loadedGlobalState = null;
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    IncludeFields = true,
    PropertyNameCaseInsensitive = true
};

LoadGlobalState("political_intrigue");
//SetupEntities(state);
;

while (true)
{
    Console.WriteLine("Enter a command (progress, display, exit):");
    string? input = Console.ReadLine();
    if (input == null) continue;
    string[] strings = input.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
    
    Result<bool, string> result;

    switch (strings[0].ToLower())
    {
        case "!":
        case "h":
            Help();
            break;
        case "p":
            Progress(strings.Length >= 2 && int.TryParse(strings[1], out int days) ? days : null);
            break;

        case "ds":
            state.DisplayState(strings.Length >= 2 && bool_params.Contains(strings[1].ToLower()), strings.Length >= 3 && debug_params.Contains(strings[2].ToLower()));
            break;

        case "de":
            result = DisplayEntity(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 && debug_params.Contains(strings[2].ToLower()));
            if (!result.IsSuccess) Console.WriteLine(result.Error);
            break;

        case "deh":
            result = DisplayEntityHistory(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 && debug_params.Contains(strings[2].ToLower()));
            if (!result.IsSuccess) Console.WriteLine(result.Error);
            break;

        case "af":
            result = AddNoteFlowToEntity(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 && int.TryParse(strings[2], out format) ? format : null, strings.Length >= 11 ? strings[3..] : null);
            if (!result.IsSuccess) Console.WriteLine(result.Error);
            break;

        case "cf":
            result = ChangeNoteFlowOnEntity(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 ? strings[2] : null, strings.Length >= 4 && int.TryParse(strings[3], out format) ? format : null, strings.Length >= 12 ? strings[4..] : null);
            if (!result.IsSuccess) Console.WriteLine(result.Error);
            break;

        case "rf":
            result = RemoveNoteFlowFromEntity(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 ? strings[2] : null, strings.Length >= 4 && force_params.Contains(strings[3].ToLower()));
            if (!result.IsSuccess) Console.WriteLine(result.Error);
            break;

        case "mt":
            result = MakeTransaction(strings.Length >= 2 ? GetEntityByReferenceOrId(strings[1]) : null, strings.Length >= 3 ? GetEntityByReferenceOrId(strings[2]) : null, strings.Length >= 4 && long.TryParse(strings[3], out long value) ? value : 0, strings.Length >= 5 ? strings[4] : GlobalState.TAX.ToString(), strings.Length >= 6 ? strings[5] : "manual_transaction");
            break;

        case "s":
            SaveGlobalState(strings.Length >= 2 ? strings[1] : null);
            break;
            
        case "l":
            LoadGlobalState(strings.Length >= 2 ? strings[1] : null);
            break;

        case "sl":
            SaveAndLoadGlobalState(strings.Length >= 2 ? (strings[1] == "null" ? loadedGlobalState : strings[1]) : null, strings.Length >= 3 ? strings[2] : null);
            break;

        case "fresh":
            FreshGlobalState(strings.Length >= 2 && bool_params.Contains(strings[1].ToLower()), strings.Length >= 3 && withEntities_params.Contains(strings[2].ToLower()));
            break;


        case "c":
        case "e":
        case "q":
            ExitAs();
            break;
        case "c!":
        case "e!":
        case "q!":
            Exit();
            break;
        default:
            Console.WriteLine("Unknown command.");
            break;
    }
}

void Help()
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("h | ! -> help: Display this help message.");
    Console.WriteLine("p [days] -> progress: Progress the simulation by a number of days.");
    Console.WriteLine("ds [relevant] [debug] -> display state: Display the current state of all entities.");
    Console.WriteLine("de [reference|id] [debug] -> display entity: Display details of a specific entity.");
    Console.WriteLine("deh [reference|id] [debug] -> display entity history: Display the history of a specific entity.");
    Console.WriteLine("af [reference|id] [format] [parameters (x9)] -> add flow: Add a new NoteFlow to a specific entity. Format 1 for one by one, 2 for comma-separated, 3 for parameters.");
    Console.WriteLine("cf [reference|id] [format] [flow reference|id] [parameters (x9)] -> change flow: Change an existing NoteFlow on a specific entity. Format 1 for one by one, 2 for comma-separated, 3 for parameters.");
    Console.WriteLine("rf [reference|id] [flow reference|id] [force] -> remove flow: Remove a NoteFlow from a specific entity. Use 'true' or 'force' to skip confirmation.");
    Console.WriteLine("mt [sender reference|id] [recipient reference|id] [value] [tax] [reference] -> make transaction: Make a transaction between two entities.");
    Console.WriteLine("s [filename] -> save: Save the current global state to a file.");
    Console.WriteLine("l [filename] -> load: Load a global state from a file.");
    Console.WriteLine("sl [saveFilename] [loadFilename] -> save and load: Save the current global state to a file and then load a global state from a file.");
    Console.WriteLine("fresh -> fresh: Create a new, empty global state.");
    Console.WriteLine("c | e | q -> exit: Exit the program. You will be prompted to save.");
    Console.WriteLine("c! | e! | q! -> exit without saving: Exit the program without saving the current state.");
}

void ExitAs() 
{
    Console.WriteLine("Save current state before exiting? (y/n/c/save as [filename])");
    string? saveInput = Console.ReadLine();
    if (saveInput != null && saveInput.ToLower() == "y")
    {
        if (loadedGlobalState != null)
        {
            SaveGlobalState(loadedGlobalState);
        }
        else
        {
            Console.WriteLine("Enter filename to save:");
            string? filename = Console.ReadLine();
            SaveGlobalState(filename);
        }
    }
    else if (saveInput != null && saveInput.ToLower().StartsWith("save as "))
    {
        string filename = saveInput.Substring(8).Trim();
        SaveGlobalState(filename);
    }
    else if (saveInput != null && saveInput.ToLower() == "c")
    {
        Console.WriteLine("Exit cancelled.");
        return;
    }
    Exit();
}

void Exit()
{
    Console.WriteLine("Exiting program.");
    Environment.Exit(0);
}

void Progress(int? days = null)
{
    if (days == null)
    {
        Console.WriteLine("Enter number of days to progress:");
        string? daysInput = Console.ReadLine();
        if (int.TryParse(daysInput, out int parsedDays))
        {
            days = parsedDays;
        }
        else
        {
            Console.WriteLine("Invalid number of days.");
            return;
        }
    }
    state.ProgressDay(days.Value);
    Console.WriteLine($"Progressed {days.Value} days.");
}

Result<bool, string> DisplayEntity(Entity? entity = null, bool debug = false)
{
    if (entity == null) entity = GetEntityManual();
    if (entity == null) return Result<bool, string>.Failure("Entity not found.");

    Console.WriteLine();
    Console.WriteLine($"Date: {Date.WrittenDate(state.system_date)}");
    entity.DisplayEntity(debug);
    return Result<bool, string>.Success(true);
}

Result<bool, string> DisplayEntityHistory(Entity? entity = null, bool debug = false)
{
    if (entity == null) entity = GetEntityManual();
    if (entity == null) return Result<bool, string>.Failure("Entity not found.");

    Console.WriteLine();
    entity.DisplayHistory();
    return Result<bool, string>.Success(true);
}

Result<bool, string> ChangeNoteFlowOnEntity(Entity? entity = null, string? reference = null, int? format = 3, string[]? parameters = null)
{
    if (entity == null) entity = GetEntityManual();
    if (entity == null) return Result<bool, string>.Failure("Entity not found.");

    if (format == null || (format != 1 && format != 2 && format != 3))
    {
        Console.WriteLine("Select flow format: 1. one by one, 2. comma-separated, 3. parameters");
        string? formatInput = Console.ReadLine();
        if (formatInput == null || !int.TryParse(formatInput, out int parsedFormat) || (parsedFormat != 1 && parsedFormat != 2 && parsedFormat != 3))
        {
            Console.WriteLine("Invalid format selection.");
            return Result<bool, string>.Failure("Invalid format selection.");
        }
        format = parsedFormat;
    }

    if (string.IsNullOrWhiteSpace(reference))
    {
        Console.WriteLine("Enter flow reference or id to change:");
        reference = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(reference)) return Result<bool, string>.Failure("Reference is required.");
    }

    NoteFlow? flowToChange = entity.GetNoteFlowByReferenceOrId(reference);
    if (flowToChange == null)
    {
        Console.WriteLine($"Flow with reference or id '{reference}' not found in entity '{entity.name}'.");
        return Result<bool, string>.Failure($"Flow with reference or id '{reference}' not found in entity '{entity.name}'.");
    }

    NoteFlow newFlow;

    Result<NoteFlow, string> result;
    switch (format)
    {
        case 1:
            result = GetNoteFlowFromUser(entity);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Failed to get new flow: {result.Error}");
                return Result<bool, string>.Failure($"Failed to get new flow: {result.Error}");
            }
            newFlow = result.Ok!;
            break;
        case 2:
            result = GetNoteFlowFromUserCommaSeparated();
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Failed to get new flow: {result.Error}");
                return Result<bool, string>.Failure($"Failed to get new flow: {result.Error}");
            }
            newFlow = result.Ok!;
            break;
        case 3:
            if (parameters == null || parameters.Length != 8)
            {
                Console.WriteLine("Invalid parameters. Expected 8 parameters for value, frequency, offset, occurance, tax, reference, sender, recipient.");
                return Result<bool, string>.Failure("Invalid parameters.");
            }
            try
            {
                for (int i = 0; i < parameters.Length; i++) parameters[i] = parameters[i].Trim().ToLower().Equals("null") ? "" : parameters[i].Trim();

                long value = long.Parse(string.IsNullOrWhiteSpace(parameters[0]) ? flowToChange.value.ToString() : parameters[0]);
                int frequency = int.Parse(string.IsNullOrWhiteSpace(parameters[1]) ? flowToChange.frequency.ToString() : parameters[1]);
                string offsetInput = string.IsNullOrWhiteSpace(parameters[2]) ? flowToChange.offset.ToString() : parameters[2];
                int occurance = int.Parse(string.IsNullOrWhiteSpace(parameters[3]) ? flowToChange.occurance.ToString() : parameters[3]);
                string taxInput = string.IsNullOrWhiteSpace(parameters[4]) ? flowToChange.tax.ToString(System.Globalization.CultureInfo.InvariantCulture) : parameters[4];
                string newReference = string.IsNullOrWhiteSpace(parameters[5]) ? flowToChange.reference : parameters[5];
                string sender = string.IsNullOrWhiteSpace(parameters[6]) ? flowToChange.sender : parameters[6];
                string recipient = string.IsNullOrWhiteSpace(parameters[7]) ? flowToChange.recipient : parameters[7];

                result = MakeNoteFlow(value, frequency, offsetInput, occurance, taxInput, newReference, sender, recipient);
                if (!result.IsSuccess)
                {
                    Console.WriteLine($"Failed to create new flow: {result.Error}");
                    return Result<bool, string>.Failure($"Failed to create new flow: {result.Error}");
                }
                newFlow = result.Ok ?? flowToChange.Clone();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing parameters: {ex.Message}");
                return Result<bool, string>.Failure($"Error parsing parameters: {ex.Message}");
            }
            break;

        default:
            Console.WriteLine("Invalid format. Use 1 for one by one or 2 for comma-separated.");
            return Result<bool, string>.Failure("Invalid format. Use 1 for one by one or 2 for comma-separated.");
    }

    var changeResult = entity.ChangeFlow(flowToChange, newFlow.value, newFlow.frequency, newFlow.offset, newFlow.occurance, newFlow.tax, newFlow.reference, newFlow.sender, newFlow.recipient);
    if (!changeResult.IsSuccess)
    {
        Console.WriteLine($"Failed to change flow '{flowToChange.reference}' in entity '{entity.name}': {changeResult.Error}");
        return Result<bool, string>.Failure($"Failed to change flow '{flowToChange.reference}' in entity '{entity.name}': {changeResult.Error}");
    }
    Console.WriteLine($"Changed flow \r\n{flowToChange.ToString()} -> \r\n{newFlow.ToString()} \r\nin entity '{entity.name}'.");
    return Result<bool, string>.Success(true);
}

Result<bool, string> RemoveNoteFlowFromEntity(Entity? entity = null, string? reference = null, bool force = false)
{
    if (entity == null) entity = GetEntityManual();
    if (entity == null) return Result<bool, string>.Failure("Entity not found.");

    if (string.IsNullOrWhiteSpace(reference))
    {
        Console.WriteLine("Enter flow reference or id to remove:");
        reference = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(reference)) return Result<bool, string>.Failure("Reference is required.");
    }

    if (!force)
    {
        Console.WriteLine($"Are you sure you want to remove flow with reference '{reference}' from entity '{entity.name}'? (y/n)");
        string? confirmation = Console.ReadLine();
        if (confirmation == null || confirmation.ToLower() != "y")
        {
            Console.WriteLine("Flow removal cancelled.");
            return Result<bool, string>.Failure("Flow removal cancelled.");
        }
    }

    NoteFlow? flowToRemove = entity.GetNoteFlowByReferenceOrId(reference);
    if (flowToRemove == null)
    {
        Console.WriteLine($"Flow with reference or id '{reference}' not found in entity '{entity.name}'.");
        return Result<bool, string>.Failure($"Flow with reference or id '{reference}' not found in entity '{entity.name}'.");
    }

    var result = entity.RemoveFlow(flowToRemove);
    if (!result.IsSuccess)
    {
        Console.WriteLine($"Failed to remove flow '{flowToRemove.reference}' from entity '{entity.name}': {result.Error}");
        return Result<bool, string>.Failure($"Failed to remove flow '{flowToRemove.reference}' from entity '{entity.name}': {result.Error}");
    }
    Console.WriteLine($"Removed flow '{flowToRemove.reference}' from entity '{entity.name}'.");
    return Result<bool, string>.Success(true);
}

Result<NoteFlow, string> MakeNoteFlow(long value, int frequency, string offsetInput, int occurance, string taxInput, string reference, string sender, string recipient)
{
    int offset;
    if (offsetInput.StartsWith("+")) 
    {
        offset = int.Parse(offsetInput.Substring(1));
        offset += state.system_date.day % frequency;
    }
    else offset = int.Parse(offsetInput ?? "1") - 1;

    var taxResult = GlobalState.ParseTaxInput(taxInput, value);

    if (reference.Length > GlobalState.NOTE_REFERENCE_LENGTH) return Result<NoteFlow, string>.Failure($"Reference '{reference}' exceeds {GlobalState.NOTE_REFERENCE_LENGTH} characters by {reference.Length - GlobalState.NOTE_REFERENCE_LENGTH} characters.");
    if (!System.Text.RegularExpressions.Regex.IsMatch(reference, @"^[a-zA-Z0-9-_]+$")) return Result<NoteFlow, string>.Failure($"Reference '{reference}' contains invalid characters.");
    if (sender.Length > GlobalState.ENTITY_REFERENCE_LENGTH) return Result<NoteFlow, string>.Failure($"Sender '{sender}' exceeds {GlobalState.ENTITY_REFERENCE_LENGTH} characters by {sender.Length - GlobalState.ENTITY_REFERENCE_LENGTH} characters.");
    if (!System.Text.RegularExpressions.Regex.IsMatch(sender, @"^[a-zA-Z0-9-_]+$")) return Result<NoteFlow, string>.Failure($"Sender '{sender}' contains invalid characters.");
    if (recipient.Length > GlobalState.ENTITY_REFERENCE_LENGTH) return Result<NoteFlow, string>.Failure($"Recipient '{recipient}' exceeds {GlobalState.ENTITY_REFERENCE_LENGTH} characters by {recipient.Length - GlobalState.ENTITY_REFERENCE_LENGTH} characters.");
    if (!System.Text.RegularExpressions.Regex.IsMatch(recipient, @"^[a-zA-Z0-9-_]+$")) return Result<NoteFlow, string>.Failure($"Recipient '{recipient}' contains invalid characters.");

    if (!taxResult.IsSuccess) return Result<NoteFlow, string>.Failure(taxResult.Error!);
    double tax = taxResult.Ok;
    return Result<NoteFlow, string>.Success(new NoteFlow(value, frequency, offset, occurance, tax, reference, sender, recipient));
}

Result<bool, string> AddNoteFlowToEntity(Entity? entity = null, int? format = null, string[]? parameters = null)
{
    if (entity == null) entity = GetEntityManual();
    if (entity == null) return Result<bool, string>.Failure("Entity not found.");

    NoteFlow noteFlow;

    if (parameters != null && parameters.Length == 9)
    {
        try
        {
            long value = long.Parse(parameters[0]);
            int frequency = int.Parse(parameters[1]);
            string offsetInput = parameters[2];
            int occurance = int.Parse(parameters[3]);
            string taxInput = parameters[4];
            string reference = parameters[5];
            string sender = parameters[6];
            string recipient = parameters[7];

            var makeResult = MakeNoteFlow(value, frequency, offsetInput, occurance, taxInput, reference, sender, recipient);
            if (!makeResult.IsSuccess) return Result<bool, string>.Failure(makeResult.Error!);
            noteFlow = makeResult.Ok!;
            entity.AddFlow(noteFlow);
            return Result<bool, string>.Success(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing parameters: {ex.Message}");
            return Result<bool, string>.Failure($"Error parsing parameters: {ex.Message}");
        }
    }

    if (format == null)
    {
        Console.WriteLine("Select flow format: 1. one by one, 2. comma-separated");
        string? formatInput = Console.ReadLine();
        if (formatInput == null || !int.TryParse(formatInput, out int parsedFormat) || (parsedFormat != 1 && parsedFormat != 2))
        {
            Console.WriteLine("Invalid format selection.");
            return Result<bool, string>.Failure("Invalid format selection.");
        }
        format = parsedFormat;
    }

    switch (format)
    {
        case 1:
            var result1 = GetNoteFlowFromUser(entity);
            if (!result1.IsSuccess) return Result<bool, string>.Failure(result1.Error!);
            noteFlow = result1.Ok!;
            break;
        case 2:
            var result2 = GetNoteFlowFromUserCommaSeparated();
            if (!result2.IsSuccess) return Result<bool, string>.Failure(result2.Error!);
            noteFlow = result2.Ok!;
            break;
        default:
            Console.WriteLine("Invalid format selection.");
            return Result<bool, string>.Failure("Invalid format selection.");
    }
    entity.AddFlow(noteFlow);
    Console.WriteLine($"Added flow '{noteFlow.reference}' to entity '{entity.name}'.");
    return Result<bool, string>.Success(true);
}

Result<NoteFlow, string> GetNoteFlowFromUserCommaSeparated()
{
    Console.WriteLine("Enter flow details in the following format:");
    Console.WriteLine("value, frequency, offset, occurance, tax, reference, sender, recipient");
    Console.WriteLine("Example: 1000, 7, 0, -1, 0.25, weekly_income, work, aya");
    string? input = Console.ReadLine();
    if (input == null) return Result<NoteFlow, string>.Failure("No input provided.");

    string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length != 8) return Result<NoteFlow, string>.Failure($"Invalid number of parameters. Expected 8 got {parts.Length}.");

    try
    {
        long value = long.Parse(parts[0].Trim());
        int frequency = int.Parse(parts[1].Trim());
        string offsetInput = parts[2].Trim();
        int occurance = int.Parse(parts[3].Trim());
        string taxInput = parts[4].Trim();
        string reference = parts[5].Trim();
        string sender = parts[6].Trim();
        string recipient = parts[7].Trim();

        return MakeNoteFlow(value, frequency, offsetInput, occurance, taxInput, reference, sender, recipient);
    }
    catch (Exception ex)
    {
        return Result<NoteFlow, string>.Failure($"Error parsing input: {ex.Message}");
    }
}

Result<NoteFlow, string> GetNoteFlowFromUser(Entity entity)
{
    Console.WriteLine("Enter flow details:");
    Console.WriteLine("Value (long): ");
    if (!long.TryParse(Console.ReadLine() ?? "0", out long value)) return Result<NoteFlow, string>.Failure("Invalid value input.");

    Console.WriteLine("Frequency (days): ");
    if (!int.TryParse(Console.ReadLine() ?? "0", out int frequency)) return Result<NoteFlow, string>.Failure("Invalid frequency input.");

    Console.WriteLine("Offset ([+]days): ");
    string? offsetInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(offsetInput)) offsetInput = "0";
    
    Console.WriteLine("Occurance (int, -1 for indefinite): ");
    if (!int.TryParse(Console.ReadLine() ?? "0", out int occurance)) return Result<NoteFlow, string>.Failure("Invalid occurance input.");

    Console.WriteLine("Tax (decimal, int, or percentage, [EMPTY] for default): ");
    string taxInput = Console.ReadLine() ?? (value < 0 ? "0" : GlobalState.TAX.ToString());

    string reference;
    bool validReference;
    do {
        Console.WriteLine($"Reference (string, limit {GlobalState.NOTE_REFERENCE_LENGTH} chars, [a-zA-Z0-9-_]): ");
        reference = Console.ReadLine() ?? "";
        validReference = !entity.ReferenceInUse(reference) && reference.Length <= GlobalState.NOTE_REFERENCE_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(reference, @"^[a-zA-Z0-9-_]+$");
        if (!validReference)
        {
            if (entity.ReferenceInUse(reference)) Console.WriteLine($"Reference '{reference}' is already in use. Please enter a unique reference:");
            else if (reference.Length > GlobalState.NOTE_REFERENCE_LENGTH) Console.WriteLine($"Reference '{reference}' exceeds {GlobalState.NOTE_REFERENCE_LENGTH} characters by {reference.Length - GlobalState.NOTE_REFERENCE_LENGTH} characters. Please enter a shorter reference:");
            else Console.WriteLine($"Reference '{reference}' contains invalid characters. Please enter a valid reference:");
            reference = Console.ReadLine() ?? "";
        }
    } while (!validReference);

    string sender;
    bool validSender;
    do {
        validSender = false;
        Console.WriteLine($"Sender (string, limit {GlobalState.ENTITY_REFERENCE_LENGTH} chars, [a-zA-Z0-9-_], 'show list' to display all existing entity references): ");
        sender = Console.ReadLine() ?? "";
        if (sender.ToLower() == "show list")
        {
            Console.WriteLine("Existing entity references:");
            foreach (var e in state.GetEntities())
            {
                Console.WriteLine($"- {e.reference}");
            }
            Console.WriteLine($"Sender (string, limit {GlobalState.ENTITY_REFERENCE_LENGTH} chars, [a-zA-Z0-9-_], 'show list' to display all existing entity references): ");
            continue;
        }
        validSender = sender.Length <= GlobalState.ENTITY_REFERENCE_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(sender, @"^[a-zA-Z0-9-_]+$");
        if (!validSender)
        {
            if (sender.Length > GlobalState.ENTITY_REFERENCE_LENGTH) Console.WriteLine($"Sender '{sender}' exceeds {GlobalState.ENTITY_REFERENCE_LENGTH} characters by {sender.Length - GlobalState.ENTITY_REFERENCE_LENGTH} characters. Please enter a shorter sender:");
            else Console.WriteLine($"Sender '{sender}' contains invalid characters. Please enter a valid sender:");
            sender = Console.ReadLine() ?? "";
        }
    } while (!validSender);

    string recipient;
    bool validRecipient;
    do {
        validRecipient = false;
        Console.WriteLine($"Recipient (string, limit {GlobalState.ENTITY_REFERENCE_LENGTH} chars, [a-zA-Z0-9-_], 'show list' to display all existing entity references): ");
        recipient = Console.ReadLine() ?? "";
        if (recipient.ToLower() == "show list")
        {
            Console.WriteLine("Existing entity references:");
            foreach (var e in state.GetEntities())
            {
                Console.WriteLine($"- {e.reference}");
            }
            Console.WriteLine($"Recipient (string, limit {GlobalState.ENTITY_REFERENCE_LENGTH} chars, [a-zA-Z0-9-_], 'show list' to display all existing entity references): ");
            continue;
        }
        validRecipient = recipient.Length <= GlobalState.ENTITY_REFERENCE_LENGTH && System.Text.RegularExpressions.Regex.IsMatch(recipient, @"^[a-zA-Z0-9-_]+$");
        if (!validRecipient)
        {
            if (recipient.Length > GlobalState.ENTITY_REFERENCE_LENGTH) Console.WriteLine($"Recipient '{recipient}' exceeds {GlobalState.ENTITY_REFERENCE_LENGTH} characters by {recipient.Length - GlobalState.ENTITY_REFERENCE_LENGTH} characters. Please enter a shorter recipient:");
            else Console.WriteLine($"Recipient '{recipient}' contains invalid characters. Please enter a valid recipient:");
            recipient = Console.ReadLine() ?? "";
        }
    } while (!validRecipient);

    return MakeNoteFlow(value, frequency, offsetInput, occurance, taxInput, reference, sender, recipient);
}

Result<bool, string> MakeTransaction(Entity? senderEntity, Entity? recipientEntity, long value, string taxInput, string reference)
{
    if (senderEntity == null) return Result<bool, string>.Failure("Sender entity not found.");
    if (recipientEntity == null) return Result<bool, string>.Failure("Recipient entity not found.");

    var taxResult = GlobalState.ParseTaxInput(taxInput, value);
    if (!taxResult.IsSuccess)
    {
        Console.WriteLine($"Invalid tax input: {taxResult.Error}");
        return Result<bool, string>.Failure($"Invalid tax input: {taxResult.Error}");
    }
    double tax = taxResult.Ok;
    var result = state.MakeTransaction(senderEntity, recipientEntity, value, tax, reference);
    if (!result.IsSuccess)
    {
        Console.WriteLine($"Failed to make transaction: {result.Error}");
        return Result<bool, string>.Failure($"Failed to make transaction: {result.Error}");
    }
    return Result<bool, string>.Success(true);
}

Entity? GetEntityByReferenceOrId(string reference)
{
    if (int.TryParse(reference, out int id))
        return GetEntityById(id);
    return GetEntityByReference(reference);
}

Entity? GetEntityManual()
{
    Console.WriteLine("Enter entity reference or id:");
    string? reference = Console.ReadLine();
    if (reference == null) return null;

    int id = int.TryParse(reference, out int parsedId) ? parsedId : -1;

    Entity? entity = id == -1 ? state.GetEntityByReference(reference) : state.GetEntityById(id);
    if (entity == null)
    {
        Console.WriteLine($"Entity with {(id == -1 ? $"reference '{reference}'" : $"id {id}")} not found.");
        return null;
    }
    return entity;
}

Entity? GetEntityByReference(string reference)
{
    Entity? entity = state.GetEntityByReference(reference);
    if (entity == null)
    {
        Console.WriteLine($"Entity with reference '{reference}' not found.");
        return null;
    }
    return entity;
}

Entity? GetEntityById(int id)
{
    Entity? entity = state.GetEntityById(id);
    if (entity == null)
    {
        Console.WriteLine($"Entity with id {id} not found.");
        return null;
    }
    return entity;
}

void SaveGlobalState(string? filename)
{
    state.SetDate(1455347);
    if (string.IsNullOrWhiteSpace(filename))
    {
        Console.WriteLine("Please provide a filename.");
        filename = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(filename)) return;
    }

    if (!Directory.Exists("globalStates")) Directory.CreateDirectory("globalStates");

    string filePath = $"globalStates/{filename}.json";
    if (File.Exists(filePath) && loadedGlobalState != filename)
    {
        Console.WriteLine($"File '{filePath}' already exists. Overwrite? (y/n)");
        string? overwriteInput = Console.ReadLine();
        if (overwriteInput == null || overwriteInput.ToLower() != "y")
        {
            Console.WriteLine("Save operation cancelled.");
            return;
        }
    }

    string json = JsonSerializer.Serialize(state, options);
    File.WriteAllText(filePath, json);
    loadedGlobalState = filename;
}

void LoadGlobalState(string? filename)
{
    if (string.IsNullOrWhiteSpace(filename))
    {
        Console.WriteLine("Please provide a filename.");
        filename = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(filename)) return;
    }

    string filePath = $"globalStates/{filename}.json";
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File '{filePath}' does not exist.");
        return;
    }

    string json = File.ReadAllText(filePath);
    GlobalState loadedState = JsonSerializer.Deserialize<GlobalState>(json, options) ?? throw new Exception("Failed to deserialize global state.");
    
    // Replace the current state with the loaded state
    UpdateGlobalState(loadedState);

    loadedGlobalState = filename;
}

void SaveAndLoadGlobalState(string? saveFilename = null, string? loadFilename = null)
{
    SaveGlobalState(saveFilename);
    LoadGlobalState(loadFilename);
}

void FreshGlobalState(bool force = false, bool withEntities = true)
{
    if (!force)
    {
        Console.WriteLine("Are you sure you want to create a new, empty global state? This will discard the current state. (Y/N)");
        string? confirmation = Console.ReadLine();
        if (confirmation == null || confirmation.ToLower() != "y")
        {
            Console.WriteLine("Operation cancelled.");
            return;
        }
    }

    GlobalState newState = new GlobalState();
    UpdateGlobalState(newState);
    loadedGlobalState = null;
    if (withEntities) SetupEntities(newState);
}

void UpdateGlobalState(GlobalState newState)
{
    GlobalState.SetInstance(newState);
    state = GlobalState.Instance;
}   

void SetupEntities(GlobalState state)
{
    state.SetDate(1455347, withProgress: false);
    // state.SetDate(0, withProgress: false);

    Entity aya = new Entity ( "aya", "\"Ata\" Aya Toma", 10000);
    state.AddEntity(aya);

    Entity nemeki = new Entity ("nemeki", "Nemeki Slogen", 5000);
    state.AddEntity(nemeki);

    Entity norpeth = new Entity ("norpeth", "Norpeth son of Normesh", 4000);
    state.AddEntity(norpeth);

    Entity arkild = new Entity ("arkild", "Arkild", 3000);
    state.AddEntity(arkild);

    Dictionary<string, Entity> otherEntities = new Dictionary<string, Entity>
    {
        { "aya_employer", new Entity ("aya_employer", "Aya's Employer", 100000, false) },
        { "aya_landlord", new Entity ("aya_landlord", "Landlord", 100000, false) },
        { "nem_employer", new Entity ("nem_employer", "Nemeki's Employer", 100000, false) },
        { "nem_landlord", new Entity ("nem_landlord", "Landlord", 100000, false) },
        { "achipol_nav", new Entity ("achipol_nav", "Achipol Navigation", 100000, false) },
        { "arkild_inn", new Entity ("arkild_inn", "Inn", 100000, false) },
        { "arkild_work", new Entity ("arkild_work", "Work", 100000, false) },
        { "varied", new Entity ("varied", "Varied", 100000, false) },
        { "informant1", new Entity ("informant1", "Informant 1", 2000, true) },
        { "Broken_Cog", new Entity ("Broken_Cog", "Broken Cog", 100000, false) }
    };

    foreach (var entity in otherEntities.Values) state.AddEntity(entity);



    // Setup initial flows for entities
    aya.AddFlow(new NoteFlow(1000, 7, 0, -1, GlobalState.TAX, "weekly_income", otherEntities["aya_employer"].reference, aya.reference));
    aya.AddFlow(new NoteFlow(-400, 28, 0, -1, 0, "rent", aya.reference, otherEntities["aya_landlord"].reference));
    aya.AddFlow(new NoteFlow(-100, 28, 0, -1, 0, "food", aya.reference, otherEntities["varied"].reference));
    aya.AddFlow(new NoteFlow(-100, 28, 0, -1, 0, "maintenance", aya.reference, otherEntities["varied"].reference));

    nemeki.AddFlow(new NoteFlow(400, 7, 0, -1, GlobalState.TAX, "weekly_income", otherEntities["nem_employer"].reference, nemeki.reference));
    nemeki.AddFlow(new NoteFlow(-200, 28, 0, -1, 0, "rent", nemeki.reference, otherEntities["nem_landlord"].reference));
    nemeki.AddFlow(new NoteFlow(-300, 28, 0, -1, 0, "food", nemeki.reference, otherEntities["varied"].reference));
    nemeki.AddFlow(new NoteFlow(-100, 28, 0, -1, 0, "maintenance", nemeki.reference, otherEntities["varied"].reference));
    
    norpeth.AddFlow(new NoteFlow(100, 7, 0, -1, GlobalState.TAX, "weekly_income", otherEntities["achipol_nav"].reference, norpeth.reference));

    arkild.AddFlow(new NoteFlow(700, 7, 0, -1, GlobalState.TAX, "weekly_income", otherEntities["arkild_work"].reference, arkild.reference));
    arkild.AddFlow(new NoteFlow(-4, 1, 0, -1, 0, "rent", arkild.reference, otherEntities["arkild_inn"].reference));

    otherEntities["informant1"].AddFlow(new NoteFlow(250, 7, 0, -1, GlobalState.TAX, "weekly_income", otherEntities["varied"].reference, otherEntities["informant1"].reference));
    otherEntities["informant1"].AddFlow(new NoteFlow(-100, 28, 0, -1, 0, "rent", otherEntities["informant1"].reference, otherEntities["varied"].reference));
    otherEntities["informant1"].AddFlow(new NoteFlow(-120, 28, 0, -1, 0, "food", otherEntities["informant1"].reference, otherEntities["varied"].reference));
    otherEntities["informant1"].AddFlow(new NoteFlow(-80, 28, 0, -1, 0, "maintenance", otherEntities["informant1"].reference, otherEntities["varied"].reference));

    // Setup transactions
    MakeTransaction(aya, nemeki, 1350, "0", "half_pay");
    MakeTransaction(nemeki, otherEntities["informant1"], 350, "0", "info_payment");
    MakeTransaction(otherEntities["informant1"], otherEntities["varied"], 200, "0", "info_fee");
    Progress(4);
    MakeTransaction(nemeki, otherEntities["informant1"], 200, "0", "info_bonus");
    MakeTransaction(otherEntities["informant1"], otherEntities["varied"], 100, "0", "info_bonus");
    Progress(1);
    MakeTransaction(nemeki, otherEntities["Broken_Cog"], 10, GlobalState.TAX.ToString(), "drinks");
    MakeTransaction(arkild, otherEntities["Broken_Cog"], 10, GlobalState.TAX.ToString(), "drinks");
    MakeTransaction(norpeth, nemeki, 2000, "0", "start_pay");
}