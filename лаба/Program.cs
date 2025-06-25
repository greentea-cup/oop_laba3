// vim: set fdm=indent :
using System.IO;
using Npgsql;

class State {
	public NpgsqlDataSource ds;
	public int? selectedPrinter;

	public State(NpgsqlDataSource ds) {
		this.ds = ds;
	}

	public async Task ListPrinters(string[] words) {
		NpgsqlCommand cmd = ds.CreateCommand("SELECT id, label, finished_jobs FROM printers");
		try {
			var reader = await cmd.ExecuteReaderAsync();
			Console.WriteLine("Список принтеров:\nid\tlabel\tfinished_jobs");
			while (await reader.ReadAsync()) {
				int id = reader.GetInt32(0);
				string label = reader.GetString(1);
				int finished_jobs = reader.GetInt32(2);
				Console.WriteLine($"{id}\t{label}\t{finished_jobs}");
			}
			Console.WriteLine("=== Конец ===");
		}
		catch (Exception e) {
			Console.WriteLine("Произошла ошибка:");
			Console.WriteLine(e);
		}
	}

	public async Task AddPrinter(string[] words) {
		if (words.Length != 2) {
			Console.WriteLine("Bad format. Usage: <printer label>");
			return;
		}
		string label = words[1];
		var cmd = ds.CreateCommand("INSERT INTO printers (label) VALUES ($1)");
		cmd.Parameters.AddWithValue(label);
		try {
			await cmd.ExecuteNonQueryAsync();
		}
		catch (Exception e) {
			Console.WriteLine("Произошла ошибка:");
			Console.WriteLine(e);
		}
	}

	public async Task SelectActivePrinter(string[] words) {
		if (words.Length != 2) {
			Console.WriteLine("Bad format. Usage: <printer label>");
			return;
		}
		string label = words[1];
		var cmd = ds.CreateCommand("SELECT id FROM printers WHERE printers.label = $1 LIMIT 1");
		cmd.Parameters.AddWithValue(label);
		var reader = await cmd.ExecuteReaderAsync();
		if (reader.Read()) {
			int printerId = reader.GetInt32(0);
			selectedPrinter = printerId;
			Console.WriteLine($"Выбран принтер #{selectedPrinter} ({label})");
		}
		else {
			Console.WriteLine("Принтер не найден");
		}
	}

	async Task AddPrintJob(string[] words) {

	}
}

class Program {
	static async Task Main() {
		string dsectionString = File.ReadAllText(".secret");
		var ds = NpgsqlDataSource.Create(dsectionString);
		if (ds == null) {Console.WriteLine("Bad connection"); return;}
		State s = new State(ds);
		bool running = true;
		while (running) {
			string? line = Console.ReadLine();
			if (line == null || line.Length < 1) return;
			string[] words = line.Split(' ');
			if (words.Length < 1) return;
			switch (words[0]) {
				default: Console.WriteLine($"Unknown command: {words[0]}"); break;
				// case "h": PrintHelp(); break;
				case "q": running = false; break;
				case "p":  await s.ListPrinters(words); break;
				case "pn": await s.AddPrinter(words); break;
				case "pd": await s.DeletePrinter(words); break;
				case "ps": await s.SelectActivePrinter(words); break;
				case "j":  await s.ListJobs(words); break;
				case "jn": await s.AddPrintJob(words); break;
				case "jc": await s.CancelPrintJob(words); break;
				case "jf": await s.FinishPrintJob(words); break;
			}
		}
	}
}

