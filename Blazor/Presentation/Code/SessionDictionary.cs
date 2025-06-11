using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Entity;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.JSInterop;
using NPOI.SS.Formula.Eval;

namespace MailFarmsBlazor.Code
{
    public class SessionDictionary
    {
        private Dictionary<string, string> NameValue;

        public SessionDictionary()
        {
            NameValue = new Dictionary<string, string>();
        }

        public void AddReplace(string name, string value)
        {
            NameValue[name] = value;
        }

        public bool ContainsKey(string key) => NameValue.ContainsKey(key);

        public bool TryGetValue(string key, out string value) => NameValue.TryGetValue(key, out value);
        
        public bool Remove(string key) => NameValue.Remove(key);
    }
}
