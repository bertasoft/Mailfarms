using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailFarmsBlazor.Pages
{
    public partial class ComponentUpload : ComponentBase
    {        
        [Parameter]
        public string Descrizione { get; set; }

        public void SaveFile()
        {

        }


        public void SetDescrizione(string valore)
        {
            Descrizione = valore;
        }

        public void Refresh()
        {
            StateHasChanged();
        }
    }
}
