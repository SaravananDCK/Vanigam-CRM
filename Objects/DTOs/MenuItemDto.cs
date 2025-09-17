using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanigam.CRM.Objects.DTOs
{
    public class MenuItemDto
    {
        public string Text { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public List<MenuItemDto> ChildItems { get; set; } = new List<MenuItemDto>();
    }
}

