using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Collection;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using MailFarmsBlazor.Code;

namespace MailFarmsBlazor.Pages.Private.Configurazione
{
    public partial class Etichette : PageComponent
    {
        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            CaricaEtichette();

            CaricaValore();
        }

        private void CaricaEtichette()
        {
            //elimino le etichette non più presenti nell'enum
            var etdb = EtichetteCollection.GetList();

            var et = Enum.GetNames(typeof(Business.Entity.Etichette.EtichetteEnum));

            foreach (var etd in etdb)
            {
                if (!et.Contains(etd.Nome))
                    etd.Delete();
            }

            //creo quelle mancanti
            foreach (var s in et)
            {
                var r = (Business.Entity.Etichette.EtichetteEnum) Enum.Parse(typeof(Business.Entity.Etichette.EtichetteEnum), s);

                Business.Entity.Etichette.GetValore(r);
            }

            if (string.IsNullOrEmpty(__TextBox_Filtro.Value))
            {
                __DropDownList_Etichette.Values = from p in EtichetteCollection.GetList(orderPredicate: "Nome ASC").OrderBy(p => p.Nome)
                                                  select new BlazorLibrary.Component.Select.Item(p.Nome, p.Id.ToString());
            }
            else
            {
                __DropDownList_Etichette.Values = from p in EtichetteCollection.GetList(wherePredicate: "Nome.Contains(@0) || Valore.Contains(@1)", whereValues: new[] { __TextBox_Filtro.Value, __TextBox_Filtro.Value }, orderPredicate: "Nome ASC").OrderBy(p => p.Nome)
                                                  select new BlazorLibrary.Component.Select.Item(p.Nome, p.Id.ToString());

            }
        }

        private void CaricaValore()
        {
            var selectedItem = __DropDownList_Etichette.SelectedItem();

            if (selectedItem != null)
            {
                var etichetta = Business.Entity.Etichette.GetItem(selectedItem.Value.ToLong());

                __TextBox_Valore.Value = etichetta.Valore;
            }
        }

        public void EtichetteOnSelectedIndexChanged()
        {
            CaricaValore();
        }

        public void SalvaClick()
        {
            var selectedItem = __DropDownList_Etichette.SelectedItem();

            var etichetta = Business.Entity.Etichette.GetItem(selectedItem.Value.ToLong());

            etichetta.Valore = __TextBox_Valore.Value.Trim();

            if (!Business.Entity.Etichette.Save(out Avviso, ref etichetta))
            {
                AlertFail(Avviso);
                return;
            }

            AlertSuccess("Salvataggio effettuato con successo");
        }

        public void FiltraClick()
        {
            CaricaEtichette();

            CaricaValore();
        }

        public void ResetClick()
        {
            __TextBox_Filtro.Value = string.Empty;

            CaricaEtichette();

            CaricaValore();
        }
    }
}
