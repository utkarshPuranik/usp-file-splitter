namespace USP.FileSplitter;

public interface IFileSplitter
{
	public Task SplitFileAsync(string filePath
	                      , int parts
	                      , string outputFolder);

	public Task CombineFilesAsync(string folderPath
	                              , string originalChecksum);
}