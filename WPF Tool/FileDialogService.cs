using Microsoft.Win32;

public class FileDialogService : IFileDialogService
{
    public string? OpenFile(string filter)
    {
        var dlg = new OpenFileDialog { Filter = filter };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}