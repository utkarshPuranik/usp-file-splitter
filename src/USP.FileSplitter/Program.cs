namespace USP.FileSplitter;

internal class Program
{
	private static async Task Main(string[] args)
	{
		var splitter = new FileSplitter();
		if (args.Length < 1)
		{
			Console.WriteLine("Please provide a command: split or combine");
			return;
		}

		var command = args[0]
			.ToLower();
		switch (command)
		{
			case "split":
				if (args.Length < 4)
				{
					Console.WriteLine("Usage: split <file_path> <parts> <output_folder>");
					return;
				}

				var filePath = args[1];
				var parts = int.Parse(args[2]);
				var outputFolder = args[3];
				await splitter.SplitFileAsync(filePath, parts, outputFolder);
				break;
			case "combine":
				if (args.Length < 3)
				{
					Console.WriteLine("Usage: combine <folder_path> <checksum>");
					return;
				}

				var folderPath = args[1];
				var checksum = args[2];
				await splitter.CombineFilesAsync(folderPath, checksum);
				break;
			default:
				Console.WriteLine("Invalid command. Please use 'split' or 'combine'.");
				break;
		}
	}
}