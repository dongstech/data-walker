using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;

namespace DataWalker.EPPlusExtension
{
    public static class EPPlusExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this ExcelWorksheet worksheet) where T : new()
        {
            Func<CustomAttributeData, bool> columnOnly = y => y.AttributeType == typeof(ColumnAttribute);

            var columns = typeof(T)
                .GetProperties()
                .Where(x => x.CustomAttributes.Any(columnOnly))
                .Select(p => new
                {
                    Property = p,
                    Column = p.GetCustomAttributes<ColumnAttribute>().First().ColumnName
                }).ToList();


            var rows = worksheet.Cells
                .Select(cell => cell.Start.Row)
                .Distinct()
                .OrderBy(x => x);

            var headerIndexMapping = worksheet.Cells[1, 1, 1, worksheet.Dimension.Columns]
                .ToDictionary(_ => _.Text, _ => _.Start.Column);

            var collection = rows.Skip(1)
                .Select(row =>
                {
                    var tnew = new T();
                    columns.ForEach(col =>
                    {
                        var column = headerIndexMapping[col.Column];
                        var val = worksheet.Cells[row, column];
                        if (val.Value == null)
                        {
                            col.Property.SetValue(tnew, null);
                            return;
                        }

                        if (col.Property.PropertyType == typeof(int))
                        {
                            col.Property.SetValue(tnew, val.GetValue<int>());
                            return;
                        }

                        if (col.Property.PropertyType == typeof(double))
                        {
                            col.Property.SetValue(tnew, val.GetValue<double>());
                            return;
                        }

                        if (col.Property.PropertyType == typeof(DateTime))
                        {
                            col.Property.SetValue(tnew, val.GetValue<DateTime>());
                            return;
                        }

                        col.Property.SetValue(tnew, val.GetValue<string>());
                    });

                    return tnew;
                });

            return collection;
        }
    }
}