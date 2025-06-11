using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLibrary.Code;
using Microsoft.AspNetCore.Components;

namespace MailFarmsBlazor.Code
{
    public class PageComponent : MyComponentBase
    {
        [Inject]
        private AlertService Alert { get; set; }

        internal void AlertSuccess(string message = null)
        {
            Alert.Success(message);
        }

        internal void AlertFail(string message)
        {
            Alert.Fail(message);
        }
    }
}
