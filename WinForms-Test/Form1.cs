using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailFarms_SharedWeb.Code;
using MailFarms_SharedWeb.Entity;

namespace WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var ping = Request.Ping();

            //var contenuto = File.ReadAllText("C:\\Public\\EmailTemplate.html");

            var contenuto = "Contenuto";

            //for (var i = 0; i < 3; i++)
            //{

                var email = new EmailWeb
                {
                    Contenuto = contenuto,
                    DestinatarioDataRegistrazione = DateTime.Now,
                    DestinatarioEmail = textBox1.Text,
                    DestinatarioNome = textBox1.Text,
                    MittenteEmail = "info@micro.it",
                    MittenteNome = "Test",
                    Oggetto = "Email Corretta",
                    Immediata = false,
                    RispondiA = "risposta@libero.it",
                    UniqueIdentifier = Guid.NewGuid().ToString(),
                    UrlEliminazione = ""
                };

                //var list = new List<Allegati>();

                //foreach (var file in Directory.EnumerateFiles("C:\\public\\allegati"))
                //{
                //    var bytes = File.ReadAllBytes(file);

                //    list.Add(new Allegati()
                //    {
                //        Bytes = bytes,
                //        NomeFile = new FileInfo(file).Name,
                //    });
                //}


                email.Allegati = Array.Empty<Allegati>();

                var result = Request.NuovaEmail(email);

                if (result == null)
                {
                    MessageBox.Show("Null");
                    //continue;
                }

                if (!result.Result)
                {
                    MessageBox.Show(result.Result + " - " + result.Avviso);
                    //continue;
                }
            //}

            //var fast = new EmailWeb
            //{
            //    Contenuto = contenuto,
            //    DestinatarioDataRegistrazione = DateTime.Now,
            //    DestinatarioEmail = textBox1.Text,
            //    DestinatarioNome = textBox1.Text,
            //    MittenteEmail = "info@davidebertagna.it",
            //    MittenteNome = "Davide",
            //    Oggetto = "Fast",
            //    Immediata = true,
            //    RispondiA = "bertasoft@libero.it",
            //    UniqueIdentifier = Guid.NewGuid().ToString(),
            //    UrlEliminazione = ""
            //};

            //var result2 = Request.NuovaEmail(fast);


            //MessageBox.Show("Ok");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //foreach (var file in Directory.EnumerateFiles("C:\\Email\\Html"))
            {
                //var html = File.ReadAllText(file);

                var html = File.ReadAllText("C:\\Email\\Temp.txt");
                var hrefs = GetHref(html);


            }
        }

        /// <summary>
        ///     Ritorna tutti gli href di una pagina html
        /// </summary>
        public static string[] GetHref(string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var admited = new[] { '-', '.', '_', '~', ':', '/', '?', '#', '[', ']', '@', '!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '%', '=' };

            var listHref = new List<string>();

            var inside = false;

            var sb = new StringBuilder();

            for (var i = 0; i < html.Length; i++)
            {
                if (i < html.Length - 3 && html[i] == '<' && html[i + 1] == 'a' && html[i + 2] == ' ')
                {
                    if (!inside)
                        sb.Clear();

                    inside = true;
                }

                if (inside)
                {
                    sb.Append(html[i]);
                }

                if (inside && html[i] == '>')
                {
                    inside = false;

                    var link = sb.ToString();

                    sb.Clear();

                    var index = link.IndexOf("http://", StringComparison.OrdinalIgnoreCase);

                    if (index == -1)
                        index = link.IndexOf("https://", StringComparison.OrdinalIgnoreCase);

                    if (index != -1)
                    {
                        var openQuote = link[index - 1];

                        var sbLink = new StringBuilder();

                        for (var x = index; x < link.Length; x++)
                        {
                            if (link[x] == openQuote)
                                break;

                            if (!admited.Contains(link[x]) && !char.IsLetterOrDigit(link[x]))
                                break;

                            sbLink.Append(link[x]);
                        }

                        link = sbLink.ToString();

                        listHref.Add(link);
                    }
                }
            }

            var linkArray = listHref.Distinct().ToArray();

            return linkArray;
        }
    }
}
