using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MailFarmsBlazor.Code
{
    /// <summary>
    /// Gestisce il titolo della pagina e il nome utente loggato
    /// </summary>
    public class LayoutService
    {
        public EventCallback<string> OnTitlePageCallback { get; set; }

        public EventCallback<string> OnUserNameCallback { get; set; }
    }
}
