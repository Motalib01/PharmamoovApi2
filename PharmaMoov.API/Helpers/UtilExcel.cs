using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace PharmaMoov.API.Helpers
{
    public class UtilExcel
    {
        IWorkbook workbook;
        ISheet sheet;

        public UtilExcel()
        {
            workbook = new XSSFWorkbook();
            sheet = workbook.CreateSheet("export");
        }

        public void AddRow(int rowNum, List<string> values)
        {
            IRow row = sheet.CreateRow(rowNum);

            ICell cell;
            for (var i = 0; i < values.Count; i++)
            {
                cell = row.CreateCell(i);
                cell.SetCellValue(values[i]);
            }
        }

        public string CreateExcel(string path)
        {

            var dateTick = DateTime.Now.Ticks;
            var fileName = dateTick.ToString() + ".xlsx";

            //FileStream stream = new FileStream("outfile.xlsx", FileMode.Create, FileAccess.Write);
            //workbook.Write(stream);

            //stream.Seek(0, SeekOrigin.Begin);
            //MemoryStream ms = new MemoryStream();
            //stream.CopyTo(ms);

            /*
            MemoryStream ms = new MemoryStream();
            using (FileStream stream = new FileStream("outfile.xlsx", FileMode.Create, FileAccess.ReadWrite))
            {
                workbook.Write(stream, true);
                stream.Seek(0, SeekOrigin.Begin);

                stream.CopyTo(ms);
            }

            return ms;
            */

            try
            {
                FileStream stream = new FileStream(path + fileName, FileMode.Create, FileAccess.ReadWrite);
                workbook.Write(stream);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return fileName;
        }
    }
}
