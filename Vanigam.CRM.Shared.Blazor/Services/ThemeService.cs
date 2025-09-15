using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Shared.Blazor.Services;

public class ThemeService
{
    public bool IsDarkMode { get; set; }

    public void SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        NotifyStateChanged(); // Fire the OnChange event
    }

    public event Action OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}

