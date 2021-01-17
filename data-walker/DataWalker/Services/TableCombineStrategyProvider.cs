using System;
using System.Collections.Generic;
using DataWalker.Models;

namespace DataWalker.Services
{
    public class TableCombineStrategyProvider
    {
        private readonly ITableCombineStrategy _horizontal;
        private readonly ITableCombineStrategy _simpleHorizontal;
        private readonly ITableCombineStrategy _vertical;
        private readonly IDictionary<string, ITableCombineStrategy> _strategies;

        public TableCombineStrategyProvider()
        {
            _horizontal = new HorizontalCombineStrategy();
            _simpleHorizontal = new SimpleHorizontalCombineStrategy();
            _vertical = new VerticalCombineStrategy();
            _strategies ??= new Dictionary<string, ITableCombineStrategy>();
            _strategies.Add(CombineType.Horizontal.ToString().ToLower(), _horizontal);
            _strategies.Add(CombineType.SimpleHorizontal.ToString().ToLower(), _simpleHorizontal);
            _strategies.Add(CombineType.Vertical.ToString().ToLower(), _vertical);
        }

        public ITableCombineStrategy Get(Dictionary<string, TableMappingItem> tableMapping, string tableType)
        {
            if (tableMapping.ContainsKey(tableType))
            {
                var combineType = tableMapping[tableType].CombineType;
                if (_strategies.ContainsKey(combineType))
                {
                    return _strategies[combineType];
                }
                else
                {
                    throw new IllegalCombineTypeException(
                        $"Combine type '{combineType}' is not in the supported list : {CombineType.Horizontal.ToString().ToLower()},{CombineType.Vertical.ToString().ToLower()}");
                }
            }
            else
            {
                return _horizontal;
            }
        }
    }

    public class IllegalCombineTypeException : Exception
    {
        public IllegalCombineTypeException(string s) : base(s)
        {
        }
    }
}