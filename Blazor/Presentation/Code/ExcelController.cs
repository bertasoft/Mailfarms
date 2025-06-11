using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Business.Collection;
using Business.Entity;
using CommonNetCore.GlobalExtension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace MailFarmsBlazor.Code
{
    /// <summary>
    /// Crea i file excel
    /// </summary>
    public class ExcelController : ControllerBase
    {
        private const string DoubleFormat = "#,##0.00_);($#,##0.00)";
        private const string DateTimeExtFormat = "dd/MM/yyyy HH:mm";
        private const string DateTimeFormat = "dd/MM/yyyy";

        private ICellStyle _icellStyleDouble;
        private ICellStyle _icellStyleDateTime;
        private ICellStyle _icellStyleDateTimeExt;

        public void SetCell(int riga, int colonna, object value, ISheet sheet)
        {
            var row = sheet.GetRow(riga) ?? sheet.CreateRow(riga);

            var cell = row.GetCell(colonna) ?? row.CreateCell(colonna);

            if (value == null)
            {
                cell.SetCellType(CellType.String);
                cell.SetCellValue("");
                return;
            }

            var type = value.GetType();

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int64:
                    {
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(Convert.ToDouble(value));
                        break;
                    }

                case TypeCode.Int32:
                    {
                        cell.SetCellType(CellType.Numeric);
                        cell.SetCellValue(Convert.ToInt32(value));
                        break;
                    }

                case TypeCode.Double:
                case TypeCode.Decimal:
                    {
                        cell.SetCellValue(Convert.ToDouble(value));
                        cell.CellStyle = _icellStyleDouble;
                        break;
                    }

                case TypeCode.String:
                    {
                        cell.SetCellType(CellType.String);

                        var valueCell = ((string)value).Decode();

                        if (valueCell.Length > 32767)
                            valueCell = "TOO LONG";

                        cell.SetCellValue(valueCell);
                        break;
                    }

                case TypeCode.DateTime:
                    {
                        var dateTime = (DateTime)value;

                        if (dateTime != default(DateTime))
                            cell.SetCellValue(dateTime);
                        cell.CellStyle = dateTime.Date == dateTime ? _icellStyleDateTime : _icellStyleDateTimeExt;
                        break;
                    }

                case TypeCode.Boolean:
                    {
                        cell.SetCellType(CellType.Boolean);
                        cell.SetCellValue((bool)value);
                        break;
                    }
            }
        }

        [HttpGet]
        public FileContentResult EmailInviateDomini()
        {
            var hssfworkbook = new XSSFWorkbook();

            var sheet = hssfworkbook.CreateSheet("Email Inviate Domini");

            _icellStyleDateTimeExt = hssfworkbook.CreateCellStyle();
            _icellStyleDateTimeExt.DataFormat = hssfworkbook.CreateDataFormat().GetFormat(DateTimeExtFormat);

            _icellStyleDateTime = hssfworkbook.CreateCellStyle();
            _icellStyleDateTime.DataFormat = hssfworkbook.CreateDataFormat().GetFormat(DateTimeFormat);

            _icellStyleDouble = hssfworkbook.CreateCellStyle();
            _icellStyleDouble.DataFormat = hssfworkbook.CreateDataFormat().GetFormat(DoubleFormat);

            var emailInviate = EmailCollection.GetList(wherePredicate: "Stato = 1");
            
            var domini = new HashSet<string>();

            for (var index = 0; index < emailInviate.Count; index++)
            {
                var emailInviata = emailInviate[index];

                var dominio = "http://" + Email.GetDomain(emailInviata.DestinatarioEmail);

                 if (domini.Contains(dominio))
                     continue;

                 domini.Add(dominio);
            }

            var dominiParsati = domini.ToArray();

            for (var i = 0; i < dominiParsati.Length; i++)
                SetCell(i, 0, dominiParsati[i], sheet);

            byte[] bytes;

            //Creo lo stream
            using (var stream = new MemoryStream())
            {
                hssfworkbook.Write(stream);

                bytes = stream.ToArray();
            }

            return File(bytes, "application/vnd.ms-excel", "EmailInviateDomini.xlsx");
        }
    }
}

