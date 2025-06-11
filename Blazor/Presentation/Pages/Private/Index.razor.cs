using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Code;
using Business.Collection;
using ChartJs.Blazor.ChartJS;
using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using ChartJs.Blazor.Util;
using CommonNetCore.GlobalExtension;
using CommonNetCore.Misc;
using Microsoft.AspNetCore.Components;

namespace MailFarmsBlazor.Pages.Private
{
    public partial class Index : IDisposable
    {
        LineConfig _lineConfig;

        const string DeCh = "de-ch";

        private bool _localeChangeWasAttempted = false;

        DateTime data = DateTime.Today;

        [Inject]
        RefreshService EmailRefreshService { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (!firstRender)
                return;

            EmailRefreshService.OnCodaRefreshCallback += Refresh;
        }

        void IDisposable.Dispose()
        {
            EmailRefreshService.OnCodaRefreshCallback -= Refresh;
        }

        private void Refresh()
        {
            InvokeAsync(StateHasChanged);
        }

        private async Task TryChangeLocale()
        {
            if (_localeChangeWasAttempted)
                return;

            _localeChangeWasAttempted = true;

            var locales = await MomentJsInterop.GetAvailableLocales(JsRuntime);

            bool localeChanged = false;

            if (locales != null && locales.Contains(DeCh))
                localeChanged = await MomentJsInterop.ChangeLocale(JsRuntime, DeCh);

            if (!localeChanged)
                Console.WriteLine($"Locale was not changed to {DeCh}. Either it already is {DeCh} or this locale doesn't exist.");
        }

        protected override async Task OnInitializedAsync()
        {
            EmailRefreshService.OnCodaRefreshCallback += Refresh;

            await layoutService.OnTitlePageCallback.InvokeAsync("Home");

            var query = GetQueryString("Data");

            if (!Check.IsNullOrEmpty(query))
                data = DateTime.Parse((string)GetQueryString("Data"));

            _lineConfig = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = "Carico Email"
                    },
                    Legend = new Legend
                    {
                        Position = Position.Right,
                        Labels = new LegendLabelConfiguration
                        {
                            UsePointStyle = true
                        }
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false
                    },
                    Scales = new Scales
                    {
                        xAxes = new List<CartesianAxis>
{
                        new TimeAxis
                        {
                            Distribution = TimeDistribution.Linear,
                            Ticks = new TimeTicks
                            {
                                Source = TickSource.Data
                            },
                            Time = new TimeOptions
                            {
                                Unit = TimeMeasurement.Day,
                                Round = TimeMeasurement.Day,
                                TooltipFormat = "DD.MM.YYYY",
                                DisplayFormats = TimeDisplayFormats.DE_CH
                            },
                            ScaleLabel = new ScaleLabel
                            {
                                LabelString = "Giorno"
                            }
                        }
                    }
                    },
                    Hover = new LineOptionsHover
                    {
                        Intersect = true,
                        Mode = InteractionMode.Y
                    }
                }
            };

            foreach (var server in ServerCollection.GetList())
            {
                var stats = ServerStatisticheCollection.GetList(wherePredicate: "IdServer == " + server.Id + " AND Data > " + data.AddDays(-20) + " AND Data <= " + data);

                #region Inviate

                var colore = (byte)(NumberGenerator.GetInt(0, 10) * 15);

                var inviateLine = new LineDataset<TimeTuple<int>>
                {
                    BorderColor = ColorUtil.ColorString(colore, 255, colore), //verde
                    Label = server.Ip + " - Inviate",
                    Fill = false, //no background color
                    LineTension = 0,
                    BorderWidth = 2,
                    PointRadius = 6,
                    PointBorderWidth = 1,
                    SteppedLine = SteppedLine.False,
                    Hidden = true,
                };

                inviateLine.AddRange(stats.Select(p => new TimeTuple<int>(new Moment(p.Data), (int)p.Inviate)));

                _lineConfig.Data.Datasets.Add(inviateLine);

                #endregion

                #region Errate

                colore = (byte)(NumberGenerator.GetInt(0, 10) * 15);

                var errateLine = new LineDataset<TimeTuple<int>>
                {
                    BorderColor = ColorUtil.ColorString(255, colore, colore), //rosso
                    Label = server.Ip + " - Errate",
                    Fill = false, //no background color
                    LineTension = 0,
                    BorderWidth = 2,
                    PointRadius = 6,
                    PointBorderWidth = 1,
                    SteppedLine = SteppedLine.False,
                    Hidden = true,
                };

                errateLine.AddRange(stats.Select(p => new TimeTuple<int>(new Moment(p.Data), (int)p.Errate)));

                _lineConfig.Data.Datasets.Add(errateLine);

                #endregion

                #region Inviate

                colore = (byte)(NumberGenerator.GetInt(0, 10) * 15);

                var totaliLine = new LineDataset<TimeTuple<int>>
                {
                    BorderColor = ColorUtil.ColorString(colore, colore, colore), //verde
                    Label = server.Ip + " - Totali",
                    Fill = false, //no background color
                    LineTension = 0,
                    BorderWidth = 2,
                    PointRadius = 6,
                    PointBorderWidth = 1,
                    SteppedLine = SteppedLine.False,
                };

                totaliLine.AddRange(stats.Select(p => new TimeTuple<int>(new Moment(p.Data), (int)(p.Inviate + p.Errate))));

                _lineConfig.Data.Datasets.Add(totaliLine);

                #endregion
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await TryChangeLocale();
        }

        public void Indietro10Click()
        {
            data = data.AddDays(-10);
            uriHelper.NavigateTo("/private?Data=" + data.ToShortDateString(), true);
        }


        public void IndietroClick()
        {
            data = data.AddDays(-1);
            uriHelper.NavigateTo("/private?Data=" + data.ToShortDateString(), true);
        }

        public void AvantiClick()
        {
            data = data.AddDays(1);
            uriHelper.NavigateTo("/private?Data=" + data.ToShortDateString(), true);
        }

        public void Avanti10Click()
        {
            data = data.AddDays(10);
            uriHelper.NavigateTo("/private?Data=" + data.ToShortDateString(), true);
        }

    }
}
