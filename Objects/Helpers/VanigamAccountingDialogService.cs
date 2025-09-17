using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace Vanigam.Objects.Helpers;

public class VanigamAccountingDialogService : DialogService
{
    public Guid Oid { get; private set; }
    public VanigamAccountingDialogService(NavigationManager uriHelper, IJSRuntime jsRuntime) : base(uriHelper, jsRuntime)
    {
        Oid = Guid.NewGuid();
    }

    private List<string> _dialogsList = new List<string>();

    public async Task<dynamic> OpenCustomDialogAsync<T>(string title, Dictionary<string, dynamic> parameters = null, int width = 75, int height = 100) where T : ComponentBase
    {
        //_dialogsList.Add(title);
        //if (_dialogsList.Count == 1)
        //{
        //    return await this.OpenSideAsync<T>(title, parameters, new SideDialogOptions() { Width = "75%", Position = DialogPosition.Right });
        //}
        //else
        //{
        return await this.OpenAsync<T>(title, parameters, new DialogOptions() { Width = $"{width}%", Height = $"{height}%", AutoFocusFirstElement = true, Draggable = true, Resizable = true });
        //}

    }
    public void CloseCustomDialog(dynamic result = null)
    {
        //if (_dialogsList.Count == 1)
        //{
        //    this.CloseSide(result);
        //}
        //else
        //{
        this.Close(result);
        //}
        //_dialogsList.Remove(_dialogsList.LastOrDefault());
    }
    public int OpenedDialogsCount()
    {
        return this.tasks.Count;
    }
}