using System;
using System.Collections.Generic;
using System.Data;

namespace NPoco.RowMappers
{
    public class DictionaryMapper : RowMapper
    {
        private PosName[] _posNames;

        public override bool ShouldMap(PocoData pocoData)
        {
            return pocoData.Type == typeof (object)
                   || pocoData.Type == typeof (Dictionary<string, object>)
                   || pocoData.Type == typeof (IDictionary<string, object>);
        }

        // JK => is cuorrect virtual = shouldn't be override?
        public override void Init(IDataReader dataReader, PocoData pocoData, Database database)
        // JK
        {
            _posNames = GetColumnNames(dataReader, pocoData);
        }

        public override object Map(IDataReader dataReader, RowMapperContext context)
        {
            IDictionary<string, object> target = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

#if !NET35
            if (context.Type == typeof(object))
                target = new PocoExpando();
#endif

            for (int i = 0; i < _posNames.Length; i++)
            {
                var converter = context.PocoData.Mapper.Find(x => x.GetFromDbConverter(typeof(object), dataReader.GetFieldType(_posNames[i].Pos))) ?? (x => x);
                target.Add(_posNames[i].Name, dataReader.IsDBNull(_posNames[i].Pos) ? null : converter(dataReader.GetValue(_posNames[i].Pos)));
            }

            return target;
        }
    }
}