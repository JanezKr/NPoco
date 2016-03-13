using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NPoco.RowMappers;

namespace NPoco
{
    public class MappingFactory
    {
        public static List<Func<IRowMapper>> RowMappers { get; private set; } 
        private readonly PocoData _pocoData;        
        private readonly IRowMapper _rowMapper;      

        static MappingFactory()
        {
            RowMappers = new List<Func<IRowMapper>>()
            {
                () => new DictionaryMapper(),
                () => new ValueTypeMapper(),
                () => new ArrayMapper(),
                () => new PropertyMapper()
            };
        }

        // JK
        public MappingFactory(PocoData pocoData, IDataReader dataReader, Database database)
        // JK
        {
            _pocoData = pocoData;
            _rowMapper = RowMappers.Select(mapper => mapper()).First(x => x.ShouldMap(pocoData));
            // JK
            _rowMapper.Init(dataReader, pocoData, database);
            // JK
        }

        public object Map(IDataReader dataReader, object instance)
        {
            return _rowMapper.Map(dataReader, new RowMapperContext()
            {
                Instance = instance,
                PocoData = _pocoData
            });
        }
    }
}
