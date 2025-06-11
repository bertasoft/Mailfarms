using System;

namespace Business.Code
{
    /// <summary>
    /// Quando varia l'elenco delle mail
    /// </summary>
    public class RefreshService
    {
        public Action OnCodaRefreshCallback { get; set; }
    }
}
